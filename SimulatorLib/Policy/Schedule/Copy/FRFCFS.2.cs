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

//    private bool cycle_debug => MC.Cycle < 3500; // (MC.Cycle > 333215 && MC.Cycle < 333396);
//    public FRFCFS(MemoryController mc)
//    {
//        MC = mc;

//        NUM_RANKS = mc.PARAM.NUM_RANKS;
//        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
//        NUM_BANKS = mc.PARAM.NUM_BANKS;

//    }

//    #region copy
//    //void IssueReadRequest()
//    //{
//    //    Request rx = TryREAD();


//    //    if (rx == null)
//    //    {
//    //        ///// if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** 0/{MC.CHANNEL.QUEUE.Length} ***.");                   
//    //        return;
//    //    }

//    //    DEBUG.Assert(rx.CanBeIssued);


//    //    MC.ISSUE(rx);
//    //    //DEBUG.Print($"*** {MC.Cycle} FRFCFS.DrainWrites: {MC.WRITE_MODE.DrainWrites} | rx.CMD => {rx.CMD}.");
//    //}
//    //void IssueWriteRequest()
//    //{
//    //    Request rx = TryWRITE();


//    //    if (rx == null)
//    //    {
//    //        ///// if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** 0/{MC.CHANNEL.QUEUE.Length} ***.");                   
//    //        return;
//    //    }

//    //    DEBUG.Assert(rx.CanBeIssued);

//    //    MC.WRITE_MODE.issued_write_cmd(COMMAND.WR);  // alert writeback mode

//    //    MC.ISSUE(rx);
//    //    //DEBUG.Print($"*** {MC.Cycle} FRFCFS.DrainWrites: {MC.WRITE_MODE.DrainWrites} | rx.CMD => {rx.CMD}.");
//    //} 
//    #endregion

//    void IssueReadRequest()
//    {
//        Request rx = FIND_RD_REQ();


//        if (rx == null)
//        {
//            ///// if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** 0/{MC.CHANNEL.QUEUE.Length} ***.");                   
//            return;
//        }

//        DEBUG.Assert(rx.CanBeIssued);


//        MC.ISSUE(rx);
//        //DEBUG.Print($"*** {MC.Cycle} FRFCFS.DrainWrites: {MC.WRITE_MODE.DrainWrites} | rx.CMD => {rx.CMD}.");
//    }
//    void IssueWriteRequest()
//    {
//        Request rx = FIND_WR_REQ();


//        if (rx == null)
//        {
//            ///// if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** 0/{MC.CHANNEL.QUEUE.Length} ***.");                   
//            return;
//        }

//        DEBUG.Assert(rx.CanBeIssued);

//        MC.WRITE_MODE.issued_write_cmd(COMMAND.WR);  // alert writeback mode

//        MC.ISSUE(rx);
//        //DEBUG.Print($"*** {MC.Cycle} FRFCFS.DrainWrites: {MC.WRITE_MODE.DrainWrites} | rx.CMD => {rx.CMD}.");
//    }
//    void IssueRefresh()
//    {
//        REF_INFO  rx = FIND_REF_REQ();

//        if (!rx.CAN_ISSUE)
//            return;


//        if (rx.BANK_IS_OPEN)
//        {
//            MC.CLOSE_BANK(rx.RANK, rx.GROUP, rx.BANK);
//            MC.QUEUE.REFS[rx.RANK].BANK_IS_OPEN=false;
//        }
//        else
//            MC.REF_RANK(rx.RANK);

//        //DEBUG.Print($"*** {MC.Cycle} FRFCFS.DrainWrites: {MC.WRITE_MODE.DrainWrites} | rx.CMD => {rx.CMD}.");
//    }
//    public void Tick()
//    {

//        if (MC.CHANNEL.refresh_waiting)
//        {
//            IssueRefresh();
//            return;
//        }
       
//        if (MC.WRITE_MODE.DrainWrites)
//            IssueWriteRequest();
//        else
//            IssueReadRequest();

//    }
//    public Request FindEarliest(List<Request> rq)
//    {

//        if (rq == null) return (null);
//        if (rq.Count == 0) return (null);

//        Request bestReq = rq[0];

//        for (int i = 1; i < rq.Count; i++)
//        {
//            if (bestReq.TsArrival > rq[i].TsArrival)
//                bestReq = rq[i];
//        }

//        return bestReq;

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
//    public Request FindHotOrEarliest(List<Request> rq, int iCnt, long ACTIVE_ROW, COMMAND cmd)
//    {

