using Assets.Input;
using Assets.Menus;
using Assets.Utility;
using Assets.Utility.Netplay;
using FooziesConstants;
using Gameplay;
using Match;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Gameplay
{
    public class OnlineGame : GameplayState
    {
        enum OnlineMatchState { WaitingForHost, WaitingForGuest, Synchronizing, Gameplay, MatchOver}

        private OnlineMatchState _currentOnlineState;
        private int _preMatchSyncCountdown;
        private const int _preMatchSyncMax = 50;
        private int _estimatedDelayMs = 0;
        private List<SyncMessage> _syncMessages;
        private List<SyncMessage> _selfSyncState;
        private bool _initializedNetworkManager = false;

        public OnlineGame(SetData _setData) : base(_setData)
        {
            _syncMessages = new List<SyncMessage>();
            _selfSyncState = new List<SyncMessage>();
            if(GameManager.Instance.NetplayState.IsFake)
            {
                InitGameplay();
            }
            else if (GameManager.Instance.NetplayState.IsHost)
                _currentOnlineState = OnlineMatchState.WaitingForGuest;
            else
                _currentOnlineState = OnlineMatchState.WaitingForHost;
            //online game needs several overrides :
            //1. before starting a match there needs to be a synchronization phase
            //2. need to send opponent inputs history during update
        }

        public override void Update(Inputs _inputs)
        {
            var comsFromOpponent = NetworkManager.Instance.GetUnreadCommunications();
            if(!_initializedNetworkManager)
            {
                _initializedNetworkManager = true;
                NetworkManager.Instance.StartListening();
            }
            switch(_currentOnlineState)
            {
                case OnlineMatchState.WaitingForGuest:
                    //wait for message from guest containing his IP so we can begin synchronizing
                    foreach(var oppCom in comsFromOpponent)
                    {
                        if(oppCom.Communication_Type == Communication.CommunicationType.ClientConnectingToHost)
                        {
                            //get opponent info
                            ConnectToHostMessage msg = JsonUtility.FromJson(oppCom.Message, typeof(ConnectToHostMessage)) as ConnectToHostMessage;
                            GameManager.Instance.NetplayState.OpponentIp = msg.MyIP;
                            m_setData.InitData.P2_Character = CharacterSelectScreen.GetCharacterFromString(msg.MyCharacter);
                            _currentOnlineState = OnlineMatchState.Synchronizing;
                            _preMatchSyncCountdown = _preMatchSyncMax;

                            //send ready
                            ConnectToGuestMessage toGuest = new ConnectToGuestMessage() { MyCharacter = CharacterSelectScreen.GetMatchingCharacterName(m_setData.InitData.P1_Character) };
                            Communication comToGuest = new Communication() { Communication_Type = Communication.CommunicationType.ReadyToBeginSync, Message = JsonUtility.ToJson(toGuest) };
                            comToGuest.Send();
                            break;
                        }
                    }
                    break;
                case OnlineMatchState.WaitingForHost:
                    //keep spamming host until we get his reply that he got our ip (and therefore is ready to sync)
                    Communication.CreateGuestFirstContact(CharacterSelectScreen.GetMatchingCharacterName(m_setData.InitData.P1_Character)).Send();

                    foreach (var oppCom in comsFromOpponent)
                    {
                        if (oppCom.Communication_Type == Communication.CommunicationType.ReadyToBeginSync)
                        {
                            _currentOnlineState = OnlineMatchState.Synchronizing;
                            _preMatchSyncCountdown = _preMatchSyncMax;
                            break;
                        }
                    }
                    break;
                case OnlineMatchState.Synchronizing:
                    SendSyncMessage(_preMatchSyncCountdown);
                    var syncMessages = comsFromOpponent.Where(o => o.Communication_Type == Communication.CommunicationType.Synchronization).Select(o => o.Message).ToList();
                    Synchronize(syncMessages);
                    _preMatchSyncCountdown--;
                    if(_preMatchSyncCountdown < 0)
                    {
                        InitGameplay();
                    }
                    break;
                case OnlineMatchState.Gameplay:
                    //estimate that opponent's inputs are the same as previous inputs
                    var currentInputs = SingleFrameInputValidation.CreateNextInputs(_inputHistory.Last());
                    currentInputs.P1_Inputs = _inputs.P1_Inputs;
                    _inputHistory.Add(currentInputs);

                    //serialize input history
                    var serializedInputHistory = CreateInputSerializedString();
                    //send inputs to other player
                    Communication latstInputsHistory = new Communication() { Message = serializedInputHistory, Communication_Type = Communication.CommunicationType.InputHistory };
                    latstInputsHistory.Send();
                    //handle opponent inputs
                    foreach(var inputComFromOpponent in comsFromOpponent.Where(o=>o.Communication_Type == Communication.CommunicationType.InputHistory))
                    {
                        var inputHistoryFromOpponentStr = JsonHelper.FromJson<string>(inputComFromOpponent.Message);
                        var inputHistoryFromOpponent = new List<SingleFrameInputValidation>();
                        foreach(var v in inputHistoryFromOpponentStr)
                        {
                            var opponentInputInfo = JsonUtility.FromJson(v, typeof(SingleFrameInputValidation)) as SingleFrameInputValidation;
                            //if both are same side, switch opponent info
                            if(currentInputs.P1_Source == opponentInputInfo.P1_Source)
                            {
                                var temp = opponentInputInfo.P2_Inputs;
                                opponentInputInfo.P2_Inputs = opponentInputInfo.P1_Inputs;
                                opponentInputInfo.P1_Inputs = temp;
                                opponentInputInfo.P1_Source = !opponentInputInfo.P1_Source;
                            }
                            inputHistoryFromOpponent.Add(opponentInputInfo);
                        }
                        AdjustStateForNetplay(inputHistoryFromOpponent);
                    }
                    //gameplay proceeds as usual
                    base.Update(_inputs);
                    break;
                case OnlineMatchState.MatchOver:
                    break;
            }
        }

        private void InitGameplay()
        {
            _currentOnlineState = OnlineMatchState.Gameplay;
            var firstFrameBlankInputs = new SingleFrameInputValidation();
            firstFrameBlankInputs.FrameNumber = 0;
            _inputHistory.Add(firstFrameBlankInputs);
        }

        private string CreateInputSerializedString()
        {
            int historyToSendCount = GameManager.Instance.Configuration.RollbackMax;
            if (historyToSendCount > _inputHistory.Count)
                historyToSendCount = _inputHistory.Count;
            var inputHistoryToSend = _inputHistory.GetRange(_inputHistory.Count - historyToSendCount, historyToSendCount);

            //use newtonsoft instead if you are not working in unity
            List<string> serializedInputsList = new List<string>();
            inputHistoryToSend.ForEach(o => serializedInputsList.Add(JsonUtility.ToJson(o)));
            var serializedInput = JsonHelper.ToJson<string>(serializedInputsList.ToArray());
            return serializedInput;

        }

        private void Synchronize(List<string> syncMessages)
        {
            //for now host will just be the master during this phase, game starts when he's done.
            //I may change this in the future if it isn't stable enough

            //add messages to history
            foreach(string msg  in syncMessages)
            {
                SyncMessage syncData = JsonUtility.FromJson(msg, typeof(SyncMessage)) as SyncMessage;
                _syncMessages.Add(syncData);
            }

            if (!GameManager.Instance.NetplayState.IsHost && _selfSyncState.Count > 4)
            {
                int numberOfMessages = 0;
                double sumOfDifference = 0;
                for (int i = _syncMessages.Count - 1; i >= 0 && _syncMessages.Count - i > 20; i--)
                {
                    var theirSyncData = _syncMessages[i];
                    var myMatchingSyncData = _selfSyncState.FirstOrDefault(o => o.FrameCount == theirSyncData.FrameCount);
                    if(myMatchingSyncData != null)
                    {
                        var timedifference = theirSyncData.Timestamp - myMatchingSyncData.Timestamp;
                        sumOfDifference += timedifference.TotalMilliseconds;
                        numberOfMessages++;
                    }
                }
                if (numberOfMessages > 0)
                {
                    //if opponent stuff occurs at like 150ms for frame 120, while your stuff is at 100ms for frame 120, the diff for each element will be at around 50ms
                    //you therefore want to elmulate that on your end frame 120 happenend at 150ms
                    //so you take that 50ms, divide by ms/frame, that's around 3 frames
                    //so we want our frame that occured on 100ms to be 123 instead of 120, so that 50ms later we're at frame #120
                    //since only the guest is adjusting his timing, over the course of X (120frames?) he should be able to sync up

                    var averageDifferenceInTiming = sumOfDifference / numberOfMessages;
                    var framesDifference = Convert.ToInt32(Math.Round(averageDifferenceInTiming * GameplayConstants.FRAME_LENGTH, MidpointRounding.AwayFromZero));
                    if(framesDifference != 0)
                    {
                        _preMatchSyncCountdown += framesDifference;
                        _selfSyncState.Clear();
                    }
                }
            }

            //estimate delay. host doesn't adjust his countdown, but guest does
            //check up to 20 last messages, 
        }

        private void SendSyncMessage(int syncFrame)
        {
            SyncMessage syncMessage = new SyncMessage()
            {
                Timestamp = DateTime.Now,
                FrameCount = syncFrame,
                EstimatedDelayAtThisTime = _estimatedDelayMs
            };
            Communication com = new Communication()
            {
                Message = JsonUtility.ToJson(syncMessage),
                Communication_Type = Communication.CommunicationType.Synchronization
            };
            com.Send();
            _selfSyncState.Add(syncMessage);
        }

        private class SyncMessage
        {
            public DateTime Timestamp
            {
                set
                {
                    Timestamp_Str = value.ToString("yyyy-MM-dd HH:mm:ss.ffffff",
                                            CultureInfo.InvariantCulture);
                }
                get
                {
                    return DateTime.Parse(Timestamp_Str);
                }
            }
            public string Timestamp_Str;
            public int FrameCount;
            public int EstimatedDelayAtThisTime;
            public SyncMessage()
            {

            }
        }

        public class ConnectToHostMessage
        {
            public string MyIP;
            public string MyCharacter;
            public ConnectToHostMessage()
            {

            }
        }

        public class ConnectToGuestMessage
        {
            public string MyCharacter;
            public ConnectToGuestMessage()
            {

            }
        }

        public override string GetDebugInfo()
        {
            switch(_currentOnlineState)
            {
                case OnlineMatchState.MatchOver:
                    break;
                case OnlineMatchState.Gameplay:
                    return base.GetDebugInfo();
                    break;
                case OnlineMatchState.Synchronizing:
                    return "Online State - Synchronizing : " + _preMatchSyncCountdown.ToString();
                    break;
                case OnlineMatchState.WaitingForGuest:
                    return "Online State - WaitingForGuest : ";
                    break;
                case OnlineMatchState.WaitingForHost:
                    return "Online State - WaitingForHost : ";
                    break;
            }
            return "online match state debug info - unexpected state"; 
        }
    }
}
