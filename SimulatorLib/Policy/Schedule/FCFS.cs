using SimulatorLib.Common;
using SimulatorLib.DDR;
using SimulatorLib.MemCtrl;
using System;
using System.Collections.Generic;

namespace SimulatorLib.Memory.Schedule;
public partial class FCFS : IScheduler
{

    IHtmlLoger HTML;
    Tools HTML_TOOL = Tools.Instance;

    private MemoryController MC;

    int NUM_RANKS;
    int NUM_BANK_GROUPS;
    int NUM_BANKS;


    public FCFS(MemoryController mc)
    {
        MC = mc;

        NUM_RANKS = mc.PARAM.NUM_RANKS;
        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
        NUM_BANKS = mc.PARAM.NUM_BANKS;
        HTML = HTML_TOOL.fcfs;
    }

    public void Tick()
    {

        if (MC.CHANNEL.refresh_waiting)
        {
            IssueRefresh();
            return;
        }

        HTML_TOOL.Cycle = MC.Cycle;

        uint servecnt = ((WriteMode.ServeN)(MC.WRITE_MODE)).ServeCnt;
        HTML.WriteLine($"<details class=\"lev1\">");
        HTML.WriteLine($"<summary>Cycle {MC.Cycle} | Write Mode({MC.QUEUE.RD_Queued}, {MC.QUEUE.WR_Queued}[{servecnt}]): [{MC.WRITE_MODE.DrainWrites}]</summary>");

        if (MC.WRITE_MODE.DrainWrites)
            IssueWriteRequest();
        else
            IssueReadRequest();

        HTML.WriteLine($"</details>");
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
        Request rx = FIND_WR_REQ();


        if (rx == null)
        {
            return;
        }

        DEBUG.Assert(rx.CanBeIssued);

        if (rx.CMD == COMMAND.WR) MC.WRITE_MODE.AlertWriteMode(COMMAND.WR);  // alert writeback mode

        MC.ISSUE(rx);
    }

    public Request FindEarliest(List<Request> rq, int iCnt)
    {

        Request bestReq = rq[0];

        HTML.WriteLine($"<p  class = \"bb\">FindEarliest of {iCnt} requests ...</p>");
        HTML.WriteLine("<ul>");
        for (int i = 1; i < iCnt; i++)
        {
            string s = $"<li>({bestReq.BlockAddress}, {bestReq.TsArrival}) vs ({rq[i].BlockAddress}, {rq[i].TsArrival})";
            if (bestReq.TsArrival > rq[i].TsArrival)
                bestReq = rq[i];

            HTML.WriteLine($"{s} => ({bestReq.BlockAddress}, {bestReq.TsArrival})</li>");
        }
        HTML.WriteLine($"</ul>");

        if (bestReq == null)
            HTML.WriteLine($"<p class = \"br\">BANK ({bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}).BEST => NULL</p>");
        else
            HTML.WriteLine($"<p class = \"bb\">BANK ({bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}).BEST => ({bestReq.BlockAddress}, {bestReq.TsArrival})</p>");

        return bestReq;

    }

    Request Earlier(Request req1, Request req2) => (req1.TsArrival <= req2.TsArrival) ? req1 : req2;

