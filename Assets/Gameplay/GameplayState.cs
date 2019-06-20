using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using FooziesConstants;
using Assets.States;
using Assets.Match;
using Assets.Gameplay;

namespace Gameplay
{


    /// <summary>
    /// When the state of the game is a match (and not a menu)
    /// </summary>

    public abstract class GameplayState : ApplicationState
    {
        public GameplayRenderer GameplayRendererObject;
        private List<GameState> m_previousStates;
        private List<List<GameState>> m_statesPreviousRounds;
        private GameState m_previousState
        {
            get
            {
                if (m_previousStates.Count > 0)
                    return m_previousStates[m_previousStates.Count - 1];
                else if (m_statesPreviousRounds.Count > 0)
                    return m_statesPreviousRounds.Last().LastOrDefault();
                return null ;
            }
        }
        public MatchState Match;
        SinglePlayerInputs m_p1LastInputs;//for debugging
        SinglePlayerInputs m_p2LastInputs;
        SplashState CurrentSplashState;
        protected Match.SetData m_setData;

        static public GameplayState CreateGameplayState(Match.SetData _setData, GameplayRenderer _renderer, bool _online)
        {
            GameplayState gs;
            if(_online)
                gs = new OnlineGame(_setData);
            else
                gs = new OfflineGame(_setData);
            gs.GameplayRendererObject = _renderer;
            gs.StateRenderer = _renderer;
            return gs;
        }

        protected GameplayState(Match.SetData _setData) : base()
        {
            m_previousStates =  new List<GameState>();
            m_statesPreviousRounds = new List<List<GameState>>();
            m_setData = _setData;
            Match = new MatchState();
            CurrentSplashState = new SplashState();
            m_previousStates.Add(new GameState(_setData.InitData, 0));
            m_previousStates.Add(SetRoundStart());
        }

        /// <inheritdoc />
        public override void Rollback(List<Inputs> _inputsHistory)
        {
            int numberOfFramesToRollback = _inputsHistory.Count;
            //set current state back X steps
            int startingFrameToRollback = m_previousStates.Count - numberOfFramesToRollback;
            if (startingFrameToRollback < 0)
                startingFrameToRollback = 0;
            m_previousStates.RemoveRange(startingFrameToRollback, m_previousStates.Count - startingFrameToRollback);

            //recalculate up to current state
            foreach(var inputAtFrame in _inputsHistory)
            {
                //there is a risk that the splash screen state might not sync correctly, so if this is a problem, consider it
                GameplayStateUpdate(inputAtFrame.P1_Inputs, inputAtFrame.P2_Inputs, true);
            }
        }


        public override void Update(Inputs _inputs)
        {
            GameplayStateUpdate(_inputs.P1_Inputs, _inputs.P2_Inputs);
        }

        public GameState GetPastState(int framesAgo)
        {
            if(framesAgo + 1 > m_previousStates.Count)
            {
                return m_previousStates[0];
            }
            if (framesAgo < 0)
            {
                return m_previousStates[m_previousStates.Count - 1];
            }
            return m_previousStates[m_previousStates.Count - 1 - framesAgo];
        }