//        Request bestReq = rq[0];

//        for (int i = 1; i < iCnt; i++)
//        {
//            bestReq = HOE(bestReq, rq[i]);
//        }

//        if (bestReq.MemAddr.Row == ACTIVE_ROW)
//        {
//            bestReq.CMD = cmd;
//            bestReq.CanBeIssued = MC.CHANNEL.CanRead(bestReq.MemAddr);
//        }
//        else
//        {
//            bestReq.CMD = COMMAND.PRE;
//            bestReq.CanBeIssued = MC.CHANNEL.CanPRE(bestReq.MemAddr);
//        }

//        return bestReq;

//        Request HOE(Request req1, Request req2)
//        {
//            bool hit1 = (req1.MemAddr.Row == ACTIVE_ROW);
//            bool hit2 = (req2.MemAddr.Row == ACTIVE_ROW);
//            {
//                if (hit1 ^ hit2)
//                    return hit1 ? req1 : req2;
//                else
//                {
//                    if (req1.TsArrival <= req2.TsArrival)
//                        return req1;
//                }
//                return req2;
//            }
//        }
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

//    Request FIND_RD_REQ()
//    {

//        Request bestReq = null;

//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            if (MC.CHANNEL.Ranks[R].IsRefreshing)
//                continue;

//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {

//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

//                    if (BANK.IsBusy)
//                    {
//                        continue;
//                    }

//                    int RD_Count = MC.QUEUE.READS[R][G][B].Count;

//                    if (RD_Count == 0)
//                        continue;

//                    Request req = null;

//                    switch (BANK.STATE)
//                    {
//                        case State.IDLE:
//                            req = FindEarliest(MC.QUEUE.READS[R][G][B]);
//                            break;
//                        case State.MULTIPLE_ACTIVE_ROWS:
//                        case State.ACTIVE:
//                            req = FindHotOrEarliest(MC.QUEUE.READS[R][G][B], RD_Count, BANK.ActiveRow, COMMAND.RD);
//                            break;
//                        default:
//                            break;
//                    }


//                    if (req == null) continue;


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

//    Request FIND_WR_REQ()
//    {

//        Request bestReq = null;

//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            if (MC.CHANNEL.Ranks[R].IsRefreshing)
//                continue;

//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {

//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

//                    if (BANK.IsBusy)
//                    {
//                        continue;
//                    }

//                    int WR_Count = MC.QUEUE.WRITES[R][G][B].Count;

//                    if (WR_Count == 0)
//                        continue;

//                    Request req = null;

//                    switch (BANK.STATE)
//                    {
//                        case State.IDLE:
//                            req = FindEarliest(MC.QUEUE.WRITES[R][G][B]);
//                            break;
//                        case State.MULTIPLE_ACTIVE_ROWS:
//                        case State.ACTIVE:
//                            req = FindHotOrEarliest(MC.QUEUE.WRITES[R][G][B], WR_Count, BANK.ActiveRow, COMMAND.WR);
//                            break;
//                        default:
//                            break;
//                    }


//                    if (req == null) continue;


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

//    REF_INFO FIND_REF_REQ()
//    {
//        DEBUG.Print($"{MC.Cycle}: UPDATING ... REF/PRE.");

//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            if (!MC.QUEUE.REFS[R].NEEDS_REFRESH)
//                continue;

//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];
//                    if (BANK.IsBusy)
//                        continue;

//                    if (BANK.STATE == State.ACTIVE)
//                    {
//                        DEBUG.Print($"{MC.Cycle}: STATE({R}{G}{B}) => {BANK.STATE}.");
//                        MC.QUEUE.REFS[R].CMD = COMMAND.PRE;
//                        MC.QUEUE.REFS[R].RANK = R;
//                        MC.QUEUE.REFS[R].GROUP = G;
//                        MC.QUEUE.REFS[R].BANK = B;
//                        MC.QUEUE.REFS[R].BANK_IS_OPEN = true;
//                        MC.QUEUE.REFS[R].CAN_ISSUE = MC.CHANNEL.Ranks[R].CanPRE(G, B);

//                        DEBUG.Print($"{MC.Cycle}: RANK({R}{G}{B}) CAN PRE => {MC.QUEUE.REFS[R].CAN_ISSUE}.");

//                        return MC.QUEUE.REFS[R];
//                    }
//                }
//            }

