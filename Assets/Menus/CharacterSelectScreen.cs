using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Match;

namespace Assets.Menus
{
    public class CharacterSelectScreen : Menu
    {
        public const string STR_SWEEP = "Sweep";
        public const string STR_DASH = "Dash";
        public const string STR_ARMOR = "Armor";
        private SetData m_currentSetData;


        public static CharacterSelectScreen GetCharacterSelectScreen(SetData _currentSetData, IRenderer _renderer)
        {
            List<MenuItem> items = new List<MenuItem>();
            items.Add(new MenuItem(STR_SWEEP, 0, 0));
            items.Add(new MenuItem(STR_DASH, 1, 0));
            items.Add(new MenuItem(STR_ARMOR, 2, 0));
            return new CharacterSelectScreen(items, _currentSetData, _renderer);
        }

        protected override void HandleMenuResult(MenuResult _result)
        {
            if (_result == MenuResult.Continue)
            {
                MatchInitializationData matchInitData = new MatchInitializationData();
                matchInitData.P1_Character = DetermineSelectedCharacter(P1);
                matchInitData.P2_Character = DetermineSelectedCharacter(P2);
                m_currentSetData.SetMatchInitData(matchInitData);

                ApplicationStateManager.GetInstance().SetGameplayState(m_currentSetData);
            }
            else if (_result == MenuResult.Back)
            {
                ApplicationStateManager.GetInstance().SetMainMenu();//go to main menu
            }
        }

        private CharacterSelectScreen(List<MenuItem> _items, SetData _currentSetData, IRenderer _renderer) : base(_items, _renderer)
        {
            m_currentSetData = _currentSetData;
            P1.SelectedItem = Items.First(o => o.ItemName == GetMatchingCharacterName(m_currentSetData.P1_SelectedCharacter));
            P2.SelectedItem = Items.First(o => o.ItemName == GetMatchingCharacterName(m_currentSetData.P2_SelectedCharacter));
        }

        public static string GetMatchingCharacterName(Character _char)
        {
            if (_char is Sweep)
                return STR_SWEEP;
            if (_char is Dash)
                return STR_DASH;
            if (_char is Armor)
                return STR_ARMOR;
            return string.Empty;
        }

        public static Character GetCharacterFromString(string characterName)
        {
            switch (characterName)
            {
                case STR_DASH:
                    return new Dash();
                case STR_SWEEP:
                    return new Sweep();
                case STR_ARMOR:
                    return new Armor();
            }
            throw new Exception("No Character for string : " + characterName);
        }

        private Character DetermineSelectedCharacter(PlayerInMenu _player)
        {
            return GetCharacterFromString(_player.SelectedItem.ItemName);
            throw new Exception("No Character Selected");
        }

        public override string GetDebugInfo()
        {
            string info = base.GetDebugInfo() + Environment.NewLine;//"Character Select Screen";
            info += Environment.NewLine + "P1 Selection : " + P1.SelectedItem.ItemName;
            if (P1.SelectionState == PlayerInMenu.SelectionStates.Confirmed)
                info += " <>";
            info += Environment.NewLine + "P2 Selection : " + P2.SelectedItem.ItemName;
            if (P2.SelectionState == PlayerInMenu.SelectionStates.Confirmed)
                info += " <>";
            info += Environment.NewLine + "P1 Score : " + m_currentSetData.P1_Score.ToString() + Environment.NewLine;
            info += "P2 Score : " + m_currentSetData.P2_Score.ToString() + Environment.NewLine;

            //info += "P1 timer : " + P1.MoveCooldown.ToString() + ", State : " + P1.SelectionState.ToString();

            return info;
        }
    }
}
