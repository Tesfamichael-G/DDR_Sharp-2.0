namespace SimulatorLib.Common
{

    public enum COMMAND
    {
        ACT, 
        PRE, 
        PREA,
        RD, 
        WR, 
        RDA,
        WRA,
        REF, 
        PDE, 
        PDX, 
        SRE, 
        SRX,
        NOP
    }



    public enum Level
    {
        Channel,
        Rank,
        BankGroup,
        Bank,
        Row,
        Column
    }


}