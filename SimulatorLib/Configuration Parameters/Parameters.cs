using System;
using System.Collections.Generic;
using SimulatorLib.Common;

namespace SimulatorLib;

public class Parameters
{

    #region Mapping

    public int PrefetchSize = 8; // 8n prefetch DDR
    public int ChannelWidth = 64;

    public int tx;

    public int tx_bits;
    public int channelBits;
    public int rankBits;
    public int bankBits;
    public int bankGBits;
    public int rowBits;
    public int colBits;

    #endregion
    public Parameters() { }
    public Parameters(CPU_Parameters cpu_param, DDR_Parameters ddr_param, MemCtrl_Parameters memCtrl_param, DeActivator_Parameters deactivator_param)
    {
        Initialize(cpu_param, ddr_param, memCtrl_param, deactivator_param);
    }

    public string ID;

    public MemoryType DDR_TYPE;
    public string ORG_ID;
    public string DDR_ID;

    public int CHIPS_PER_RANK;

    public int Max_Address => (ChannelWidth / 8) * channelBits * rankBits * bankGBits * bankBits * rowBits * colBits;

    #region Memory

    // cache-line size (ints)
    //public int CACHELINESIZE;// 64;

    public string MemoryID;
    public string MemType;

    public int DQ;
    public int WiDTH;
    public int NUM_RANKS;
    public int NUM_BANK_GROUPS;
    public int NUM_BANKS;
    public int NUM_BANKS_TOTAL;

    public int NUM_ROWS;
    public int NUM_COLUMNS;

    public int RATE;

    public int RCD;
    public int RP;
    public int CAS;
    public int RAS;
    public int RC;
    public int CWL;
    public int WR;

    //public int RRD ;
    //public int CCD ;
    //public int WTR ;

    public int RTRS;
    public int BL;
    public int RTP;
    public int XP;
    public int XP_DLL;
    public int CKESR;
    public int PD;
    public int FAW;
    public int REFI;
    public int RFC;
    public int CL;
    public int AL;

    public int XS;
    public int XSDLL;
    public int CCDS;
    public int RRDS;
    public int WTRS;
    public int CCDL;
    public int RRDL;
    public int WTRL;


    public float DRAM_CLK_FREQUENCY;
    public double TCK;
    public float VDD;
    public float VDD2;
    public float IDD0;
    public float IDD02;
    public float VDD1;
    public float IDD1;
    public float IDD2P0;
    public float IDD2P1;
    public float IDD2P;
    public float IDD2N;
    public float IDD3P;
    public float IDD3P0;
    public float IDD3P1;
    public float IDD3N;
    public float IDD4R;
    public float IDD4W;
    public float IDD5;
    public float IDD6;
    public float IDD62;

    #endregion

    #region MEM_CTRL

    public int NUM_CHANNELS;

    public TranslationMethod ADDRESS_MAPPING; // 1;

    public bool OPEN_PAGE_POLICY = true;
    public int WQ_LOOKUP_LATENCY;
    public REF_MODE REFRESH_MODE = REF_MODE.RANK;

    public int READQ_MAX = 64;
    public int WRITEQ_MAX = 64;

    public PageMappingMethod MAPPING_METHOD = PageMappingMethod.RANDOM;

    public int copy_gran = 128; // # of cachelines (128 = 8KB)

    public bool INCLUDE_XBAR_LATENCY = true;
    public int XBAR_LATENCY = 16;


    #endregion

    #region CPU

    #region Proc
    public int NUMCORES;
    public int ClockFactor;

    public int PROCESSOR_FREQ;
    public int ROBSIZE;

    public int MAX_RETIRE;
    public int MAX_FETCH;

    public int PIPELINEDEPTH;
    public int MSHR_MAX = 32;
    public int WriteBackQ_MAX = 32;
    public int RDWRQ_MAX = 32;

    public bool WriteBack = true;
    public bool ISSUE_DUPLICATE_REQUEST = true;

