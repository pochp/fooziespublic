
#if REWIRED
using UnityEngine;
using System.Collections;
using Rewired;
using System.Linq;

class InputReaderRewired
{
    //public int playerId = 0; // The Rewired player id of this character
    //
    //public float moveSpeed = 3.0f;
    //public float bulletSpeed = 15.0f;
    //public GameObject bulletPrefab;
    //
    //private Player player; // The Rewired Player
    //private CharacterController cc;
    //private Vector3 moveVector;
    //private bool fire;
    //
    //void Awake()
    //{
    //    // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
    //    player = ReInput.players.GetPlayer(playerId);
    //
    //    // Get the character controller
    //    cc = GetComponent<CharacterController>();
    //}
    //
    //void Update()
    //{
    //    GetInput();
    //    ProcessInput();
    //}
    //
    //private void GetInput()
    //{
    //    // Get the input from the Rewired Player. All controllers that the Player owns will contribute, so it doesn't matter
    //    // whether the input is coming from a joystick, the keyboard, mouse, or a custom controller.
    //
    //    moveVector.x = player.GetAxis("Move Horizontal"); // get input by name or action id
    //    moveVector.y = player.GetAxis("Move Vertical");
    //    fire = player.GetButtonDown("Fire");
    //}
    //
    //private void ProcessInput()
    //{
    //    // Process movement
    //    if (moveVector.x != 0.0f || moveVector.y != 0.0f)
    //    {
    //        cc.Move(moveVector * moveSpeed * Time.deltaTime);
    //    }
    //
    //    // Process fire
    //    if (fire)
    //    {
    //        GameObject bullet = (GameObject)Instantiate(bulletPrefab, transform.position + transform.right, transform.rotation);
    //        bullet.rigidbody.AddForce(transform.right * bulletSpeed, ForceMode.VelocityChange);
    //    }
    //}


    static public SinglePlayerInputs GetInputs(bool _p1)
    {
        Log();
        SinglePlayerInputs inputs = new SinglePlayerInputs();
        var playerId = RewiredJoystickAssigner.GetPlayerId(_p1);
        if (playerId < 0)
            return inputs;
        Player player = ReInput.players.GetPlayer(playerId);
        
        float h;
        float v;

        float diag_TL_BR = player.GetAxisRaw("Move_Diagonal_TL_BR");
        float diag_TR_BL = player.GetAxisRaw("Move_Diagonal_TR_BL");

        //adjusting because d-pads are sometimes considered as HAT inputs, where the diagonals require their own axis....... I hate this
        if(Mathf.Abs(diag_TL_BR) > 0.1)
        {
            if(diag_TL_BR < 0)
            {
                h = -1f;
                v = 1f;
            }
            else
            {
                h = 1f;
                v = -1f;
            }
        }
        else if(Mathf.Abs(diag_TR_BL) > 0.1)
        {
            if (diag_TR_BL < 0)
            {
                h = 1f;
                v = 1f;
            }
            else
            {
                h = -1f;
                v = -1f;
            }
        }
        else
        {
            h = player.GetAxisRaw("Move_Horizontal");
            v = player.GetAxisRaw("Move_Vertical");
        }
        inputs.JoystickDirection = GetNumpadDirection(h, v, _p1);


        inputs.A = player.GetButton("Button_A");
        inputs.B = player.GetButton("Button_B");
        inputs.C = player.GetButton("Button_C");
        
        return inputs;
    }

    static private short GetNumpadDirection(float h, float v, bool _p1)
    {
        short direction = 5;
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
        return direction;
    }

