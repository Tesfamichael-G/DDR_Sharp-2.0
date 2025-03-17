using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.Memory.WriteMode
{

    public class AutoWriteMode : IWriteMode
    {

        private MemoryController MC;

        private int Max = 40;
        private int Min = 20;

        private bool drain_writes;

        public AutoWriteMode(MemoryController mctrl)
        {
            MC = mctrl;
        }
        
        //public bool WQAboveLO_WM => MC.WriteQueue.Count > Min;
        //public bool WQAboveHI_WM => MC.WriteQueue.Count > Max;
        //public bool RQEmpty => (MC.ReadQueue.Count == 0);

        public bool WQAboveLO_WM => MC.QUEUE.WR_Queued > Min;
        public bool WQAboveHI_WM => MC.QUEUE.WR_Queued  > Max;
        public bool RQEmpty => (MC.QUEUE.RD_Queued == 0);
        public bool DrainWrites => drain_writes;


        public void AlertWriteMode(COMMAND cmd)
        {
        }

        public void FORCE_Writes(bool write)
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            drain_writes = (drain_writes && WQAboveLO_WM) || WQAboveHI_WM || RQEmpty;
        }



    }


}
