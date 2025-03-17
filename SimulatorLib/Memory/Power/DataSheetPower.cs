using System;

namespace SimulatorLib;

public class DataSheetPower
{

    public double ACT = 0;
    public double PRE_PDN_FAST = 0;
    public double PRE_PDN_SLOW = 0;
    //public double PRE_PDN = 0;

    public double ACT_PDN = 0;
    public double ACT_PDNF = 0;
    public double ACT_PDNS = 0;

    public double ACT_STBY = 0;
    public double PRE_STBY = 0;

    public double READ;
    public double WRITE;
    public double REF;

    public double DQ = 0;
    public double TERM_WRITE = 0;

    public double TERM_READ_OTHER = 0;
    public double TERM_WRITE_OTHER = 0;

    public void Initialize(Parameters Param)
    {

        {
            //Pds(ACT) = (IDD0 - [IDD3N × tRAS / tRC + IDD2N × (tRC - tRAS) / tRC]) × VDD
            //Pds(PRE_PDN)  = IDD2P × VDD 
            //Pds(ACT_PDN)  = IDD3P × VDD 
            //Pds(PRE_STBY) = IDD2N × VDD 
            //Pds(ACT_STBY) = IDD3N × VDD
            //Pds(WR) = (IDD4W - IDD3N) × VDD
            //Pds(RD) = (IDD4R - IDD3N) × VDD
            //
        }
      
        ACT = (
                 Param.IDD0 - 
                            (
                               Param.IDD3N * Param.RAS + 
                               Param.IDD2N * (Param.RC - Param.RAS)
                            ) / Param.RC
              ) * Param.VDD;


        PRE_PDN_SLOW = Param.IDD2P0 * Param.VDD;
        PRE_PDN_FAST = Param.IDD2P1 * Param.VDD;
        //PRE_PDN = Param.IDD2P * Param.VDD;

        if (Param.DDR_TYPE == MemoryType.DDR4)
            ACT_PDN = Param.IDD3P * Param.VDD;
        else if (Param.DDR_TYPE == MemoryType.DDR3)
        {
            ACT_PDNF = Param.IDD3P1 * Param.VDD;
            ACT_PDNS = Param.IDD3P0 * Param.VDD;
        }

        PRE_STBY = Param.IDD2N * Param.VDD;
        ACT_STBY = Param.IDD3N * Param.VDD;
        WRITE = (Param.IDD4W - Param.IDD3N) * Param.VDD;
        READ = (Param.IDD4R - Param.IDD3N) * Param.VDD;
        REF = (Param.IDD5 - Param.IDD3N) * Param.VDD;

        //DQ = Param.IO_Power;             // in mW
        //TERM_WRITE = Param.WR_ODTPower;    // in mW

        if (Param.NUM_RANKS > 1)
        {
            TERM_READ_OTHER = Param.TERM_RDPower; // in mW
            TERM_WRITE_OTHER = Param.TERM_WRPower; // in mW
        }

        if (Param.Capacitance != 0.0) // If capacitance is known, the IO Power can be computerd from DRAM clock frequency.
        {
            DQ = Param.Capacitance * 0.5 * Math.Pow(Param.VDD2, 2.0) * Param.DRAM_CLK_FREQUENCY * 1000000;
        }

        int DQS = 2;                  //DQS signals
        int DQR = Param.DQ + DQS;     //For a x8 device, num_DQR includes eight DQ and two DQS signals for a total of 10
        int DQW = Param.DQ + DQS + 1; //to account for the addition of the data mask.

        DQ = Param.IO_Power * DQR;
        TERM_WRITE = Param.WR_ODTPower * DQW;
        TERM_READ_OTHER = Param.TERM_RDPower * DQW;
        TERM_WRITE_OTHER = Param.TERM_WRPower * DQW;

    }

}




