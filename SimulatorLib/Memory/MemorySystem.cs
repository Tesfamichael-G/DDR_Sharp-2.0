using System.Collections.Generic;
using System.Text;
using SimulatorLib.Common;
using SimulatorLib.MemCtrl;

namespace SimulatorLib;

public class MemorySystem
{

    public long MergedReads;
    public long MergedWrites;

    public Simulator Sim;
    public List<MemoryController> MemControllers;
    public Parameters Param;
    IAddressTranslator Translator;

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

    public MemorySystem(Simulator simulator)
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

    }

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

    public void FetchRequest()
    {
        //if (cycle > 2700)
        //{
        //    DEBUG.Print($"{cycle}: Fetching...");
        //}

        TRACE = Sim.TraceReader.NextTraceRecord();

        if (TRACE == TraceRecord.NULL)
        {
            //DEBUG.Print($"Sim.TRACE_EOF = true");
            Sim.TRACE_EOF = true;

            return;
        }

        //TRACE.Request.CreateData(cycle);

        DEBUG.Print($"{cycle}: FETCH[REQUEST] | BlockAddress: {TRACE.Request.BlockAddress}");

    }

    public TraceRecord TRACE;

    public Request REQUEST
    {
        get
        {
            if (TRACE == null)
                return null;

            return TRACE.Request;
        }
    }

    bool cycle_debug => (Cycle < 2500);
    public void RobStalled(long addr) => MemControllers[0].RobStalled(addr);
}
