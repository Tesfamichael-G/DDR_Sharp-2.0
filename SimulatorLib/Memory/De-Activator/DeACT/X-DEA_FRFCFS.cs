//using SimulatorLib.Common;
//using SimulatorLib.MemCtrl;
//using SimulatorLib.Memory.Schedule;
//using System;
//using System.Collections.Generic;

//namespace SimulatorLib.DeActivator;

//public class X_DEA_FRFCFS : IScheduler
//{
//    private MemoryController MC;

//    int NUM_RANKS;
//    int NUM_BANK_GROUPS;
//    int NUM_BANKS;

//    private bool cycle_debug => MC.Cycle < 3500; // (MC.Cycle > 333215 && MC.Cycle < 333396);
//    string fn = @"C:\Users\tmik\Desktop\DeActivator\Workspace\output\" + $"FRFCFS({DateTime.Now.Minute}-{DateTime.Now.Second}).txt";
//    System.IO.StreamWriter sw;
//    public X_DEA_FRFCFS(MemoryController mc)
//    {
//        MC = mc;

//        NUM_RANKS = mc.PARAM.NUM_RANKS;
//        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
//        NUM_BANKS = mc.PARAM.NUM_BANKS;
//        sw = new System.IO.StreamWriter(fn);

//    }

//    void IssueReadRequest()
//    {
//        Request rx = FIND_RD_REQ();

//        if (rx == null)
//        {
//            ///// if (cycle_debug) DEBUG.Print($"*** {MC.Cycle} FRFCFS: Selected *** 0/{MC.CHANNEL.QUEUE.Length} ***.");                   
//            //(int RL, int CL, int WL) = (MC.PARAM.RRDL, MC.PARAM.CCDL, MC.PARAM.WTRL);
//            //(int RS, int CS, int WS) = (MC.PARAM.RRDS, MC.PARAM.CCDS, MC.PARAM.WTRS);
//            //DEBUG.Print($"{MC.Cycle} FRFCFS:[READ] | <NULL>/{MC.CHANNEL.QUEUE.WR_Queued} *** | (RRD,CCD,WTR)[L|S]=>({RL},{CL},{WL})|({RS},{CS},{WS}).");
//            return;
//        }

//        DEBUG.Assert(rx.CanBeIssued);

//        //DEBUG.Print($"\t\t**** [{rx.MemAddr.Rank}{rx.MemAddr.BankGroup}{rx.MemAddr.Bank}] | READ.CAN_ISSUE => {rx.CanBeIssued}");

//        MC.ISSUE(rx);

//    }

//    void IssueWriteRequest()
//    {
//        Request rx = FIND_WR_REQ();


//        if (rx == null)
//        {
//            //(int RL, int CL, int WL) = (MC.PARAM.RRDL, MC.PARAM.CCDL, MC.PARAM.WTRL);
//            //(int RS, int CS, int WS) = (MC.PARAM.RRDS, MC.PARAM.CCDS, MC.PARAM.WTRS);
//            //DEBUG.Print($"{MC.Cycle} FRFCFS:[WRITE] | <NULL>/{MC.CHANNEL.QUEUE.WR_Queued} *** | (RRD,CCD,WTR)[L|S]=>({RL},{CL},{WL})|({RS},{CS},{WS}).");
//            return;
//        }

//        DEBUG.Assert(rx.CanBeIssued);

//        //DEBUG.Print($"\t\t**** [{rx.MemAddr.Rank}{rx.MemAddr.BankGroup}{rx.MemAddr.Bank}] | WRITE.CAN_ISSUE => {rx.CanBeIssued}");
//        MC.WRITE_MODE.AlertWriteMode(COMMAND.WR);  // alert writeback mode

//        MC.ISSUE(rx);
//        //DEBUG.Print($"*** {MC.Cycle} FRFCFS.DrainWrites: {MC.WRITE_MODE.DrainWrites} | rx.CMD => {rx.CMD}.");
//    }

//    void IssueRefresh()
//    {
//        REF_INFO rx = FIND_REF_REQ();

//        //DEBUG.Print($"\t\t**** [{rx.RANK}{rx.GROUP}{rx.BANK}] | REF.CAN_ISSUE => {rx.CAN_ISSUE}");

//        if (!rx.CAN_ISSUE)
//            return;


