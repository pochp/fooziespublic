using Assets.Input.InputSources.AI;
using Assets.Utility.Netplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Menus
{
    //base class for any menu in the game (ex: main menu, character select)
    abstract public class Menu : ApplicationState
    {
        public enum MenuResult { Continue, Back, Remain}
        public List<MenuItem> Items;
        public PlayerInMenu P1;
        public PlayerInMenu P2;
        private SinglePlayerInputs m_lastInputsP1;
        private SinglePlayerInputs m_lastInputsP2;

        private MenuRenderer m_renderer;

        protected Menu(List<MenuItem> _items, IRenderer _renderer) : base()
        {
            Items = _items;
            P1 = new PlayerInMenu(_items.First());
            P2 = new PlayerInMenu(_items.First());
            m_lastInputsP1 = null;
            m_lastInputsP2 = null;
            StateRenderer = _renderer;
        }

        public override void Update(Inputs _inputs)
        {
            if (_inputs.Common_Inputs.F3)
            {
                Input.InputSources.InputSourceManager.GetInstance().P2_InputSource = new Input.AiPlayer(false, typeof(BasicDelayedAi));
            }
            if (_inputs.Common_Inputs.F2)
            {
                Input.InputSources.InputSourceManager.GetInstance().P1_InputSource = new Input.AiPlayer(true, typeof(BasicAi));
                Input.InputSources.InputSourceManager.GetInstance().P2_InputSource = new Input.AiPlayer(false, typeof(BasicDelayedAi));
            }
            if (_inputs.Common_Inputs.F5)
            {
                Input.InputSources.InputSourceManager.GetInstance().P2_InputSource = new Input.RemotePlayer();
                GameManager.Instance.IsOnlineMatch = true;
                GameManager.Instance.NetplayState.IsHost = true;
                Communication.InitializeCommuncationSettings(true);
            }
            if (_inputs.Common_Inputs.F6)
            {
                Input.InputSources.InputSourceManager.GetInstance().P2_InputSource = new Input.RemotePlayer();
                GameManager.Instance.IsOnlineMatch = true;
                GameManager.Instance.NetplayState.IsHost = false;
                Communication.InitializeCommuncationSettings(false);
            }
            if (_inputs.Common_Inputs.F12)
            {
                GameManager.Instance.CreateConfigFile();
            }
#if REWIRED
            if (_inputs.Common_Inputs.F4)
            {
                InputMappingInCourse = true;
                RewiredJoystickAssigner.UnbindPlayerIds();
            }
#endif
            MenuResult res;
#if REWIRED
            if (InputMappingInCourse)
            {
                if(RewiredJoystickAssigner.AssignPlayerIds())
                {
                    InputMappingInCourse = false;
                    System.Threading.Thread.Sleep(300);
                }
                res = MenuResult.Remain;

            }
            else
#endif
            {
                res = UpdateMenu(_inputs.P1_Inputs, _inputs.P2_Inputs);
            }
            HandleMenuResult(res);
        }

        protected abstract void HandleMenuResult(MenuResult _result);

        public MenuResult UpdateMenu(SinglePlayerInputs _p1Inputs, SinglePlayerInputs _p2Inputs)
        {
            //for the first time, inputs will be null, so we can copy the first inputs. this makes it so when entering the menu you don't auto select stuff
            if (m_lastInputsP1 == null)
                m_lastInputsP1 = _p1Inputs;
            if (m_lastInputsP2 == null)
                m_lastInputsP2 = _p2Inputs;

            //update character selection
            UpdatePlayerSelection(_p1Inputs, P1, m_lastInputsP1);
            UpdatePlayerSelection(_p2Inputs, P2, m_lastInputsP2);
            m_lastInputsP1 = _p1Inputs;
            m_lastInputsP2 = _p2Inputs;

            //check menu state
            return EvaluateMenuResult();
        }

        protected virtual MenuResult EvaluateMenuResult()
        {
            var gamemanager = GameManager.Instance;
            if (gamemanager.IsOnlineMatch)
            {
                //assume that if p1 has selected a character it means he's ready.
                //
                // p1 has selected a character, and recieved that opponent is ready : PROCEED
                // p1 has selected a character, but opponent has not : remain, and send out signal saying that you are ready
                // p1 has not selected a character : remain, and send out signal saying that you are NOT ready
                //

                if(P1.SelectionState == PlayerInMenu.SelectionStates.Confirmed)// && gamemanager.NetplayState.HasRecievedReadyFromOpponent)
                {
                    return MenuResult.Continue;
                }
                if (P1.SelectionState == PlayerInMenu.SelectionStates.Cancel ||
                    P2.SelectionState == PlayerInMenu.SelectionStates.Cancel)
                    return MenuResult.Back;

            }
            else
            {
                if (P1.SelectionState == PlayerInMenu.SelectionStates.Confirmed &&
                P2.SelectionState == PlayerInMenu.SelectionStates.Confirmed)
                    return MenuResult.Continue;
                if (P1.SelectionState == PlayerInMenu.SelectionStates.Cancel ||
                    P2.SelectionState == PlayerInMenu.SelectionStates.Cancel)
                    return MenuResult.Back;
            }
            return MenuResult.Remain;
        }

        protected void UpdatePlayerSelection(SinglePlayerInputs _inputs, PlayerInMenu _player, SinglePlayerInputs _lastInputs)
        {
            if (_inputs.A && !_lastInputs.A)
                _player.SelectionState = PlayerInMenu.SelectionStates.Confirmed;
            if (_inputs.B && !_lastInputs.B)
            {
                if (_player.SelectionState == PlayerInMenu.SelectionStates.Confirmed)
                    _player.SelectionState = PlayerInMenu.SelectionStates.Pending;
                else if (_player.SelectionState == PlayerInMenu.SelectionStates.Pending)
                    _player.SelectionState = PlayerInMenu.SelectionStates.Cancel;
            }

            if (_player.SelectionState != PlayerInMenu.SelectionStates.Pending)
                return;

            if(_inputs.JoystickDirection == 5 || _inputs.JoystickDirection != _lastInputs.JoystickDirection)
            {
                _player.MoveCooldown = 0;
            }
            else
            {
                _player.MoveCooldown--;
            }
            if(_player.MoveCooldown <= 0 && _inputs.JoystickDirection!=5)
            {
                MenuItem nextSelection = FindNextItemInDirection(_inputs.JoystickDirection, _player.SelectedItem);
                if(nextSelection != null)
                {
                    _player.SelectedItem = nextSelection;
                    _player.MoveCooldown = 10;
                }
            }
        }

        protected MenuItem FindNextItemInDirection(int _joystickDirection, MenuItem _currentSelection)
        {
            List<MenuItemDistance> items = GetItemDistances(_joystickDirection, _currentSelection).OrderBy(o=> o.Distance).ToList();
            try { return items.First().Item; }
            catch(Exception e) { return null; }
        }

        protected IEnumerable<MenuItemDistance> GetItemDistances(int _joystickDirection, MenuItem _currentSelection)
        {
            foreach(MenuItem mi in Items)
            {
                //if distance < 0 we assume there is no intersection
                //if direction is diagonal but intersection is cardinal, we add 2 so it's bigger than an equivalent diagonal
                //same for vice versa
                int distance = -1;
                switch(_joystickDirection)
                {
                    case 1:
                        if (mi.PositionX < _currentSelection.PositionX && mi.PositionY < _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionX < _currentSelection.PositionX || mi.PositionY < _currentSelection.PositionY)
                            distance = 2;
                        break;
                    case 2:
                        if (mi.PositionX == _currentSelection.PositionX && mi.PositionY < _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionY < _currentSelection.PositionY)
                            distance = 2;
                        break;
                    case 3:
                        if (mi.PositionX > _currentSelection.PositionX && mi.PositionY < _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionX > _currentSelection.PositionX || mi.PositionY < _currentSelection.PositionY)
                            distance = 2;
                        break;
                    case 4:
                        if (mi.PositionX < _currentSelection.PositionX && mi.PositionY == _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionX < _currentSelection.PositionX)
                            distance = 2;
                        break;
                    case 6:
                        if (mi.PositionX > _currentSelection.PositionX && mi.PositionY == _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionX > _currentSelection.PositionX)
                            distance = 2;
                        break;
                    case 7:
                        if (mi.PositionX < _currentSelection.PositionX && mi.PositionY > _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionX < _currentSelection.PositionX || mi.PositionY > _currentSelection.PositionY)
                            distance = 2;
                        break;
                    case 8:
                        if (mi.PositionX == _currentSelection.PositionX && mi.PositionY > _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionY > _currentSelection.PositionY)
                            distance = 2;
                        break;
                    case 9:
                        if (mi.PositionX > _currentSelection.PositionX && mi.PositionY > _currentSelection.PositionY)
                            distance = 0;
                        else if (mi.PositionX > _currentSelection.PositionX || mi.PositionY > _currentSelection.PositionY)
                            distance = 2;
                        break;
                }
                if (distance < 0)
                    continue;
                distance += Math.Abs(mi.PositionX - _currentSelection.PositionX);
                distance += Math.Abs(mi.PositionY - _currentSelection.PositionY);
                yield return new MenuItemDistance(mi, distance);
            }
        }

        protected class MenuItemDistance
        {
            public MenuItem Item;
            public float Distance;
            public MenuItemDistance(MenuItem _item, float _distance)
            {
                Item = _item;
                Distance = _distance;
            }
        }

        public override void Rollback(List<Inputs> _inputsHistory)
        {
            //menus don't get to rollback
        }
    }
}
