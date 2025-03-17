using SimulatorLib;
using SimulatorLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorLib.DDR;

public class BankGroup
{
    protected Parameters t;
    public Bank[] Banks;
    protected int RankID;
    protected int ID;

    public BankGroup(Parameters param, int rank, int id)
    {
        t = param;
        RankID = rank;
        ID = id;
        Banks = new Bank[param.NUM_BANKS];


        for (int i = 0; i < param.NUM_BANKS; i++)
        {
            Banks[i] = new Bank(param, RankID, ID, i);
        }
        WR_RD = t.CWL + t.BL + t.WTRL;
    }

    public BankGroup()
    {
    }

    protected long cycle;

    public long Cycle
    {
        get => cycle;
        set
        {
            cycle = value;
            for (int B = 0; B < Banks.Length; B++)
            {
                Banks[B].Cycle = value;
            }
        }
    }


    public long CountDown = 0;


    public long NextACT = 0;
    public long NextRD = 0;
    public long NextWR = 0;

    protected int WR_RD;

    public void Initialize()
    {
        for (int i = 0; i < t.NUM_BANKS; i++)
        {
            Banks[i].Initialize();
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < Banks.Length; i++)
        {
            Banks[i].Refresh();
        }
    }


    #region RAS

    public void PRECHARGE(MemoryAddress mAddr)
    {
        Banks[mAddr.Bank].PRECHARGE();
    }

    public bool PRECHARGE(int bank)
    {
        Banks[bank].PRECHARGE();
        return true;
    }

    public void PRE_ALL()
    {
        for (int i = 0; i < Banks.Length; i++)
        {
            Banks[i].PRECHARGE();
        }
    }

    public void ACTIVATE(MemoryAddress mAddr, Operation opr)
    {
        //t[ACT - ACT] => t.nRRDL;

        NextACT = Math.Max(cycle + t.RRDL, NextACT);
        Banks[mAddr.Bank].ACTIVATE(mAddr, opr);
    }

    #endregion

    #region CAS

    public void READ(MemoryAddress mAddr)
    {

        NextRD = Math.Max(cycle + t.CCDL, NextRD);//long RD_RD = cycle + t.CCDL;
        Banks[mAddr.Bank].READ(mAddr);

    }

    public void READ_AP(MemoryAddress mAddr)
    {

        NextRD = Math.Max(cycle + t.CCDL, NextRD);        //long RDA_RD = cycle + t.CCDL;
        Banks[mAddr.Bank].READ_AP(mAddr);

    }

    public void WRITE(MemoryAddress mAddr)
    {

        NextWR = Math.Max(cycle + t.CCDL, NextWR);//long WR_WR = cycle + t.CCDL;
        NextRD = Math.Max(cycle + WR_RD, NextRD);//long WR_RD = cycle + t.CWL + t.BL + t.WTRL;

        Banks[mAddr.Bank].WRITE(mAddr);

    }

    public void WRITE_AP(MemoryAddress mAddr)
    {

        NextWR = Math.Max(cycle + t.CCDL, NextWR); //long WR_WR = cycle + t.CCDL;
        NextRD = Math.Max(cycle + WR_RD, NextRD);  //long WR_RD = cycle + t.CWL + t.BL + t.WTRL;

        Banks[mAddr.Bank].WRITE_AP(mAddr);
    }

    #endregion

    #region Rules

    public bool CanNotRead_Act => NextRD > cycle && cycle < NextACT;
    public bool CanNotWrite_Act => NextWR > cycle && cycle < NextACT;

    public bool CanPRE(MemoryAddress mAddr) => Banks[mAddr.Bank].CanPRE;
    public bool CanPRE(int bank) => Banks[bank].CanPRE;

    public bool CanActivate(MemoryAddress mAddr) => (Cycle < NextACT) ? false : Banks[mAddr.Bank].CanActivate;

    public bool CanRead(MemoryAddress mAddr)
    {
        //if (mAddr.Bank == 6)
        //    DEBUG.Print($"\t*** {cycle}: [NextRD: {NextRD}] Group.CanRead => {cycle >= NextRD}");

        return (cycle < NextRD) ? false : Banks[mAddr.Bank].CanRead(mAddr.Row);
    }

    public bool CanWrite(MemoryAddress mAddr) => (cycle < NextWR) ? false : Banks[mAddr.Bank].CanWrite(mAddr.Row);


    #endregion

}