        void GameplayStateUpdate(SinglePlayerInputs _p1Inputs, SinglePlayerInputs _p2Inputs, bool skipRender = false)
        {


            //1.1 Check for pause? todo
            if (CurrentSplashState.CurrentState == SplashState.State.GameOver)
            {
                //check if any button is pressed to start a new game
                if (_p1Inputs.A || _p1Inputs.B || _p1Inputs.C || _p1Inputs.Start ||
                    _p2Inputs.A || _p2Inputs.B || _p2Inputs.C || _p2Inputs.Start)
                {
                    ApplicationStateManager.GetInstance().SetCharacterSelectScreen(m_setData);
                }
            }
            else if (UpdateSplashScreen())
            {

            }
            else if (m_previousState.RemainingHitstop > 0)
            {
                m_previousState.RemainingHitstop--;
            }
            else
            {

                //2. Sees if the inputs can be applied to current action
                GameState currentState = UpdateGameStateWithInputs(_p1Inputs, _p2Inputs, m_previousState);
                m_p1LastInputs = _p1Inputs;
                m_p2LastInputs = _p2Inputs;

                //3. Resolves actions
                MatchOutcome outcome = ResolveActions(_p1Inputs, _p2Inputs, currentState);



                //4. Checks if the match is over
                if (outcome.IsEnd())
                {
                    //Extra render on game end?
                    GameplayRendererObject.RenderScene(m_previousState, Match, CurrentSplashState);
                    HandleOutcome(outcome);
                    CurrentSplashState.CurrentState = SplashState.State.RoundOver_ShowResult;
                    CurrentSplashState.FramesRemaining = GameplayConstants.FRAMES_END_ROUND_SPLASH;


                    //camera tests
                    //isPanning = true;
                    isRotating = true;
                }
                --currentState.RemainingTime;
                m_previousStates.Add(currentState);

            }
            if (!skipRender)
            {
                //5. Render Scene
                GameplayRendererObject.RenderScene(m_previousState, Match, CurrentSplashState);

                //6. Camera Tests
                CameraMoveTest(Match);
            }
        }

        //returns whether or not to stop here and wait a few more frames
        public bool UpdateSplashScreen()
        {


            //removing frames has to be done before checking the state as if the splash happens during gameplay it should still be rendered
            if (CurrentSplashState.FramesRemaining > 0)
                --CurrentSplashState.FramesRemaining;

            if (CurrentSplashState.CurrentState == SplashState.State.None)
                return false;
            if (CurrentSplashState.FramesRemaining == 0)
            {
                switch (CurrentSplashState.CurrentState)
                {
                    case SplashState.State.RoundOver_ShowResult:
                        m_statesPreviousRounds.Add(m_previousStates); //keep a backup of the round that just ended, regardless if it's match end.
                        if (Match.GameOver)
                        {
                            CurrentSplashState.CurrentState = SplashState.State.GameOver;
                            CurrentSplashState.FramesRemaining = GameplayConstants.GAME_OVER_LENGTH;
                        }
                        else
                        {
                            m_previousStates = new List<GameState>();
                            m_previousStates.Add(SetRoundStart());
                        }
                        break;
                    case SplashState.State.RoundStart_3:
                        CurrentSplashState.CurrentState = SplashState.State.RoundStart_2;
                        CurrentSplashState.FramesRemaining = GameplayConstants.FRAMES_COUNTDOWN;
                        break;
                    case SplashState.State.RoundStart_2:
                        CurrentSplashState.CurrentState = SplashState.State.RoundStart_1;
                        CurrentSplashState.FramesRemaining = GameplayConstants.FRAMES_COUNTDOWN;
                        break;
                    case SplashState.State.RoundStart_1:
                        CurrentSplashState.CurrentState = SplashState.State.RoundStart_F;
                        CurrentSplashState.FramesRemaining = GameplayConstants.FRAMES_COUNTDOWN;
                        break;
                    case SplashState.State.RoundStart_F:
                        CurrentSplashState.CurrentState = SplashState.State.None;
                        CurrentSplashState.FramesRemaining = GameplayConstants.FRAMES_COUNTDOWN;
                        break;
                    case SplashState.State.GameOver:
                        //hmm this could go on indefinitely
                        //ApplicationStateManager.GetInstance().SetCharacterSelectScreen(m_setData);
                        break;

                }
            }
            if (CurrentSplashState.CurrentState == SplashState.State.RoundStart_F)
                return false;
            return true;
        }

        public override string GetDebugInfo()
        {
            if (m_previousState == null)
                return "previous state null";
            string score = "\n\rP1 Score : " + Match.P1_Score.ToString() +
                "\n\rP2 Score : " + Match.P2_Score.ToString() +
                "\n\rTime : " + (m_previousState.RemainingTime / 60).ToString();


            //string splash = CurrentSplashState.CurrentState.ToString() + CurrentSplashState.FramesRemaining.ToString();

            return "";//score;

            //GUI.TextArea(new Rect(10, 10, Screen.width - 10, Screen.height / 2), score + sweepdebug);
        }

