using System.Collections.Generic;

namespace SimulatorLib
{

    public class deact_parameter
    {

        public int MaxCounter { get; set; }

        public int MaxRowBuffer { get; set; }

        //public int OldThreshhold { get; set; } = 8;

        public int MaxActivation { get; set; }

        public int ValidationTime { get; set; }

        public override string ToString()
        {
            return $"[ {MaxCounter} | {MaxRowBuffer} | {MaxActivation} | {ValidationTime} ]";
        }
    }

    public class DeActivator_Parameters
    {

        public bool DEACTIVATOR_ENABLED = false;

        public List<deact_parameter> LIST;

        public DeActivator_Parameters()
        {
            LIST = new List<deact_parameter>();
        }

        public void Add(deact_parameter param)
        {
            LIST.Add(param);
        }

        public deact_parameter DefaultParameter => LIST[0];


    }


}