    public int ADDRESS_BITS = 32;// total number of address bits (i.e. indicates size of memory)

    // Traces with cloning and setting calls
    public bool b_read_rc_traces = false;
    public bool stats_exclude_cpy = false;

    #endregion

    #region CACHE
    public bool DEBUG_CACHE;
    public bool CACHE_ENABLED = false;        // Turn on cache?= false;        // Turn on cache?


    public int CACHE_BLOCK_SIZE_BITS;    // Cache block size in power of 2^6; 

    public bool SHARED_LLC_CACHE_ONLY;     // If the cache is one, is it a shared LLC?= false;// If the cache is one, is it a shared LLC?
    public int L1CACHE_SIZE_BITS = 15;   //L1 cache size in power of 2. Ex: 15->32KB
    public int L1CACHE_ASSOCIATIVITY = 8;
    public int L1CACHE_HIT_LATENCY = 4;

    public bool SHARED_L2CACHE = true;
    public int L2CACHE_SIZE_BITS = 22;  //L2 cache size in power of 2.
    public int L2CACHE_ASSOCIATIVITY = 8;
    public int L2CACHE_HIT_LATENCY = 20;


    public bool SHARED_L3CACHE = false;
    public int L3CACHE_SIZE_BITS = 32;
    public int L3CACHE_ASSOCIATIVITY = 8;
    public int L3CACHE_HIT_LATENCY = 32;


    #endregion



    #endregion

