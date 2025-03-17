using System.Collections.Generic;

namespace SimulatorLib.CPU
{
    public partial class MissStatusHandlingRegister
    {
        private List<long> Addresses;
        private int Capacity;

        private int CoreID;
        public int MSHR_Stall;
        private int MSHR_Merge;

        //private SimulationStatistics Stat;
        public MissStatusHandlingRegister(int capacity, int core_ID)
        {
            Capacity = capacity;
            Addresses = new List<long>(Capacity);
            CoreID = core_ID;
            MSHR_Stall = 0;
            MSHR_Merge = 0;
            //Stat = ((DRAMSimulator)System.Windows.Application.Current.MainWindow).Sim.Stat;
        }

        public bool Insert(long BlockAddress)
        {
            if (Addresses.Count == Capacity)
            {
                MSHR_Stall++;//Stat.procs[CoreID].stall_mshr.collect(); 
                //if (CoreID == 1)
                //    DEBUG.Print($"\t\t\t\t~~~~~~~~~~FULL:  MSHR[{CoreID}]: {Addresses.Count}/{Capacity} address stored. Returning) ... ");

                return false;
            }

            if (Addresses.Contains(BlockAddress))
            {
                MSHR_Merge++;
                return true;
            }
            Addresses.Add(BlockAddress);
            //if (CoreID == 1)
            //    DEBUG.Print($"\t\t\t\t~~~~~~~~~~  MSHR[{CoreID}].Insert({BlockAddress}) | {Addresses.Count}/{Capacity} ... ");
            return true;
        }

        public void Remove(long BlockAddress)
        {
            Addresses.Remove(BlockAddress);
        }

        public void RemoveAll(long BlockAddress)
        {
            Addresses.RemoveAll(addr => addr == BlockAddress);
            //if (CoreID == 1)
            //    DEBUG.Print($"\t\t\t\t~~~~~~~~~~  MSHR[{CoreID}].RemoveAll({BlockAddress}) | {Addresses.Count}/{Capacity}  ... ");
        }

        public string Load => $"{Addresses.Count}/{Capacity}";
        public string First => (Addresses.Count==0)?"": $"{Addresses[0]}..{Addresses[Addresses.Count-1]}";
        //public bool add_to_mshr(ProcRequest req_)
        //{
        //    if (MSHR.Exists(x => x.actual_addr == req_.actual_addr && x.block_addr == req_.block_addr))
        //    {
        //        for (int i = 0; i < MSHR.Count; i++)
        //        {
        //            if (MSHR[i].actual_addr == req_.actual_addr && MSHR[i].block_addr == req_.block_addr)
        //            {
        //                if (Config.DEBUG_PROC)
        //                    DEBUG.WriteLine("-- MSHR : Merge Reqs : [" + req_.type + "] [0x" + req_.actual_addr.ToString("X") + "]");
        //                return true;
        //            }
        //        }
        //    }

        //    if (MSHR.Count > Config.mshr_size)
        //    {
        //        if (Config.DEBUG_PROC)
        //            DEBUG.WriteLine("-- MSHR : Failed to add Req to MSHR.");
        //        mshr_stalled++;
        //        return false;
        //    }
        //    mshr_loaded++;
        //    MSHR.Add(req_);
        //    if (Config.DEBUG_PROC)
        //        DEBUG.WriteLine("-- MSHR : New Entry : [" + req_.type + "] [0x" + req_.actual_addr.ToString("X") + "]");
        //    return true;
        //}

    }
}
