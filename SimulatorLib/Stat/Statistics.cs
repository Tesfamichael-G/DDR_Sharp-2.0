using SimulatorLib.DRAM;

namespace SimulatorLib
{
    public partial class Stat
    {

        Simulator Sim;

        public Core_Stat[] CORE;

        public Stat(Simulator sim)
        {
            Sim = sim;

            int BankMax = (sim.Param.DDR_TYPE == MemoryType.DDR4) ? sim.Param.NUM_BANK_GROUPS * sim.Param.NUM_BANKS : sim.Param.NUM_BANKS;

            //CHANNEL = new ChannelStat[sim.Param.NUM_CHANNELS];
            //RANK = new RankStat[sim.Param.NUM_CHANNELS, sim.Param.NUM_RANKS];
            //BANK = new BankStat[sim.Param.NUM_CHANNELS, sim.Param.NUM_RANKS, BankMax];
            //MEM = new MemoryStat();

            //for (int i = 0; i < sim.Param.NUM_CHANNELS; i++)
            //{
            //    CHANNEL[i] = new ChannelStat();
            //    for (int j = 0; j < sim.Param.NUM_RANKS; j++)
            //    {
            //        RANK[i, j] = new RankStat();

            //        for (int k = 0; k < BankMax; k++)
            //        {
            //            BANK[i, j, k] = new BankStat();
            //        }
            //    }
            //}

            CORE = new Core_Stat[sim.Param.NUMCORES];
            for (int i = 0; i < CORE.Length; i++)
                CORE[i] = new Core_Stat();

        }


        //public ChannelStat[] CHANNEL;
        //public RankStat[,] RANK;
        //public BankStat[,,] BANK;
        //public MemoryStat MEM;
        public long Cycle => Sim.Cycle;





    }



}