        private void HandleOutcome(MatchOutcome _outcome)
        {
            GameManager.Instance.SoundManager.LowerVolume();
            GameManager.Instance.SoundManager.PlayRoundEndVoice(_outcome.Outcome);
            if (_outcome.P1_Scores)
            {
                Match.P1_Score++;
            }
            if (_outcome.P2_Scores)
            {
                Match.P2_Score++;
            }
            Match.Outcomes.Add(_outcome);
            if (_outcome.Outcome == GameplayEnums.Outcome.TimeOut || _outcome.Outcome == GameplayEnums.Outcome.Trade)
            {
                if (Match.P1_Score >= GameplayConstants.ROUNDS_TO_WIN)
                    Match.P1_Score = GameplayConstants.ROUNDS_TO_WIN - 1;
                if (Match.P2_Score >= GameplayConstants.ROUNDS_TO_WIN)
                    Match.P2_Score = GameplayConstants.ROUNDS_TO_WIN - 1;
            }
            if (Match.P1_Score >= GameplayConstants.ROUNDS_TO_WIN || Match.P2_Score >= GameplayConstants.ROUNDS_TO_WIN)
            {
                Match.GameOver = true;
                m_setData.AddResult(_outcome);
            }
        }

        private GameState SetRoundStart()
        {
            GameManager.Instance.SoundManager.RestoreVolume();
            CurrentSplashState.CurrentState = SplashState.State.RoundStart_3;
            CurrentSplashState.FramesRemaining = GameplayConstants.FRAMES_COUNTDOWN;

            GameState state = new GameState(m_setData.InitData, 0);

            state.P1_Gauge = m_previousState.P1_Gauge;
            state.P1_Hitboxes.Add(CreateCharacterStartingHitbox());
            state.P1_Position = GameplayConstants.STARTING_POSITION * -1;
            state.P1_State = GameplayEnums.CharacterState.Idle;
            state.P1_StateFrames = 0;

            state.P2_Gauge = m_previousState.P2_Gauge;
            state.P2_Hitboxes.Add(CreateCharacterStartingHitbox());
            state.P2_Position = GameplayConstants.STARTING_POSITION;
            state.P2_State = GameplayEnums.CharacterState.Idle;
            state.P2_StateFrames = 0;

            state.RemainingHitstop = 0;
            state.RemainingTime = GameplayConstants.FRAMES_PER_ROUND;


            m_p1LastInputs = new SinglePlayerInputs();
            m_p2LastInputs = new SinglePlayerInputs();


            ResetCamera();

            return state;
        }



        private Hitbox_Gameplay CreateCharacterStartingHitbox()
        {
            Hitbox_Gameplay hbox = new Hitbox_Gameplay(GameplayEnums.HitboxType.Hurtbox_Main, 0, GameplayConstants.CHARACTER_HURTBOX_WIDTH);
            return hbox;
        }

