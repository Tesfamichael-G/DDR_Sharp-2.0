//using SimulatorLib.Common;
//using SimulatorLib.CPU;
//using SimulatorLib.DRAM;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace SimulatorLib.Memory.Schedule;

//public class FRFCFS : IScheduler
//{
//    private MemoryController MC;
//    int NUM_RANKS;
//    int NUM_BANKS;
//    int NUM_BANK_GROUPS;
//    int NUM_BANKS_PER_GROUP;

//    int BANK_GROUP_BITS;

//    private bool cycle_debug => MC.Cycle < 3500; // (MC.Cycle > 333215 && MC.Cycle < 333396);
//    public FRFCFS(MemoryController mc) 
//    {
//        MC = mc;

//        NUM_RANKS = mc.PARAM.NUM_RANKS;
//        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
//        NUM_BANKS_PER_GROUP = mc.PARAM.NUM_BANKS;
//        NUM_BANKS = NUM_BANKS_PER_GROUP * NUM_BANK_GROUPS;

//        BANK_GROUP_BITS = mc.PARAM.bankGBits;

//    }

//    public void Tick()
//    {
        
//        Request rx = MC.WRITE_MODE.DrainWrites ? best_write_req() : best_read_req() ;


//        if (rx == null)
//        {
//           ///// if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** 0/{MC.CHANNEL.QUEUE.Length} ***.");                   
//            return;
//        }

//        if (!rx.CanBeIssued)
//        {
//            if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** un-issuable request ***.");
//            return;
//        }
//        if (cycle_debug)
//        {
//            if (MC.Cycle == 333240) MC.QUEUE.PrintRQStatus();
//            if (rx.MemAddr.Bank==2)
//                DEBUG.Print($"*** {MC.Cycle}: FRFCFS SELECTED  *** {rx.BlockAddress}" +
//                        $"[00{rx.MemAddr.Bank}, {rx.MemAddr.Row}, {rx.MemAddr.Column}] for {rx.CMD} *** ");
//        }

//        if (rx.CMD == COMMAND.WRITE)   MC.WRITE_MODE.issued_write_cmd(COMMAND.WRITE);  // alert writeback mode

//        MC.ISSUE(rx);
//    }

//        //***********************Abstract**************************

//    public bool is_row_hit(Request req)
//    {
//        return (req.CMD == COMMAND.READ || req.CMD == COMMAND.WRITE);
//    }

//    //public (Request, string) FindHotOrEarliest(List<Request> rq)
//    //{
//    //    //DEBUG.Print($"************** HotOrEarliest **************");
//    //    if (rq == null) return (null, string.Empty);
//    //    if (rq.Count == 0) return (null, string.Empty);

//    //    Request bestReq = rq[0];
//    //    string s = string.Empty;
//    //    for (int i = 1; i < rq.Count; i++)
//    //    {
//    //        (bestReq, s) = XHotOrEarlier(bestReq, rq[i]);
//    //    }

//    //    return ( bestReq, s);

//    //}
//    public Request FindHotOrEarliest(List<Request> rq)
//    {

//        if (rq == null) return (null );
//        if (rq.Count == 0) return (null );

//        Request bestReq = rq[0];

//        for (int i = 1; i < rq.Count; i++)
//        {
//            bestReq = XHotOrEarlier(bestReq, rq[i]);
//        }

//        return bestReq;

//    }

//    //public Request best_read_req()
//    //{

//    //    Request bestReq = null;
//    //    string s = string.Empty;

//    //    #region Backup
//    //    //for (int i = 0; i < MC.ReadQueue.Count; i++)
//    //    //{

//    //    //    Request REQ = MC.ReadQueue[i];

//    //    //    if (REQ.IsServed) continue;

//    //    //    if (!REQ.CanBeIssued) continue;

//    //    //    if (bestReq == null)
//    //    //        bestReq = REQ;
//    //    //    else
//    //    //        (bestReq,s) = XHotOrEarlier(bestReq, REQ);

//    //    //}

//    //    //if (cycle_debug)
//    //    //    DEBUG.Print($"\t- Best Read Request {bestReq?.BlockAddress} [{s}]");

//    //    //return bestReq;

//    //    #endregion

//    //    int rCount = MC.CHANNEL.Ranks.Count;

//    //    for (int r = 0; r < rCount; r++)
//    //    {
//    //        int bCount = MC.CHANNEL.Ranks[r].Banks.Count;
//    //        for (int b = 0; b < bCount; b++)
//    //        {
//    //            Request req = null;

//    //            (req, s) = FindHotOrEarliest(MC.QUEUE.READS[r, b]);

//    //            if (req == null) continue;

//    //            if (cycle_debug)
//    //                DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//    //            if (!req.CanBeIssued) continue;

//    //            if (bestReq == null)
//    //            {
//    //                bestReq = req;
//    //                continue;
//    //            }
//    //            (bestReq, s) = XHotOrEarlier(bestReq, req);
//    //        }

//    //    }

//    //    return bestReq;

//    //}

//    //public Request best_write_req()
//    //{
//    //    Request bestReq = null;
//    //    string s = string.Empty;

//    //    #region backup
//    //    //for (int i = 0; i < MC.WriteQueue.Count; i++)
//    //    //{

//    //    //    Request REQ = MC.WriteQueue[i];

//    //    //    if (REQ.IsServed) continue;

//    //    //    if (!REQ.CanBeIssued) continue;

//    //    //    if (bestReq == null)
//    //    //        bestReq = REQ;
//    //    //    else
//    //    //    {
//    //    //        //string s = $"HotOrEarlier({ bestReq.BlockAddress}, { REQ.BlockAddress}) => ";
//    //    //        bestReq = HotOrEarlier(bestReq, REQ);
//    //    //        //if (MC.WRITE_MODE.DrainWrites)
//    //    //        //{
//    //    //        //    if (++x % 1000 == 0)
//    //    //        //    {
//    //    //        //        DEBUG.Print($"{s} => {bestReq.BlockAddress}");
//    //    //        //    }
//    //    //        //}