    Request FIND_RD_REQ()
    {

        Request bestReq = null; 
        for (int R = 0; R < NUM_RANKS; R++)
        {
            if (MC.CHANNEL.Ranks[R].IsRefreshing)
                continue;

            int prevBank = 0;
            int prevGroup = 0;

            for (int G = 0; G < NUM_BANK_GROUPS; G++)
            {

                for (int B = 0; B < NUM_BANKS; B++)
                {
                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    HTML.WriteLine($"<details  class=\"lev2\">");
                    HTML.WriteLine($"<summary> BANK({G}{B}) READ REQUESTS </summary>");

                    if (BANK.IsBusy)
                    {
                        HTML.WriteLine($"<p class = \"br\">BANK({G}{B}).IsBusy</P>");
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    int RD_Count = MC.QUEUE.READS[R][G][B].Count;

                    if (RD_Count == 0)
                    {
                        HTML.WriteLine($"<p  class = \"b4\"><b>BANK({G}{B}).IsEmpty</b></P>");
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    Request req = FindEarliest(MC.QUEUE.READS[R][G][B], RD_Count);

                    if (req == null)
                    {
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    switch (BANK.STATE)
                    {
                        case State.IDLE:

                            req.CMD = COMMAND.ACT;
                            req.CanBeIssued = MC.CHANNEL.CanActivate(req.MemAddr);

                            break;
                        case State.ACTIVE:
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
                            break;
                    }

                    if (!req.CanBeIssued)
                    {
                        HTML.WriteLine($"<p class = \"br\">{req.CMD}({req.BlockAddress}) can not be issued</p>"); ;
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    if (prevBank == 0)
                    {
                        bestReq = (bestReq == null) ? req : Earlier(bestReq, req);
                        HTML.WriteLine($"</details>");
                        continue;
                    }
                    HTML.WriteLine($"</details>");

                    HTML.WriteLine($"<p class = \"bb\">{prevGroup}{prevBank} vs {G}{B}</p>");

                    bestReq = (bestReq == null) ? req : Earlier(bestReq, req);

                    HTML.WriteLine($"<p  class = \"bb\">({bestReq.BlockAddress}, {bestReq.TsArrival})[{bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}]</p>");
                    prevBank = B; prevGroup = G;

                    //HTML.WriteLine($"</details>");
                }
            }
        }

        //if (bestReq != null)
        //    DEBUG.Print($"{MC.Cycle}: [FIND_READ(Best)] | {bestReq.BlockAddress}[{bestReq.MemAddr.Rank}{bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}].");

        return bestReq;

    }

    Request FIND_WR_REQ()
    {

        Request bestReq = null;
        for (int R = 0; R < NUM_RANKS; R++)
        {
            if (MC.CHANNEL.Ranks[R].IsRefreshing)
                continue;

            int prevBank = 0;
            int prevGroup = 0;

            for (int G = 0; G < NUM_BANK_GROUPS; G++)
            {
                for (int B = 0; B < NUM_BANKS; B++)
                {
                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

                    HTML.WriteLine($"<details  class=\"lev2\">");
                    HTML.WriteLine($"<summary> BANK({G}{B}) WRITE REQUESTS </summary>");

                    if (BANK.IsBusy)
                    {
                        HTML.WriteLine($"<p class = \"br\">BANK({G}{B}).IsBusy</P>");
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    int WR_Count = MC.QUEUE.WRITES[R][G][B].Count;

                    if (WR_Count == 0)
                    {
                        HTML.WriteLine($"<p  class = \"br\"><b>BANK({G}{B}).IsEmpty</b></P>");
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    Request req = FindEarliest(MC.QUEUE.WRITES[R][G][B], WR_Count);

                    if (req == null)
                    {
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    switch (BANK.STATE)
                    {
                        case State.IDLE:
                            req.CMD = COMMAND.ACT;
                            req.CanBeIssued = MC.CHANNEL.CanActivate(req.MemAddr);
                            break;
                        case State.ACTIVE:
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
                            break;
                    }


                    if (!req.CanBeIssued)
                    {
                        HTML.WriteLine($"<p class = \"br\">{req.CMD}({req.BlockAddress}) can not be issued</p>"); ;
                        HTML.WriteLine($"</details>");
                        continue;
                    }

                    if (prevBank == 0 && prevGroup == 0)
                    {
                        bestReq = (bestReq == null) ? req : Earlier(bestReq, req);
                        HTML.WriteLine($"</details>");
                        continue;
                    }
                    HTML.WriteLine($"</details>");

                    HTML.WriteLine($"<p class = \"bb\">{prevGroup}{prevBank} vs {G}{B}</p>");

                    bestReq = (bestReq == null) ? req : Earlier(bestReq, req);

                    HTML.WriteLine($"<p  class = \"bb\">({bestReq.BlockAddress}, {bestReq.TsArrival})[{bestReq.MemAddr.BankGroup}{bestReq.MemAddr.Bank}]</p>");
                    //HTML.WriteLine($"</details>");
                    prevBank = B; prevGroup = G;

                    //if (bestReq == null)
                    //{
                    //    bestReq = req;
                    //    continue;
                    //}
                    //bestReq = Earlier(bestReq, req);

                }
            }
        }

        return bestReq;

    }


    #region REFREESH

    void IssueRefresh()
    {
        REF_INFO rx = FIND_REF_REQ();

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
                    Bank BANK = MC.CHANNEL.Ranks[R].Groups[G].Banks[B];

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

    #endregion


}