    public void Initialize(CPU_Parameters cpu_param, DDR_Parameters ddr_param, MemCtrl_Parameters memCtrl_param, DeActivator_Parameters deactivator_param)
    {
        ClockFactor = (int)(cpu_param.PROCESSOR_FREQ / ddr_param.FREQ);


        #region MEMORY
        ID = ddr_param.ID;

        DDR_TYPE = ddr_param.DDR_TYPE;
        ORG_ID = ddr_param.ORG_ID;
        DDR_ID = ddr_param.DDR_ID;

        //ORG
        DQ = ddr_param.DQ;
        NUM_CHANNELS = ddr_param.Channel;
        NUM_RANKS = ddr_param.Rank;
        NUM_BANK_GROUPS = ddr_param.BankGroup;
        NUM_BANKS = ddr_param.Bank;
        NUM_ROWS = ddr_param.Row;
        NUM_COLUMNS = ddr_param.Column;

        //Timing
        RATE = ddr_param.RATE;
        DRAM_CLK_FREQUENCY = (float)ddr_param.FREQ;
        TCK = ddr_param.TCK;
        BL = ClockFactor * ddr_param.BL;
        CL = ClockFactor * ddr_param.CL;
        CAS = ClockFactor * ddr_param.CL;

        CWL = ClockFactor * ddr_param.CWL;
        CKESR = ClockFactor * ddr_param.CKESR;

        FAW = ClockFactor * ddr_param.FAW;
        RAS = ClockFactor * ddr_param.RAS;
        RC = ClockFactor * ddr_param.RC;
        RCD = ClockFactor * ddr_param.RCD;
        RFC = ClockFactor * ddr_param.RFC;
        REFI = ClockFactor * ddr_param.REFI;
        RP = ClockFactor * ddr_param.RP;
        RTP = ClockFactor * ddr_param.RTP;
        RTRS = ClockFactor * ddr_param.RTRS;
        WR = ClockFactor * ddr_param.WR;

        PD = ClockFactor * ddr_param.PD;
        XP = ClockFactor * ddr_param.XP;
        XP_DLL = ClockFactor * ddr_param.XPDLL;
        XS = ClockFactor * ddr_param.XS;
        XSDLL = ClockFactor * ddr_param.XSDLL;

        CCDS = ClockFactor * ddr_param.CCDS;
        RRDS = ClockFactor * ddr_param.RRDS;
        WTRS = ClockFactor * ddr_param.WTRS;
        CCDL = ClockFactor * ddr_param.CCDL;
        RRDL = ClockFactor * ddr_param.RRDL;
        WTRL = ClockFactor * ddr_param.WTRL;

        //Power
        IDD0 = (float)ddr_param.IDD0;
        IDD02 = (float)ddr_param.IDD02;
        IDD2P0 = (float)ddr_param.IDD2P0;
        IDD2P1 = (float)ddr_param.IDD2P1;
        IDD2P = (float)ddr_param.IDD2P;
        IDD2N = (float)ddr_param.IDD2N;
        IDD3P = (float)ddr_param.IDD3P;
        IDD3P0 = (float)ddr_param.IDD3P0;
        IDD3P1 = (float)ddr_param.IDD3P1;
        IDD3N = (float)ddr_param.IDD3N;
        IDD4R = (float)ddr_param.IDD4R;
        IDD4W = (float)ddr_param.IDD4W;
        IDD5 = (float)ddr_param.IDD5;
        IDD6 = (float)ddr_param.IDD6;
        IDD62 = (float)ddr_param.IDD62;
        VDD = (float)ddr_param.VDD;
        VDD2 = (float)ddr_param.VDD2;
        #endregion

        #region CPU

        NUMCORES = cpu_param.NUMCORES;
        PROCESSOR_FREQ = cpu_param.PROCESSOR_FREQ;
        ROBSIZE = cpu_param.ROBSIZE;
        MAX_RETIRE = cpu_param.MAX_RETIRE;
        MAX_FETCH = cpu_param.MAX_FETCH;
        PIPELINEDEPTH = cpu_param.PIPELINEDEPTH;
        MSHR_MAX = cpu_param.MSHR_MAX;
        WriteBackQ_MAX = cpu_param.WriteBackQ_MAX;
        RDWRQ_MAX = cpu_param.RDWRQ_MAX;
        WriteBack = cpu_param.WriteBack;
        ISSUE_DUPLICATE_REQUEST = cpu_param.ISSUE_DUPLICATE_REQUEST;
        ADDRESS_BITS = cpu_param.ADDRESS_BITS;
        b_read_rc_traces = cpu_param.b_read_rc_traces;
        stats_exclude_cpy = cpu_param.stats_exclude_cpy;

        #endregion

        #region Cache
        CACHE_ENABLED = cpu_param.CACHE_ENABLED;
        CACHE_BLOCK_SIZE_BITS = cpu_param.CACHE_BLOCK_SIZE_BITS;// CACHE_BLOCKSIZE_BITS;
        SHARED_LLC_CACHE_ONLY = cpu_param.SHARED_LLC_CACHE_ONLY;// SHAREDLLC_CACHE_ONLY;
        L1CACHE_SIZE_BITS = cpu_param.L1CACHE_SIZE_BITS;// L1CACHESIZE_BITS;
        L1CACHE_ASSOCIATIVITY = cpu_param.L1CACHE_ASSOCIATIVITY;
        L1CACHE_HIT_LATENCY = cpu_param.L1CACHE_HIT_LATENCY;// L1CACHE_HITLATENCY;
        SHARED_L2CACHE = cpu_param.SHARED_L2CACHE;
        L2CACHE_SIZE_BITS = cpu_param.L2CACHE_SIZE_BITS;// L2CACHESIZE_BITS;
        L2CACHE_ASSOCIATIVITY = cpu_param.L2CACHE_ASSOCIATIVITY;
        L2CACHE_HIT_LATENCY = cpu_param.L2CACHE_HIT_LATENCY;// L2CACHE_HITLATENCY;
        SHARED_L3CACHE = cpu_param.SHARED_L3CACHE;// SHAREDL3CACHE;
        L3CACHE_SIZE_BITS = cpu_param.L3CACHE_SIZE_BITS;// L3CACHESIZE_BITS;
        L3CACHE_ASSOCIATIVITY = cpu_param.L3CACHE_ASSOCIATIVITY;
        L3CACHE_HIT_LATENCY = cpu_param.L3CACHE_HIT_LATENCY;// L3CACHE_HITLATENCY;

        #endregion

        #region MEM_CTRL

        ADDRESS_MAPPING = memCtrl_param.ADDRESS_MAPPING;
        WQ_LOOKUP_LATENCY = memCtrl_param.WQ_LOOKUP_LATENCY; 

        XBAR_LATENCY = memCtrl_param.XBAR_LATENCY;
        OPEN_PAGE_POLICY = memCtrl_param.OPEN_PAGE_POLICY;
        INCLUDE_XBAR_LATENCY = memCtrl_param.INCLUDE_XBAR_LATENCY;

        READQ_MAX = memCtrl_param.READQ_MAX;
        WRITEQ_MAX = memCtrl_param.WRITEQ_MAX;
        MAPPING_METHOD = memCtrl_param.MAPPING_METHOD;

        REFRESH_MODE = memCtrl_param.REFRESH_MODE;

        #endregion

        #region DeActivator
        //DEACTIVATOR_ENABLED = deactivator_param.DEACTIVATOR_ENABLED;

        //MAX_COUNTERSIZE = deactivator_param.DefaultParameter.MaxCounter;
        //MAX_ROW_BUFFERS_PER_BANK = deactivator_param.DefaultParameter.MaxRowBuffer;
        //OLD_THRESHOLD = deactivator_param.DefaultParameter.OldThreshhold;
        //MAX_ACTIVATIONS_PERMITTED = deactivator_param.DefaultParameter.MaxActivation;
        //VALIDATION_TIME = deactivator_param.DefaultParameter.ValidationTime;
        //DEACT_Parameters = deactivator_param.LIST;
        #endregion

        NUM_BANKS_TOTAL = NUM_BANK_GROUPS * NUM_BANKS;
        CHIPS_PER_RANK = 64 / DQ;

        tx = (PrefetchSize * ChannelWidth / 8);

        tx_bits = (int)Math.Log(tx, 2);
        channelBits = (int)Math.Log(NUM_CHANNELS, 2);
        rankBits = (int)Math.Log(NUM_RANKS, 2);
        bankBits = (int)Math.Log(NUM_BANKS, 2);
        bankGBits = (int)Math.Log(NUM_BANK_GROUPS, 2);
        rowBits = (int)Math.Log(NUM_ROWS, 2);
        colBits = (int)Math.Log(NUM_COLUMNS, 2);

        colBits -= (int)Math.Log(PrefetchSize, 2);

        SET_TIMING_CONSTANTS();

    }

