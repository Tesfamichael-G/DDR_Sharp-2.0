using System.Collections.Generic;
using System.Text;
using SimulatorLib.Common;
using SimulatorLib.MemCtrl;

namespace SimulatorLib;

public class xMemorySystem
{

    public long MergedReads;
    public long MergedWrites;

    public Simulator Sim;
    public List<MemoryController> MemControllers;
    public Parameters Param;
    IAddressTranslator Translator;

    Queue<TraceRecord> requestQueue;


    long cycle = 0;

    public long Cycle
    {
        get
        {
            return cycle;
        }
        set
        {
            cycle = value;
            for (int channel = 0; channel < Param.NUM_CHANNELS; channel++)
            {
                MemControllers[channel].Cycle = Cycle;
            }
        }
    }


    public xMemorySystem(Simulator simulator)
    {
        Sim = simulator;
        Param = Sim.Param;

        Translator = Sim.Translator;

        MemControllers = new List<MemoryController>(Param.NUM_CHANNELS);
        for (int i = 0; i < Param.NUM_CHANNELS; i++)
        {
            MemoryController mc = new MemoryController(Sim, i);
            MemControllers.Add(mc);
        }
        //Console.WriteLine($"Param.NUM_CHANNELS: {Param.NUM_CHANNELS}. Memory controller created. {MemControllers.Count}");
        MEM_TRACE = new MemTrace(simulator);
        requestQueue = new Queue<TraceRecord>(256);
    }

    private MemTrace MEM_TRACE;
    private void Init()
    {

        MergedReads = 0;
        MergedWrites = 0;

        for (int i = 0; i < Param.NUM_CHANNELS; i++)
        {
            MemControllers[i].CHANNEL.Intialize();
        }
        //sw = new System.IO.StreamWriter(@"C:\Users\tmik\Desktop\Validation\ENQUEUE.txt");
    }

    public void Initialize() => Init();

    public void RobStalled(long addr) => MemControllers[0].RobStalled(addr);

    public bool Enqueue(Request request) => MemControllers[request.MemAddr.Channel].Enqueue(request);


    private bool RetryMemCtrl;

    internal bool SendToMemory()
    {

        REQUEST.ID = Cycle;

        //DEBUG.Print($"{cycle}: SendToMemory({REQUEST.BlockAddress})");

        MemoryController MC = MemControllers[REQUEST.MemAddr.Channel];

        if (REQUEST.TYPE == Operation.WRITE && MC.WriteQueueContains(REQUEST.BlockAddress))
            return true;

        //REQUEST.CALLBACK = (REQUEST.TYPE == Operation.READ) ? new CallBack(ReadCompleted) : new CallBack(WriteCompleted);
        RetryMemCtrl = !MC.Enqueue(REQUEST);

        if (RetryMemCtrl)
        {
            //if (cycle > 2700)
            //{
            //    DEBUG.Print($"{cycle}: Retry MemCtrl ... true.");
            //}
            return false;
        }

        //////////////////////////////REQUEST.REQ_Data.TS_Queued = Cycle;

        //if (cycle > 2700)
        //{
        //    DEBUG.Print($"{cycle}: Retry MemCtrl ... false.");
        //}
        return true;

    }

    //internal void ReadCompleted(Request request)
    //{
    //    //////////////////////request.REQ_Data.TS_Endeded = Cycle;
    //}

    //private void WriteCompleted(Request request)
    //{
    //    //////////////////////request.REQ_Data.TS_Endeded = Cycle;
    //}

    public void tick()
    {
        Cycle++;

        if (cycle % Param.ClockFactor == 0)
        {
            for (int c = 0; c < Param.NUM_CHANNELS; c++)
                MemControllers[c].Tick();
        }
    }

    public void Tick()
    {
        //if (cycle > 2700)
        //{
        //    DEBUG.Print($"{cycle}: Memory.Tick() RetryMemCtrl ... {RetryMemCtrl}.");
        //}

        //MEM_TRACE.Tick();

        if (RetryMemCtrl)
        {
            bool IsRetryOK = MemControllers[REQUEST.MemAddr.Channel].Enqueue(REQUEST);

            if (!IsRetryOK) return;

            RetryMemCtrl = false;

            FetchRequest();
            return;
        }

        //if (REQUEST == null)
        //    return;

        //if (Sim.TRACE_EOF)
        //    return;

        DEBUG.Assert(REQUEST != null);

        if (!SendToMemory())
        {
            //if (cycle > 2700)
            //{
            //    DEBUG.Print($"{cycle}: SendToMemory ... false <return>.");
            //}
            return;
        }

        FetchRequest();

    }

