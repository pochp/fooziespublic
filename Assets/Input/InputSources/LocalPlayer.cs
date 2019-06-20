using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Input
{
    class LocalPlayer : IInputSource
    {
        private bool _p1;

        public LocalPlayer(bool p1)
        {
            _p1 = p1;
        }


        public SinglePlayerInputs GetInputs()
        {
#if REWIRED
            return InputReaderRewired.GetInputs(_p1);
#endif
            short direction = 5;
            SinglePlayerInputs inputs = new SinglePlayerInputs();
            float h;
            float v;

            if (_p1)
            {
                //if(P1_Joystick)
                {
                    h = UnityEngine.Input.GetAxisRaw("Horizontal_PsStick1") + UnityEngine.Input.GetAxisRaw("Horizontal_KB1"); ;
                    v = UnityEngine.Input.GetAxisRaw("Vertical_PsStick1") + UnityEngine.Input.GetAxisRaw("Vertical_KB1");
                }
                //else
                //{
                //    h = UnityEngine.Input.GetAxis("Horizontal_KB1");
                //    v = UnityEngine.Input.GetAxis("Vertical_KB1");
                //}
                inputs.A = UnityEngine.Input.GetButton("A_P1");
                inputs.B = UnityEngine.Input.GetButton("B_P1");
                inputs.C = UnityEngine.Input.GetButton("C_P1");
                inputs.Start = UnityEngine.Input.GetButtonDown("Start_P1");
            }
            else
            {
                //if (P2_Joystick)
                {
                    h = UnityEngine.Input.GetAxisRaw("Horizontal_PsStick2") + UnityEngine.Input.GetAxisRaw("Horizontal_KB2");
                    v = UnityEngine.Input.GetAxisRaw("Vertical_PsStick2") + UnityEngine.Input.GetAxisRaw("Vertical_KB2");
                }
                //else
                //{
                //    h = UnityEngine.Input.GetAxis("Horizontal_KB2");
                //    v = UnityEngine.Input.GetAxis("Vertical_KB2");
                //}
                inputs.A = UnityEngine.Input.GetButton("A_P2");
                inputs.B = UnityEngine.Input.GetButton("B_P2");
                inputs.C = UnityEngine.Input.GetButton("C_P2");
                inputs.Start = UnityEngine.Input.GetButtonDown("Start_P2");
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

        public bool IsP1()
        {
            throw new NotImplementedException();
        }
    }
}