//            //ALL BANKS are Idle. Try Refresh
//            MC.QUEUE.REFS[R].CMD = COMMAND.REF;
//            MC.QUEUE.REFS[R].BANK_IS_OPEN = false;
//            MC.QUEUE.REFS[R].CAN_ISSUE = MC.CHANNEL.Ranks[R].CAN_REF_RANK();
//            MC.QUEUE.REFS[R].NEEDS_REFRESH = false;
//            DEBUG.Print($"{MC.Cycle}: RANK({R}) CAN REF => {MC.QUEUE.REFS[R].CAN_ISSUE}.");
//            return MC.QUEUE.REFS[R];

//        }
//        DEBUG.Print($"{MC.Cycle}: RETURNING RANK(-1) FOR REF => NULL.");
//        return REF_INFO.NULL; ;
//    }

//    Request TryREAD()
//    {

//        //if (MC.CHANNEL.CanNotRead)
//        //{
//        //    //DEBUG.Print($"{MC.Cycle}: Channel cannot read.\n\t\t{MC.Cycle} > {MC.CHANNEL.NextRD}");
//        //    return null;
//        //}

//        Request bestReq = null;
//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            if (MC.CHANNEL.Ranks[R].IsRefreshing)
//                continue;
//            //if (MC.CHANNEL.Ranks[R].CanNotRead_Act)
//            //{
//            //    //DEBUG.Print($"{MC.Cycle}: *** Rank *** " +
//            //    //                         $"\n\t\t - cannot read.{MC.Cycle} > {MC.CHANNEL.Ranks[R].NextRD}" +
//            //    //                         $"\n\t\t - cannot act. {MC.Cycle} > {MC.CHANNEL.Ranks[R].NextACT}" +
//            //    //                         $"\n\t\t - cannot faw act. {MC.Cycle} > {MC.CHANNEL.Ranks[R].NextFAW}");
//            //    continue;
//            //}

//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                //if (MC.CHANNEL.Ranks[R].Groups[G].CanNotRead_Act)
//                //{
//                //    //DEBUG.Print($"{MC.Cycle}: *** Bank Group[{G}] *** " +
//                //    // $"\n\t\t - cannot read.{MC.Cycle} > {MC.CHANNEL.Ranks[R].Groups[G].NextRD}" +
//                //    // $"\n\t\t - cannot act. {MC.Cycle} > {MC.CHANNEL.Ranks[R].Groups[G].NextACT}");
//                //    continue;
//                //}
//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

//                    if (BANK.IsBusy)
//                    {
//                        //DEBUG.Print($"{MC.Cycle}: *** Bank[{B}] is bussy.*** ");
//                        continue;
//                    }

//                    Request req = FindHotOrEarliest(MC.QUEUE.READS[R][G][B]);

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

//    Request TryWRITE()
//    {
//        //if (MC.CHANNEL.CanNotWrite)
//        //    return null;

//        Request bestReq = null;
//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            //if (MC.CHANNEL.Ranks[R].CanNotWrite_Act)
//            //    return null ;
//            if (MC.CHANNEL.Ranks[R].IsRefreshing)
//                continue;

//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                //if (MC.CHANNEL.Ranks[R].Groups[G].CanNotWrite_Act)
//                //    return null ;

//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

//                    if (BANK.IsBusy)
//                        continue;

//                    Request req = FindHotOrEarliest(MC.QUEUE.WRITES[R][G][B]);

//                    if (req == null) continue;

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

//    void IssueRefresh2()
//    {

//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            if (MC.QUEUE.REFS[R].NEEDS_REFRESH && MC.QUEUE.REFS[R].CAN_ISSUE)
//            {
//                if (MC.QUEUE.REFS[R].BANK_IS_OPEN)
//                {
//                    MC.CLOSE_BANK(R, MC.QUEUE.REFS[R].GROUP, MC.QUEUE.REFS[R].BANK);
//                    MC.QUEUE.REFS[R].BANK_IS_OPEN = false;
//                }
//                else
//                    MC.REF_RANK(R);

//                //MC.QUEUE.REFS[R].NEEDS_REFRESH = MC.QUEUE.REFS[R].CAN_ISSUE = false;
//                DEBUG.Print($"{MC.Cycle}: Try REF OK");
//            }
//            else
//                DEBUG.Print($"{MC.Cycle}: RANK({R}) CAN NOT ISSUE REF/PRE.");
//        }
//    }

//    #region BestREQ

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

//    #endregion

//}