    public void BatchFetchRequest()
    {

        var traces = MEM_TRACE.FetchTraces(128);
        for (int i = 0; i < traces.Length; i++) 
        {
            requestQueue.Enqueue(traces[i]);
        } 
    }

    void dequeueRequest()
    {
        TRACE = requestQueue.Dequeue();
        //Console.WriteLine($"consuming...");

        if (TRACE == TraceRecord.NULL)
        {
            //DEBUG.Print($"Sim.TRACE_EOF = true");
            Sim.TRACE_EOF = true;

            return;
        }

    }
    public void FetchRequest()
    {

        dequeueRequest();
        ////if (cycle > 2700)
        ////{
        ////    DEBUG.Print($"{cycle}: Fetching...");
        ////}

        //TRACE = Sim.TraceReader.NextTraceRecord(0);

        //if (TRACE == null)
        //{
        //    //DEBUG.Print($"Sim.TRACE_EOF = true");
        //    Sim.TRACE_EOF = true;

        //    return;
        //}

        ////TRACE.Request.CreateData(cycle);

        ////DEBUG.Print($"{cycle}: FETCH[REQUEST] => {TRACE.Request.BlockAddress}");

    }

    public TraceRecord TRACE;

    public Request REQUEST
    {
        get
        {
            if (TRACE == TraceRecord.NULL)
                return null;

            return TRACE.Request;
        }
    }

    bool cycle_debug => (Cycle < 2500);

    public class MemTrace
    {

        Queue<TraceRecord> requestQueue;
        Simulator SIM;
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        int count = 0;
        int MaxSize = 1024;
        int MinSize = 128;

        public MemTrace(Simulator sim)
        {
            requestQueue = new Queue<TraceRecord>(512);
            SIM = sim;
            Initialize();
            Tick();
        }

        public void Initialize()
        {
            //rwLock.EnterWriteLock();
            //try
            //{
            for (int i = 0; i < 256; i++)
            {
                var TRACE = SIM.TraceReader.NextTraceRecord(0);
                requestQueue.Enqueue(TRACE);
            }
            count = 256;
            //}
            //finally
            //{
            //    rwLock.ExitWriteLock();
            //}
        }

        private bool IsEmpty = true;

        public void Tick()
        {
            if (SIM.TRACE_EOF == true) return;

            Task.Run(() =>
            {
                while (SIM.TRACE_EOF != true && count < MinSize)
                {
                    getTraces();
                }
            });
        }

        private void getTraces()
        {

            rwLock.EnterWriteLock();
            int i;
            for (i = count; i < MaxSize; i++)
            {
                var TRACE = SIM.TraceReader.NextTraceRecord(0);
                if (TRACE != TraceRecord.NULL)
                {
                    SIM.TRACE_EOF = true;
                    break;
                }
                requestQueue.Enqueue(TRACE);
            }
            rwLock.ExitWriteLock();
            //try
            //{           //}
            //finally
            //{

            //}

        }

        public TraceRecord Dequeue()
        {
            TraceRecord result = TraceRecord.NULL;

            rwLock.EnterReadLock();

            try
            {
                if (requestQueue.Count > 0)
                {
                    //Console.WriteLine("DEQUEUE");
                    result = requestQueue.Dequeue();
                }
                // No need for an else statement; the queue is empty
            }
            finally
            {
                IsEmpty = false;
                rwLock.ExitReadLock();
            }

            return result;
        }

        public TraceRecord[] FetchTraces(int iCnt)
        {
            TraceRecord result = TraceRecord.NULL;
            if (iCnt > count)
            {
                iCnt = count;
            }
            TraceRecord[] traces = new TraceRecord[iCnt];
            rwLock.EnterReadLock();
            for (int i = 0; i < iCnt; i++)
            {
                traces[i] = requestQueue.Dequeue();
            }
            rwLock.ExitReadLock();
            
            return traces;
        }

    }
}

#region Commented

//public void Tick0()
//{
//    //DEBUG.Print($"Core.Tick({Cycle})| Issue | ({REQUEST.PhysicalAddress})=>{REQUEST.BlockAddress}");

//    //Cycle++;

//    bool RequestAlreadyIssued = false;

//    if (RetryMemCtrl)
//    {
//        bool IsRetryOK = MemControllers[REQUEST.MemAddr.Channel].Enqueue(REQUEST);

//        if (!IsRetryOK) return;

//        RetryMemCtrl = false;

//        RequestAlreadyIssued = true;
//        FetchRequest();
//    }

