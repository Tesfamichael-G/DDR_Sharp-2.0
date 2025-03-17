using System.Collections.Generic;
using System.Linq;
using SimulatorLib.Memory.Refresh;
using SimulatorLib.Common;
using SimulatorLib.DDR;
using SimulatorLib.Memory.Schedule;
using SimulatorLib.Memory.WriteMode;
using System.Text;

namespace SimulatorLib.MemCtrl
{
    public partial class MemoryController
    {

        internal int ID;

        public Simulator SIM;
        public Parameters PARAM;
        public Channel CHANNEL;
        public IScheduler SCHEDULER;
        public IRefresh REFRESHER;
        public IWriteMode WRITE_MODE;
        ///////////////////////public DeActivator DEACTIVATOR;
        public RequestQueue QUEUE;
        public Bus BUS;

        public Stat STAT;

        //public ResponseQueue ResponseQ;

        private long cycle = 0;
        public long Cycle
        {
            get => cycle;
            set
            {
                cycle = value;
                CHANNEL.Cycle = value;
            }
        }


        public bool ValidationMode = true;
        public void RobStalled(long addr)
        {
            SCHEDULER.RobStalled(addr);
            QUEUE.RobStalled(addr);
        }

        public MemoryController(Simulator simulator, int id)
        {

            SIM = simulator;
            PARAM = SIM.Param;
            STAT = simulator.STAT;

            if (SIM.DEACTIVATOR_ENABLED)
            {
                CHANNEL = new DeActivator.DEA_Channel(this);
                SCHEDULER = new DeActivator.DEA_FRFCFS(this); //WRITE_MODE = new HalfServe(this);
            }
            else
            {
                CHANNEL = new Channel(this);
                SCHEDULER = new FRFCFS(this); //new FCFS(this);
            }
            WRITE_MODE = new ServeN(this);

            CHANNEL.ID = id;
            ID = id;

            if (PARAM.REFRESH_MODE == REF_MODE.RANK) REFRESHER = new RankRefresh(this); else REFRESHER = new RankRefresh(this);

            QUEUE = new RequestQueue(this);
            BUS = new Bus(this);

        }

        public void Initialize()
        {
            CHANNEL.Intialize();
        }

        public void Tick()
        {
            //if (SIM.TraceEnding || SIM.ROBStalled)
            //{
            //    DEBUG.Print($"Memory Controller.Queue: READS: {QUEUE.RD_Queued} WRITES: {QUEUE.WR_Queued} | WriteMode: {WRITE_MODE.DrainWrites}");
            //}

            WRITE_MODE.Tick();

            REFRESHER.Tick();

            CHANNEL.Update();

            BUS.Tick();

            SCHEDULER.Tick();

            CHANNEL.UpdateStat();
        }

        public bool FinishPending()
        {

            bool writemode = (QUEUE.RD_Queued == 0) ? true : false;

            WRITE_MODE.FORCE_Writes(writemode);

            REFRESHER.Tick();

            CHANNEL.Update();

            BUS.Tick();

            SCHEDULER.Tick();

            CHANNEL.UpdateStat();

            //DEBUG.Print($"Finishing : {QUEUE.Length} remaining. WriteMode: {WRITE_MODE.DrainWrites} (r, w): ({QUEUE.RD_Queued},{QUEUE.WR_Queued})");
            return (QUEUE.RD_Queued + QUEUE.WR_Queued) == 0;
        }


        const long CYCLES = 50 * 1000 * 1000;
        public void REF_RANK(int rank)
        {
            //if (Cycle > CYCLES && Cycle % 1000 == 0) //(MC.cycle_debug)
            //    DEBUG.Print($"{Cycle}: Refresh Issued for rank {rank}");
            CHANNEL.RefreshRank(rank);
        }
        public void CLOSE_BANK(int rank, int group, int bank)
        {
            //if (Cycle > CYCLES && Cycle % 1000 == 0) //(MC.cycle_debug)
            //    DEBUG.Print($"{Cycle}: Refresh Issued for close [{rank}, {group}, {bank}] ");
            CHANNEL.CloseBank(rank, group, bank);
        }
        public bool ISSUE(Request request) => CHANNEL.IssueRequest(request);

        public bool Enqueue(Request REQ) => QUEUE.Enqueue(REQ);

        public bool WriteQueueContains(long BlockAddress) => QUEUE.WriteContains(BlockAddress);
        internal bool ReadQueueContains(long BlockAddress) => QUEUE.ReadContains(BlockAddress);
        public int QueueContains(long block_address) => QUEUE.Contains(block_address);

        //public StringBuilder sb = new StringBuilder();

        public bool cycle_debug => false;// (Cycle > 333215 && Cycle < 333396);

    }


}

#region X-CODE

//private void UpdateQueue()
//{

//    if (WRITE_MODE.DrainWrites)
//    {
//        //if (x % 1000 == 0)
//        //{
//        //    DEBUG.Print($"~~~~~~~~~~~  Update WriteQueue ~~~~~~~~~~~~~~~ ");
//        //    PrintWQStatus();
//        //}

//        int wCount = (int)(WriteQueue?.Count);

//        for (int i = 0; i < wCount; i++)
//        {
//            if (WriteQueue[i].IsServed) continue;
//            WriteQueue[i].CanBeIssued = CHANNEL.CanIssue(WriteQueue[i], Operation.WRITE);
//        }
//    }
//    else
//    {
//        int rCount = (int)ReadQueue?.Count;

//        for (int i = 0; i < rCount; i++)
//        {
//            if (ReadQueue[i].IsServed) continue;
//            ReadQueue[i].CanBeIssued = CHANNEL.CanIssue(ReadQueue[i], Operation.READ);
//        }
//    }
//}

