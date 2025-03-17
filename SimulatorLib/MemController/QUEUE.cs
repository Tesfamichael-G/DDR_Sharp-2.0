using SimulatorLib.Common;
using SimulatorLib.DDR;
using System.Collections.Generic;
using SimulatorLib.Memory.Schedule;

namespace SimulatorLib.MemCtrl;

public class RequestQueue
{
    //IHtmlLoger HTML;
    //Tools HTML_TOOL = Tools.Instance;

    public List<Request>[][][] READS;
    public List<Request>[][][] WRITES;

    public REF_INFO[] REFS;
    //public List<REF_Request>[] rREFS;

    private MemoryController MC;

    public int RD_Queued = 0;
    public int WR_Queued = 0;

    int WRITEQ_MAX;
    int READQ_MAX;

    int NUM_RANKS;
    int NUM_BANK_GROUPS;
    int NUM_BANKS;

    public void RobStalled(long addr)
    {
        bool IsQueuedInRead = ReadContains(addr);
        bool IsQueuedInWrite = WriteContains(addr);
        DEBUG.Print($"\n => QUEUE[{RD_Queued}, {WR_Queued}]: {addr} in ReadQueue: {IsQueuedInRead} | {addr} in WriteQueue: {IsQueuedInWrite}\n");
    }

    public RequestQueue(MemoryController mc)
    {
        MC = mc;
        READQ_MAX = mc.PARAM.READQ_MAX;
        WRITEQ_MAX = mc.PARAM.WRITEQ_MAX;

        NUM_RANKS = mc.PARAM.NUM_RANKS;
        NUM_BANK_GROUPS = mc.PARAM.NUM_BANK_GROUPS;
        NUM_BANKS = mc.PARAM.NUM_BANKS;


        READS = new List<Request>[NUM_RANKS][][];
        WRITES = new List<Request>[NUM_RANKS][][];
        REFS = new REF_INFO[NUM_RANKS];

        for (int r = 0; r < NUM_RANKS; r++)
        {
            READS[r] = new List<Request>[NUM_BANK_GROUPS][];
            WRITES[r] = new List<Request>[NUM_BANK_GROUPS][];
            REFS[r] = new REF_INFO();

            for (int g = 0; g < NUM_BANK_GROUPS; g++)
            {
                READS[r][g] = new List<Request>[NUM_BANKS];
                WRITES[r][g] = new List<Request>[NUM_BANKS];

                for (int b = 0; b < NUM_BANKS; b++)
                {
                    READS[r][g][b] = new List<Request>();
                    WRITES[r][g][b] = new List<Request>();
                }

            }
        }
        //HTML_TOOL.QUEUE = this;
        //HTML = HTML_TOOL.queue;
    }

    public bool Enqueue(Request REQ)
    {

        //P($"\t\t => Enqueue({REQ.BlockAddress}) @ {MC.Cycle}");

        if (REQ.TYPE == Operation.WRITE && WR_Queued == WRITEQ_MAX)
            return false;

        if (REQ.TYPE == Operation.READ && RD_Queued == READQ_MAX)
            return false;

        REQ.TsArrival = MC.Cycle;

        if (MC.PARAM.INCLUDE_XBAR_LATENCY)
            return xEnqueue(REQ);

        int r = REQ.MemAddr.Rank;
        int g = REQ.MemAddr.BankGroup;
        int b = REQ.MemAddr.Bank;

        switch (REQ.TYPE)
        {
            case Operation.READ:

                if (WriteContains(REQ.BlockAddress)) return true;

                READS[r][g][b].Add(REQ);
                RD_Queued++;
                break;
            case Operation.WRITE:
                WRITES[r][g][b].Add(REQ);
                WR_Queued++;
                break;
            default:
                break;
        }
        if (REQ.BlockAddress == 43209711458) DEBUG.Print($"\t - Cycle:{MC.Cycle} | 43209711458 queued.");
        return true;
    }
    public void Dequeue(Request REQ)
    {

        int r = REQ.MemAddr.Rank;
        int g = REQ.MemAddr.BankGroup;
        int b = REQ.MemAddr.Bank;

        //HTML.WriteLine($"<details class=\"lev1\">");

        if (REQ.TYPE == Operation.READ)
        {
            //HTML.WriteLine($"<summary>Cycle {MC.Cycle} | READ QUEUE | Dequeue(Bank[{REQ.MemAddr.Bank}])</summary>");
            //WriteHTML(REQ, READS[0][0], "Dequeue");
            //HTML.WriteLine($"</details>");

            READS[r][g][b].Remove(REQ);
            RD_Queued--;
        }
        else
        {
            //HTML.WriteLine($"<summary>Cycle {MC.Cycle} | WRITE QUEUE | Dequeue(Bank[{REQ.MemAddr.Bank}])</summary>");
            //WriteHTML(REQ, WRITES[0][0], "Dequeue");
            //HTML.WriteLine($"</details>");

            WRITES[r][g][b].Remove(REQ);
            WR_Queued--;
        }
        if (REQ.BlockAddress == 43209711458) DEBUG.Print($"\t - Cycle:{MC.Cycle} | 43209711458 dequeued.");

    }
    void WriteHTML(Request REQ, List<Request>[] lst, string action)
    {
        //int b = REQ.MemAddr.Bank;

        //for (int B = 0; B < NUM_BANKS; B++)
        //{
        //    List<Request> bankList = lst[B];
        //    if (bankList == null) continue;

        //    int count = bankList.Count;
        //    if (count == 0) continue;

        //    HTML.WriteLine($"<details class=\"lev2\">");
        //    if (B == b)
        //        HTML.WriteLine($"<summary>BANK {B} | {action}({REQ.BlockAddress})</summary>");
        //    else
        //        HTML.WriteLine($"<summary>BANK {B} | {action}</summary>");

        //    for (int i = 0; i < count; i++)
        //    {
        //        if (bankList[i].BlockAddress == REQ.BlockAddress)
        //            HTML.WriteLine($"<p class = \"bg\"> -[ {bankList[i].TsArrival} | {bankList[i].BlockAddress}], [{B}, {bankList[i].MemAddr.Row}]</p>");
        //        else if (bankList[i].BlockAddress == 111251689860)
        //            HTML.WriteLine($"<p class = \"br\"> - [{bankList[i].TsArrival} | {bankList[i].BlockAddress}], [{B}, {bankList[i].MemAddr.Row}]</p>");
        //        else
        //            HTML.WriteLine($"<p> - [{bankList[i].TsArrival} | {bankList[i].BlockAddress}], [{B}, {bankList[i].MemAddr.Row}]</p>");
        //    }
        //    HTML.WriteLine($"</details>");
        //}

    }



