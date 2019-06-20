using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Menus
{
    //Represents an item in a menu such as the main menu, or character select
    public class MenuItem
    {
        public string ItemName
        {
            get
            {
                return m_itemName;
            }
        }
        private string m_itemName;

        public MenuItem(string _name, int _posX, int _posY)
        {
            m_itemName = _name;
            PositionX = _posX;
            PositionY = _posY;
        }

        public int PositionX;
        public int PositionY;

       
    }
}