    #region TIMING CONSTANTS

    //public int burst_cycle  ;
    public int RL;  
    public int WL  ;

    public int RD_delay  ;
    public int WR_delay  ;

    public int RD_TO_RD_L  ;
    public int RD_TO_RD_S  ;
    public int RD_TO_RD_o  ;
    public int RD_TO_WR  ;

    public int RD_TO_WR_o ;
    public int RD_TO_PRE  ;
    public int RDA_TO_ACT  ;

    public int WR_TO_RD_L ;
    public int WR_TO_RD_S ;
    public int WR_TO_RD_o;

    public int WR_TO_WR_L;
    public int WR_TO_WR_S;
    public int WR_TO_WR_o;
    public int WR_TO_PRE;

    public int PRE_TO_ACT;

    public int RD_TO_ACT ;
    public int WR_TO_ACT;

    public int ACT_TO_ACT  ;
    public int ACT_TO_ACT_L;
    public int ACT_TO_ACT_S;
    public int ACT_TO_PRE;
    public int ACT_TO_RD, ACT_TO_WR;

    public int ACT_TO_REF;  // need to PRE before ref, so it's tRC

    // TODO: deal with different REF rate
    public int REF_TO_REF;  // REF intervals (per rank level)
    public int REF_TO_ACT ;  // tRFC is defined as ref to ACT

    //int self_REF_entry_TO_exit = CKESR;
    //int self_REF_exit = XS;

