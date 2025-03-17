using SimulatorLib;
using SimulatorLib.Common;
using System;
using System.Collections.Generic;

namespace SimulatorLib.DDR;

public partial class Rank
{
    #region Fields
    public int ID;

    protected Parameters t;
    protected Parameters CONFIG;

    public BankGroup[] Groups;
    private long cycle;

    public RankStat Stat;

    public long CountDown = 0;

    public long LastActivate;
    public long LastRefresh;

    public AverageObject GAP_between_Activates;
    public AverageObject GAP_between_Refreshes;

    #endregion

    #region Nexts
    public long NextFAW;
    public Queue<long> PAST_FOUR_ACTS;

    public long NextPRE = 0;
    public long NextPREA = 0;
    public long NextACT = 0;

    public long NextRD = 0;
    public long NextWR = 0;

    public long NextPDE = 0;
    public long NextPDX = 0;

    public long NextREF = 0;
    public long NextSRE = 0;
    public long NextSRX = 0;

    #endregion

    public bool IsRefreshing = false;

    protected int RD_WR;    //  t.CL + t.BL + t.RTRS - t.CWL;
    protected int RDA_WR;   // t.CL + t.BL + t.RTRS - t.CWL;
    protected long WR_RD;   // t.CWL + t.BL + t.WTRS;
    protected long WR_PD;   // t.CWL + t.BL + t.WR;
    protected long WR_PREA;  //  t.CWL + t.BL + t.WR;
    protected long WRA_REF; //  t.CWL + t.BL + t.WR + t.RP;


    public Rank(Parameters param, int id)
    {
        CONFIG = t = param;
        ID = id;
        Groups = new BankGroup[CONFIG.NUM_BANK_GROUPS];
        for (int i = 0; i < CONFIG.NUM_BANK_GROUPS; i++)
        {
            Groups[i] = new BankGroup(param, ID, i);
        }
    }

    public Rank()
    {
    }

    public void Initialize()
    {
        SetTimingConstraints();
        LastActivate = 0;

        NextFAW = -3 * t.FAW;

        PAST_FOUR_ACTS = new Queue<long>(4);
        PAST_FOUR_ACTS.Enqueue(-t.FAW);
        PAST_FOUR_ACTS.Enqueue(-t.FAW);
        PAST_FOUR_ACTS.Enqueue(-t.FAW);
        PAST_FOUR_ACTS.Enqueue(-t.FAW);

        GAP_between_Activates = new AverageObject();
        GAP_between_Refreshes = new AverageObject();

        Stat = new RankStat();
        Stat.Initialize();

        Stat.ACT = 0;
        for (int G = 0; G < CONFIG.NUM_BANK_GROUPS; G++)
        {
            Groups[G].Initialize();
        }
    }

    public long Cycle
    {
        get => cycle;
        set
        {
            cycle = value;
            for (int i = 0; i < CONFIG.NUM_BANK_GROUPS; i++)
            {
                Groups[i].Cycle = value;
            }
            CountDown -= t.ClockFactor;
            if (IsRefreshing && CountDown <= 0)
                IsRefreshing = false;
        }
    }

    protected void SetTimingConstraints()
    {
        RD_WR = t.CL + t.BL + t.RTRS - t.CWL;
        RDA_WR = t.CL + t.BL + t.RTRS - t.CWL;
        WR_RD = t.CWL + t.BL + t.WTRS;
        WR_PD = t.CWL + t.BL + t.WR;
        WR_PREA = t.CWL + t.BL + t.WR;
        WRA_REF = t.CWL + t.BL + t.WR + t.RP;

        SIBLING_RD_RD = t.BL + t.RTRS;
        SIBLING_RD_WR = t.CL + t.BL + t.RTRS - t.CWL;
        SIBLING_RDA_RD = t.BL + t.RTRS;
        SIBLING_RDA_WR = t.CL + t.BL + t.RTRS - t.CWL;
        SIBLING_WR_RD = t.CWL + t.BL + t.RTRS - t.CL;

    }

    #region RAS

    public void PRECHARGE(MemoryAddress mAddr)
    {
        //t[PRE - REF = t.RP; //t[PRE - SRE = t.RP;
        //DEBUG.Assert(STATE == State.IDLE || STATE == State.ACTIVE);
        //CountDown = t.RP;

        long temp = cycle + t.RP;

        NextREF = Math.Max(temp, NextREF);
        NextSRE = Math.Max(temp, NextSRE);

        Stat.PRE++;

        Groups[mAddr.BankGroup].PRECHARGE(mAddr);
    }