//int RQ_LOOKUP_LATENCY = 1;

//public bool Enqueue(Request REQ)
//{

//    //if (REQ.BlockAddress == 1545299)
//    //    System.Diagnostics.Debug.Print($"\t\tTRY ENQUEUE [{REQ.TYPE}] 1545299 at cycle {Cycle}");

//    if (REQ.TYPE == Operation.WRITE && WriteQueue.Count >= PARAM.WRITEQ_MAX)
//        return false;

//    if (REQ.TYPE == Operation.READ && ReadQueue.Count >= PARAM.READQ_MAX)
//        return false;

//    REQ.TsArrival = Cycle;

//    if (cycle_debug)
//        DEBUG.Print($"{Cycle}: ****** REQ.TsArrival({REQ.BlockAddress}) = {Cycle}");

//    if (AddXbarLatency)
//        return xEnqueue(REQ);

//    switch (REQ.TYPE)
//    {
//        case Operation.READ:
//            if (WriteQueueContains(REQ.BlockAddress))
//            {

//                SIM.STAT.CORE[REQ.CoreID].WriteBack_Hit++;

//                REQ.TsDeparture = Cycle;
//                CallBack callback = REQ.CALLBACK;
//                callback(REQ);

//                return true;
//            }
//            ReadQueue.Add(REQ);
//            break;
//        case Operation.WRITE:
//            WriteQueue.Add(REQ);
//            break;
//        default:
//            break;
//    }
//    return true;
//}
//public void Dequeue(Request REQ)
//{

//    if (AddXbarLatency)
//    {
//        xDequeue(REQ);
//        return;
//    }

//    REQ.TsDeparture = Cycle;
//    CallBack callback = REQ.CALLBACK;
//    callback(REQ);

//    if (REQ.TYPE == Operation.READ)
//    {
//        ReadQueue.Remove(REQ);
//    }
//    else
//    {
//        WriteQueue.Remove(REQ);
//    }

//}

//public bool WriteQueueContains(long BlockAddress) 
//{
//    if (WriteQueue == null)
//        return false;

//    if (WriteQueue.Count == 0)
//        return false;

//    for (int i = 0; i < WriteQueue.Count; i++)
//    {
//        if (WriteQueue[i].BlockAddress == BlockAddress)
//        {
//            SIM.MemSystem.MergedWrites++;
//            return true;
//        }

//    }
//    return false;

//}

//internal bool ReadQueueContains(long BlockAddress)  
//{
//    if (ReadQueue == null)
//        return false;

//    if (ReadQueue.Count == 0)
//        return false;

//    for (int i = 0; i < ReadQueue.Count; i++)
//    {
//        if (ReadQueue[i].BlockAddress == BlockAddress)
//        {
//            SIM.MemSystem.MergedReads++;
//            return true;
//        }

//    }

//    return false;
//}

//public int QueueContains(long block_address)  
//{

//    if (WriteQueue != null)
//    {
//        foreach (var req in WriteQueue)
//        {
//            if (req.MemAddr.BlockAddress == block_address)
//            {
//                SIM.MemSystem.MergedReads++;
//                return PARAM.WQ_LOOKUP_LATENCY;
//            }
//        }
//    }

//    if (ReadQueue != null)
//    {
//        foreach (var req in ReadQueue)
//        {
//            if (req.MemAddr.BlockAddress == block_address)
//            {
//                SIM.MemSystem.MergedReads++;
//                return RQ_LOOKUP_LATENCY;
//            }
//        }
//    }
//    return 0;


//}
#region X-BAR
//public bool xEnqueue(Request REQ)
//{

//    if (REQ.BlockAddress == 1545299)
//        System.Diagnostics.Debug.Print($"\t\t\t*** ENQUEUE [{REQ.TYPE}] 1545299 at cycle {Cycle}");

//    switch (REQ.TYPE)
//    {
//        case Operation.READ:

//            if (REQ.BlockAddress == 1545299)
//            {
//                System.Diagnostics.Debug.Print($"\t\t\t {Cycle} => WriteQueueContains[{WriteQueue.Count}](1545299): {WriteQueueContains(REQ.BlockAddress)}");
//            }
//            if (WriteQueueContains(REQ.BlockAddress))
//            {
//                if (REQ.BlockAddress == 1545299)
//                    System.Diagnostics.Debug.Print($"\t\t\t => 1545299: writeback hit at cycle {Cycle}");
//                SIM.XBAR.Enqueue(REQ);
//                SIM.STAT.CORE[REQ.CoreID].WriteBack_Hit++;
//                return true;
//            }
//            //Rload++;
//            ReadQueue.Add(REQ);
//            //DEBUG.Print($"{Cycle}: READ_Q.Add({request.BlockAddress})");
//            break;
//        case Operation.WRITE:
//            //Wload++;
//            WriteQueue.Add(REQ);
//            //DEBUG.Print($"{Cycle}: WRITE_Q.Add({request.BlockAddress})");
//            break;
//        default:
//            break;
//    }

//    return true;

//}
//public void xDequeue(Request REQ)
//{

//    REQ.TsDeparture = Cycle;

//    if (REQ.TYPE == Operation.READ)
//    {
//        SIM.XBAR.Enqueue(REQ);
//        ReadQueue.Remove(REQ);
//    }
//    else
//    {
//        WriteQueue.Remove(REQ);
//        REQ.Latency = (int)(REQ.TsDeparture - REQ.TsArrival);
//        CallBack callback = REQ.CALLBACK;
//        callback(REQ);
//    }

//}

#endregion


#endregion