        private SinglePlayerInputs GetInputs(bool _p1)
        {
            short direction = 5;
            SinglePlayerInputs inputs = new SinglePlayerInputs();
            float h;
            float v;

            if (_p1)
            {
                //if(P1_Joystick)
                {
                    h = Input.GetAxisRaw("Horizontal_PsStick1") + Input.GetAxisRaw("Horizontal_KB1"); ;
                    v = Input.GetAxisRaw("Vertical_PsStick1") + Input.GetAxisRaw("Vertical_KB1");
                }
                //else
                //{
                //    h = Input.GetAxis("Horizontal_KB1");
                //    v = Input.GetAxis("Vertical_KB1");
                //}
                inputs.A = Input.GetButton("A_P1");
                inputs.B = Input.GetButton("B_P1");
                inputs.C = Input.GetButton("C_P1");
                inputs.Start = Input.GetButtonDown("Start_P1");
            }
            else
            {
                //if (P2_Joystick)
                {
                    h = Input.GetAxisRaw("Horizontal_PsStick2") + Input.GetAxisRaw("Horizontal_KB2");
                    v = Input.GetAxisRaw("Vertical_PsStick2") + Input.GetAxisRaw("Vertical_KB2");
                }
                //else
                //{
                //    h = Input.GetAxis("Horizontal_KB2");
                //    v = Input.GetAxis("Vertical_KB2");
                //}
                inputs.A = Input.GetButton("A_P2");
                inputs.B = Input.GetButton("B_P2");
                inputs.C = Input.GetButton("C_P2");
                inputs.Start = Input.GetButtonDown("Start_P2");
            }
            if (h > 0.1)
            {
                if (v > 0.1)
                {
                    if (_p1)
                        direction = 9;
                    else
                        direction = 7;

                }
                else if (v < -0.1)
                {
                    if (_p1)
                        direction = 3;
                    else
                        direction = 1;
                }
                else
                {
                    if (_p1)
                        direction = 6;
                    else
                        direction = 4;
                }
            }
            else if (h < -0.1)
            {
                if (v > 0.1)
                {
                    if (_p1)
                        direction = 7;
                    else
                        direction = 9;

                }
                else if (v < -0.1)
                {
                    if (_p1)
                        direction = 1;
                    else
                        direction = 3;
                }
                else
                {
                    if (_p1)
                        direction = 4;
                    else
                        direction = 6;
                }
            }
            else
            {
                if (v > 0.1)
                {
                    if (_p1)
                        direction = 8;
                    else
                        direction = 8;

                }
                else if (v < -0.1)
                {
                    if (_p1)
                        direction = 2;
                    else
                        direction = 2;
                }
                else
                {
                    direction = 5;
                }
            }
            inputs.JoystickDirection = direction;
            return inputs;
        }

        GameState UpdateGameStateWithInputs(SinglePlayerInputs _p1Inputs, SinglePlayerInputs _p2Inputs, GameState _previousState)
        {
            GameState updatedState = new GameState(_previousState, _previousState.FrameNumber + 1);

            //Check if current state can be affected by inputs (idle, crouch, walking into buttons, as well as throw tech)

            //1. p1
            updatedState.P1_CState.UpdateStateWithInputs(_p1Inputs, m_p1LastInputs, updatedState.P2_CState);

            //2. p2
            updatedState.P2_CState.UpdateStateWithInputs(_p2Inputs, m_p2LastInputs, updatedState.P1_CState);


            return updatedState;
        }



        private MatchOutcome ResolveActions(SinglePlayerInputs _p1Inputs, SinglePlayerInputs _p2Inputs, GameState _currentState)
        {
            //1. Update individual actions and positions
            MatchOutcome p1_update_outcome = _currentState.P1_CState.UpdateCharacterState();
            if (p1_update_outcome.IsEnd())
                return p1_update_outcome;
            MatchOutcome p2_update_outcome = _currentState.P2_CState.UpdateCharacterState();
            if (p2_update_outcome.IsEnd())
                return p2_update_outcome;

            //2. Resolve hitbox interaction
            MatchOutcome hitbox_outcome = ResolveHitboxInteractions(_p1Inputs, _p2Inputs, _currentState);
            if (hitbox_outcome.IsEnd())
                return hitbox_outcome;

            //3. Resolve Positions
            ResolvePositions(_currentState);

            //4. Check timer
            if (_currentState.RemainingTime < 0)
                return new MatchOutcome(true, true, GameplayEnums.Outcome.TimeOut);

            return new MatchOutcome();
        }

