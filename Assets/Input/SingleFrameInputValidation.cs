using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Input
{
    /// <summary>
    /// inputs to be stored in a history of inputs so that they can be compared to sync for netplay
    /// the validation is to indicate where you don't have to go back and compare any more
    /// </summary>
    ///
    [Serializable]
    public class SingleFrameInputValidation
    {
        public SinglePlayerInputs P1_Inputs;
        public SinglePlayerInputs P2_Inputs;
        public bool SyncConfirmed;
        public int FrameNumber;
        /// <summary>
        /// specifies if the input that is confirmed comes from p1 if true or p2 if false
        /// </summary>
        public bool P1_Source;

        public SingleFrameInputValidation()
        {
            P1_Inputs = new SinglePlayerInputs();
            P2_Inputs = new SinglePlayerInputs();
            SyncConfirmed = false;
            FrameNumber = -1;
            P1_Source = true;
        }

        public bool AreInputsSame(SingleFrameInputValidation other)
        {
            return P1_Inputs.Equals(other.P1_Inputs) && P2_Inputs.Equals(other.P2_Inputs);
        }

        /// <summary>
        /// Source inputs don't need to be compared for rollback, high chance opponent doesn't send the right data. it's a weird thing to send now that I think about it...
        /// todo : only send opponent inputs, not their guess on yours?
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AreOpponentInputsSame(SingleFrameInputValidation other)
        {
            if(P1_Source)
            {
                return P2_Inputs.Equals(other.P2_Inputs);
            }
            else
            {
                return P1_Inputs.Equals(other.P1_Inputs);
            }
        }

        /// <summary>
        /// Creates a copy from previous inputs. Does not copy sync confirmation
        /// </summary>
        /// <param name="otherInputs"></param>
        /// <returns></returns>
        static public SingleFrameInputValidation CreateNextInputs(SingleFrameInputValidation otherInputs)
        {
            var output = new SingleFrameInputValidation() {
                P1_Inputs = new SinglePlayerInputs(otherInputs.P1_Inputs),
                P2_Inputs = new SinglePlayerInputs(otherInputs.P2_Inputs),
                SyncConfirmed = false,
                FrameNumber = otherInputs.FrameNumber+1,
                P1_Source = otherInputs.P1_Source
            };
            return output;
        }
    }
}
