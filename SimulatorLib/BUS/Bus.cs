using SimulatorLib.Common;
using SimulatorLib.DDR;
using System.Collections.Generic;
using SimulatorLib.Memory.Schedule;
using SimulatorLib.MemCtrl;

namespace SimulatorLib;

public class Bus
{
    public Queue<Request> QUEUE;
    private MemoryController MC;

    private int CountDown;
    private int clock_factor;
    private int transport_latency;
    private bool include_xbar;
    public Bus(MemoryController mc)
    {
        MC = mc;
        QUEUE = new Queue<Request>();
        clock_factor = mc.PARAM.ClockFactor;
        transport_latency = mc.PARAM.BL;
        include_xbar = mc.PARAM.INCLUDE_XBAR_LATENCY;
    }

    public void Add(Request REQ)
    {
        DEBUG.Assert(CountDown <= 0);
        QUEUE.Enqueue(REQ);
        CountDown = transport_latency;

        if (REQ.BlockAddress == 1400289)
            DEBUG.Print($"{MC.Cycle}: BUS.Add({REQ.BlockAddress}): ");
    }

    public void Tick()
    {
        if (QUEUE.Count == 0)
            return;

        CountDown -= clock_factor;

        if (MC.Cycle >= QUEUE.Peek().TsCompletion)
        {

            Request REQ = QUEUE.Dequeue();


            if (REQ.BlockAddress == 43209711458)
                DEBUG.Print($"Cycle: {MC.Cycle} - Completion time reached for  43209711458 now.");

            if (REQ.BlockAddress == 112332342044)
                DEBUG.Print($"Cycle: {MC.Cycle} - Completion time reached for  112332342044 now.");

            if (REQ.CALLBACK == null)
                return;

            if (!include_xbar)
            {
                if (REQ.BlockAddress == 43209711458)
                    DEBUG.Print($"{MC.Cycle}: Callback.Invoke({REQ.BlockAddress}): ");

                if (REQ.BlockAddress == 1400289)
                    DEBUG.Print($"{MC.Cycle}: Callback.Invoke({REQ.BlockAddress}): ");

                CallBack callback = REQ.CALLBACK;
                callback(REQ);
                return;
            }

            if (REQ.TYPE == Operation.READ)
            {
                MC.SIM.XBAR.Enqueue(REQ);
            }
            else
            {
                REQ.CALLBACK(REQ);
                if (MC.Cycle < 1000)
                    DEBUG.Print($"{MC.Cycle}: BUS.Callback.Invoke({REQ.BlockAddress}): ");
            }

        }

    }


}
