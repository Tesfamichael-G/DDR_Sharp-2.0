namespace SimulatorLib
{
    public partial class RankStat
    {

        public int PDX;
        public int PDE;
        public int PRE;

        public long TS_ACT_Standby ;
        public long TS_ACT_PDN ;
        public long TS_ACT_PDNF ;
        public long TS_ACT_PDNS ;

        public long TS_PRE_PDNF ;
        public long TS_PRE_PDNS ;
        public long TS_PowerUp ;
        public long TS_TERM_READS_FROM_OTHER_RANKS ;
        public long TS_TERM_WRITES_TO_OTHER_RANKS ;

        public long ACT ;
        public long NUM_READS ;
        public long NUM_WRITES ;
        public long REF ;
        public long PDNS ;
        public long PDNF ;
        public long PowerUp ;

        public double BackgroundPower ;
        public double ActPower ;
        public double ReadPower ;
        public double WritePower ;
        public double ReadTerminatePower ;
        public double WriteTerminatePower ;
        public double ReadTerminateOtherPower ;
        public double WriteTerminateOtherPower ;
        public double RefreshPower ;
        public double TotalRankPower ;


        public double BackgroundEnergy ;
        public double ActEnergy ;
        public double ReadEnergy ;
        public double WriteEnergy ;
        public double ReadTerminateEnergy ;
        public double WriteTerminateEnergy ;
        public double ReadTerminateOtherEnergy ;
        public double WriteTerminateOtherEnergy ;
        public double RefreshEnergy ;
        public double TotalRankEnergy ;

        public void Initialize()
        {

            TS_ACT_PDNS = 0;
            TS_ACT_PDNF = 0;
            TS_ACT_PDN = 0;

            TS_PRE_PDNS = 0;
            TS_PRE_PDNF = 0;

            PDNS = 0;
            PDNF = 0;
            PowerUp = 0;

            ACT = 0;

        }



    }



}
