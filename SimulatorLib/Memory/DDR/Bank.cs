using System;
using SimulatorLib.Common;
 

namespace SimulatorLib.DDR;

public partial class Bank
{
    protected long cycle;

    public Parameters t;
    public int ID;
    public int Index;
    public int GroupID;
    public int RankID;


    public BankStat Stat;

    public State STATE;
    public State NextState;

    public long CountDown = 0;

    public Bank()
    {
        Initialize();
    }
    public Bank(Parameters param, int rank, int group, int id)
    {
        t = param;
        RankID = rank;
        GroupID = group;
        ID = id;
        Index = group * param.NUM_BANK_GROUPS + id;
        Stat = new BankStat();
        //Initialize();
    }

    public void Initialize()
    {
        STATE = State.IDLE;
        ActiveRow = -1;

        SET_TIMING_CONSTANTS();

    }

    public long Cycle
    {
        get => cycle;
        set
        {
            cycle = value;
            CountDown -= t.ClockFactor;
            UpdateState();
        }
    }

    private void UpdateState()
    {

        if (CountDown > 0)
            return;

        STATE = NextState;
        IsBusy = false;

    }

    public bool IsBusy;

    #region Nexts

    public int ActiveRow = -1;

    public long NextPRE = 0;
    public long NextACT = 0;

    public long NextRD = 0;
    public long NextWR = 0;

    public long NextRDA = 0;
    public long NextWRA = 0;

    #endregion

    #region TIMING

    public int RL;
    public int WL;

    public int RD_delay;
    public int WR_delay;

    public int RDA_delay;
    public int WRA_delay;

    public int WR_TO_PRE;
    public int RDA_TO_ACT;
    public int WRA_TO_ACT;

    public int RD_CountDown;
    public int WR_CountDown;

    protected void SET_TIMING_CONSTANTS()
    {

        RL = t.AL + t.CL;
        WL = t.AL + t.CWL;

        RD_delay = RL + t.BL;
        WR_delay = WL + t.BL;

        RDA_delay = t.RTP + t.RP;
        WRA_delay = t.CWL + t.WR + t.RP;

        WR_TO_PRE = t.CWL + t.BL + t.WR;

        RDA_TO_ACT = t.RTP + t.RP;
        WRA_TO_ACT = t.CWL + t.BL + t.WR + t.RP;

        RD_CountDown = Math.Min(Math.Min(t.CL, t.CCDL),t.RTP);  
        WR_CountDown = Math.Min(t.CWL, t.CCDL);

    }

    #endregion

    #region RAS

    public bool PRECHARGE()
    {

        DEBUG.Assert(STATE == State.IDLE || STATE == State.ACTIVE);
        DEBUG.Assert(CanPRE);

        ActiveRow = -1;

        CountDown = t.RP;
        NextACT = Math.Max(cycle + t.RP, NextACT);

        STATE = State.PRECHARGING;
        NextState = State.IDLE;

        Stat.PRE++;

        IsBusy = true;
        CAS_Issued = false;

        return true;
    }

    public bool CAS_Issued = false;
    public bool ACTIVATE(MemoryAddress mAddr, Operation opr)
    {

        DEBUG.Assert(mAddr.BankGroup == GroupID && mAddr.Bank == ID);
        DEBUG.Assert(STATE == State.IDLE);
        DEBUG.Assert(CanActivate);

        //DEBUG.Print($"\t\t=>ACT({GroupID},{ID})[{mAddr.Row}]");

        ActiveRow = mAddr.Row;

        CountDown = t.RCD;

        long tmp = cycle + t.RCD;

        NextRD = Math.Max(tmp, NextRD);
        NextWR = Math.Max(tmp, NextWR);
        NextRDA = Math.Max(tmp, NextRDA);
        NextWRA = Math.Max(tmp, NextWRA);

        NextACT = Math.Max(cycle + t.RC, NextACT);
        NextPRE = Math.Max(cycle + t.RAS, NextPRE);


        STATE = State.ACTIVATING;
        NextState = State.ACTIVE;

        if (opr == Operation.READ)
        {
            Stat.ACT_READ++;
        }
        else
        {
            Stat.ACT_WRITE++;
        }

        IsBusy = true;
        CAS_Issued = false;
        return true;

    }

    #endregion

    #region CAS

    public virtual void READ(MemoryAddress mAddr)
    {
        DEBUG.Assert(mAddr.BankGroup == GroupID && mAddr.Bank == ID);
        DEBUG.Assert(STATE == State.ACTIVE);
        DEBUG.Assert(CanRead(mAddr.Row));

        CountDown = RD_CountDown;
        IsBusy = true;
        CAS_Issued = true;

        NextPRE = Math.Max(cycle + t.RTP, NextPRE);
        STATE = State.READING;
        NextState = State.ACTIVE;
        Stat.READ++;

    }