//    //if (RetryMemCtrl)
//    //{

//    //    DEBUG.Assert(REQUEST != null && TRACE.NumCPU_Ins == 0);

//    //    bool RetryOK = Retry();

//    //    if (!RetryOK) return;

//    //    RequestAlreadyIssued = true;
//    //    FetchRequest();

//    //}

//    if (REQUEST == null && Sim.TRACE_EOF)
//        return;

//    DEBUG.Assert(REQUEST != null);

//    //if (REQUEST == null)
//    //    return;

//    //DEBUG.Print($"           {Cycle}| Issue | ({REQUEST.PhysicalAddress})=>{REQUEST.BlockAddress}");

//    //Issue(RequestAlreadyIssued);
//    //DEBUG.Assert(REQUEST != null);

//    if (RequestAlreadyIssued) return;

//    if (!SendToMemory())
//        return;

//    FetchRequest();

//}

//private bool Retry()
//{
//    if (RetryMemCtrl)
//    {
//        bool IsRetryOK = MemControllers[REQUEST.MemAddr.Channel].Enqueue(REQUEST);

//        if (!IsRetryOK) return false;

//        RetryMemCtrl = false;

//        return true;
//    }

//    throw new System.Exception($"ERROR: Reissue Request at cycle {Cycle}");
//}

//private void Issue(bool REQ_Issued)
//{

//    DEBUG.Assert(REQUEST != null);

//    if (REQ_Issued) return;

//    if (!SendToMemory())
//        return;

//    FetchRequest();

//}
//internal void ReadCompleted(Request request)
//{
//    //DEBUG.Print($"{ID}|{Cycle}: ReadCompleted  ({REQUEST.ID}, {REQUEST.TS_Created}) | ({request.BlockAddress}|{request.PhysicalAddress}");// | {request.TracePAddr}");
//    request.REQ_Data.TS_Endeded = Cycle;
//    //************************ Sim.sb.Append(request.REQ_Data.History + "\n");

//    if (REQUEST.BlockAddress == 1545299)
//        DEBUG.Print($"\t\t\t => 1545299: read completed callback run at cycle {Cycle}");

//    //ROB.UpdateInstruction(request);
//    //MSHR.RemoveAll(request.BlockAddress);

//}

//private void WriteCompleted(Request request)
//{
//    request.REQ_Data.TS_Endeded = Cycle;
//    if (REQUEST.BlockAddress == 1545299)
//        DEBUG.Print($"\t\t\t => 1545299: write completed call back run at cycle {Cycle}");

//    //************************ Sim.sb.Append(request.REQ_Data.History + "\n");
//    //ROB.UpdateInstruction(request);
//    //DEBUG.Print($"Core[{ID}]|{Cycle}: WriteCompleted({request.BlockAddress})");
//}
//internal bool SendToMemory0()
//{
//    //DEBUG.Print($"              {Cycle}| SendToMemory");

//    REQUEST.ID = Cycle;
//    //REQUEST.CALLBACK = (REQUEST.TYPE == Operation.READ) ? new CallBack(ReadCompleted) : new CallBack(WriteCompleted);

//    MemoryController MC = MemControllers[REQUEST.MemAddr.Channel];

//    if (REQUEST.TYPE == Operation.WRITE && MC.WriteQueueContains(REQUEST.BlockAddress))
//        return true;

//    //if (cycle_debug)
//    //    DEBUG.Print($"{Cycle}: CPU TRY SEND ({REQUEST.BlockAddress}) at {Cycle}");
//    //MemControllers[request.MemAddr.Channel].Enqueue(request)
//    bool IsMctrlEnqueueOK = MC.Enqueue(REQUEST);

//    if (!IsMctrlEnqueueOK)
//    {
//        //if (cycle_debug)
//        //    DEBUG.Print($"{Cycle}: CPU TRY SEND MEMORY ({REQUEST.BlockAddress}) FAILED at {Cycle}");
//        RetryMemCtrl = true;
//        return false;
//    }

//    //if (cycle_debug)
//    //    DEBUG.Print($"{Cycle}: CPU sent ({REQUEST.BlockAddress}) at {Cycle}");

//    REQUEST.REQ_Data.TS_Queued = Cycle;

//    if (REQUEST.BlockAddress == 1545299)
//        DEBUG.Print($"\t\t\t => 1545299: queued at cycle {Cycle}");

//    //DEBUG.Print($"                  {Cycle}| GONE to memory | ({REQUEST.PhysicalAddress})=>{REQUEST.BlockAddress}");
//    return true;

//}

#endregion