//    //    //    }

//    //    //}

//    //    #endregion

//    //    for (int r = 0; r < MC.CHANNEL.Ranks.Count; r++)
//    //    {
//    //        for (int b = 0; b < MC.CHANNEL.Ranks[r].Banks.Count; b++)
//    //        {
//    //            Request req = null;

//    //            (req, s) = FindHotOrEarliest(MC.QUEUE.WRITES[r, b]);

//    //            if (req == null) continue;

//    //            if (cycle_debug)
//    //                DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//    //            if (!req.CanBeIssued) continue;

//    //            if (bestReq == null)
//    //            {
//    //                bestReq = req;
//    //                continue;
//    //            }
//    //            (bestReq, s) = XHotOrEarlier(bestReq, req);
//    //        }

//    //    }

//    //    return bestReq;

//    //}

//    (Request, string) HotOrEarlier(Request req1, Request req2)
//    {

//        bool hit1 = (req1.CMD == COMMAND.READ || req1.CMD == COMMAND.WRITE);
//        bool hit2 = (req2.CMD == COMMAND.READ || req2.CMD == COMMAND.WRITE);

//        {
//            if (hit1 ^ hit2)  
//                return hit1 ? (req1, "Hit") : (req2, "Hit");
//            else
//            {
//                if (req1.TsArrival <= req2.TsArrival) 
//                    return (req1, "Earlier");

//            }

//            return (req2, "Earlier");

//        }
//    }

//    Request XHotOrEarlier(Request req1, Request req2)
//    {

//        bool hit1 = (req1.CMD == COMMAND.READ || req1.CMD == COMMAND.WRITE);
//        bool hit2 = (req2.CMD == COMMAND.READ || req2.CMD == COMMAND.WRITE);
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

//    #region BANK GROUPING

//    public Request best_read_req()
//    {

//        Request bestReq = null;
//        //string s = string.Empty;
//        for (int r = 0; r < NUM_RANKS; r++)
//        {
//            int b = 0;
//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                for (int B = 0; B < NUM_BANKS_PER_GROUP; B++, b++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[r].Groups[G].Banks[B];// .Banks[G][B];
//                    if (BANK.IsBusy)
//                        continue;

//                    Request req = FindHotOrEarliest(MC.QUEUE.READS[r][b]);

//                    if (req == null) continue;

//                    //if (cycle_debug)
//                    //    DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//                    if (!req.CanBeIssued) continue;

//                    if (bestReq == null)
//                    {
//                        bestReq = req;
//                        continue;
//                    }
//                    bestReq = XHotOrEarlier(bestReq, req);
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
//            int b = 0;
//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                for (int B = 0; B < NUM_BANKS_PER_GROUP; B++, b++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[r].Groups[G].Banks[B];// .Banks[G][B];
//                    if (BANK.IsBusy)
//                        continue;

//                    Request req = FindHotOrEarliest(MC.QUEUE.WRITES[r][b]);

//                    if (req == null) continue;

//                    //if (cycle_debug)
//                    //    DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//                    if (!req.CanBeIssued) continue;

//                    if (bestReq == null)
//                    {
//                        bestReq = req;
//                        continue;
//                    }
//                    bestReq = XHotOrEarlier(bestReq, req);
//                }
//            }
//        }
//        return bestReq;

//    }


//    #endregion

//    #region NO BANK GROUPING

//    //public Request best_read_req()
//    //{

//    //    Request bestReq = null;
//    //    //string s = string.Empty;

//    //    int rCount = MC.CHANNEL.Ranks.Count;

//    //    for (int r = 0; r < rCount; r++)
//    //    {
//    //        int bCount = MC.CHANNEL.Ranks[r].Banks.Count;
//    //        for (int b = 0; b < bCount; b++)
//    //        {
//    //            Bank BANK = MC.CHANNEL.Ranks[r].Banks[b];
//    //            if (BANK.IsBusy)
//    //                continue;

//    //            Request req = FindHotOrEarliest(MC.QUEUE.READS[r][b]);

//    //            if (req == null) continue;

//    //            //if (cycle_debug)
//    //            //    DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//    //            if (!req.CanBeIssued) continue;

//    //            if (bestReq == null)
//    //            {
//    //                bestReq = req;
//    //                continue;
//    //            }
//    //            bestReq = XHotOrEarlier(bestReq, req);
//    //        }

//    //    }

//    //    return bestReq;

//    //}

//    //public Request best_write_req()
//    //{
//    //    Request bestReq = null;
//    //    //string s = string.Empty;

//    //    for (int r = 0; r < MC.CHANNEL.Ranks.Count; r++)
//    //    {
//    //        for (int b = 0; b < MC.CHANNEL.Ranks[r].Banks.Count; b++)
//    //        {
//    //            Bank BANK = MC.CHANNEL.Ranks[r].Banks[b];
//    //            if (BANK.IsBusy)
//    //                continue;

//    //            Request req  = FindHotOrEarliest(MC.QUEUE.WRITES[r][b]);

//    //            if (req == null) continue;

//    //            //if (cycle_debug)
//    //            //    DEBUG.Print($"\t- Bank {b} Best Request {req?.BlockAddress} [{s}]");

//    //            if (!req.CanBeIssued) continue;

//    //            if (bestReq == null)
//    //            {
//    //                bestReq = req;
//    //                continue;
//    //            }
//    //            bestReq = XHotOrEarlier(bestReq, req);
//    //        }

//    //    }

//    //    return bestReq;

//    //}

//    #endregion

//}
