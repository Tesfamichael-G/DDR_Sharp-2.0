using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.Memory.WriteMode
{
    public class HalfServe : IWriteMode
    {

        private MemoryController MC;

        public uint ServeMax = 32;
        public uint ServeCnt;


        private bool drain_writes;

        public HalfServe(MemoryController mctrl)
        {
            MC = mctrl;
            ForceWrite = false;
        }

        public bool RQEmpty => (MC.QUEUE.RD_Queued == 0);
        public bool WQFull => MC.QUEUE.WR_Queued == MC.PARAM.WRITEQ_MAX;
        public bool DrainWrites => drain_writes;

        public void AlertWriteMode(COMMAND cmd)
        {
            ServeCnt++;
        }

        public bool ForceWrite = false;
        public void Tick()
        {

            drain_writes = CanWrite();

            bool CanWrite()
            {
                if (MC.QUEUE.WR_Queued == MC.PARAM.WRITEQ_MAX)
                    return true;

                if (MC.QUEUE.WR_Queued == 0)
                    return false;

                if (MC.QUEUE.RD_Queued == 0)
                    return true;

                return (ServeCnt < ServeMax);
            }

        }

        public void FORCE_Writes(bool write)
        {
            drain_writes = write ;
        }


    }


}
