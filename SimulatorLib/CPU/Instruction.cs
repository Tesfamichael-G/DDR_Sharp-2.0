using System;
using SimulatorLib.Common;

namespace SimulatorLib.CPU
{
    public class Instruction
    {

        public int CoreID;

        public Operation TYPE;
        public long MemAddress;
        public long PC;
        public long? CompletetionTime;

        //public UInt64 served_cycle = 0;
        public UInt64 Cycle;

        public bool IsMem = false;
        public bool IsWrite = false;
        public bool IsCopy = false;
        public int WordOffset;
        public bool IsReady = false;

        public Instruction(Operation type = Operation.NOP, bool is_mem = false, bool is_ready = false, long pc = 0)
        {
            TYPE = Operation.NOP;
            IsMem = false;
            IsReady = false;
            PC = pc;
            CompletetionTime = null;
            MemAddress = 0;
        }




    }


}