    #region Find/Search

    public bool WriteContains(long BlockAddress)
    {
        for (int r = 0; r < NUM_RANKS; r++)
        {
            for (int g = 0; g < NUM_BANK_GROUPS; g++)
            {
                for (int b = 0; b < NUM_BANKS; b++)
                {
                    if (WRITES[r][g][b] == null) continue;

                    int count = WRITES[r][g][b].Count;
                    if (count == 0) continue;

                    for (int i = 0; i < count; i++)
                    {
                        if (WRITES[r][g][b][i].BlockAddress == BlockAddress)
                        {
                            MC.SIM.MemSystem.MergedWrites++;
                            return true;
                        }
                    }
                }
            }
        }
        return false;

    }
    public bool ReadContains(long BlockAddress)
    {
        for (int r = 0; r < NUM_RANKS; r++)
        {
            for (int g = 0; g < NUM_BANK_GROUPS; g++)
            {
                for (int b = 0; b < NUM_BANKS; b++)
                {
                    if (READS[r][g][b] == null) continue;

                    int count = READS[r][g][b].Count;
                    if (count == 0) continue;

                    for (int i = 0; i < count; i++)
                    {
                        if (READS[r][g][b][i].BlockAddress == BlockAddress)
                        {
                            MC.SIM.MemSystem.MergedReads++;
                            return true;
                        }
                    }
                }

            }
        }
        return false;
    }
    public int Contains(long block_address)
    {
        for (int r = 0; r < NUM_RANKS; r++)
        {
            Rank RANK = MC.CHANNEL.Ranks[r];
            for (int g = 0; g < NUM_BANK_GROUPS; g++)
            {
                for (int b = 0; b < NUM_BANKS; b++)
                {
                    if (WRITES[r][g][b] == null) continue;

                    int count = WRITES[r][g][b].Count;
                    if (count == 0) continue;

                    for (int i = 0; i < count; i++)
                    {
                        if (WRITES[r][g][b][i].BlockAddress == block_address)
                        {
                            MC.SIM.MemSystem.MergedReads++;
                            return MC.PARAM.WQ_LOOKUP_LATENCY;
                        }
                    }
                }
            }
        }

        for (int r = 0; r < NUM_RANKS; r++)
        {
            for (int g = 0; g < NUM_BANK_GROUPS; g++)
            {
                for (int b = 0; b < NUM_BANKS; b++)
                {
                    if (READS[r][g][b] == null) continue;

                    int count = READS[r][g][b].Count;
                    if (count == 0) continue;

                    for (int i = 0; i < count; i++)
                    {
                        if (READS[r][g][b][i].BlockAddress == block_address)
                        {
                            MC.SIM.MemSystem.MergedReads++;
                            return RQ_LOOKUP_LATENCY;
                        }
                    }
                }
            }
        }
        return 0;
    }

