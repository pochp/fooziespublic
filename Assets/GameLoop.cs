using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FooziesConstants;


/// <summary>
/// TO DO :
/// ok-keep working on finishing up hitbox detection
/// ok-make sure throw breaks remove hurtboxes
/// -renderer
/// -scoring
/// -main menu, declare winner
/// -gauge, special
/// </summary>

public class GameLoop : MonoBehaviour {

    public GameplayRenderer GameplayRendererObject;
    public MenuRenderer MainMenuRenderer;
    public MenuRenderer CharacterSelectRenderer;

    public bool P1_Joystick = true;
    public bool P2_Joystick = false;

    float deltaTime = 0.0f;//for fps display
    float m_timeSinceLastUpdate = 0.0f;

    private void Awake()
    {
        deltaTime = 0;
        ApplicationStateManager.GetInstance().InitManager(CreateRenderers());
        ApplicationStateManager.GetInstance().SetMainMenu();
    }

    // Use this for initialization
    void Start() {
    }

    private Renderers CreateRenderers()
    {
        Renderers renderers = new Renderers();
        renderers.GameplayR = GameplayRendererObject;
        renderers.CharacterSelectR = CharacterSelectRenderer;
        renderers.MainMenuR = MainMenuRenderer;
        return renderers;
    }

    // Update is called once per frame
    void Update() {
        deltaTime = (Time.deltaTime);// - deltaTime);// * 0.1f;

        m_timeSinceLastUpdate += deltaTime;
        if (m_timeSinceLastUpdate > GameplayConstants.FRAME_LENGTH)
            m_timeSinceLastUpdate -= GameplayConstants.FRAME_LENGTH;
        else
            return;


        //1. Checks for inputs : done before
        ApplicationStateManager.GetInstance().UpdateCurrentState(GetAllInputs());

    }

    private Inputs GetAllInputs()
    {
        SinglePlayerInputs p1_inputs = GetInputs(true);
        SinglePlayerInputs p2_inputs = GetInputs(false);
        CommonInputs commonInputs = GetCommonInputs();
        return new Inputs(p1_inputs, p2_inputs, commonInputs);
    }

    private CommonInputs GetCommonInputs()
    {
        CommonInputs ci = new CommonInputs();
        ci.F4 = Input.GetKeyDown(KeyCode.F4);
        ci.F3 = Input.GetKeyDown(KeyCode.F3);
        ci.F2 = Input.GetKeyDown(KeyCode.F2);
        ci.F5 = Input.GetKeyDown(KeyCode.F5);
        ci.F6 = Input.GetKeyDown(KeyCode.F6);
        ci.F12 = Input.GetKeyDown(KeyCode.F12);
        return ci;
    }

    private SinglePlayerInputs GetInputs(bool _p1)
    {
        return Assets.Input.InputSources.InputSourceManager.GetInputs(_p1);
        //rewired hidden by inputsourcemanager now to allow AI and netplay
        //return InputReaderRewired.GetInputs(_p1);
        //not rewired version, default unity input binding
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

    void OnGUI()
    {

        GUIStyle style = new GUIStyle();
        style.fontSize = 26;
        string currentGameStatus = ApplicationStateManager.GetCurrentStateName();

        int halfScreenWidth = Screen.width / 2;
        int quarterScreenWidth = Screen.width / 4;


        GUI.TextArea(new Rect(halfScreenWidth - quarterScreenWidth, 10, halfScreenWidth + quarterScreenWidth, Screen.height / 2), currentGameStatus, style);
    }



}



