using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using SimulatorLib.DDR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimulatorLib.Memory.Schedule;

public partial class FRFCFS : IScheduler
{
    public void RobStalled(long addr)
    {
        stall_addr = addr;
        DEBUG.Print($"\n*** *** STALL: {addr} *** ***\n");
    }

    private long stall_addr = -1;
    private System.Text.StringBuilder sb = new();
    private bool Stalled => (stall_addr != -1);
    private void PrintStallInfo()
    {
        if (!Stalled) return;

        DEBUG.Print($"Cycle: {MC.Cycle}");
        DEBUG.Print($"~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        //DEBUG.Print(sb.ToString());
        stall_addr = -1;
        sb.Clear();
    }

    private MemoryController MC;

    int NUM_RANKS;
    int NUM_BANK_GROUPS;
    int NUM_BANKS;

    public FRFCFS(MemoryController mc)
    {
        MC = mc;

        NUM_RANKS = mc.PARAM.NUM_RANKS;
        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
        NUM_BANKS = mc.PARAM.NUM_BANKS;

    }

    void IssueReadRequest()
    {
        Request rx = FIND_RD_REQ();

        if (rx == null)
        {
            return;
        }

        DEBUG.Assert(rx.CanBeIssued);

        MC.ISSUE(rx);

    }

    void IssueWriteRequest()
    {
        Request REQ = FIND_WR_REQ();


        if (REQ == null)
        {
            return;
        }

        DEBUG.Assert(REQ.CanBeIssued);

        if (REQ.CMD == COMMAND.WR) MC.WRITE_MODE.AlertWriteMode(COMMAND.WR);  // alert writeback mode

        MC.ISSUE(REQ);
    }

    void IssueRefresh()
    {
        REF_INFO REQ = FIND_REF_REQ();

        if (!REQ.CAN_ISSUE)
            return;

        if (REQ.BANK_IS_OPEN)
        {
            MC.CLOSE_BANK(REQ.RANK, REQ.GROUP, REQ.BANK);
            MC.QUEUE.REFS[REQ.RANK].BANK_IS_OPEN = false;
        }
        else
        {
            MC.REF_RANK(REQ.RANK);
            MC.QUEUE.REFS[REQ.RANK].NEEDS_REFRESH = false;
        }
    }

    public void Tick()
    {

        if (MC.CHANNEL.refresh_waiting)
        {
            IssueRefresh();
            return;
        }

        if (MC.WRITE_MODE.DrainWrites)
            IssueWriteRequest();
        else
            IssueReadRequest();

    }

    public Request FindEarliest(List<Request> rq, int iCnt)
    {

        Request bestReq = rq[0];

        for (int i = 1; i < iCnt; i++)
        {
            if (bestReq.TsArrival > rq[i].TsArrival)
                bestReq = rq[i];
        }

        bestReq.CMD = COMMAND.ACT;
        bestReq.CanBeIssued = MC.CHANNEL.CanActivate(bestReq.MemAddr);

        if (Stalled)
        {
            sb.Append($"\tEarliest(List<Request>)  => {bestReq.BlockAddress} Is bestReq.\n");
        }

        return bestReq;

    }

    public Request FindHotOrEarliest(List<Request> rq, int iCnt, long ACTIVE_ROW)
    {
        Request bestReq = rq[0];

        for (int i = 1; i < iCnt; i++)
        {
            bestReq = HotOrEarlier(bestReq, rq[i], ACTIVE_ROW);
        }

        if (Stalled)
        {
            sb.Append($"\tHotOrEarliest(List<Request>)  => {bestReq.BlockAddress} Is bestReq.\n");
        }

        return bestReq;

    }

    Request HotOrEarlier(Request req1, Request req2)
    {
        bool hit1 = (req1.CMD == COMMAND.RD || req1.CMD == COMMAND.WR);
        bool hit2 = (req2.CMD == COMMAND.RD || req2.CMD == COMMAND.WR);

        //if (hit1 ^ hit2) return hit1 ? req1 : req2;

        //if (req1.TsArrival <= req2.TsArrival) return req1;

        //return req2;

        if (hit1 ^ hit2)
            if (hit1)
            {
                if (Stalled)
                {
                    sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} Is Hot.\n");
                }
                return req1;
            }
            else
            {
                if (Stalled)
                {
                    sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} Is Hot.\n");
                }
                return req2;
            }

