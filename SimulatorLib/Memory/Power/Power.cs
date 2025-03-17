namespace SimulatorLib;

public class Power
{

    private Parameters Param;

    public Power(Parameters param)
    {
        Param = param;
    }

    public double ACT;
    public double PRE_PDN_SLOW;
    public double PRE_PDN_FAST;

    public double ACT_PDNS;
    public double ACT_PDNF;
    public double ACT_PDN;
    public double ACT_STBY;
    public double PRE_STBY;

    public double READ;
    public double WRITE;
    public double REF;

    public double DQ;
    public double TERM_WRITE;
    public double TERM_READ_OTHER;
    public double TERM_WRITE_OTHER;

    public double T4 => ACT + B3 + READ + WRITE + REF + DQ + TERM_WRITE + TERM_READ_OTHER + TERM_WRITE_OTHER;
    public double T3 => ACT + B4 + READ + WRITE + REF + DQ + TERM_WRITE + TERM_READ_OTHER + TERM_WRITE_OTHER;

    public double TOTAL => Param.DDR_TYPE switch
    {
        MemoryType.DDR3 => T3,
        MemoryType.DDR4 => T4,
        _ => double.NaN
    };

    public double B4 => PRE_PDN_SLOW + PRE_PDN_FAST + ACT_PDN + ACT_STBY + PRE_STBY;
    public double B3 => PRE_PDN_SLOW + PRE_PDN_FAST + ACT_PDNS + ACT_PDNF + ACT_STBY + PRE_STBY;
    public double BACKGROUND => Param.DDR_TYPE switch
    {
        MemoryType.DDR3 => B3,
        MemoryType.DDR4 => B4,
        _ => double.NaN
    };


}



