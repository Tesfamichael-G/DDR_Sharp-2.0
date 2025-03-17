using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulatorLib.Common;
using SimulatorLib.CPU;

namespace SimulatorLib.DDR;

public partial class Channel
{


    public bool refresh_waiting = false;


    #region Rules

    public bool CanNotRead => NextRD > cycle;
    public bool CanNotWrite => NextWR > cycle;

    internal bool CanPRE(MemoryAddress mAddr) => Ranks[mAddr.Rank].CanPRE(mAddr);

    internal bool CanActivate(MemoryAddress mAddr) => Ranks[mAddr.Rank].CanActivate(mAddr);

    internal bool CanRead(MemoryAddress mAddr) 
    {
        //if (mAddr.Bank == 6)
        //    DEBUG.Print($"\t*** {cycle}: [NextRD: {NextRD}] Channel.CanRead => {cycle >= NextRD}");

        return (cycle < NextRD) ? false : Ranks[mAddr.Rank].CanRead(mAddr); 
    }

    internal bool CanWrite(MemoryAddress mAddr) => (cycle < NextWR) ? false : Ranks[mAddr.Rank].CanWrite(mAddr);

    internal bool CanPowerUp(MemoryAddress mAddr)
    {

        //request.CMD = COMMAND.PDX;

        //if (Cycle < BANK.Next_PDX)
        //{
        //    if (request.MemAddr.BlockAddress == 100886550675)
        //        DEBUG.Print($"{Cycle}: Channel.CanPowerUp(100886550675) at Bank[{BANK.ID}] | State: {BANK.STATE} => {false}");
        //    return false;
        //}

        ////if (BANK.STATE == BankState.PRECHARGE_POWER_DOWN_SLOW && REFRESHER.DisAllows(COMMAND.PWR_UP, false))//RANK.CanNotPowerUpSlow)
        //if (BANK.STATE == State.PRE_PDNS)//RANK.CanNotPowerUpSlow)
        //{
        //    if (request.MemAddr.BlockAddress == 100886550675)
        //        DEBUG.Print($"{Cycle}: Channel.CanPowerUp(100886550675) at Bank[{BANK.ID}] | State: {BANK.STATE} => {false}");
        //    return false;
        //}
        ////&& REFRESHER.DisAllows(COMMAND.PWR_UP)
        //if ((BANK.STATE == State.PRE_PDNF ||
        //    BANK.STATE == State.ACT_PDNF))//RANK.CanNotPowerUpFast)
        //{
        //    if (request.MemAddr.BlockAddress == 100886550675)
        //        DEBUG.Print($"{Cycle}: Channel.CanPowerUp(100886550675) at Bank[{BANK.ID}] | State: {BANK.STATE} => {false}");
        //    return false;
        //}

        //if (request.MemAddr.BlockAddress == 100886550675)
        //    DEBUG.Print($"{Cycle}: Channel.CanPowerUp(100886550675) at Bank[{BANK.ID}] | State: {BANK.STATE} => {true}");

        return true;
    }


    #endregion


    public bool PRECHARGE(MemoryAddress mem_addr)
    {
        Stat.PRE++;
        Ranks[mem_addr.Rank].PRECHARGE(mem_addr);
        return true;
    }

    public virtual bool ACTIVATE(Request REQ)
    {
        Ranks[REQ.MemAddr.Rank].ACTIVATE(REQ.MemAddr, REQ.TYPE);
        REQ.NUM_ACTS++;

        Stat.ACTS++;

        if (REQ.TYPE == Operation.READ)
        {
            Stat.ACTS_RD++;
        }
        else
        {
            Stat.ACTS_WR++;
        }
        ///////////////////HTML.WriteLine($"<p> - Cycle: {Cycle}| ACT({REQ.BlockAddress}], [{REQ.MemAddr.ToString()}]) ACTS: {Stat.ACTS} ({Stat.ACTS_RD},{Stat.ACTS_WR})</p>");
        return true;
    }

    public bool READ(Request REQ)
    {
        //DEBUG.Print($"\t\t>>>Cycle: {cycle} - Reading ...");
        Stat.READS++;
        if (REQ.NUM_ACTS == 0)
            Stat.RD_HITS++;

        //if (REQ.BlockAddress == 112332342044)
        //    DEBUG.Print($"Cycle: {cycle} - Reading 112332342044 now.");
        //HTML.WriteLine($"<p class = \"bg\"> - Cycle: {Cycle}| RD({REQ.BlockAddress}], [{REQ.MemAddr.ToString()}]) RDS: {Stat.READS}, HTS: {Stat.RD_HITS}</p>");

        NextRD = NextWR = cycle + Param.BL;

        REQ.TsCompletion = cycle + Param.RD_delay;
        REQ.Latency = REQ.TsCompletion - REQ.TsArrival;
        REQ.TsDeparture = cycle;

        QUEUE.Dequeue(REQ);
        BUS.Add(REQ);

        if (!Param.OPEN_PAGE_POLICY)
        {
            int pending_requests_to_same_row = MC.QUEUE.FindRowInReadQueue(REQ.MemAddr);

            if (pending_requests_to_same_row == 0)
            {
                READ_AP(REQ); // precharge a bank after a read/write 
                return true;
            }
        }

        for (int i = 0; i < Param.NUM_RANKS; i++)
        {
            if (i == REQ.MemAddr.Rank)
                Ranks[REQ.MemAddr.Rank].READ(REQ.MemAddr);
            else
                Ranks[i].SIBLING_READ(REQ);
        }

        Stat.AVG_ReadLatency.Add(REQ.Latency);
        Stat.AVG_ReadQue_Latency.Add(REQ.TsDeparture - REQ.TsArrival);
        return true;
    }