    public bool PRECHARGE(int group, int bank)
    {
        long temp = cycle + t.RP;

        NextREF = Math.Max(temp, NextREF);
        NextSRE = Math.Max(temp, NextSRE);

        Stat.PRE++;

        Groups[group].PRECHARGE(bank);
        return true;
    }

    public void PRE_ALL()
    {

        //DEBUG.Assert(STATE == State.IDLE || STATE == State.ACTIVE);

        CountDown = t.RP;
        long temp = cycle + t.RP;

        NextACT = Math.Max(temp, NextACT);
        NextREF = Math.Max(temp, NextREF);
        NextSRE = Math.Max(temp, NextSRE);

        Stat.PRE = Stat.PRE + t.NUM_BANKS_TOTAL;

        for (int i = 0; i < Groups.Length; i++)
        {
            Groups[i].PRE_ALL();
        }
    }

    public void ACTIVATE(MemoryAddress mAddr, Operation opr)
    {
        PAST_FOUR_ACTS.Dequeue();
        PAST_FOUR_ACTS.Enqueue(cycle);

        DEBUG.Assert(PAST_FOUR_ACTS.Count == 4);

        long first = PAST_FOUR_ACTS.Peek();
        NextFAW = Math.Max(first + t.FAW, NextFAW);

        //DEBUG.Print($"{cycle}: NextFAW => {NextFAW} [{faws()}]");

        NextACT = Math.Max(cycle + t.RRDS, NextACT);  //t.RRD;
        NextREF = Math.Max(cycle + t.RC, NextREF);    //t.RC;
        NextPDE = Math.Max(cycle + 1, NextPDE);       //1;
        NextPREA = Math.Max(cycle + t.RAS, NextPREA); //t.RAS;

        Stat.ACT++;
        //long gap = cycle - LastActivate;
        GAP_between_Activates.Add(cycle - LastActivate);
        LastActivate = cycle;
        Groups[mAddr.BankGroup].ACTIVATE(mAddr, opr);

    }

    #endregion

    #region CAS

    public void READ(MemoryAddress mAddr)
    {
        Stat.NUM_READS++;

        //long read_to_read = cycle + t.CCDS;
        //long read_to_write = cycle + RD_WR;

        NextRD = Math.Max(cycle + t.CCDS, NextRD);
        NextWR = Math.Max(cycle + RD_WR, NextWR);
        NextPREA = Math.Max(cycle + t.RTP, NextPREA);
        NextPDE = Math.Max(cycle + t.CL + t.BL + 1, NextPDE);

        Groups[mAddr.BankGroup].READ(mAddr);
    }
    public void READ_AP(MemoryAddress mAddr)
    {

        //long RDA_to_read = cycle + t.CCDS;
        //long RDA_write = cycle + RDA_WR;

        NextRD = Math.Max(cycle + t.CCDS, NextRD);
        NextWR = Math.Max(cycle + RDA_WR, NextWR);

        NextPDE = Math.Max(cycle + t.CL + t.BL + 1, NextPDE);
        NextREF = Math.Max(cycle + t.RTP + t.RP, NextREF);

        Groups[mAddr.BankGroup].READ_AP(mAddr);
        Stat.NUM_READS++;
        //Stat.NUM_PRES++;

    }

    public void WRITE(MemoryAddress mAddr)
    {

        //long WR_WR = cycle + t.CCDS;
        NextWR = Math.Max(cycle + t.CCDS, NextWR);

        //long write_to_read = cycle + WR_RD; // t.CWL + t.BL + t.WTRS;
        NextRD = Math.Max(cycle + WR_RD, NextRD);

        long temp = cycle + WR_PD; // => WR_PREA // t.CWL + t.BL + t.WR;

        NextPDE = Math.Max(temp, NextPDE);
        NextPREA = Math.Max(temp, NextPREA);

        Stat.NUM_WRITES++;
        Groups[mAddr.BankGroup].WRITE(mAddr);

    }
    public void WRITE_AP(MemoryAddress mAddr)
    {
        //long write_to_write = cycle + t.CCDS;
        NextWR = Math.Max(cycle + t.CCDS, NextWR);

        //long write_to_read = cycle + WR_RD; // t.CWL + t.BL + t.WTRS;
        NextRD = Math.Max(cycle + WR_RD, NextRD);

        NextPDE = Math.Max(cycle + WR_PD + 1, NextPDE);   //t.CWL + t.BL + t.WR + 1 , NextPDE);
        NextREF = Math.Max(cycle + WRA_REF, NextREF);     //t.CWL + t.BL + t.WR + t.RP, NextREF);

        Groups[mAddr.BankGroup].WRITE_AP(mAddr);
        Stat.NUM_WRITES++;
    }