        private void ResolvePositions(GameState _currentState)
        {
            Hitbox_Gameplay p1 = _currentState.P1_Hitboxes.Find(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main);
            Hitbox_Gameplay p2 = _currentState.P2_Hitboxes.Find(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main);
            //1. if characters are overlapping, push each other back enough that they don't overlap
            if (DoHitboxesOverlap(p1, p2, _currentState))
            {
                int overlapAmount = (_currentState.P1_Position + p1.Position + p1.Width / 2) - (_currentState.P2_Position - p2.Position - p2.Width / 2);
                if (overlapAmount > 0)
                {
                    int pushback = overlapAmount / 2;
                    _currentState.P1_Position -= pushback;
                    _currentState.P2_Position += pushback;
                }
            }

            //2. if either character extends beyond the edge, push him out of the corner.
            //2.1 if this causes characters to overlap, push the other character back
            if (_currentState.P1_Position < GameplayConstants.ARENA_RADIUS * -1)
            {
                _currentState.P1_Position -= GameplayConstants.ARENA_RADIUS + _currentState.P1_Position;
                if (DoHitboxesOverlap(p1, p2, _currentState))
                {
                    int overlapAmount = (_currentState.P1_Position + p1.Position + p1.Width / 2) - (_currentState.P2_Position + p2.Position + p2.Width / 2);
                    if (overlapAmount > 0)
                    {
                        _currentState.P2_Position += overlapAmount;
                    }
                }
            }
            if (_currentState.P2_Position > GameplayConstants.ARENA_RADIUS)
            {
                _currentState.P2_Position -= _currentState.P2_Position - GameplayConstants.ARENA_RADIUS;
                if (DoHitboxesOverlap(p1, p2, _currentState))
                {
                    int overlapAmount = (_currentState.P1_Position + p1.Position + p1.Width / 2) - (_currentState.P2_Position + p2.Position + p2.Width / 2);
                    if (overlapAmount > 0)
                    {
                        _currentState.P1_Position -= overlapAmount;
                    }
                }
            }

        }

        private MatchOutcome ResolveHitboxInteractions(SinglePlayerInputs _p1Inputs, SinglePlayerInputs _p2Inputs, GameState _currentState)
        {
            bool p1_hits_p2 = false;
            bool p2_hits_p1 = false;
            bool clash = false;
            bool throwBreak = false;
            bool p1_throws_p2 = false;
            bool p2_throws_p1 = false;
            //check p1 attacks first
            foreach (Hitbox_Gameplay hg in _currentState.P1_Hitboxes)
            {
                if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack || hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                {
                    if (hg.HasStruck)
                        continue;
                    foreach (Hitbox_Gameplay p2_hg in _currentState.P2_Hitboxes)
                    {
                        if (p2_hg.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb || p2_hg.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main)
                        {
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack)
                                if (DoHitboxesOverlap(hg, p2_hg, _currentState))
                                {
                                    p1_hits_p2 = true;
                                    hg.HasStruck = true;
                                }
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                                if (DoHitboxesOverlap(hg, p2_hg, _currentState) && p2_hg.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main)
                                {
                                    if (_currentState.P2_State == GameplayEnums.CharacterState.ThrowStartup)
                                        throwBreak = true;
                                    else
                                        p1_throws_p2 = true;
                                    hg.HasStruck = true;
                                }
                        }
                        else if (p2_hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack)
                        {
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack)
                                if (DoHitboxesOverlap(hg, p2_hg, _currentState))
                                {
                                    clash = true;
                                    hg.HasStruck = true;
                                }
                        }
                        else if (p2_hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                        {
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                                if (DoHitboxesOverlap(hg, p2_hg, _currentState))
                                {
                                    throwBreak = true;
                                    hg.HasStruck = true;
                                }
                        }
                    }
                }
            }

