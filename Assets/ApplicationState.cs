using Assets.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class ApplicationState
{
    protected List<SingleFrameInputValidation> _inputHistory;

    protected ApplicationState()
    {
        _inputHistory = new List<SingleFrameInputValidation>();
    }

    /// <summary>
    /// adjusts the state of the game to account for delayed inputs from a netplay player
    /// </summary>
    /// <param name="inputsListFromOtherPlayer"></param>
    public virtual void AdjustStateForNetplay(List<SingleFrameInputValidation> inputsListFromOtherPlayer)
    {
        //compare inputs + validate
        var frameToRollbackTo = CompareAndValidateInputs(inputsListFromOtherPlayer);
        if (frameToRollbackTo < 0)
            return;

        //rollback if required
        //todo : if it's slow consider optimizing this section
        var inputsToRollback = _inputHistory.Where(o => o.FrameNumber >= frameToRollbackTo).ToList();
        var inputsThemselves = inputsToRollback.Select(o => new Inputs(o.P1_Inputs, o.P2_Inputs, new CommonInputs())).ToList();
        Rollback(inputsThemselves);
    }

    /// <summary>
    /// compare inputs of matching frame# and then validate that those that have been compared
    /// -1 : no rollback required
    /// -2 : error, mismatched frame numbers when comparing lists
    /// -3 : error, oldestNonValidatedFrame number is -1, meaning unassigned
    /// </summary>
    /// <param name="inputsListFromOtherPlayer"></param>
    /// <returns>number of the frames to roll back to</returns>
    private int CompareAndValidateInputs(List<SingleFrameInputValidation> inputsListFromOtherPlayer)
    {
        //start iterating from latest non-validated input
        int currentPlayerInputIndex = 0; //todo : check if this index should be a static, so it isn't recalculated as much
        for(;currentPlayerInputIndex < _inputHistory.Count; currentPlayerInputIndex++)
        {
            if (!_inputHistory[currentPlayerInputIndex].SyncConfirmed)
                break;
        }

        //just making sure
        if (currentPlayerInputIndex >= _inputHistory.Count)
            currentPlayerInputIndex = _inputHistory.Count - 1;

        var oldestNonValidatedInput = _inputHistory[currentPlayerInputIndex];
        var oldestNonValidatedFrame = oldestNonValidatedInput.FrameNumber;

        //error in frames, should not happen
        if(oldestNonValidatedFrame < 0)
        {
            return -3;
        }

        //compare inputs from 
        int otherPlayerInputIndex = 0;
        for(; otherPlayerInputIndex < inputsListFromOtherPlayer.Count; otherPlayerInputIndex++ )
        {
            if (inputsListFromOtherPlayer[otherPlayerInputIndex].FrameNumber == oldestNonValidatedFrame)
                break;
        }

        bool mismatchFound = false;
        int earliestMismatchedFrame = -1;

        //revise input history
        //todo : consider case where opponent's game state is ahead of current system's game state
        for (; otherPlayerInputIndex < inputsListFromOtherPlayer.Count && currentPlayerInputIndex < _inputHistory.Count;
            otherPlayerInputIndex++, currentPlayerInputIndex++)
        {
            var stateToCompare_local = _inputHistory[currentPlayerInputIndex];
            var stateToCompare_other = inputsListFromOtherPlayer[otherPlayerInputIndex];
            if(stateToCompare_local.FrameNumber == stateToCompare_other.FrameNumber)
            {
                if(!stateToCompare_local.AreOpponentInputsSame(stateToCompare_other))
                {
                    //if inputs dont match, replace the local inputs that belong to the other player
                    if(stateToCompare_other.P1_Source)
                    {
                        stateToCompare_local.P1_Inputs = new SinglePlayerInputs(stateToCompare_other.P1_Inputs);
                    }
                    else
                    {
                        stateToCompare_local.P2_Inputs = new SinglePlayerInputs(stateToCompare_other.P2_Inputs);
                    }
                    if(!mismatchFound)
                    {
                        earliestMismatchedFrame = stateToCompare_local.FrameNumber;
                    }
                }
                stateToCompare_local.SyncConfirmed = true;
            }
            else
            {
                return -2;
            }
        }
        return earliestMismatchedFrame;
    }


    public abstract void Update(Inputs _inputs);
    /// <summary>
    /// Tells state to go back a certain amount of frames (input history count), and then re-evaluate up to current
    /// </summary>
    /// <param name="_inputsHistory">updated input history for the players</param>
    public abstract void Rollback(List<Inputs> _inputsHistory);
    public IRenderer StateRenderer;
    public static bool InputMappingInCourse = false;

    public virtual string GetDebugInfo()
    {
        var output = "current state : " + this.GetType().Name + Environment.NewLine;
#if REWIRED
        if (InputMappingInCourse)
        {
            output += "P1 Joystick : " + RewiredJoystickAssigner.GetInputName(true) + Environment.NewLine;
            output += "P2 Joystick : " + RewiredJoystickAssigner.GetInputName(false) + Environment.NewLine;
        }
#endif
        return output;
    }
}