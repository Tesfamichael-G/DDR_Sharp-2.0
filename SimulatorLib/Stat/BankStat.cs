namespace SimulatorLib
{

    public partial class BankStat
    {

        public long ACT_READ = 0;
        public long ACT_WRITE = 0;
        public long PRE = 0;
        public long READ = 0;
        public long WRITE = 0;

        public long ACT => ACT_READ + ACT_WRITE;

    }


}
