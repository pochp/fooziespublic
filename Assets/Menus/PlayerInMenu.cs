using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Menus
{
    //represents one player's selection in a menu
    public class PlayerInMenu
    {
        private const int MoveCooldownMax = 10;
        public enum SelectionStates { Pending, Confirmed, Cancel }
        public int MoveCooldown { get; set; }
        public MenuItem SelectedItem { get; set; }
        public SelectionStates SelectionState;

        public PlayerInMenu(MenuItem _defaultItem)
        {
            MoveCooldown = 0;
            SelectedItem = _defaultItem;
            SelectionState = SelectionStates.Pending;
        }
    }
}