            //check p2
            foreach (Hitbox_Gameplay hg in _currentState.P2_Hitboxes)
            {
                if (hg.HasStruck)
                    continue;
                if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack || hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                {
                    foreach (Hitbox_Gameplay p1_hg in _currentState.P1_Hitboxes)
                    {
                        if (p1_hg.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb || p1_hg.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main)
                        {
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack)
                                if (DoHitboxesOverlap(p1_hg, hg, _currentState))
                                {
                                    p2_hits_p1 = true;
                                    hg.HasStruck = true;
                                }
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                                if (DoHitboxesOverlap(p1_hg, hg, _currentState) && p1_hg.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main)
                                {
                                    if (_currentState.P1_State == GameplayEnums.CharacterState.ThrowStartup)
                                        throwBreak = true;
                                    else
                                        p2_throws_p1 = true;
                                    hg.HasStruck = true;
                                }
                        }
                        else if (p1_hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack)
                        {
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack)
                                if (DoHitboxesOverlap(p1_hg, hg, _currentState))
                                {
                                    clash = true;
                                    hg.HasStruck = true;
                                }
                        }
                        else if (p1_hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                        {
                            if (hg.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw)
                                if (DoHitboxesOverlap(p1_hg, hg, _currentState))
                                {
                                    throwBreak = true;
                                    hg.HasStruck = true;
                                }
                        }
                    }
                }
            }
            MatchOutcome res = new MatchOutcome();
            //handle results :
            bool p1_is_hit = false;
            bool p2_is_hit = false;