    static void Log()
    {
        // Log assigned Joystick information for all joysticks regardless of whether or not they've been assigned
        Debug.Log("Rewired found " + ReInput.controllers.joystickCount + " joysticks attached.");
        for (int i = 0; i < ReInput.controllers.joystickCount; i++)
        {
            Joystick j = ReInput.controllers.Joysticks[i];
            Debug.Log(
                "[" + i + "] Joystick: " + j.name + "\n" +
                "Hardware Name: " + j.hardwareName + "\n" +
                "Is Recognized: " + (j.hardwareTypeGuid != System.Guid.Empty ? "Yes" : "No") + "\n" +
                "Is Assigned: " + (ReInput.controllers.IsControllerAssigned(j.type, j) ? "Yes" : "No")
            );
        }

        // Log assigned Joystick information for each Player
        foreach (var p in ReInput.players.Players)
        {
            Debug.Log("PlayerId = " + p.id + " is assigned " + p.controllers.joystickCount + " joysticks.");

            // Log information for each Joystick assigned to this Player
            foreach (var j in p.controllers.Joysticks)
            {
                Debug.Log(
                  "Joystick: " + j.name + "\n" +
                  "Is Recognized: " + (j.hardwareTypeGuid != System.Guid.Empty ? "Yes" : "No")
                );

                // Log information for each Controller Map for this Joystick
                foreach (var map in p.controllers.maps.GetMaps(j.type, j.id))
                {
                    Debug.Log("Controller Map:\n" +
                        "Category = " +
                        ReInput.mapping.GetMapCategory(map.categoryId).name + "\n" +
                        "Layout = " +
                        ReInput.mapping.GetJoystickLayout(map.layoutId).name + "\n" +
                        "enabled = " + map.enabled
                    );
                    foreach (var aem in map.GetElementMaps())
                    {
                        var action = ReInput.mapping.GetAction(aem.actionId);
                        if (action == null) continue; // invalid Action
                        Debug.Log("Action \"" + action.name + "\" is bound to \"" +
                            aem.elementIdentifierName + "\""
                        );
                    }
                }
            }
        }
    }
}

class RewiredJoystickAssigner
{
    enum AssigningState { P1, P2, AllSet};
    AssigningState assigningState
    {
        get
        {
            if (P1_PlayerId == -1)
                return AssigningState.P1;
            if (P2_PlayerId == -1)
                return AssigningState.P2;
            return AssigningState.AllSet;
        }
    }

    static private RewiredJoystickAssigner assigner;
    private int P1_PlayerId;
    private int P2_PlayerId;
    private RewiredJoystickAssigner()
    {
        P1_PlayerId = 0;
        P2_PlayerId = 1;
    }
    static public RewiredJoystickAssigner GetAssigner()
    {
        if (assigner == null)
            assigner = new RewiredJoystickAssigner();
        return assigner;
    }
    static public int GetPlayerId(bool p1)
    {
        var instance = GetAssigner();
        return p1 ? instance.P1_PlayerId : instance.P2_PlayerId;
    }

    static public void UnbindPlayerIds()
    {
        var assigner = GetAssigner();
        assigner.P1_PlayerId = -1;
        assigner.P2_PlayerId = -2;
    }

    static public bool AssignPlayerIds()
    {
        var assigner = GetAssigner();
        if (assigner.assigningState == AssigningState.AllSet)
            return false;
        int id;
        foreach (var p in ReInput.players.Players)
        {
            if (p.GetAnyButton())
            {
                id = p.id;
                if(assigner.assigningState == AssigningState.P1)
                {
                    assigner.P1_PlayerId = p.id;
                    assigner.P2_PlayerId = -1;
                    return false;
                }
                else if(assigner.assigningState == AssigningState.P2 && assigner.P1_PlayerId != p.id)//so you don't bind the same device twice
                {
                    assigner.P2_PlayerId = p.id;
                    return true;
                }
            }
        }
        return false;
    }

    static public string GetInputName(bool p1)
    {
        var assigner = GetAssigner();
        var playerId = p1 ? assigner.P1_PlayerId : assigner.P2_PlayerId;
        if(playerId == -1)
        {
            return "Press Any Attack Button On the Joystick";
        }
        if(playerId == -2)
        {
            return "Please Wait";
        }
        Player player = ReInput.players.GetPlayer(playerId);
        var stick = player.controllers.Joysticks.FirstOrDefault();
        if(stick == null)
        {
            return "No joystick found for player";
        }
        else
        {
            return stick.name;
        }
    }
}
#endif