        if (req1.TsArrival <= req2.TsArrival)
        {
            if (Stalled)
            {
                sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} Is Earlier.\n");
            }
            return req1;
        }

        if (Stalled)
        {
            sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} Is Earlier.\n");
        }
        return req2;

    }

    Request HotOrEarlier(Request req1, Request req2, long ACTIVE_ROW)
    {
        //bool hit1 = (req1.MemAddr.Row == ACTIVE_ROW);
        //bool hit2 = (req2.MemAddr.Row == ACTIVE_ROW);
        //if (hit1 ^ hit2) return hit1 ? req1 : req2;

        //if (req1.TsArrival <= req2.TsArrival) return req1;

        bool hit1 = (req1.MemAddr.Row == ACTIVE_ROW);
        bool hit2 = (req2.MemAddr.Row == ACTIVE_ROW);
        if (hit1 ^ hit2)
            if (hit1)
            {
                if (Stalled)
                {
                    sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} Is Hot.\n");
                }
                return req1;
            }
            else
            {
                if (Stalled)
                {
                    sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} Is Hot.\n");
                }
                return req2;
            }

        if (req1.TsArrival <= req2.TsArrival)
        {
                if (Stalled)
                {
                    sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req1.BlockAddress} Is Earlier.\n");
                }
            return req1;
        }

                if (Stalled)
                {
                    sb.Append($"\tHotOrEarlier({req1.BlockAddress}, {req2.BlockAddress}) => {req2.BlockAddress} Is Earlier.\n");
                }
        return req2;
    }

    Request FIND_RD_REQ()
    {

        Request bestREQ = null;

        for (int R = 0; R < NUM_RANKS; R++)
        {
            if (MC.CHANNEL.Ranks[R].IsRefreshing)
                continue;

            for (int G = 0; G < NUM_BANK_GROUPS; G++)
            {

                for (int B = 0; B < NUM_BANKS; B++)
                {
                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    if (BANK.IsBusy)  continue;

                    int RD_Count = MC.QUEUE.READS[R][G][B].Count;

                    if (RD_Count == 0)  continue;

                    Request req = null;

                    switch (BANK.STATE)
                    {
                        case State.IDLE:
                            req = FindEarliest(MC.QUEUE.READS[R][G][B], RD_Count);
                            if (req == null) continue;
                            break;
                        case State.ACTIVE:

                            req = FindHotOrEarliest(MC.QUEUE.READS[R][G][B], RD_Count, BANK.ActiveRow);

                            if (req == null) continue;
                            if (req.MemAddr.Row == BANK.ActiveRow)
                            {
                                req.CMD = COMMAND.RD;
                                req.CanBeIssued = MC.CHANNEL.CanRead(req.MemAddr);
                            }
                            else
                            {
                                req.CMD = COMMAND.PRE;
                                req.CanBeIssued = MC.CHANNEL.CanPRE(req.MemAddr);
                            }

                            break;
                        default:
                            if (req == null) continue;
                            break;
                    }

                    if (!req.CanBeIssued)
                    {
                        if (Stalled)
                        {
                            sb.Append($"{req.BlockAddress}.CanBeIssued => {req.CanBeIssued}.\n");
                        }
                        continue;
                    }

                    if (bestREQ == null)
                    {
                        bestREQ = req;
                        continue;
                    }

                    bestREQ = HotOrEarlier(bestREQ, req);

                }
            }
        }

        PrintStallInfo();
        return bestREQ;
    }

    Request FIND_WR_REQ()
    {

        Request bestReq = null;

        for (int R = 0; R < NUM_RANKS; R++)
        {
            if (MC.CHANNEL.Ranks[R].IsRefreshing)
                continue;

            for (int G = 0; G < NUM_BANK_GROUPS; G++)
            {

                for (int B = 0; B < NUM_BANKS; B++)
                {
                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    if (BANK.IsBusy)  continue;

                    int WR_Count = MC.QUEUE.WRITES[R][G][B].Count;

                    if (WR_Count == 0)  continue;
                      

                    Request req = null;

                    switch (BANK.STATE)
                    {
                        case State.IDLE:
                            req = FindEarliest(MC.QUEUE.WRITES[R][G][B], WR_Count);
                            if (req == null) continue;
                            break;
                        case State.ACTIVE:
                            req = FindHotOrEarliest(MC.QUEUE.WRITES[R][G][B], WR_Count, BANK.ActiveRow);
                            if (req == null) continue;
                            if (req.MemAddr.Row == BANK.ActiveRow)
                            {
                                req.CMD = COMMAND.WR;
                                req.CanBeIssued = MC.CHANNEL.CanWrite(req.MemAddr);
                            }
                            else
                            {
                                req.CMD = COMMAND.PRE;
                                req.CanBeIssued = MC.CHANNEL.CanPRE(req.MemAddr);
                            }

                            break;
                        default:
                            if (req == null) continue;
                            break;
                    }

                    if (!req.CanBeIssued) continue;

                    if (bestReq == null)
                    {
                        bestReq = req;
                        continue;
                    }
                    bestReq = HotOrEarlier(bestReq, req);

                }
            }
        }

        return bestReq;

    }

    REF_INFO FIND_REF_REQ()
    {
        for (int R = 0; R < NUM_RANKS; R++)
        {
            if (!MC.QUEUE.REFS[R].NEEDS_REFRESH)
                continue;

            for (int G = 0; G < NUM_BANK_GROUPS; G++)
            {
                for (int B = 0; B < NUM_BANKS; B++)
                {
                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    if (BANK.IsBusy)
                        continue;

                    if (BANK.STATE == State.ACTIVE)
                    {
                        MC.QUEUE.REFS[R].CMD = COMMAND.PRE;
                        MC.QUEUE.REFS[R].RANK = R;
                        MC.QUEUE.REFS[R].GROUP = G;
                        MC.QUEUE.REFS[R].BANK = B;
                        MC.QUEUE.REFS[R].BANK_IS_OPEN = true;
                        MC.QUEUE.REFS[R].CAN_ISSUE = MC.CHANNEL.Ranks[R].CanPRE(G, B);

                        return MC.QUEUE.REFS[R];
                    }
                }
            }

            //ALL BANKS are Idle. Try Refresh
            MC.QUEUE.REFS[R].RANK = R;
            MC.QUEUE.REFS[R].CMD = COMMAND.REF;
            MC.QUEUE.REFS[R].BANK_IS_OPEN = false;
            MC.QUEUE.REFS[R].CAN_ISSUE = MC.CHANNEL.Ranks[R].CAN_REF_RANK();

            //if (CHANNEL.Cycle > CYCLES && CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
            //    DEBUG.Print($"{MC.Cycle}: RANK({R}) CAN REF => {MC.QUEUE.REFS[R].CAN_ISSUE}.");
            return MC.QUEUE.REFS[R];

        }

        //if (CHANNEL.Cycle > CYCLES && CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
        //    DEBUG.Print($"{MC.Cycle}: RETURNING RANK(-1) FOR REF => NULL.");
        return REF_INFO.NULL; ;
    }


}


