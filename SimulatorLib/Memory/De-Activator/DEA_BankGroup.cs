namespace SimulatorLib.DeActivator;

public class DEA_BankGroup : DDR.BankGroup
{

    public DEA_BankGroup(Parameters param, int rank, int id) :base(param, rank, id) 
    {
        t = param;
        RankID = rank;
        ID = id;
        Banks = new DEA_Bank[param.NUM_BANKS];


        for (int i = 0; i < param.NUM_BANKS; i++)
        {
            Banks[i] = new DEA_Bank(param, RankID, ID, i);
        }

        WR_RD = t.CWL + t.BL + t.WTRL;

    }

    public DEA_BankGroup(Parameters param, int rank, int id, Parameters_DeActivator config) :this(param, rank, id) 
    {
        for (int i = 0; i < param.NUM_BANKS; i++)
        {
            Banks[i] = new DEA_Bank(param, RankID, ID, i, config);
        }

    }



}


