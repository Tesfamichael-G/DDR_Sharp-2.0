namespace SimulatorLib;

public class Energy
{
    private Parameters Param;
    public Energy(Parameters param)
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

    public double Total4 => ACT + Back3 +  READ + WRITE + REF +  DQ + TERM_WRITE + TERM_READ_OTHER + TERM_WRITE_OTHER;

    public double Total3 => ACT + Back4 + READ + WRITE + REF +  DQ + TERM_WRITE + TERM_READ_OTHER + TERM_WRITE_OTHER;

    public double TOTAL => Param.DDR_TYPE switch
    {
        MemoryType.DDR3 => Total3,
        MemoryType.DDR4 => Total4,
        _ => double.NaN
    };

    public double Back4 => PRE_PDN_SLOW + PRE_PDN_FAST + ACT_PDN + ACT_STBY + PRE_STBY;

    public double Back3 => PRE_PDN_SLOW + PRE_PDN_FAST + ACT_PDNS + ACT_PDNF + ACT_STBY + PRE_STBY;

    public double BACKGROUND => Param.DDR_TYPE switch
    {
        MemoryType.DDR3 => Back3,
        MemoryType.DDR4 => Back4,
        _ => double.NaN
    };

    //public double Back4
    //{
    //    get
    //    {
    //        DEBUG.Print($"Background Energy | PRE_PDN_SLOW: {PRE_PDN_SLOW} + PRE_PDN_FAST: {PRE_PDN_FAST} + ACT_PDN: {ACT_PDN} + ACT_STBY: {ACT_STBY} + PRE_STBY: {PRE_STBY} => { PRE_PDN_SLOW + PRE_PDN_FAST + ACT_PDN + ACT_STBY + PRE_STBY}");
    //        return PRE_PDN_SLOW + PRE_PDN_FAST + ACT_PDN + ACT_STBY + PRE_STBY;
    //    }
    //}

}