//        if (rx.BANK_IS_OPEN)
//        {
//            MC.CLOSE_BANK(rx.RANK, rx.GROUP, rx.BANK);
//            MC.QUEUE.REFS[rx.RANK].BANK_IS_OPEN = false;
//        }
//        else
//        {
//            MC.REF_RANK(rx.RANK);
//            MC.QUEUE.REFS[rx.RANK].NEEDS_REFRESH = false;
//        }
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

//    public Request FindEarliest(List<Request> rq, int iCnt)
//    {

//        Request bestReq = rq[0];

//        for (int i = 1; i < iCnt; i++)
//        {
//            if (bestReq.TsArrival > rq[i].TsArrival)
//                bestReq = rq[i];
//        }

//        bestReq.CMD = COMMAND.ACT;
//        bestReq.CanBeIssued = MC.CHANNEL.CanActivate(bestReq.MemAddr);

//        return bestReq;

//    }

//    public Request FindHotOrEarliestREAD(List<Request> rq, int iCnt, long ACTIVE_ROW, int ID = -1)
//    {
//        Request bestReq = rq[0];

//        for (int i = 1; i < iCnt; i++)
//        {
//            bestReq = HOE(bestReq, rq[i]);
//        }

//        if (bestReq.MemAddr.Row == ACTIVE_ROW)
//        {
//            bestReq.CMD =  COMMAND.RD;
//            bestReq.CanBeIssued = MC.CHANNEL.CanRead(bestReq.MemAddr);
//        }
//        else
//        {
//            bestReq.CMD = COMMAND.PRE;
//            bestReq.CanBeIssued = MC.CHANNEL.CanPRE(bestReq.MemAddr);
//        }

//        if (MC.Cycle == 1716)
//        {
//            sw.WriteLine($" *** BANK({bestReq.MemAddr.Bank}) Best REQ => {bestReq.BlockAddress}");
//        }

//        return bestReq;

//        Request HOE(Request req1, Request req2)
//        {
//            bool hit1 = (req1.MemAddr.Row == ACTIVE_ROW);
//            bool hit2 = (req2.MemAddr.Row == ACTIVE_ROW);
//            {
//                if (hit1 ^ hit2)
//                {
//                    if (hit1)
//                    {
//                        if (MC.Cycle == 1716)
//                        {
//                            sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} *** HOT");
//                        }
//                        return req1;
//                    }
//                    else
//                    {
//                        if (MC.Cycle == 1716)
//                        {
//                            sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} *** HOT");
//                        }
//                        return req2;
//                    }
//                }
//                //return hit1 ? req1 : req2;
//                else
//                {
//                    if (req1.TsArrival <= req2.TsArrival)
//                    {
//                        if (MC.Cycle == 1716)
//                        {
//                            sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} *** EARLIER");
//                        }
//                        return req1;
//                    }
//                }
//                if (MC.Cycle == 1716)
//                {
//                    sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} *** EARLIER");
//                }
//                return req2;
//            }
//        }
//    }
//    public Request FindHotOrEarliestWRITE(List<Request> rq, int iCnt, long ACTIVE_ROW, int ID = -1)
//    {
//        Request bestReq = rq[0];

//        for (int i = 1; i < iCnt; i++)
//        {
//            bestReq = HOE(bestReq, rq[i]);
//        }

//        if (bestReq.MemAddr.Row == ACTIVE_ROW)
//        {
//            bestReq.CMD = COMMAND.WR;
//            bestReq.CanBeIssued = MC.CHANNEL.CanWrite(bestReq.MemAddr);
//        }
//        else
//        {
//            bestReq.CMD = COMMAND.PRE;
//            bestReq.CanBeIssued = MC.CHANNEL.CanPRE(bestReq.MemAddr);
//        }

//        if (MC.Cycle == 1716)
//        {
//            sw.WriteLine($" *** BANK({bestReq.MemAddr.Bank}) Best REQ => {bestReq.BlockAddress}");
//        }

//        return bestReq;