    #endregion

    #region PD
    public void POWER_UP()
    {
        long temp = cycle + t.XP;

        NextRD = Math.Max(temp, NextRD);
        NextWR = Math.Max(temp, NextWR);
        NextPREA = Math.Max(temp, NextPREA);
        NextPDE = Math.Max(temp, NextPDE);

        NextACT = Math.Max(temp, NextACT);
        NextPRE = Math.Max(temp, NextPRE);
        NextREF = Math.Max(temp, NextREF);
        NextSRE = Math.Max(temp, NextSRE);

        Stat.PDX++;

    }
    public void POWER_DOWN()
    {
        NextPDX = Math.Max(cycle + t.PD, NextPDX);

        Stat.PDE++;

    }

    #endregion

    #region REF

    public void rank_REFRESH()
    {
        NextREF = Math.Max(cycle + t.RFC, NextREF);
        NextPDE = Math.Max(cycle + 1, NextPDE);
        NextACT = Math.Max(cycle + t.RFC, NextACT);
        CountDown = t.RFC;

        IsRefreshing = true;
        Stat.REF++;

        for (int i = 0; i < Groups.Length; i++)
            Groups[i].Refresh();

    }

    public void SREF()
    {
        long temp = cycle + t.XS; ;

        NextACT = Math.Max(temp, NextACT);
        NextPDE = Math.Max(temp, NextPDE);

        NextSRX = Math.Max(cycle + t.CKESR, NextSRX);

        NextSRE = Math.Max(temp, NextSRE);
        NextREF = Math.Max(temp, NextREF);
        NextPRE = Math.Max(temp, NextPRE);

    }

    #endregion

    public void UpdateStat()
    {
        //BankState state = Banks[0][0].STATE;

        State state = Groups[0].Banks[0].STATE;

        if (state == State.PRE_PDNS) Stat.TS_PRE_PDNS += CONFIG.ClockFactor;
        else if (state == State.PRE_PDNF) Stat.TS_PRE_PDNF += CONFIG.ClockFactor;
        else if (state == State.ACT_PDNF)
        {
            Stat.TS_ACT_PDNF += CONFIG.ClockFactor;
            Stat.TS_ACT_PDN += CONFIG.ClockFactor;
        }
        else if (state == State.ACT_PDNS)
        {
            Stat.TS_ACT_PDNS += CONFIG.ClockFactor;
            Stat.TS_ACT_PDN += CONFIG.ClockFactor;
        }
        else
        {
            for (int G = 0; G < CONFIG.NUM_BANK_GROUPS; G++)
            {
                for (int B = 0; B < CONFIG.NUM_BANKS; B++)
                {
                    if (Groups[G].Banks[B].STATE == State.ACTIVE)
                    {
                        Stat.TS_ACT_Standby += CONFIG.ClockFactor;
                        break;
                    }
                }
                Stat.TS_PowerUp += CONFIG.ClockFactor;
            }

        }

    }

    #region Rules

    public bool CanNotRead_Act => (NextRD > cycle) && (Cycle < NextACT || cycle < NextFAW);
    public bool CanNotWrite_Act => (NextWR > cycle) && (Cycle < NextACT || cycle < NextFAW);

    public bool CanPRE(MemoryAddress mAddr) => Groups[mAddr.BankGroup].CanPRE(mAddr);
    public bool CanPRE(int group, int bank) => Groups[group].CanPRE(bank);

    public bool CanActivate(MemoryAddress mAddr) => (Cycle < NextACT || cycle < NextFAW) ? false : Groups[mAddr.BankGroup].CanActivate(mAddr);

    public bool CanRead(MemoryAddress mAddr)
    {
        //if (mAddr.Bank == 6)
        //    DEBUG.Print($"\t*** {cycle}: [NextRD: {NextRD}] Rank.CanRead => {cycle >= NextRD}");

        return (cycle < NextRD) ? false : Groups[mAddr.BankGroup].CanRead(mAddr);
    }
    public bool CanWrite(MemoryAddress mAddr) => (cycle < NextWR) ? false : Groups[mAddr.BankGroup].CanWrite(mAddr);

    public bool CAN_REF_RANK() => cycle >= NextREF;

    public bool CanPowerUp(MemoryAddress mAddr)
    {
        return true;
    }



    #endregion

}