    //int powerdown_TO_exit = CKE;
    //int powerdown_exit = XP;

    public int RDA_delay ;
    public int WRA_delay;


    void SET_TIMING_CONSTANTS()
    {
        RDA_delay = RTP + RP;
        WRA_delay = CWL + WR + RP;


        //burst_cycle = (BL == 0) ? 0 : BL / 2;
        RL = AL + CL;
        WL = AL + CWL;

        RD_delay = RL + BL;// burst_cycle;
        WR_delay = WL + BL;//burst_cycle;

        RD_TO_RD_L = Math.Max(BL, CCDL);
        RD_TO_RD_S = Math.Max(BL, CCDS);
        RD_TO_RD_o = BL + RTRS;
        RD_TO_WR = RL + BL - WL + RTRS;
                       
        RD_TO_WR_o = RD_delay + BL + RTRS - WR_delay;
                         
        RD_TO_PRE = AL + RTP;
        RDA_TO_ACT = AL + BL + RTP + RP;
        

        WR_TO_RD_L = WR_delay + WTRL;
        WR_TO_RD_S = WR_delay + WTRS;
        WR_TO_RD_o = WR_delay + BL +  RTRS - RD_delay;
                        
        WR_TO_WR_L = Math.Max(BL, CCDL);
        WR_TO_WR_S = Math.Max(BL, CCDS);
        WR_TO_WR_o = BL;
        WR_TO_PRE = WL + BL + WR;

        PRE_TO_ACT = RP;

        RD_TO_ACT = RD_TO_PRE + PRE_TO_ACT;
        WR_TO_ACT = WR_TO_PRE + PRE_TO_ACT;

        ACT_TO_ACT = RC;
        ACT_TO_ACT_L = RRDL;
        ACT_TO_ACT_S = RRDS;
        ACT_TO_PRE = RAS;

        ACT_TO_RD = RCD - AL;
        ACT_TO_WR = RCD - AL;

        ACT_TO_REF = RC;  // need to PRE before ref, so it's tRC
           

        // TODO: deal with different REF rate
        REF_TO_REF = REFI;  // REF intervals (per rank level)
           
        REF_TO_ACT = RFC;  // tRFC is defined as ref to ACT
        
        //int REF_TO_ACT_bank = RFCb;

        // int self_REF_entry_TO_exit = CKESR;
        // int self_REF_exit = XS;
        // int powerdown_TO_exit = CKE;
        // int powerdown_exit = XP;

        if ( NUM_BANK_GROUPS == 1)
        {
            RD_TO_RD_L = Math.Max(BL, CCDS);
            WR_TO_RD_L = WR_delay + WTRS;
            WR_TO_WR_L = Math.Max(BL, CCDS);
            ACT_TO_ACT_L = RRDS;
        }

    }

    #endregion

    public double IO_Power => DDR_TYPE switch { MemoryType.DDR2 => 1.5, MemoryType.DDR3 => 4.6, MemoryType.DDR4 => 3.7, _ => 0.0 };
    public double WR_ODTPower => DDR_TYPE switch { MemoryType.DDR2 => 8.2, MemoryType.DDR3 => 21.2, MemoryType.DDR4 => 17.0, _ => 0.0 };
    public double TERM_RDPower => DDR_TYPE switch { MemoryType.DDR2 => 13.1, MemoryType.DDR3 => 15.5, MemoryType.DDR4 => 12.4, _ => 0.0 };
    public double TERM_WRPower => DDR_TYPE switch { MemoryType.DDR2 => 14.6, MemoryType.DDR3 => 15.4, MemoryType.DDR4 => 12.3, _ => 0.0 };
    public double Capacitance => DDR_TYPE switch { MemoryType.LPDDR => 0.0000000045, MemoryType.LPDDR2 => 0.0000000025, MemoryType.LPDDR3 => 0.0000000018, _ => 0.0 };

    public TraceType TRACE_TYPE;

}



