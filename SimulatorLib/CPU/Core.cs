using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using System.Collections.Generic;
using System.Net;

namespace SimulatorLib.CPU;



public enum ProcState
{
    OK = 1,
    MSHR_Stall = 2,
    Memory_Stall = 4,
    WrBack_Stall = 8,
}

public partial class Core
{

    public static uint NULL = uint.MaxValue;

    public MissStatusHandlingRegister MSHR;
    public List<Request> WriteBackQ;

    public HashSet<long> ReadWriteQ;
    public int out_read_req;
    public ReorderBuffer ROB;
    internal Simulator SIM;

    public long time_done;


    public int ID;
    internal long Cycle;

    ProcState STATE = ProcState.OK;

    //public bool RetryMemCtrl = false;
    //private bool RetryMSHR = false;

    public Core(int id, Simulator sim)
    {
        SIM = sim;
        ID = id;

        //if (Param.TRACE_TYPE == TraceType.CPU)
        //    FetchRequest();

        ROB = new ReorderBuffer(this);


        MSHR = new MissStatusHandlingRegister(Param.MSHR_MAX, ID);
        WriteBackQ = new List<Request>(Param.WriteBackQ_MAX);
        ReadWriteQ = new HashSet<long>();

        if (Param.TRACE_TYPE==TraceType.CPU) FetchRequest();
    }

    //private bool c => Cycle < 1000;
    //void P(string txt)
    //{
    //    if (c)
    //    {
    //        DEBUG.Print($"{txt}");

    //    }
    //}

    int fCnt = 0;
    private void Tick_Debug()
    {
        //if (SIM.TraceEnding || SIM.ROBStalled)
        //{
        //    SIM.TraceReader.TestReaderState();

        //    return;
        //}

        //DEBUG.Print($"Core.Tick({Cycle})| Issue | ({REQUEST.PhysicalAddress})=>{REQUEST.BlockAddress}");

        //if (SIM.TraceEnding)
        //{

        //    DEBUG.Print($"{Cycle}: SIM.TraceEnding(SIM.ROBStalled = {SIM.ROBStalled}). | Fetch Count Since Stall: <<{fCnt}>>");
        //    //if (SIM.TraceEnding)
        //    //    DEBUG.Print($"{Cycle}: SIM.TraceEnding: ");
        //    //else
        //    //    DEBUG.Print($"{Cycle}: SIM.ROBStalled: ");

        //    DEBUG.Print($"\tCore[{ID}]:  Current Request: {REQUEST.TYPE}({REQUEST.BlockAddress})[{REQUEST.MemAddr.Rank}{REQUEST.MemAddr.BankGroup}{REQUEST.MemAddr.Bank}].");
        //    DEBUG.Print($"\t\tState: => {STATE} | MSHR stalled for {MSHR.MSHR_Stall} cycless.");

        //    //FetchRequest();

        //}


    }
    public void Tick()
    {

        Cycle++;

        Tick_Debug();

        ROB.Tick();

        if (Param.WriteBack)
            IssueWrBack();

        //P($"{Cycle}: Tick()-> Cycle++ State => {STATE}");

        switch (STATE)
        {
            case ProcState.WrBack_Stall:
                return;
            case ProcState.MSHR_Stall:
                RetryMSHR();
                return;
            case ProcState.Memory_Stall:
                RetryMemory();
                return;
        }

        if (REQUEST == null)
        {
            DEBUG.Print($"  ++++++++++++++++++    {Cycle}| if (REQUEST == null) => TRUE.");
            return;
        }

        if (SIM.TRACE_EOF)
        {
            DEBUG.Print($"  ++++++++++++++++++    {Cycle}| SIM.TRACE_EOF[{SIM.TRACE_EOF}])");
            return;
        }

        IssueRD();

    }

    private void RetryMSHR()
    {
        if (MSHR.Insert(REQUEST.BlockAddress))
        {
            RetryMemory();
        }
    }

    private void RetryMemory()
    {
        if (MEMORY.Enqueue(REQUEST))
        {
            FetchRequestAndConsume();
            //ROB.ConsumeCPUInstructions();
            STATE = ProcState.OK;
            return;
        }
        STATE = ProcState.Memory_Stall;
    }

    private void IssueWrBack()
    {

        if (WriteBackQ.Count == 0)
            return;

        DEBUG.Assert(WriteBackQ[0].TYPE == Operation.WRITE);

        if (MEMORY.Enqueue(WriteBackQ[0]))
        {
            WriteBackQ[0].ID = Cycle;
            WriteBackQ[0].CALLBACK = new CallBack(WriteBackCompleted);

            WriteBackQ.RemoveAt(0);
            STATE = ProcState.OK;
        }

        if (WriteBackQ.Count > Param.WriteBackQ_MAX)
        {
            DEBUG.Print($"{Cycle}: Core[{ID}] WriteBackQ.Count > Param.WriteBackQ_MAX ... return.");
            STATE = ProcState.WrBack_Stall;
        }

    }

    private void IssueRD()
    {

        DEBUG.Assert(REQUEST != null);

        if (REQUEST.BlockAddress == 43209711458)
            DEBUG.Print($"{Cycle}: IssueRD()");
        
        ROB.ConsumeInstructions();

        if (ROB.MemIssued)//(TRACE.NumCPU_Ins == 0)
            SendMemRequest();

   
    }


