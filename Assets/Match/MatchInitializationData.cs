using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Match
{
    /// <summary>
    /// Stores data used to begin a match, including character selection data, perhaps keybindings, and maybe eventually AI (in a child object)
    /// </summary>
    public class MatchInitializationData
    {
        public Character P1_Character;
        public Character P2_Character;

        public MatchInitializationData()
        {
            P1_Character = new Sweep();
            P2_Character = new Dash();
        }

        public void InitCharacters()
        {
            P1_Character.Initialize();
            P2_Character.Initialize();
        }
    }
}
