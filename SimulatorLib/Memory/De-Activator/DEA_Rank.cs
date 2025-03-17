namespace SimulatorLib.DeActivator
{
    public class DEA_Rank : DDR.Rank
    {
        public DEA_Rank(Parameters param, int id)
        {
            CONFIG = t = param;
            ID = id;
            Groups = new DEA_BankGroup[CONFIG.NUM_BANK_GROUPS];
            for (int i = 0; i < CONFIG.NUM_BANK_GROUPS; i++)
            {
                Groups[i] = new DEA_BankGroup(param, ID, i);
            }

        }

        public DEA_Rank(Parameters param, int id, Parameters_DeActivator config):this(param,id)
        {
            CONFIG = t = param;
            ID = id;
            Groups = new DEA_BankGroup[CONFIG.NUM_BANK_GROUPS];
            for (int i = 0; i < CONFIG.NUM_BANK_GROUPS; i++)
            {
                Groups[i] = new DEA_BankGroup(param, ID, i, config);
            }

        }

    }


}
