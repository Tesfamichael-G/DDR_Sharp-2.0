using System;
using System.Collections.Generic;
using SimulatorLib.Common;

namespace SimulatorLib
{
    public class Parameters_DeActivator
    {
        #region DeActivator

        /********************************************************************************************************************************************/
        /*      DE-ACTIVATOR        */
        /********************************************************************************************************************************************/

        //public List<deact_parameter> DEACT_Parameters;

        //private int index = -1;
        //public int DEACT_Index
        //{
        //    get => index;
        //    set
        //    {
        //        index = value;
        //        if (index < 0)
        //        {
        //            DEACTIVATOR_ENABLED = false;
        //            MAX_COUNTER_SIZE = 0;
        //            MAX_ROW_BUFFERS_PER_BANK = 0;
        //            OLD_THRESHOLD = 0;
        //            MAX_ACTIVATIONS_PERMITTED = 0;
        //            VALIDATION_TIME = 0;
        //        }
        //        else
        //        {
        //            MAX_COUNTER_SIZE = DEACT_Parameters[index].MaxCounter;
        //            MAX_ROW_BUFFERS_PER_BANK = DEACT_Parameters[index].MaxRowBuffer;
        //            OLD_THRESHOLD = DEACT_Parameters[index].OldThreshhold;
        //            MAX_ACTIVATIONS_PERMITTED = DEACT_Parameters[index].MaxActivation;
        //            VALIDATION_TIME = DEACT_Parameters[index].ValidationTime;
        //        }
        //    }
        //}

        /********************************************************************************************************************************************/

        #endregion
     
        public int MAX_COUNTER_SIZE;

        public int MAX_ROW_BUFFERS_PER_BANK;

        public int MAX_ACTIVATIONS_PERMITTED;

        public int VALIDATION_TIME;


    }

}
