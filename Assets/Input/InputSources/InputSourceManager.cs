using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Input.InputSources
{
    class InputSourceManager
    {
        private static InputSourceManager m_instance;

        static public InputSourceManager GetInstance()
        {
            if (m_instance == null)
                m_instance = new InputSourceManager();
            return m_instance;
        }

        private InputSourceManager()
        {
            P1_InputSource = new LocalPlayer(true);
            P2_InputSource = new LocalPlayer(false);
        }

        public IInputSource P1_InputSource { get; set; }
        public IInputSource P2_InputSource { get; set; }

        public static SinglePlayerInputs GetInputs(bool _p1)
        {
            if (_p1)
                return GetInstance().P1_InputSource.GetInputs();
            else
                return GetInstance().P2_InputSource.GetInputs();
        }
    }
}