//        Request HOE(Request req1, Request req2)
//        {
//            bool hit1 = (req1.MemAddr.Row == ACTIVE_ROW);
//            bool hit2 = (req2.MemAddr.Row == ACTIVE_ROW);
//            {
//                if (hit1 ^ hit2)
//                {
//                    if (hit1)
//                    {
//                        if (MC.Cycle == 1716)
//                        {
//                            sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} *** HOT");
//                        }
//                        return req1;
//                    }
//                    else
//                    {
//                        if (MC.Cycle == 1716)
//                        {
//                            sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} *** HOT");
//                        }
//                        return req2;
//                    }
//                }
//                //return hit1 ? req1 : req2;
//                else
//                {
//                    if (req1.TsArrival <= req2.TsArrival)
//                    {
//                        if (MC.Cycle == 1716)
//                        {
//                            sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} *** EARLIER");
//                        }
//                        return req1;
//                    }
//                }
//                if (MC.Cycle == 1716)
//                {
//                    sw.WriteLine($" *** HotOrEarliest({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} *** EARLIER");
//                }
//                return req2;
//            }
//        }
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
//        if (MC.Cycle == 1716)
//        {
//            sw.WriteLine($"\n ============= BANK({req1.MemAddr.Bank}) VS BANK({req2.MemAddr.Bank}) =============\n");
//        }

//        bool hit1 = (req1.CMD == COMMAND.RD || req1.CMD == COMMAND.WR);
//        bool hit2 = (req2.CMD == COMMAND.RD || req2.CMD == COMMAND.WR);
//        {
//            if (hit1 ^ hit2)
//            {
//                if (hit1)
//                {
//                    if (MC.Cycle == 1716)
//                    {
//                        sw.WriteLine($" *** HotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} *** HOT");
//                    }
//                    return req1;
//                }
//                else
//                {
//                    if (MC.Cycle == 1716)
//                    {
//                        sw.WriteLine($" *** HotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} *** HOT");
//                    }
//                    return req2;
//                }
//                //return hit1 ? req1 : req2;
//            }
//            else
//            {
//                if (req1.TsArrival <= req2.TsArrival)
//                {
//                    if (MC.Cycle == 1716)
//                    {
//                        sw.WriteLine($" *** HotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} *** EARLIER");
//                    }
//                    return req1;
//                }
//            }
//            {
//                if (MC.Cycle == 1716)
//                {
//                    sw.WriteLine($" *** HotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} *** EARLIER");
//                }
//                return req2;
//            }
//        }
//    }

//    Request FIND_RD_REQ()
//    {

//        Request bestReq = null;

//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            if (((DEA_Rank)MC.CHANNEL.Ranks[R]).IsRefreshing)
//                continue;

//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {

//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    DEA_Bank BANK = (DEA_Bank)MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

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
//                            req = FindEarliest(MC.QUEUE.READS[R][G][B], RD_Count);
//                            break;
//                        case State.MULTIPLE_ACTIVE_ROWS:
//                        case State.ACTIVE:
//                            req = FindHotOrEarliestREAD(MC.QUEUE.READS[R][G][B], RD_Count, BANK.ActiveRow, BANK.ID);
//                            break;
//                        default:
//                            break;
//                    }


//                    if (req == null)
//                    {
//                        //DEBUG.Print($"{MC.Cycle} [{BANK.STATE}] | " +
//                        //    $"[FIND_READ({req.MemAddr.Rank}{req.MemAddr.BankGroup}{req.MemAddr.Bank})] | " +
//                        //    $" => NULL | continue ({R}{G}{B});");
//                        continue;
//                    }

//                    //(int r, int g, int b) = (req.MemAddr.Rank, req.MemAddr.BankGroup, req.MemAddr.Bank);
//                    //DEBUG.Print($"*** {MC.Cycle} FRFCFS.BestSoFar: {req.CMD}({req.BlockAddress}[{r}{g}{b}]).Can_be_Issued => {req.CanBeIssued}.");

//                    if (!req.CanBeIssued)
//                    {
//                        //DEBUG.Print($"{MC.Cycle} " +
//                        //    $"[FIND_READ({req.MemAddr.Rank}{req.MemAddr.BankGroup}{req.MemAddr.Bank})] | " +
//                        //    $"{req.BlockAddress} Can not be issued. continue ({R}{G}{B});");
//                        if (MC.Cycle == 1716)
//                        {
//                            sw.WriteLine($" *** TIMING BANK[{B}]: {req.BlockAddress} can not be issued.");
//                        }
//                        continue;
//                    }

//                    if (bestReq == null)
//                    {
//                        bestReq = req;
//                        //if (bestReq != null)
//                        //    DEBUG.Print($"{MC.Cycle}: [FIND_READ(Best So Far)] | {bestReq.BlockAddress}[{bestReq.MemAddr.Rank}{bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}]. continue ({R}{G}{B});");
//                        continue;
//                    }