    public bool READ_AP(Request REQ)
    {

        REQ.TsCompletion = cycle + Param.RDA_delay;
        REQ.Latency = REQ.TsCompletion - REQ.TsArrival;
        REQ.TsDeparture = cycle;
        REQ.IsServed = true;

        QUEUE.Dequeue(REQ); //MC.Dequeue(REQ);
        BUS.Add(REQ);

        Stat.READS++;
        Stat.PRE++;
        if (REQ.NUM_ACTS == 0)
            Stat.RD_HITS++;

        NextRD = NextWR = cycle + Param.BL;

        for (int i = 0; i < Param.NUM_RANKS; i++)
        {
            if (i == REQ.MemAddr.Rank)
                Ranks[i].READ_AP(REQ.MemAddr);
            else
                Ranks[i].SIBLING_READ_AP(REQ);
        }

        ////////////////////////////////REQ.REQ_Data.IssuedList[COMMAND.READ_AP] = Cycle;

        Stat.AVG_ReadLatency.Add(REQ.Latency);
        Stat.AVG_ReadQue_Latency.Add(REQ.TsDeparture - REQ.TsArrival);
        return true;

    }

    public bool WRITE(Request REQ)
    {

        NextRD = NextWR = cycle + Param.BL;
        Stat.WRITES++;
        if (REQ.NUM_ACTS == 0)
            Stat.WR_HITS++;

        //HTML.WriteLine($"<p class = \"br\"> - Cycle: {Cycle}| WR({REQ.BlockAddress}], [{REQ.MemAddr.ToString()}]) WRS: {Stat.READS}, HTS: {Stat.RD_HITS}</p>");

        REQ.TsCompletion = cycle + Param.WR_delay;
        REQ.Latency = REQ.TsCompletion - REQ.TsArrival;
        REQ.TsDeparture = cycle;
        REQ.IsServed = true;

        QUEUE.Dequeue(REQ);
        BUS.Add(REQ);

        if (!Param.OPEN_PAGE_POLICY)
        {

            int pending_requests_to_same_row = MC.QUEUE.FindRowInWriteQueue(REQ.MemAddr);

            if (pending_requests_to_same_row == 0)
            {
                WRITE_AP(REQ);// precharge a bank after a read/write 
                return true;
            }
        }

        for (int i = 0; i < Param.NUM_RANKS; i++)
        {
            if (i == REQ.MemAddr.Rank)
                Ranks[i].WRITE(REQ.MemAddr);
            else
                Ranks[i].SIBLING_WRITE(REQ);
        }

        Stat.AVG_Write_Latency.Add(REQ.Latency);
        Stat.AVG_WriteQue_Latency.Add(REQ.TsDeparture - REQ.TsArrival);

        return true;
    }

    public bool WRITE_AP(Request REQ)
    {

        REQ.TsCompletion = cycle + Param.WRA_delay;
        REQ.Latency = REQ.TsCompletion - REQ.TsArrival;
        REQ.TsDeparture = cycle;
        REQ.IsServed = true;

        QUEUE.Dequeue(REQ);
        BUS.Add(REQ);

        Stat.WRITES++;
        Stat.PRE++;

        if (REQ.NUM_ACTS == 0)
            Stat.WR_HITS++;

        NextRD = NextWR = cycle + Param.BL;

        for (int i = 0; i < Param.NUM_RANKS; i++)
        {
            if (i == REQ.MemAddr.Rank)
                Ranks[i].WRITE_AP(REQ.MemAddr);
            else
                Ranks[i].SIBLING_WRITE_AP(REQ);
        }

        /////////////////////////////REQ.REQ_Data.IssuedList[COMMAND.WRITE_AP] = Cycle;

        Stat.AVG_Write_Latency.Add(REQ.Latency);
        Stat.AVG_WriteQue_Latency.Add(REQ.TsDeparture - REQ.TsArrival);
        return true;

    }


    #region REF_PWR

    public int Ranks_to_Refresh;

    public bool CloseBank(int rank, int group, int bank)
    {
        Stat.PRE++;

        Bank BANK = Ranks[rank].Groups[group].Banks[bank];

        //if (!BANK.CAS_Issued)
        //{
        //    Stat.X_ACTS++;
        //    if (MC.WRITE_MODE.DrainWrites)
        //        Stat.X_ACTS_WR++;
        //    else
        //        Stat.X_ACTS_RD++;
        //}

        Ranks[rank].PRECHARGE(group, bank);
        return true;
    }
    public bool FORCE_REFRESH(int CompletionTime)
    {
        //for (int r = 0; r < Ranks.Length; r++)
        //    Ranks[r].FORCE_REFRESH(CompletionTime);

        return true;
    }
    public void RefreshRank(int rank)
    {
        Ranks[rank].rank_REFRESH();
        Ranks_to_Refresh--;
        if (Ranks_to_Refresh == 0)
            refresh_waiting = false;
    }


    #endregion


}
