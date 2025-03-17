using SimulatorLib.Common;
using SimulatorLib.DDR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorLib;

public class DefaultSettings
{
    public Parameters Param;
    public DefaultSettings()
    {
        Param = new Parameters();
    }



    public void Initialize()
    {
        #region CPU
        Param.NUMCORES = 1;

        Param.PROCESSOR_FREQ = 3200;
        Param.ROBSIZE = 128;

        Param.MAX_RETIRE = 3;
        Param.MAX_FETCH = 3;

        Param.PIPELINEDEPTH = 10;
        Param.MSHR_MAX = 32;
        Param.WriteBackQ_MAX = 32;
        Param.RDWRQ_MAX = 32;

        Param.WriteBack = true;
        Param.ISSUE_DUPLICATE_REQUEST = true;

        Param.ADDRESS_BITS = 32;// total number of address bits (i.e. indicates size of memory)

        Param.PrefetchSize = 8; // 8n prefetch DDR
        Param.ChannelWidth = 64;

        int Max_Address;

        //public int Max_Address => 8 * 3 * 16 * 10;

        #endregion

        #region MEM_CTRL

        Param.NUM_CHANNELS = 1;

        //Param.TranslationMethod ADDRESS_MAPPING; // 1;
        Param.MAPPING_METHOD = PageMappingMethod.RANDOM;

        Param.OPEN_PAGE_POLICY = true;
        //Param.WQ_LOOKUP_LATENCY;
        Param.REFRESH_MODE = REF_MODE.RANK;

        Param.READQ_MAX = 64;
        Param.WRITEQ_MAX = 64;


        Param.copy_gran = 128; // # of cachelines (128 = 8KB)

        Param.INCLUDE_XBAR_LATENCY = false;
        Param.XBAR_LATENCY = 16;


        #endregion

        InitPower();
        InitTiming();
        SET_TIMING_CONSTANTS();
    }

    void InitPower()
    {
        #region Power

        Param.IDD0 = 110;   //</IDD0>
        Param.IDD02 = 0;    //</IDD02>
        Param.IDD2P0 = 12;  //</IDD2P0>
        Param.IDD2P1 = 40;  //</IDD2P1>
        Param.IDD2N = 42;   //</IDD2N>
        Param.IDD3P = 0;    // </IDD3P>
        Param.IDD3P0 = 45;  //</IDD3P0>
        Param.IDD3P1 = 45;  //</IDD3P1>
        Param.IDD3N = 45;   //</IDD3N>
        Param.IDD4R = 270;  //</IDD4R>
        Param.IDD4W = 280;  //</IDD4W>
        Param.IDD5 = 215;   //</IDD5>
        Param.IDD6 = 12;    //</IDD6>
        Param.IDD62 = 0;    //</IDD62>
        Param.VDD = 1.5f;    //</VDD>
        Param.VDD2 = 0; //</VDD2>

        #endregion

    }
    private void InitTiming()
    {
        Param.DQ = 8; //</DQ>
        int SIZE = 4096;

        Param.RATE = 1600; //<RATE>1600</RATE>

        Param.DRAM_CLK_FREQUENCY = 800;//</FREQ>
        Param.TCK = 1.25;//</TCK> 
        int ClockFactor = 4;

        Param.ClockFactor = ClockFactor;


        Param.NUM_RANKS = 1;//<Rank>1</Rank>
        Param.NUM_BANK_GROUPS = 1;
        Param.NUM_BANKS = 8;//<Bank>8</Bank>
        Param.NUM_BANKS_TOTAL = 8;//<Bank>8</Bank>

        Param.NUM_ROWS = 65536;//<Row>65536</Row>
        Param.NUM_COLUMNS = 1024;//<Column>1024</Column>


        Param.CHIPS_PER_RANK = 8;
        Param.tx = (Param.PrefetchSize * Param.ChannelWidth / 8);
        Param.tx_bits = (int)Math.Log(Param.tx, 2);
        Param.channelBits = (int)Math.Log(1, 2);
        Param.rankBits = (int)Math.Log(1, 2);
        Param.bankBits = (int)Math.Log(8, 2);
        Param.bankGBits = (int)Math.Log(1, 2);
        Param.rowBits = (int)Math.Log(65536, 2);
        Param.colBits = (int)Math.Log(1024, 2);
        Param.colBits -= (int)Math.Log(Param.PrefetchSize, 2);
        //Param.Max_Address = (Param.ChannelWidth / 8) * channelBits * rankBits * bankGBits * bankBits * rowBits * colBits;

        Param.CKESR = ClockFactor * 5;

        Param.BL = ClockFactor * 4;
        Param.CL = ClockFactor * 11;
        Param.CAS = ClockFactor * 11;

        Param.CWL = ClockFactor * 8;

        Param.RAS = ClockFactor * 28;
        Param.RC = ClockFactor * 39;
        Param.RCD = ClockFactor * 11;

        Param.RP = ClockFactor * 11;
        Param.RTP = ClockFactor * 6;
        Param.RTRS = ClockFactor * 2;
        Param.WR = ClockFactor * 12;

        Param.XP = ClockFactor * 5;
        Param.XP_DLL = ClockFactor * 20;

        //Param.FAW = ClockFactor * FAW;
        //Param.RFC = ClockFactor * RFC;
        Param.REFI = ClockFactor * 6240;

        Param.XSDLL = ClockFactor * 512;

        Param.CCDS = 4;     //</CCD>
        Param.CCDL = 4;     //</CCD>

        Param.RRDS = 0;     //</RRD>
        Param.RRDL = 0;     //</RRD>

        Param.WTRL = 6;     //</WTR>
        Param.WTRS = 6;     //</WTR>


        Param.PD = ClockFactor * 4;

        Param.TRACE_TYPE = TraceType.DRAM;

        int PAGE = (Param.DQ * Param.NUM_COLUMNS >> 13);

        (Param.RRDS, Param.RRDL, Param.FAW) = (PAGE == 1)
            ? (ClockFactor * 5, ClockFactor * 5, ClockFactor * 24)
            : (ClockFactor * 6, ClockFactor * 6, ClockFactor * 32);

        int DENSITY = SIZE; // org_entry.size;

        (Param.RFC, Param.XS) = (ClockFactor * 208, ClockFactor * 216);


    }