    public virtual  bool WRITE(MemoryAddress mAddr)
    {
        DEBUG.Assert(mAddr.BankGroup == GroupID && mAddr.Bank == ID);
        DEBUG.Assert(STATE == State.ACTIVE);
        DEBUG.Assert(CanWrite(mAddr.Row));

        CountDown = WR_CountDown;
        IsBusy = true;
        CAS_Issued = true;

        NextPRE = Math.Max(cycle + WR_TO_PRE, NextPRE); //t[WR<->PRE]  = t.CWL + t.BL + t.WR});
        Stat.WRITE++;
        STATE = State.WRITING;
        NextState = State.ACTIVE;

        return true;
    }

    public bool READ_AP(MemoryAddress mAddr)
    {

        DEBUG.Assert(mAddr.BankGroup == GroupID && mAddr.Bank == ID);
        DEBUG.Assert(STATE == State.ACTIVE);
        DEBUG.Assert(CanRead(mAddr.Row));

        ActiveRow = -1;

        CountDown = RDA_TO_ACT;
        IsBusy = true;

        NextACT = Math.Max(cycle + RDA_TO_ACT, NextACT); //t[RDA <-> ACT] = t.RTP + t.RP});

        Stat.READ++;

        STATE = State.READING_AP;
        NextState = State.IDLE;

        Stat.PRE++;

        return true;
    }

    public bool WRITE_AP(MemoryAddress mAddr)
    {

        DEBUG.Assert(mAddr.BankGroup == GroupID && mAddr.Bank == ID);
        DEBUG.Assert(STATE == State.ACTIVE);
        DEBUG.Assert(CanWrite(mAddr.Row));

        ActiveRow = -1;

        CountDown = WRA_TO_ACT;
        IsBusy = true;

        NextACT = Math.Max(cycle + WRA_TO_ACT, NextACT); //[WRA<->ACT] = t.CWL + t.BL + t.WR + t.RP});
        STATE = State.WRITING_AP;
        NextState = State.IDLE;

        Stat.WRITE++;
        Stat.PRE++;

        return true;
    }

    #endregion

    public void Refresh()
    {
        DEBUG.Assert(!IsBusy);
        DEBUG.Assert(STATE == State.IDLE);

        CountDown = t.RFC;
        IsBusy = true;
        CAS_Issued = false;

        STATE = State.REFRESHING;
        NextState = State.IDLE;

    }


    #region Rules

    public bool CanPRE => cycle >= NextPRE;
    public bool CanActivate
    {
        get
        {
            if (Cycle < NextACT)
            {
                //EBUG.Print($"\t\tCanActivate({GroupID}{ID}) | Cycle: {Cycle} >= NextACT: {NextACT} => {Cycle >= NextACT}");
                return false;
            }
            //DEBUG.Print($"\t\tCanActivate({GroupID}{ID}) | TRUE");
            return true;
        }

    }

    //=> cycle >= NextACT;

    public virtual bool CanRead(int row) // => ActiveRow == row && cycle >= NextRD;
    {
        //if (ID == 6)
        //    DEBUG.Print($"\t*** {cycle}: [NextRD: {NextRD}] Bank.CanRead => {cycle >= NextRD}");

        return ActiveRow == row && cycle >= NextRD;
    }
    public virtual bool CanWrite(int row) => ActiveRow == row && cycle >= NextWR;
    public bool CanReadA(int row) => ActiveRow == row && cycle >= NextRDA;
    public bool CanWriteA(int row) => ActiveRow == row && cycle >= NextWRA;

    #endregion

}


/*** DDR 4 Bank ***/
//t = timing[int(Level::Bank)];

// CAS <-> RAS
//t[ACT <-> RD ] = t.RCD});
//t[ACT <-> RDA] = t.RCD});
//t[ACT <-> WR ] = t.RCD});
//t[ACT <-> WRA] = t.RCD});
//t[ACT <-> ACT] = t.RC});
//t[ACT <-> PRE] = t.RAS});

//t[RD  <-> PRE] = t.RTP});
//t[RDA <-> ACT] = t.RTP + t.RP});

//t[WR  <-> PRE] 1, t.CWL + t.BL + t.WR});
//t[WRA  <-> ACT] = t.CWL + t.BL + t.WR + t.RP});

// RAS <-> RAS
//t[PRE <-> ACT] = t.RP});