            if (p1_hits_p2)
            {
                SetAttackAsConnectedWithOpponent(true, _currentState);
                if (DoesPlayerArmor(false, _currentState))
                {
                    //set hitstop
                    _currentState.RemainingHitstop = GameplayConstants.BLOCK_HITSTOP;
                    //play sfx
                    GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Block);
                }
                else if (DoesPlayerBlock(false, _currentState, _p2Inputs))
                {
                    //set p2 to be in blockstun
                    _currentState.P2_State = GameplayEnums.CharacterState.Blockstun;
                    _currentState.P2_StateFrames = 0;
                    //set hitstop
                    _currentState.RemainingHitstop = GameplayConstants.BLOCK_HITSTOP;
                    //give gauge
                    _currentState.P1_CState.AddGauge(1);

                    //play sfx
                    GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Block);
                }
                else
                {
                    p2_is_hit = true;
                    
                    //play sfx
                    GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Hit);
                }
            }
            if (p2_hits_p1)
            {
                SetAttackAsConnectedWithOpponent(false, _currentState);
                if (DoesPlayerArmor(true, _currentState))
                {
                    //set hitstop
                    _currentState.RemainingHitstop = GameplayConstants.BLOCK_HITSTOP;
                    //play sfx
                    GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Block);
                }
                else if (DoesPlayerBlock(true, _currentState, _p1Inputs))
                {
                    //set p2 to be in blockstun
                    _currentState.P1_State = GameplayEnums.CharacterState.Blockstun;
                    _currentState.P1_StateFrames = 0;
                    //set hitstop
                    _currentState.RemainingHitstop = GameplayConstants.BLOCK_HITSTOP;
                    //give gauge
                    _currentState.P2_CState.AddGauge(1);

                    //play sfx
                    GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Block);
                }
                else
                {
                    p1_is_hit = true;

                    //play sfx
                    GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Hit);
                }
            }

            if (p1_is_hit && p2_is_hit)
                res = new MatchOutcome(true, true, GameplayEnums.Outcome.Trade);
            else if (p1_is_hit)
            {
                GameplayEnums.Outcome outcome = GetOutcomeFromOpponentState(_currentState.P1_CState);
                outcome = GetOutcomeFromPlayerState(_currentState.P2_CState, outcome);
                res = new MatchOutcome(false, true, outcome);
            }
            else if (p2_is_hit)
            {
                GameplayEnums.Outcome outcome = GetOutcomeFromOpponentState(_currentState.P2_CState);
                outcome = GetOutcomeFromPlayerState(_currentState.P1_CState, outcome);
                res = new MatchOutcome(true, false, outcome);
            }
            else if ((p2_throws_p1 && p1_throws_p2) || throwBreak)
            {
                _currentState.P2_State = GameplayEnums.CharacterState.ThrowBreak;
                _currentState.P2_StateFrames = 0;
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
                _currentState.P1_State = GameplayEnums.CharacterState.ThrowBreak;
                _currentState.P1_StateFrames = 0;
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);

                //play sfx
                GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.ThrowBreak);
            }
            else if (p1_throws_p2)
            {
                _currentState.P2_State = GameplayEnums.CharacterState.BeingThrown;
                _currentState.P2_StateFrames = 0;
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
                _currentState.P1_State = GameplayEnums.CharacterState.ThrowingOpponent;
                _currentState.P1_StateFrames = 0;
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
            }
            else if (p2_throws_p1)
            {
                _currentState.P2_State = GameplayEnums.CharacterState.ThrowingOpponent;
                _currentState.P2_StateFrames = 0;
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
                _currentState.P1_State = GameplayEnums.CharacterState.BeingThrown;
                _currentState.P1_StateFrames = 0;
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
            }
            else if (clash)
            {
                _currentState.P2_State = GameplayEnums.CharacterState.Clash;
                _currentState.P2_StateFrames = 0;
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P2_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack);
                _currentState.P1_State = GameplayEnums.CharacterState.Clash;
                _currentState.P1_StateFrames = 0;
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                _currentState.P1_Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack);

                //play sfx
                GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Clash);
            }

            return res;
        }

        private void SetAttackAsConnectedWithOpponent(bool _p1, GameState _currentState)
        {
            CharacterState cstate = _p1 ? _currentState.P1_CState : _currentState.P2_CState;
            cstate.AttackConnected = true;
        }

        private bool DoesPlayerArmor(bool _p1, GameState _currentState)
        {
            CharacterState cstate = _p1 ? _currentState.P1_CState : _currentState.P2_CState;
            return cstate.HasArmor();
        }

        private bool CanPlayerBreakThrow(bool _p1, GameState _currentState)
        {
            CharacterState cstate = _p1 ? _currentState.P1_CState : _currentState.P2_CState;
            return cstate.CanBreakThrow();
        }

        private bool DoesPlayerBlock(bool _p1, GameState _currentState, SinglePlayerInputs _inputs)
        {
            if (!(_inputs.JoystickDirection == 1 || _inputs.JoystickDirection == 4 || _inputs.JoystickDirection == 7))
                return false;

            GameplayEnums.CharacterState charState;

            Hitbox_Gameplay hitbox = _currentState.P1_CState.Hitboxes.Find(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack);
            if (_p1)
            {
                charState = _currentState.P1_State;
                hitbox = _currentState.P2_CState.Hitboxes.Find(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack);
            }
            else
            {
                charState = _currentState.P2_State;
                hitbox = _currentState.P1_CState.Hitboxes.Find(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack);
            }
            if (charState == GameplayEnums.CharacterState.Crouch || charState == GameplayEnums.CharacterState.Idle || charState == GameplayEnums.CharacterState.WalkBack)
            {
                switch (hitbox.AttackAttribute)
                {
                    case GameplayEnums.AttackAttribute.High:
                        if ((_inputs.JoystickDirection == 1 || _inputs.JoystickDirection == 2 || _inputs.JoystickDirection == 3))
                            return false;
                        break;
                    case GameplayEnums.AttackAttribute.Low:
                        if (!(_inputs.JoystickDirection == 1 || _inputs.JoystickDirection == 2 || _inputs.JoystickDirection == 3))
                            return false;
                        break;
                }
                return true;
            }
            return false;
        }

        private GameplayEnums.Outcome GetOutcomeFromOpponentState(CharacterState _charState)
        {
            GameplayEnums.Outcome outcome;
            switch (_charState.State)
            {
                case GameplayEnums.CharacterState.AttackActive:
                case GameplayEnums.CharacterState.AttackStartup:
                case GameplayEnums.CharacterState.ThrowActive:
                case GameplayEnums.CharacterState.ThrowStartup:
                    outcome = GameplayEnums.Outcome.Counter;
                    break;
                case GameplayEnums.CharacterState.AttackRecovery:
                    outcome = GameplayEnums.Outcome.WhiffPunish;
                    break;
                case GameplayEnums.CharacterState.ThrowRecovery:
                    outcome = GameplayEnums.Outcome.Shimmy;
                    break;
                case GameplayEnums.CharacterState.Special:
                    outcome = _charState.SelectedCharacter.GetOutcomeIfHit();
                    break;
                default:
                    outcome = GameplayEnums.Outcome.StrayHit;
                    break;
            }
            if(outcome == GameplayEnums.Outcome.WhiffPunish || outcome == GameplayEnums.Outcome.StrayHit)
            {
                if (_charState.AttackConnected)
                    outcome = GameplayEnums.Outcome.Punish;
            }
            return outcome;
        }

        private GameplayEnums.Outcome GetOutcomeFromPlayerState(CharacterState _winner, GameplayEnums.Outcome _previousOutcome)
        {
            //if previous outcome is "stray hit", it can be overridden with 
            if (_previousOutcome == GameplayEnums.Outcome.StrayHit)
            {
                if (_winner.State == GameplayEnums.CharacterState.Special)
                {
                    return _winner.SelectedCharacter.GetCurrentCharacterSpecialOutcome();
                }
            }
            return _previousOutcome;
        }

        private bool DoHitboxesOverlap(Hitbox_Gameplay _p1Box, Hitbox_Gameplay _p2Box, GameState _currentState)
        {
            int p1_left = _currentState.P1_Position + _p1Box.Position - _p1Box.Width / 2;
            int p1_right = _currentState.P1_Position + _p1Box.Position + _p1Box.Width / 2;
            int p2_left = _currentState.P2_Position + _p2Box.Position - _p2Box.Width / 2;
            int p2_right = _currentState.P2_Position + _p2Box.Position + _p2Box.Width / 2;

            if (p1_right < p2_left)
                return false;
            if (p2_right < p1_left)
                return false;
            return true;
        }

        #region CameraTests
        public float turnSpeed = 4.0f;      // Speed of camera turning when mouse moves in along an axis
        public float panSpeed = 4.0f;       // Speed of the camera when being panned
        public float zoomSpeed = 4.0f;      // Speed of the camera going back and forth
        private Vector3 mouseOrigin = Vector3.zero;    // Position of cursor when mouse dragging starts
        private bool isPanning = false;     // Is the camera being panned?
        private bool isRotating = false;    // Is the camera being rotated?
        private bool isZooming = false;		// Is the camera zooming?

        private void CameraMoveTest(MatchState _matchState)
        {
            if (_matchState.Outcomes.Count <= 0)
                return;
            MatchOutcome lastOutcome = _matchState.Outcomes[_matchState.Outcomes.Count - 1];
            if (lastOutcome.Outcome != GameplayEnums.Outcome.Shimmy && lastOutcome.Outcome != GameplayEnums.Outcome.WhiffPunish)
                return;
            float rotationSpeed = 0f;
            if (lastOutcome.P1_Scores)
                rotationSpeed += 20f;
            if (lastOutcome.P2_Scores)
                rotationSpeed -= 20f;
            Camera mc = Camera.main;
            // Rotate camera along X and Y axis
            if (isRotating)
            {
                mc.transform.RotateAround(Vector3.zero, Vector3.up, rotationSpeed * Time.deltaTime);
                Camera.main.fieldOfView -= 0.01f;

                //Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                //mc.transform.RotateAround(mc.transform.position, mc.transform.right, -pos.y * turnSpeed);
                //mc.transform.RotateAround(mc.transform.position, Vector3.up, pos.x * turnSpeed);
            }

            // Move the camera on it's XY plane
            if (isPanning)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
                mc.transform.Translate(move, Space.Self);
            }

            // Move the camera linearly along Z axis
            if (isZooming)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                Vector3 move = pos.y * zoomSpeed * mc.transform.forward;
                mc.transform.Translate(move, Space.World);
            }
        }

        private void ResetCamera()
        {
            Camera.main.transform.position = new Vector3(0, 0, -100);
            Camera.main.transform.rotation = new Quaternion(0, 0, 0, 0);
            Camera.main.fieldOfView = 5.8f;
            isRotating = false;
        }
        #endregion  
    }

}