    private void SendMemRequest()
    {

        if (SIM.TraceEnding)
        {
            DEBUG.Print($">>>>>>>>>>>>>>>>>>>>>>>>>>> {Cycle}: ConsumeMemoryInstruction().START\tState: => {STATE}");
        }

        //P($"\t* ConsumeMemoryInstruction()");


        REQUEST.ID = Cycle;
        REQUEST.CALLBACK = (REQUEST.TYPE == Operation.READ) ? new CallBack(ReadCompleted) : new CallBack(WriteCompleted);
        MemoryController MC = SIM.MemSystem.MemControllers[REQUEST.MemAddr.Channel];

        if (REQUEST.TYPE == Operation.READ)
        {
            if (!MSHR.Insert(REQUEST.BlockAddress))
            {
                STATE = ProcState.MSHR_Stall;
                return;
            }
        }


        if (REQUEST.TYPE == Operation.WRITE && MC.WriteQueueContains(REQUEST.BlockAddress))
            return;

        if (!MC.Enqueue(REQUEST))
        {
            STATE = ProcState.Memory_Stall;
            return;
        }

        STATE = ProcState.OK;

        FetchRequestAndConsume();

        if (SIM.TraceEnding)
        {
            DEBUG.Print($">>>>>>>>>>>>>>>>>>>>>>>>>>> {Cycle}: ConsumeMemoryInstruction().ENDtState: => {STATE} ");
        }



    }

    internal void ReadCompleted(Request request)
    {
        //DEBUG.Print($"{ID}|{Cycle}: ReadCompleted  ({REQUEST.ID}, {REQUEST.TS_Created}) | ({request.BlockAddress}|{request.PhysicalAddress}");// | {request.TracePAddr}");
        request.REQ_Data.TS_Endeded = Cycle;
        //************************SIM.sb.Append(request.REQ_Data.History + "\n");


        //if (REQUEST.BlockAddress == 1545299)
        //    DEBUG.Print($"\t\t\t => 1545299: read completed callback run at cycle {Cycle}");

        if (request.BlockAddress == 1400289)
            DEBUG.Print($"{Cycle}: ReadCompleted({request.BlockAddress}): ");

        if (request.BlockAddress == 43209711458)
            DEBUG.Print($"{Cycle}: ReadCompleted({request.BlockAddress}): ");

        ROB.UpdateInstruction(request);
        MSHR.RemoveAll(request.BlockAddress);

        out_read_req--;

    }

    private void WriteCompleted(Request request)
    {
        ////////////////////////////request.REQ_Data.TS_Endeded = Cycle;
        if (REQUEST.BlockAddress == 1545299)
            DEBUG.Print($"\t\t\t => 1545299: write completed call back run at cycle {Cycle}");
        if (REQUEST.BlockAddress == 43209711458)
            DEBUG.Print($"\t\t\t => 43209711458: write completed call back run at cycle {Cycle}");

        //************************ Sim.sb.Append(request.REQ_Data.History + "\n");
        ROB.UpdateInstruction(request);
        //DEBUG.Print($"Core[{ID}]|{Cycle}: WriteCompleted({request.BlockAddress})");
    }

    private void WriteBackCompleted(Request request)
    {
        //////////////////////////request.REQ_Data.TS_Endeded = Cycle;
        if (REQUEST.BlockAddress == 1545299)
            DEBUG.Print($"\t\t\t => 1545299: write back completed callback run at cycle {Cycle}");
        if (REQUEST.BlockAddress == 43209711458)
            DEBUG.Print($"\t\t\t => 43209711458: write back completed callback run at cycle {Cycle}");

        //************************ Sim.sb.Append(request.REQ_Data.History + "\n");
        //DEBUG.Print($"Core[{ID}]|{Cycle}: WriteCompleted({request.BlockAddress})");
    }

    public void AddWriteBackRequestToQueue(Request wb_req)
    {
        WriteBackQ.Add(wb_req);
        STAT.write_misses++;//Stat.procs[pid].write_misses.collect();
        STAT.wmpc++;//Stat.procs[pid].wmpc.collect();
    }

    public void FetchRequestAndConsume()
    {
        FetchRequest();

        if (REQUEST.BlockAddress == 43209711458)
            DEBUG.Print($"{Cycle}: FetchRequestAndConsume()");
        ROB.ConsumeInstructions();
    }

    public void FetchRequest()
    {

        if (SIM.TraceEnding)
        {
            fCnt++;
            //DEBUG.Print($"{Cycle}: FetchRequest while SIM.TraceEnding: ");
        }

        TRACE = SIM.TraceReader.NextTraceRecord(ID);

        //if (TRACE == null)
        //{
        //    Sim.TRACE_EOF = true;
        //    return;
        //}

        //if (TRACE.Request.BlockAddress == 21226215)
        //    DEBUG.Print($"***************** {Cycle} Core.FetchRequest() => (21226215) *****************");

        TRACE.Request.CreateData(Cycle);

        if (TRACE.Request.WriteBackReq != null)
            TRACE.Request.WriteBackReq.CreateData(Cycle);

        ///REQUEST.ts_started = Cycle;
        //DEBUG.Print($"Core {ID} | Cycle: {Cycle} | FetchRequest() => {TRACE.NumCPU_Ins} - {REQUEST.BlockAddress}");

    }

    internal TraceRecord TRACE;
    internal Request REQUEST
    {
        get
        {
            if (TRACE == null)
                return null;

            return TRACE.Request;
        }
    }
    internal MemoryController Controller(int channel) => SIM.MemSystem.MemControllers[channel];
    private MemorySystem MEMORY => SIM.MemSystem;
    internal Parameters Param => SIM.Param;
    internal IAddressTranslator Translator => SIM.Translator;
    internal Core_Stat STAT => SIM.STAT.CORE[ID];


    bool cycle_debug => (Cycle < 2500);

}


