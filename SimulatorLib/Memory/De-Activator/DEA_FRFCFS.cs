using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using SimulatorLib.Memory.Schedule;

namespace SimulatorLib.DeActivator;

public class DEA_FRFCFS : IScheduler
{
    private MemoryController MC;

    int NUM_RANKS;
    int NUM_BANK_GROUPS;
    int NUM_BANKS;

    public DEA_FRFCFS(MemoryController mc)
    {
        MC = mc;

        NUM_RANKS = mc.PARAM.NUM_RANKS;
        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
        NUM_BANKS = mc.PARAM.NUM_BANKS;
        //sw = new System.IO.StreamWriter(fn);

    }

    void IssueReadRequest()
    {
        Request REQ = FIND_RD_REQ();

        if (REQ == null) return;

        DEBUG.Assert(REQ.CanBeIssued);

        MC.ISSUE(REQ);
    }

    void IssueWriteRequest()
    {
        Request REQ = FIND_WR_REQ();

        if (REQ == null) return;

        DEBUG.Assert(REQ.CanBeIssued);

        if (REQ.CMD == COMMAND.WR) MC.WRITE_MODE.AlertWriteMode(COMMAND.WR);

        MC.ISSUE(REQ);
    }

    void IssueRefresh()
    {
        REF_INFO rx = FIND_REF_REQ();

        //DEBUG.Print($"\t\t**** [{rx.RANK}{rx.GROUP}{rx.BANK}] | REF.CAN_ISSUE => {rx.CAN_ISSUE}");

        if (!rx.CAN_ISSUE)
            return;


        if (rx.BANK_IS_OPEN)
        {
            MC.CLOSE_BANK(rx.RANK, rx.GROUP, rx.BANK);
            MC.QUEUE.REFS[rx.RANK].BANK_IS_OPEN = false;
        }
        else
        {
            MC.REF_RANK(rx.RANK);
            MC.QUEUE.REFS[rx.RANK].NEEDS_REFRESH = false;
        }
        //DEBUG.Print($"*** {MC.Cycle} FRFCFS.DrainWrites: {MC.WRITE_MODE.DrainWrites} | rx.CMD => {rx.CMD}.");
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

    public Request FindHotOrEarliest(List<Request> rq_list, DEA_Bank BANK)
    {
        Request bestReq = rq_list[0];

        for (int i = 1; i < rq_list.Count; i++)
        {
            bestReq = HotOrEarlier(bestReq, rq_list[i], BANK);
        }
        return bestReq;

    }

    Request HotOrEarlier(Request req1, Request req2)
    {
        bool hit1 = (req1.CMD == COMMAND.RD || req1.CMD == COMMAND.WR);
        bool hit2 = (req2.CMD == COMMAND.RD || req2.CMD == COMMAND.WR);
        {
            if (hit1 ^ hit2) return hit1 ? req1 : req2;

            if (req1.TsArrival <= req2.TsArrival)
                return req1;

            return req2;
        }
    }

    Request HotOrEarlier(Request req1, Request req2, DEA_Bank BANK)
    {
        bool hit1 = BANK.IsHit(req1);
        bool hit2 = BANK.IsHit(req2);

        if (hit1 ^ hit2)
            return hit1 ? req1 : req2;

        if (req1.TsArrival <= req2.TsArrival)
            return req1;

        return req2;
    }

    Request FIND_RD_REQ()
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
                    DEA_Bank BANK = (DEA_Bank)MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    if (BANK.IsBusy)
                    {
                        continue;
                    }

                    int RD_Count = MC.QUEUE.READS[R][G][B].Count;

                    if (RD_Count == 0)
                        continue;

                    Request req = FindHotOrEarliest(MC.QUEUE.READS[R][G][B], BANK);

                    if (req == null) continue;


                    if (BANK.IsHit(req)) // (req.MemAddr.Row == BANK.ActiveRow)
                    {
                        req.CMD = COMMAND.RD;
                        req.CanBeIssued = MC.CHANNEL.CanRead(req.MemAddr);
                    }
                    else
                    {
                        if (BANK.STATE == State.ACTIVE)
                        {
                            req.CMD = COMMAND.PRE;
                            req.CanBeIssued = MC.CHANNEL.CanPRE(req.MemAddr);
                        }
                        else
                        {
                            req.CMD = COMMAND.ACT;
                            req.CanBeIssued = MC.CHANNEL.CanActivate(req.MemAddr);
                        }
                    }


                    if (req.BlockAddress == 112332342044)
                        DEBUG.Print($"***BANK({G}{B})=> {MC.Cycle}: DEA_FCRFCS Selected {req.BlockAddress} for {req.CMD} Canbeissued => {req.CanBeIssued}");

                    if (!req.CanBeIssued) continue;

                    if (bestReq == null)
                    {
                        bestReq = req;
                        continue;
                    }

                    long br = bestReq.BlockAddress;
                    bestReq = HotOrEarlier(bestReq, req);

                    if (req.BlockAddress == 112332342044)
                        DEBUG.Print($"***BestOf({req.BlockAddress}, {br}) => {MC.Cycle}: DEA_FCRFCS Selected ({bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}, {bestReq.BlockAddress}) for {bestReq.CMD}");

                }
            }
        }

        if (bestReq != null && bestReq.BlockAddress == 112332342044)
            DEBUG.Print($"***{MC.Cycle}: DEA_FCRFCS Selected {bestReq.BlockAddress} for read.");

        return bestReq;

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
                    DEA_Bank BANK = (DEA_Bank)MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    if (BANK.IsBusy)
                    {
                        continue;
                    }

                    int WR_Count = MC.QUEUE.WRITES[R][G][B].Count;

                    if (WR_Count == 0)
                        continue;

                    Request req = FindHotOrEarliest(MC.QUEUE.WRITES[R][G][B], BANK);

                    if (req == null) continue;

                    if (BANK.IsHit(req)) //  (req.MemAddr.Row == BANK.ActiveRow)
                    {
                        req.CMD = COMMAND.WR;
                        req.CanBeIssued = MC.CHANNEL.CanWrite(req.MemAddr);
                    }
                    else
                    {
                        if (BANK.STATE == State.ACTIVE)
                        {
                            req.CMD = COMMAND.PRE;
                            req.CanBeIssued = MC.CHANNEL.CanPRE(req.MemAddr);
                        }
                        else
                        {
                            req.CMD = COMMAND.ACT;
                            req.CanBeIssued = MC.CHANNEL.CanActivate(req.MemAddr);
                        }
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
        //if (CHANNEL.Cycle > CYCLES && CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
        //    DEBUG.Print($"{MC.Cycle}: UPDATING ... REF/PRE.");

        for (int R = 0; R < NUM_RANKS; R++)
        {
            if (!MC.QUEUE.REFS[R].NEEDS_REFRESH)
                continue;

            for (int G = 0; G < NUM_BANK_GROUPS; G++)
            {
                for (int B = 0; B < NUM_BANKS; B++)
                {
                    DEA_Bank BANK = (DEA_Bank)MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    if (BANK.IsBusy)
                        continue;

                    if (BANK.STATE == State.ACTIVE)
                    {
                        //if (CHANNEL.Cycle > CYCLES && CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
                        //    DEBUG.Print($"{MC.Cycle}: STATE({R}{G}{B}) => {BANK.STATE}.");
                        MC.QUEUE.REFS[R].CMD = COMMAND.PRE;
                        MC.QUEUE.REFS[R].RANK = R;
                        MC.QUEUE.REFS[R].GROUP = G;
                        MC.QUEUE.REFS[R].BANK = B;
                        MC.QUEUE.REFS[R].BANK_IS_OPEN = true;
                        MC.QUEUE.REFS[R].CAN_ISSUE = MC.CHANNEL.Ranks[R].CanPRE(G, B);

                        //if (CHANNEL.Cycle > CYCLES && CHANNEL.Cycle % 1000 == 0) //(MC.cycle_debug)
                        //    DEBUG.Print($"{MC.Cycle}: RANK({R}{G}{B}) CAN PRE => {MC.QUEUE.REFS[R].CAN_ISSUE}.");

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