//                    bestReq = HotOrEarlier(bestReq, req);
//                    //if (bestReq != null)
//                    //    DEBUG.Print($"{MC.Cycle}: [FIND_READ(Best So Far)] | {bestReq.BlockAddress}[{bestReq.MemAddr.Rank}{bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}]. Step: ({R}{G}{B});");

//                }
//            }
//        }

//        //if (bestReq != null)
//        //    DEBUG.Print($"{MC.Cycle}: [FIND_READ(Best)] | {bestReq.BlockAddress}[{bestReq.MemAddr.Rank}{bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}].");

//        return bestReq;

//    }
//    private void PrintInfo()
//    {

//        if (MC.Cycle == 1716)
//        {

//        }

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
//                    DEA_Bank BANK = (DEA_Bank)MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

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
//                            req = FindEarliest(MC.QUEUE.WRITES[R][G][B], WR_Count);
//                            break;
//                        case State.MULTIPLE_ACTIVE_ROWS:
//                        case State.ACTIVE:
//                            req = FindHotOrEarliestWRITE(MC.QUEUE.WRITES[R][G][B], WR_Count, BANK.ActiveRow, BANK.ID);
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

//    //DeActChannel CHANNEL => (DeActChannel)MC.CHANNEL;

//    const long CYCLES = 50 * 1000 * 1000;

//    REF_INFO FIND_REF_REQ()
//    {
//        if (MC.CHANNEL.Cycle > CYCLES && MC.CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
//            DEBUG.Print($"{MC.Cycle}: UPDATING ... REF/PRE.");

//        for (int R = 0; R < NUM_RANKS; R++)
//        {
//            if (!MC.QUEUE.REFS[R].NEEDS_REFRESH)
//                continue;

//            for (int G = 0; G < NUM_BANK_GROUPS; G++)
//            {
//                for (int B = 0; B < NUM_BANKS; B++)
//                {
//                    DEA_Bank BANK = (DEA_Bank)MC.CHANNEL.Ranks[R].Groups[G].Banks[B];
//                    if (BANK.IsBusy)
//                        continue;

//                    if (BANK.STATE == State.ACTIVE)
//                    {
//                        if (MC.CHANNEL.Cycle > CYCLES && MC.CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
//                            DEBUG.Print($"{MC.Cycle}: STATE({R}{G}{B}) => {BANK.STATE}.");
//                        MC.QUEUE.REFS[R].CMD = COMMAND.PRE;
//                        MC.QUEUE.REFS[R].RANK = R;
//                        MC.QUEUE.REFS[R].GROUP = G;
//                        MC.QUEUE.REFS[R].BANK = B;
//                        MC.QUEUE.REFS[R].BANK_IS_OPEN = true;
//                        MC.QUEUE.REFS[R].CAN_ISSUE = MC.CHANNEL.Ranks[R].CanPRE(G, B);

//                        if (MC.CHANNEL.Cycle > CYCLES && MC.CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
//                            DEBUG.Print($"{MC.Cycle}: RANK({R}{G}{B}) CAN PRE => {MC.QUEUE.REFS[R].CAN_ISSUE}.");

//                        return MC.QUEUE.REFS[R];
//                    }
//                }
//            }

//            //ALL BANKS are Idle. Try Refresh
//            MC.QUEUE.REFS[R].RANK = R;
//            MC.QUEUE.REFS[R].CMD = COMMAND.REF;
//            MC.QUEUE.REFS[R].BANK_IS_OPEN = false;
//            MC.QUEUE.REFS[R].CAN_ISSUE = MC.CHANNEL.Ranks[R].CAN_REF_RANK();

//            if (MC.CHANNEL.Cycle > CYCLES && MC.CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
//                DEBUG.Print($"{MC.Cycle}: RANK({R}) CAN REF => {MC.QUEUE.REFS[R].CAN_ISSUE}.");
//            return MC.QUEUE.REFS[R];

//        }

//        if (MC.CHANNEL.Cycle > CYCLES && MC.CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
//            DEBUG.Print($"{MC.Cycle}: RETURNING RANK(-1) FOR REF => NULL.");
//        return REF_INFO.NULL; ;
//    }

//    public void CloseFile()
//    {
//        sw.Close();
//    }


//}