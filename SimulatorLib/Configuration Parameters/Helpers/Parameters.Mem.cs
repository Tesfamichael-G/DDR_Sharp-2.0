using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimulatorLib;

enum RefreshRate
{
    SINGLE,
    DOUBLE,
    QUADRUPLE
};


public class DDR_Parameters
{

    public DDR_Parameters() { }

    public DDR_Parameters(DRAM_Specification Parameters)
    {
        Initialize(Parameters);
    }

    //********* DDR4 ********* 

    RefreshRate refresh_mode = RefreshRate.SINGLE;


    public string ID ;

    public MemoryType DDR_TYPE { get; set; } // ;
    public string ORG_ID { get; set; }   // ;
    public string DDR_ID { get; set; }   // ;

    #region Mapping

    public int PrefetchSize  { get; set; }= 8; // 8n prefetch DDR
    public int ChannelWidth { get; set; } = 64;

    public int tx ;

    public int tx_bits ;
    public int channelBits;
    public int rankBits ;
    public int bankBits;
    public int bankGBits;
    public int rowBits;
    public int colBits;

    #endregion

    #region Organization

    public int DQ { get; set; }
    public int Channel { get; set; }
    public int Rank  { get; set; }
    public int Bank { get; set; }
    public int BankGroup { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

    #endregion

    #region Power Parameters

    public float IDD0 { get; set; }
    public float IDD02 { get; set; }
    public float IDD2P0   { get; set; } 
    public float IDD2P1   { get; set; } 
    public float IDD2P    { get; set; }
    public float IDD2N    { get; set; }
    public float IDD3P    { get; set; }
    public float IDD3P0   { get; set; } 
    public float IDD3P1   { get; set; } 
    public float IDD3N    { get; set; }
    public float IDD4R    { get; set; }
    public float IDD4W { get; set; }
    public float IDD5  { get; set; }
    public float IDD6  { get; set; }
    public float IDD62 { get; set; }
    public float VDD { get; set; }
    public float VDD2 { get; set; }

    #endregion

    #region DRAM_ParametersTiming

    public int RATE { get; set; }

    public float FREQ { get; set; }
    public float TCK { get; set; }

    public int BL  { get; set; }
    public int CL  { get; set; }
    public int CCD { get; set; } 
    public int CWL { get; set; }
    public int CKESR { get; set; }
    public int FAW  { get; set; }
    public int RAS { get; set; }
    public int RC { get; set; }
    public int RCD  { get; set; }
    public int RFC  { get; set; }
    public int REFI { get; set; }
    public int RP { get; set; }
    public int RRD  { get; set; }
    public int RTP  { get; set; }
    public int RTRS { get; set; }
    public int WR  { get; set; }
    public int WTR { get; set; } 
    public int PD  { get; set; }
    public int XP { get; set; }
    public int XPDLL { get; set; }
    public int XS { get; set; }
    public int XSDLL { get; set; }

    public int CCDS  { get; set; }
    public int RRDS  { get; set; }
    public int WTRS  { get; set; }
    public int CCDL  { get; set; }
    public int RRDL  { get; set; }
    public int WTRL { get; set; }

    #endregion 

    #region Power Specification as per the Micron Datasheet 

    /*----------------------------------------------------
    //On Die Termination (ODT) Power: Values obtained from Micron Technical Note
    //This is dependent on the termination configuration of the simulated configuration our simulator uses the same config as that used in the Tech Note
    ----------------------------------------------------*/
    //P_DQ = 3.2 * 10; P_TERM_WR = 0;  P_TERM_RD_O = 24.9 * 10;  P_TERM_WR_O = 20.8 * 11;

    public float P_IO => DDR_TYPE switch 
    { 
        MemoryType.DDR2 => 1.5F, MemoryType.DDR3 => 4.6F, MemoryType.DDR4 => 3.7F, _ => 0.0F 
    };
    public float P_WRODT => DDR_TYPE switch 
    { 
        MemoryType.DDR2 => 8.2F, MemoryType.DDR3 => 21.2F, MemoryType.DDR4 => 17.0F, _ => 0.0F 
    };
    public float P_TERMRD => DDR_TYPE switch 
    { MemoryType.DDR2 => 13.1F, MemoryType.DDR3 => 15.5F, MemoryType.DDR4 => 12.4F, _ => 0.0F 
    };
    public float P_TERMWR => DDR_TYPE switch 
    { 
        MemoryType.DDR2 => 14.6F, MemoryType.DDR3 => 15.4F, MemoryType.DDR4 => 12.3F, _ => 0.0F 
    };

    //Capacitance in mF
    public float P_Capacitance => DDR_TYPE switch 
    { 
        MemoryType.LPDDR => 0.0000000045F, MemoryType.LPDDR2 => 0.0000000025F, MemoryType.LPDDR3 => 0.0000000018F, _ => 0.0F 
    };

    public float PDS_ACT = 0;
    public float PDS_PRE_PDN_FAST = 0;
    public float PDS_PRE_PDN_SLOW = 0;

    public float PDS_ACT_PDN_FAST = 0;
    public float PDS_ACT_PDN_SLOW = 0;

    public float PDS_ACT_STBY = 0;
    public float PDS_PRE_STBY = 0;

    public float PDS_READ;
    public float PDS_WRITE;
    public float PDS_REF;

    public float PDS_IO = 0;
    public float PDS_WRITE_ODT = 0;
    public float PDS_TERM_READ_OTHER = 0;
    public float PDS_TERM_WRITE_OTHER = 0;

    #endregion

    #region Methods

    public void Initialize(DRAM_Specification Parameters)
    {
        InitializeParameters(Parameters);
        UpdateTimingParameters(Parameters);
    }

    private void InitializeParameters(DRAM_Specification Parameters)
    {

        ID = Parameters.ID;

        DDR_TYPE = (MemoryType)Enum.Parse(typeof(MemoryType), Parameters.DDR_TYPE);
        ORG_ID = Parameters.ORG_ID;
        DDR_ID = Parameters.DDR_ID;

        //ORG
        DQ = Parameters.ORG.DQ;
        Channel = Parameters.ORG.Channel;
        Rank = Parameters.ORG.Rank;
        Bank = Parameters.ORG.Bank;
        Row = Parameters.ORG.Row;
        Column = Parameters.ORG.Column;
        BankGroup = Parameters.ORG.BankGroup;

        //Timing
        RATE = Parameters.TIMING.RATE;
        FREQ = Parameters.TIMING.FREQ;
        TCK = Parameters.TIMING.TCK;
        BL = Parameters.TIMING.BL;
        CL = Parameters.TIMING.CL;
        CWL = Parameters.TIMING.CWL;
        CKESR = Parameters.TIMING.CKESR;
        FAW = Parameters.TIMING.FAW;
        RAS = Parameters.TIMING.RAS;
        RC = Parameters.TIMING.RC;
        RCD = Parameters.TIMING.RCD;
        RFC = Parameters.TIMING.RFC;
        REFI = Parameters.TIMING.REFI;
        RP = Parameters.TIMING.RP;
        RTP = Parameters.TIMING.RTP;
        RTRS = Parameters.TIMING.RTRS;
        WR = Parameters.TIMING.WR;
        PD = Parameters.TIMING.PD;
        XP = Parameters.TIMING.XP;
        XPDLL = Parameters.TIMING.XPDLL;
        XS = Parameters.TIMING.XS;
        XSDLL = Parameters.TIMING.XSDLL;

        CCDS = Parameters.TIMING.CCDS;
        RRDS = Parameters.TIMING.RRDS;
        WTRS = Parameters.TIMING.WTRS;
        CCDL = Parameters.TIMING.CCDL;
        RRDL = Parameters.TIMING.RRDL;
        WTRL = Parameters.TIMING.WTRL;

        //Power
        IDD0 = Parameters.POWER.IDD0;
        IDD02 = Parameters.POWER.IDD02;
        IDD2P0 = Parameters.POWER.IDD2P0;
        IDD2P1 = Parameters.POWER.IDD2P1;
        IDD2P = Parameters.POWER.IDD2P;
        IDD2N = Parameters.POWER.IDD2N;
        IDD3P = Parameters.POWER.IDD3P;
        IDD3P0 = Parameters.POWER.IDD3P0;
        IDD3P1 = Parameters.POWER.IDD3P1;
        IDD3N = Parameters.POWER.IDD3N;
        IDD4R = Parameters.POWER.IDD4R;
        IDD4W = Parameters.POWER.IDD4W;
        IDD5 = Parameters.POWER.IDD5;
        IDD6 = Parameters.POWER.IDD6;
        IDD62 = Parameters.POWER.IDD62;
        VDD = Parameters.POWER.VDD;
        VDD2 = Parameters.POWER.VDD2;

        PDS_ACT = (IDD0 - (IDD3N * RAS + IDD2N * (RC - RAS)) / RC) * VDD;

        PDS_PRE_PDN_SLOW = IDD2P0 * VDD;
        PDS_PRE_PDN_FAST = IDD2P1 * VDD;

        //PDS.ACT_PDN = IDD3P * VDD;
        PDS_ACT_PDN_FAST = VDD * IDD3P1;
        PDS_ACT_PDN_SLOW = VDD * IDD3P0;

        PDS_PRE_STBY = IDD2N * VDD;
        PDS_ACT_STBY = IDD3N * VDD;
        PDS_WRITE = (IDD4W - IDD3N) * VDD;
        PDS_READ = (IDD4R - IDD3N) * VDD;
        PDS_REF = (IDD5 - IDD3N) * VDD;

        PDS_IO = P_IO;             // in mW
        PDS_WRITE_ODT = P_WRODT;    // in mW

        if (Parameters.ORG.Rank > 1)
        {
            PDS_TERM_READ_OTHER = P_TERMRD; // in mW
            PDS_TERM_WRITE_OTHER = P_TERMWR; // in mW
        }

        if (P_Capacitance != 0.0) // If capacity is given, then IO Power depends on DRAM clock frequency.
        {
            float pow = (float)Math.Pow(VDD2, 2.0);
            PDS_IO = P_Capacitance * 0.5F * pow * FREQ * 1000000.0F;
        }

        int DQS = 2;                //DQS signals
        int DQR = (int)(Parameters.ORG.DQ + DQS);     //For a x8 device, num_DQR includes eight DQ and two DQS signals for a total of 10
        int DQW = Parameters.ORG.DQ + DQS + 1; //to account for the addition of the data mask.

        PDS_IO = P_IO * DQR;                    // 3.2 * 10;
        PDS_WRITE_ODT = P_WRODT * DQW;           // 0;
        PDS_TERM_READ_OTHER = P_TERMRD * DQW;    // 24.9 * 10;
        PDS_TERM_WRITE_OTHER = P_TERMWR * DQW;   // 20.8 * 11;

    }

    private void UpdateTimingParameters(DRAM_Specification Parameters)
    {

        UpdateTiming();

        void UpdateTiming()
        {
            if (BankGroup > 1)
            {
                CalcGroupedBankTimings();
                return;
            }
            CalcUngroupedBankTimings();
        }

        void CalcGroupedBankTimings()
        {

            (RRDS, RRDL) = RATE switch
            {
                1600 => (Parameters.ORG.DQ == 16) ? (5, 6) : (4, 5),
                1866 => (Parameters.ORG.DQ == 16) ? (5, 6) : (4, 5),
                2133 => (Parameters.ORG.DQ == 16) ? (6, 7) : (4, 6),
                2400 => (Parameters.ORG.DQ == 16) ? (7, 8) : (4, 6),
                _ => (0, 0)
            };


            FAW = RATE switch
            {
                1600 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 20 : 28,
                1866 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 22 : 28,
                2133 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 23 : 32,
                2400 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 26 : 36,
                _ => 0
            };


            REFI = RATE switch
            {
                1600 => 6240 >> (int)refresh_mode,
                1866 => 7280 >> (int)refresh_mode,
                2133 => 8320 >> (int)refresh_mode,
                2400 => 9360 >> (int)refresh_mode,
            };

            int DENSITY = Parameters.ORG.SIZE >> 10;

            XS = RATE switch
            {
                1600 => (DENSITY == 2) ? (136) : (DENSITY == 4) ? (216) : (DENSITY == 8) ? (288) : (-1),
                1866 => (DENSITY == 2) ? (159) : (DENSITY == 4) ? (252) : (DENSITY == 8) ? (336) : (-1),
                2133 => (DENSITY == 2) ? (182) : (DENSITY == 4) ? (288) : (DENSITY == 8) ? (384) : (-1),
                2400 => (DENSITY == 2) ? (204) : (DENSITY == 4) ? (324) : (DENSITY == 8) ? (432) : (-1),
            };

            RFC = (refresh_mode, RATE) switch
            {
                (RefreshRate.SINGLE, 1600) => (DENSITY == 2) ? (128) : (DENSITY == 4) ? (208) : (DENSITY == 8) ? (280) : (-1),
                (RefreshRate.SINGLE, 1866) => (DENSITY == 2) ? (150) : (DENSITY == 4) ? (243) : (DENSITY == 8) ? (327) : (-1),
                (RefreshRate.SINGLE, 2133) => (DENSITY == 2) ? (171) : (DENSITY == 4) ? (278) : (DENSITY == 8) ? (374) : (-1),
                (RefreshRate.SINGLE, 2400) => (DENSITY == 2) ? (193) : (DENSITY == 4) ? (313) : (DENSITY == 8) ? (421) : (-1),

                (RefreshRate.DOUBLE, 1600) => (DENSITY == 2) ? 88 : (DENSITY == 4) ? 128 : (DENSITY == 8) ? 208 : (-1),
                (RefreshRate.DOUBLE, 1866) => (DENSITY == 2) ? 103 : (DENSITY == 4) ? 150 : (DENSITY == 8) ? 243 : (-1),
                (RefreshRate.DOUBLE, 2133) => (DENSITY == 2) ? 118 : (DENSITY == 4) ? 171 : (DENSITY == 8) ? 278 : (-1),
                (RefreshRate.DOUBLE, 2400) => (DENSITY == 2) ? 132 : (DENSITY == 4) ? 192 : (DENSITY == 8) ? 312 : (-1),

                (RefreshRate.QUADRUPLE, 1600) => (DENSITY == 2) ? 72 : (DENSITY == 4) ? 88 : (DENSITY == 8) ? 128 : (-1),
                (RefreshRate.QUADRUPLE, 1866) => (DENSITY == 2) ? 84 : (DENSITY == 4) ? 103 : (DENSITY == 8) ? 150 : (-1),
                (RefreshRate.QUADRUPLE, 2133) => (DENSITY == 2) ? 96 : (DENSITY == 4) ? 118 : (DENSITY == 8) ? 171 : (-1),
                (RefreshRate.QUADRUPLE, 2400) => (DENSITY == 2) ? 108 : (DENSITY == 4) ? 132 : (DENSITY == 8) ? 192 : (-1),
            };

        }

        void CalcUngroupedBankTimings()
        {
            int PAGE = (Parameters.ORG.DQ * Parameters.ORG.Column >> 13);

            (RRDS, RRDL, FAW) = RATE switch
            {
                 800 => (PAGE == 1) ? (4, 4, 16) : (4, 4, 20),
                1066 => (PAGE == 1) ? (4, 4, 20) : (6, 6, 27),
                1333 => (PAGE == 1) ? (4, 4, 20) : (5, 5, 30),
                1600 => (PAGE == 1) ? (5, 5, 24) : (6, 6, 32),
                1866 => (PAGE == 1) ? (5, 5, 26) : (6, 6, 33),
                2133 => (PAGE == 1) ? (5, 5, 27) : (6, 6, 34),
                _ => (-1,-1,-1)
            };

            int DENSITY = Parameters.ORG.SIZE; // org_entry.size;

            (RFC, XS) = RATE switch
            {
                800 => (DENSITY == 512) ? (36, 40) : (DENSITY == 1024) ? (44, 48) : (DENSITY == 2048) ? (64, 68) : (DENSITY == 4096) ? (104, 108) : (140, 144),
                1066 => (DENSITY == 512) ? (48, 54) : (DENSITY == 1024) ? (59, 64) : (DENSITY == 2048) ? (86, 91) : (DENSITY == 4096) ? (139, 144) : (187, 192),
                1333 => (DENSITY == 512) ? (60, 67) : (DENSITY == 1024) ? (74, 80) : (DENSITY == 2048) ? (107, 114) : (DENSITY == 4096) ? (174, 180) : (234, 240),
                1600 => (DENSITY == 512) ? (72, 80) : (DENSITY == 1024) ? (88, 96) : (DENSITY == 2048) ? (128, 136) : (DENSITY == 4096) ? (208, 216) : (280, 288),
                1866 => (DENSITY == 512) ? (84, 94) : (DENSITY == 1024) ? (103, 112) : (DENSITY == 2048) ? (150, 159) : (DENSITY == 4096) ? (243, 252) : (327, 336),
                2133 => (DENSITY == 512) ? (96, 107) : (DENSITY == 1024) ? (118, 128) : (DENSITY == 2048) ? (171, 182) : (DENSITY == 4096) ? (278, 288) : (374, 384),
                _ => (-1,-1)
            };

        }



    }

    #endregion

    #region Spec Generator

    private SpecInfo spec;
    public SpecInfo Spec
    {
        get
        {
            if (spec == null)
                spec = new SpecInfo(this);

            return spec;
        }
    }

    public class SpecInfo
    {

        string[] power_spec_names = { "IDD0", "IDD02", "IDD2P0", "IDD2P1", "IDD2P", "IDD2N", "IDD3P", "IDD3P0", "IDD3P1", "IDD3N",
                                      "IDD4R", "IDD4W", "IDD5", "IDD6", "IDD62", "VDD", "VDD2"};
        string[] arch_spec_names = { "DDR_ID", "ORG_ID", "DQ", "Channel", "Rank", "BankGroup", "Bank", "Row", "Column" };
        string[] ddr3_timing_names = { "CCD", "RRD", "WTR" };
        string[] ddr4_timing_names = { "CCDS", "RRDS", "WTRS", "CCDL", "RRDL", "WTRL" };

        public List<KeyValue_Item> Arch;
        public List<KeyValue_Item> Power;
        public List<KeyValue_Item> Timing;
        PropertyInfo[] properties;

        public SpecInfo(DDR_Parameters ddr)
        {

            if (Arch == null)
                Arch = new List<KeyValue_Item>();

            if (Power == null)
                Power = new List<KeyValue_Item>();

            if (Timing == null)
                Timing = new List<KeyValue_Item>();

            properties = ddr.GetType().GetProperties();

            //System.Diagnostics.Debug.Print($"properties.count = {properties.Count()}");

            foreach (var prop in properties)
            {
                var name = prop.Name;

                if (name.StartsWith("P_") || name == "Spec") continue;
                if (ddr.DDR_TYPE == MemoryType.DDR3 && ddr4_timing_names.Contains(name)) continue;
                else if (ddr.DDR_TYPE == MemoryType.DDR4 && ddr3_timing_names.Contains(name)) continue;
                else if (prop.Name == "ID") continue;

                name += ": ";
                var val = prop.GetValue(ddr);

                var line = new KeyValue_Item { Name = name.ToUpper(), Value = val };

                if (power_spec_names.Contains(prop.Name))
                {
                    if (float.IsNaN((float)val)) continue;

                    Power.Add(line);
                }
                else if (arch_spec_names.Contains(prop.Name))
                    Arch.Add(line);
                else
                    Timing.Add(line);

            }




        }


    }

    #endregion



}


//void CalcRDD_Grouped()
//{

//    int PAGE = (Parameters.ORG.DQ * Parameters.ORG.Column >> 13);

//    RRDS = RRDL = RATE switch
//    {
//        800 => (PAGE == 1) ? 4 : 4,
//        1066 => (PAGE == 1) ? 4 : 6,
//        1333 => (PAGE == 1) ? 4 : 5,
//        1600 => (PAGE == 1) ? 5 : 6,
//        1866 => (PAGE == 1) ? 5 : 6,
//        2133 => (PAGE == 1) ? 5 : 6,
//        _ => 0
//    };

//}

//void CalcFAW_Grouped()
//{
//    FAW = RATE switch
//    {
//        1600 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 20 : 28,
//        1866 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 22 : 28,
//        2133 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 23 : 32,
//        2400 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 26 : 36,
//        _ => 0
//    };
//}

//void CalcREF_Grouped()
//{
//    int CHIP = Parameters.ORG.SIZE >> 10;
//    REFI = RATE switch
//    {
//        1600 => 6240 >> (int)refresh_mode,
//        1866 => 7280 >> (int)refresh_mode,
//        2133 => 8320 >> (int)refresh_mode,
//        2400 => 9360 >> (int)refresh_mode,
//        _ => 0
//    };

//    RFC = (refresh_mode, RATE) switch
//    {
//        (RefreshRate.SINGLE, 1600) => (CHIP == 2) ? (128) : (CHIP == 4) ? (208) : (CHIP == 8) ? (280) : (-1),
//        (RefreshRate.SINGLE, 1866) => (CHIP == 2) ? (150) : (CHIP == 4) ? (243) : (CHIP == 8) ? (327) : (-1),
//        (RefreshRate.SINGLE, 2133) => (CHIP == 2) ? (171) : (CHIP == 4) ? (278) : (CHIP == 8) ? (374) : (-1),
//        (RefreshRate.SINGLE, 2400) => (CHIP == 2) ? (193) : (CHIP == 4) ? (313) : (CHIP == 8) ? (421) : (-1),

//        (RefreshRate.DOUBLE, 1600) => (CHIP == 2) ? 88 : (CHIP == 4) ? 128 : (CHIP == 8) ? 208 : (-1),
//        (RefreshRate.DOUBLE, 1866) => (CHIP == 2) ? 103 : (CHIP == 4) ? 150 : (CHIP == 8) ? 243 : (-1),
//        (RefreshRate.DOUBLE, 2133) => (CHIP == 2) ? 118 : (CHIP == 4) ? 171 : (CHIP == 8) ? 278 : (-1),
//        (RefreshRate.DOUBLE, 2400) => (CHIP == 2) ? 132 : (CHIP == 4) ? 192 : (CHIP == 8) ? 312 : (-1),

//        (RefreshRate.QUADRUPLE, 1600) => (CHIP == 2) ? 72 : (CHIP == 4) ? 88 : (CHIP == 8) ? 128 : (-1),
//        (RefreshRate.QUADRUPLE, 1866) => (CHIP == 2) ? 84 : (CHIP == 4) ? 103 : (CHIP == 8) ? 150 : (-1),
//        (RefreshRate.QUADRUPLE, 2133) => (CHIP == 2) ? 96 : (CHIP == 4) ? 118 : (CHIP == 8) ? 171 : (-1),
//        (RefreshRate.QUADRUPLE, 2400) => (CHIP == 2) ? 108 : (CHIP == 4) ? 132 : (CHIP == 8) ? 192 : (-1),
//        _ => 0
//    };
//}

//void CalcXS_Grouped()
//{
//    int CHIP = Parameters.ORG.SIZE >> 10;

//    if (BankGroup > 1)
//    {
//        XS = RATE switch
//        {
//            1600 => (CHIP == 2) ? (136) : (CHIP == 4) ? (216) : (CHIP == 8) ? (288) : (-1),
//            1866 => (CHIP == 2) ? (159) : (CHIP == 4) ? (252) : (CHIP == 8) ? (336) : (-1),
//            2133 => (CHIP == 2) ? (182) : (CHIP == 4) ? (288) : (CHIP == 8) ? (384) : (-1),
//            2400 => (CHIP == 2) ? (204) : (CHIP == 4) ? (324) : (CHIP == 8) ? (432) : (-1),
//        };
//    }
//}

//void CalcRDD()
//{
//    if (BankGroup > 0)
//    {
//        (RRDS, RRDL) = RATE switch
//        {
//            1600 => (Parameters.ORG.DQ == 16) ? (5, 6) : (4, 5),
//            1866 => (Parameters.ORG.DQ == 16) ? (5, 6) : (4, 5),
//            2133 => (Parameters.ORG.DQ == 16) ? (6, 7) : (4, 6),
//            2400 => (Parameters.ORG.DQ == 16) ? (7, 8) : (4, 6),
//            _ => (0, 0)
//        };
//        return;
//    }

//    int PAGE = (Parameters.ORG.DQ * Parameters.ORG.Column >> 13);

//    RRDS = RRDL = RATE switch
//    {
//        800 => (PAGE == 1) ? 4 : 4,
//        1066 => (PAGE == 1) ? 4 : 6,
//        1333 => (PAGE == 1) ? 4 : 5,
//        1600 => (PAGE == 1) ? 5 : 6,
//        1866 => (PAGE == 1) ? 5 : 6,
//        2133 => (PAGE == 1) ? 5 : 6,
//        _ => 0
//    };
//}

//void CalcFAW()
//{
//    if (BankGroup > 1)
//    {
//        FAW = RATE switch
//        {
//            1600 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 20 : 28,
//            1866 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 22 : 28,
//            2133 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 23 : 32,
//            2400 => (Parameters.ORG.DQ == 4) ? 16 : (Parameters.ORG.DQ == 8) ? 26 : 36,
//            _ => 0
//        };
//        return;
//    }

//    int PAGE = (Parameters.ORG.DQ * Parameters.ORG.Column >> 13);

//    FAW = RATE switch
//    {
//        800 => (PAGE == 1) ? 16 : 20,
//        1066 => (PAGE == 1) ? 20 : 27,
//        1333 => (PAGE == 1) ? 20 : 30,
//        1600 => (PAGE == 1) ? 24 : 32,
//        1866 => (PAGE == 1) ? 26 : 33,
//        2133 => (PAGE == 1) ? 27 : 34,
//        _ => 0
//    };

//}

//void CalcREF()
//{
//    int CHIP = Parameters.ORG.SIZE >> 10;
//    if (BankGroup > 1)
//    {
//        REFI = RATE switch
//        {
//            1600 => 6240 >> (int)refresh_mode,
//            1866 => 7280 >> (int)refresh_mode,
//            2133 => 8320 >> (int)refresh_mode,
//            2400 => 9360 >> (int)refresh_mode,
//            _ => 0
//        };


//        RFC = (refresh_mode, RATE) switch
//        {
//            (RefreshRate.SINGLE, 1600) => (CHIP == 2) ? (128) : (CHIP == 4) ? (208) : (CHIP == 8) ? (280) : (-1),
//            (RefreshRate.SINGLE, 1866) => (CHIP == 2) ? (150) : (CHIP == 4) ? (243) : (CHIP == 8) ? (327) : (-1),
//            (RefreshRate.SINGLE, 2133) => (CHIP == 2) ? (171) : (CHIP == 4) ? (278) : (CHIP == 8) ? (374) : (-1),
//            (RefreshRate.SINGLE, 2400) => (CHIP == 2) ? (193) : (CHIP == 4) ? (313) : (CHIP == 8) ? (421) : (-1),

//            (RefreshRate.DOUBLE, 1600) => (CHIP == 2) ? 88 : (CHIP == 4) ? 128 : (CHIP == 8) ? 208 : (-1),
//            (RefreshRate.DOUBLE, 1866) => (CHIP == 2) ? 103 : (CHIP == 4) ? 150 : (CHIP == 8) ? 243 : (-1),
//            (RefreshRate.DOUBLE, 2133) => (CHIP == 2) ? 118 : (CHIP == 4) ? 171 : (CHIP == 8) ? 278 : (-1),
//            (RefreshRate.DOUBLE, 2400) => (CHIP == 2) ? 132 : (CHIP == 4) ? 192 : (CHIP == 8) ? 312 : (-1),

//            (RefreshRate.QUADRUPLE, 1600) => (CHIP == 2) ? 72 : (CHIP == 4) ? 88 : (CHIP == 8) ? 128 : (-1),
//            (RefreshRate.QUADRUPLE, 1866) => (CHIP == 2) ? 84 : (CHIP == 4) ? 103 : (CHIP == 8) ? 150 : (-1),
//            (RefreshRate.QUADRUPLE, 2133) => (CHIP == 2) ? 96 : (CHIP == 4) ? 118 : (CHIP == 8) ? 171 : (-1),
//            (RefreshRate.QUADRUPLE, 2400) => (CHIP == 2) ? 108 : (CHIP == 4) ? 132 : (CHIP == 8) ? 192 : (-1),
//            _ => 0
//        };
//        return;
//    }

//    int SZ = Parameters.ORG.SIZE; // org_entry.size;

//    RFC = RATE switch //RefreshRate.SINGLE
//    {
//        800 => (CHIP == 0) ? 36 : (CHIP == 1) ? 44 : (CHIP == 2) ? 64 : (CHIP == 4) ? 104 : 140,
//        1066 => (CHIP == 0) ? 48 : (CHIP == 1) ? 59 : (CHIP == 2) ? 86 : (CHIP == 4) ? 139 : 187,
//        1333 => (CHIP == 0) ? 60 : (CHIP == 1) ? 74 : (CHIP == 2) ? 107 : (CHIP == 4) ? 174 : 234,
//        1600 => (CHIP == 0) ? 72 : (CHIP == 1) ? 88 : (CHIP == 2) ? 128 : (CHIP == 4) ? 208 : 280,
//        1866 => (CHIP == 0) ? 84 : (CHIP == 1) ? 103 : (CHIP == 2) ? 150 : (CHIP == 4) ? 243 : 327,
//        2133 => (CHIP == 0) ? 967 : (CHIP == 1) ? 118 : (CHIP == 2) ? 171 : (CHIP == 4) ? 278 : 374,
//        _ => 0
//    };

//}

//void CalcXS()
//{
//    int CHIP = Parameters.ORG.SIZE >> 10;

//    if (BankGroup > 1)
//    {
//        XS = RATE switch
//        {
//            1600 => (CHIP == 2) ? (136) : (CHIP == 4) ? (216) : (CHIP == 8) ? (288) : (-1),
//            1866 => (CHIP == 2) ? (159) : (CHIP == 4) ? (252) : (CHIP == 8) ? (336) : (-1),
//            2133 => (CHIP == 2) ? (182) : (CHIP == 4) ? (288) : (CHIP == 8) ? (384) : (-1),
//            2400 => (CHIP == 2) ? (204) : (CHIP == 4) ? (324) : (CHIP == 8) ? (432) : (-1),
//        };
//    }

//    XS = RATE switch//2
//    {
//        800 => (CHIP == 0) ? (40) : (CHIP == 1) ? (48) : (CHIP == 2) ? (68) : (CHIP == 4) ? (108) : (144),
//        1066 => (CHIP == 0) ? (54) : (CHIP == 1) ? (64) : (CHIP == 2) ? (91) : (CHIP == 4) ? (144) : (192),
//        1333 => (CHIP == 0) ? (67) : (CHIP == 1) ? (80) : (CHIP == 2) ? (114) : (CHIP == 4) ? (180) : (240),
//        1600 => (CHIP == 0) ? (80) : (CHIP == 1) ? (96) : (CHIP == 2) ? (136) : (CHIP == 4) ? (216) : (288),
//        1866 => (CHIP == 0) ? (94) : (CHIP == 1) ? (112) : (CHIP == 2) ? (159) : (CHIP == 4) ? (252) : (336),
//        2133 => (CHIP == 0) ? (107) : (CHIP == 1) ? (128) : (CHIP == 2) ? (182) : (CHIP == 4) ? (288) : (384),
//        _ => 0
//    };

//}
