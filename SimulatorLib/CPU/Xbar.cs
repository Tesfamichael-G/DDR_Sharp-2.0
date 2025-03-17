using SimulatorLib;
using SimulatorLib.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.CPU
{
    // This code is run only to validate the correctness of this simulator against ramulator#
    public class Xbar
    {
        public long Cycles;
        public List<Request> Reqs;
        Simulator SIM;

        public Xbar(Simulator sim)
        {
            Reqs = new List<Request>(128);
            SIM = sim;
        }

        public void Enqueue(Request req)
        {
            
            if (req.BlockAddress == 112332342044 )
                DEBUG.Print($"Cycle: {Cycles} - Xbar.enqueued for  112332342044 now.");

            if (req.BlockAddress == 43209711458)
                DEBUG.Print($"Cycle: {Cycles} - Xbar.enqueued for  43209711458 now.");
            //stats
            req.TsDeparture = Cycles;
            req.Latency = (int)(req.TsDeparture - req.TsArrival);
            //enqueue proper
            Reqs.Add(req);
            //if (SIM.Cycle < 1000)
            //    DEBUG.Print($"\t\t\t => XBAR.Enqueue({req.BlockAddress})");
        }

        public void Tick()
        {
            Cycles++;

            int sent = 0;
            foreach (Request req in Reqs)
            {
                //if (req.BlockAddress == 99814114965)
                //    System.Diagnostics.Debug.Print($"\t\t\t => 99814114965: Test if " +
                //        $"(Cycles: {Cycles} - req.TsDeparture: {req.TsDeparture} < xbar_latency: {SIM.Param.XBAR_LATENCY})" +
                //        $" => {Cycles - req.TsDeparture < SIM.Param.XBAR_LATENCY}");

                if (Cycles - req.TsDeparture < SIM.Param.XBAR_LATENCY) break;

                //if (req.BlockAddress == 99814114965)
                //    System.Diagnostics.Debug.Print($"\t\t\t => 99814114965: callback called at cycle {Cycles}");

                // Send back to processor

                if (req.BlockAddress == 112332342044)
                    DEBUG.Print($"Cycle: {Cycles} - Callback for 112332342044 now.");
 
                if (req.BlockAddress == 43209711458)
                    DEBUG.Print($"Cycle: {Cycles} - Callback for 43209711458 now.");


                sent += 1;
                CallBack cb = req.CALLBACK;
                cb(req);
                //if (SIM.Cycle < 1000)
                //    DEBUG.Print($"\t\t\t => XBAR.callback({req.BlockAddress})");
            }
            Reqs.RemoveRange(0, sent);
        }
    }
}