    #region TIMING CONSTANTS

    void SET_TIMING_CONSTANTS()
    {
        Param.RDA_delay = Param.RTP + Param.RP;
        Param.WRA_delay = Param.CWL + Param.WR + Param.RP;


        //burst_cycle = (BL == 0) ? 0 : BL / 2;
        Param.RL = Param.AL + Param.CL;
        Param.WL = Param.AL + Param.CWL;

        Param.RD_delay = Param.RL + Param.BL;// burst_cycle;
        Param.WR_delay = Param.WL + Param.BL;//burst_cycle;

        Param.RD_TO_RD_L = Math.Max(Param.BL, Param.CCDL);
        Param.RD_TO_RD_S = Math.Max(Param.BL, Param.CCDS);
        Param.RD_TO_RD_o = Param.BL + Param.RTRS;
        Param.RD_TO_WR = Param.RL + Param.BL - Param.WL + Param.RTRS;

        Param.RD_TO_WR_o = Param.RD_delay + Param.BL + Param.RTRS - Param.WR_delay;

        Param.RD_TO_PRE = Param.AL + Param.RTP;
        Param.RDA_TO_ACT = Param.AL + Param.BL + Param.RTP + Param.RP;


        Param.WR_TO_RD_L = Param.WR_delay + Param.WTRL;
        Param.WR_TO_RD_S = Param.WR_delay + Param.WTRS;
        Param.WR_TO_RD_o = Param.WR_delay + Param.BL + Param.RTRS - Param.RD_delay;

        Param.WR_TO_WR_L = Math.Max(Param.BL, Param.CCDL);
        Param.WR_TO_WR_S = Math.Max(Param.BL, Param.CCDS);
        Param.WR_TO_WR_o = Param.BL;
        Param.WR_TO_PRE = Param.WL + Param.BL + Param.WR;

        Param.PRE_TO_ACT = Param.RP;

        Param.RD_TO_ACT = Param.RD_TO_PRE + Param.PRE_TO_ACT;
        Param.WR_TO_ACT = Param.WR_TO_PRE + Param.PRE_TO_ACT;

        Param.ACT_TO_ACT = Param.RC;
        Param.ACT_TO_ACT_L = Param.RRDL;
        Param.ACT_TO_ACT_S = Param.RRDS;
        Param.ACT_TO_PRE = Param.RAS;

        Param.ACT_TO_RD = Param.RCD - Param.AL;
        Param.ACT_TO_WR = Param.RCD - Param.AL;

        Param.ACT_TO_REF = Param.RC;  // need to PRE before ref, so it's tRC


        // TODO: deal with different REF rate
        Param.REF_TO_REF = Param.REFI;  // REF intervals (per rank level)

        Param.REF_TO_ACT = Param.RFC;  // tRFC is defined as ref to ACT

        //int REF_TO_ACT_bank = RFCb;

        // int self_REF_entry_TO_exit = CKESR;
        // int self_REF_exit = XS;
        // int powerdown_TO_exit = CKE;
        // int powerdown_exit = XP;

        if (Param.NUM_BANK_GROUPS == 1)
        {
            Param.RD_TO_RD_L = Math.Max(Param.BL, Param.CCDS);
            Param.WR_TO_RD_L = Param.WR_delay + Param.WTRS;
            Param.WR_TO_WR_L = Math.Max(Param.BL, Param.CCDS);
            Param.ACT_TO_ACT_L = Param.RRDS;
        }

    }

    #endregion


}