    public (Request, bool) GetRequest(long BlockAddress, bool fromReadQueue = true)
    {
        List<Request>[][][] q = fromReadQueue ? READS : WRITES;

        for (int r = 0; r < NUM_RANKS; r++)
        {
            for (int g = 0; g < NUM_BANK_GROUPS; g++)
            {
                for (int b = 0; b < NUM_BANKS; b++)
                {
                    if (q[r][g][b] == null) continue;

                    int count = q[r][g][b].Count;
                    DeActivator.DEA_Channel C = (DeActivator.DEA_Channel)MC.CHANNEL;
                    if (C == null) return (null, false);
                    if (count == 0) continue;

                    DeActivator.DEA_Bank BANK = (DeActivator.DEA_Bank)C.Ranks[r].Groups[g].Banks[b];

                    if (BANK == null) return (null, false);
                    for (int i = 0; i < count; i++)
                    {
                        if (q[r][g][b][i].BlockAddress == BlockAddress)
                        {
                            return (q[r][g][b][i], BANK.IsHit(q[r][g][b][i]));
                        }
                    }
                }

            }
        }
        return (null,false) ;
    }

    public int FindRowInReadQueue(MemoryAddress addr)
    {
        return READS[addr.Rank][addr.BankGroup][addr.Bank].FindAll(r => r.MemAddr.Row == addr.Row).Count;
    }
    public int FindRowInReadQueue(int RankID, int BankID, int GroupID, long row)
    {
        return READS[RankID][GroupID][BankID].FindAll(r => r.MemAddr.Row == row).Count;
    }

    public int FindRowInWriteQueue(int RankID, int GroupID, int BankID, long row)
    {
        return WRITES[RankID][GroupID][BankID].FindAll(r => r.MemAddr.Row == row).Count;
    }
    public int FindRowInWriteQueue(MemoryAddress addr)
    {
        return WRITES[addr.Rank][addr.BankGroup][addr.Bank].FindAll(r => r.MemAddr.Row == addr.Row).Count;
    }


    #endregion

    #region Calculated Fileds
    public bool IsWriteEmpty => WR_Queued == 0;

    public bool IsWriteFull => WR_Queued == WRITEQ_MAX;

    public bool IsReadEmpty => RD_Queued == 0;

    public int Length => RD_Queued + WR_Queued;

    int RQ_LOOKUP_LATENCY = 1;

    #endregion


    #region X-BAR
    public bool xEnqueue(Request REQ)
    {
        int r = REQ.MemAddr.Rank;
        int g = REQ.MemAddr.BankGroup;
        int b = REQ.MemAddr.Bank;

        //HTML.WriteLine($"<details class=\"lev1\">");
        switch (REQ.TYPE)
        {
            case Operation.READ:
                //HTML.WriteLine($"<summary>Cycle {MC.Cycle} | READ QUEUE | Enqueue(Bank[{REQ.MemAddr.Bank}])</summary>");
                if (WriteContains(REQ.BlockAddress))
                {
                    //HTML.WriteLine($"<p class = \"bg\" -[WriteContains({REQ.BlockAddress})]</p>");
                    MC.SIM.XBAR.Enqueue(REQ);
                    MC.SIM.STAT.CORE[REQ.CoreID].WriteBack_Hit++;
                    //HTML.WriteLine($"</details>");
                    return true;
                }
                READS[r][g][b].Add(REQ);
                RD_Queued++;

                //WriteHTML(REQ, READS[0][0], "Enqueue");
                //HTML.WriteLine($"</details>");
                break;
            case Operation.WRITE:
                //HTML.WriteLine($"<summary>Cycle {MC.Cycle} | WRITE QUEUE | Enqueue(Bank[{REQ.MemAddr.Bank}])</summary>");
                WRITES[r][g][b].Add(REQ);
                WR_Queued++;
                //WriteHTML(REQ, WRITES[0][0], "Enqueue");
                //HTML.WriteLine($"</details>");
                break;
            default:
                break;
        }

        return true;

    }
    public void xDequeue(Request REQ)
    {
        int r = REQ.MemAddr.Rank;
        int g = REQ.MemAddr.BankGroup;
        int b = REQ.MemAddr.Bank;


        REQ.TsDeparture = MC.Cycle;

        if (REQ.TYPE == Operation.READ)
        {
            MC.SIM.XBAR.Enqueue(REQ);
            READS[r][g][b].Remove(REQ);
            RD_Queued--;
        }
        else
        {
            WRITES[r][g][b].Remove(REQ);
            WR_Queued--;
            REQ.Latency = (int)(REQ.TsDeparture - REQ.TsArrival);
            CallBack callback = REQ.CALLBACK;
            callback(REQ);
        }

    }

 

    #endregion



}
