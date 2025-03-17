//using SimulatorLib.Common;
//using SimulatorLib.MemCtrl;
//using SimulatorLib.DDR;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace SimulatorLib.Memory.Schedule;

//public partial class FRFCFS : IScheduler
//{
//    private MemoryController MC;
//    int NUM_RANKS;
//    int NUM_BANK_GROUPS;
//    int NUM_BANKS;
//    int NUM_BANKS_TOTAL;


//    int BANK_GROUP_BITS;

//    private bool cycle_debug => MC.Cycle < 3500; // (MC.Cycle > 333215 && MC.Cycle < 333396);
//    public FRFCFS(MemoryController mc)
//    {
//        MC = mc;

//        NUM_RANKS = mc.PARAM.NUM_RANKS;
//        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
//        NUM_BANKS = mc.PARAM.NUM_BANKS;
//        NUM_BANKS_TOTAL = NUM_BANKS * NUM_BANK_GROUPS;

//        BANK_GROUP_BITS = mc.PARAM.bankGBits;

//    }

//    public void Tick()
//    {

//        Request rx = MC.WRITE_MODE.DrainWrites ? best_write_req() : best_read_req();


//        if (rx == null)
//        {
//            ///// if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** 0/{MC.CHANNEL.QUEUE.Length} ***.");                   
//            return;
//        }

//        if (!rx.CanBeIssued)
//        {
//            if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** un-issuable request ***.");
//            return;
//        }

//        if (rx.CMD == COMMAND.WR) MC.WRITE_MODE.issued_write_cmd(COMMAND.WR);  // alert writeback mode

//        MC.ISSUE(rx);
//    }

//    public bool is_row_hit(Request req)
//    {
//        return (req.CMD == COMMAND.RD || req.CMD == COMMAND.WR);
//    }

//    public Request FindHotOrEarliest(List<Request> rq)
//    {

//        if (rq == null) return (null);
//        if (rq.Count == 0) return (null);

//        Request bestReq = rq[0];

//        for (int i = 1; i < rq.Count; i++)
//        {
//            bestReq = HotOrEarlier(bestReq, rq[i]);
//        }

//        return bestReq;

//    }

//    Request HotOrEarlier(Request req1, Request req2)
//    {

//        bool hit1 = (req1.CMD == COMMAND.RD || req1.CMD == COMMAND.WR);
//        bool hit2 = (req2.CMD == COMMAND.RD || req2.CMD == COMMAND.WR);
//        {
//            if (hit1 ^ hit2)
//                return hit1 ? req1 : req2;
//            else
//            {
//                if (req1.TsArrival <= req2.TsArrival)
//                    return req1;

//            }

//            return req2;
//        }
//    }

//    public Request best_read_req()
//    {

//        Request bestReq = null;
//        //string s = string.Empty;
//        for (int r = 0; r < NUM_RANKS; r++)
//        {
//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[r].Groups[G].Banks[B];// .Banks[G][B];
//                    if (BANK.IsBusy)
//                        continue;

//                    Request req = FindHotOrEarliest(MC.QUEUE.READS[r][G][B]);

//                    if (req == null) continue;

//                    //if (cycle_debug)
//                    //    DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//                    if (!req.CanBeIssued) continue;

//                    if (bestReq == null)
//                    {
//                        bestReq = req;
//                        continue;
//                    }
//                    bestReq = HotOrEarlier(bestReq, req);
//                }
//            }
//        }

//        return bestReq;

//    }

//    public Request best_write_req()
//    {
//        Request bestReq = null;
//        //string s = string.Empty;
//        for (int r = 0; r < NUM_RANKS; r++)
//        {
//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[r].Groups[G].Banks[B];// .Banks[G][B];
//                    if (BANK.IsBusy)
//                        continue;

//                    Request req = FindHotOrEarliest(MC.QUEUE.WRITES[r][G][B]);

//                    if (req == null) continue;

//                    //if (cycle_debug)
//                    //    DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//                    if (!req.CanBeIssued) continue;

//                    if (bestReq == null)
//                    {
//                        bestReq = req;
//                        continue;
//                    }
//                    bestReq = HotOrEarlier(bestReq, req);
//                }
//            }
//        }
//        return bestReq;

//    }


//}
