namespace SimulatorLib
{
    public class Proc_Stat
    {

        private Core_Stat[] core;
        public Proc_Stat(int numCores)
        {
            core = new Core_Stat[numCores];
        }

        public Core_Stat this[int index]
        {
            get
            {

                if (core[index] == null)
                {
                    core[index] = new Core_Stat();

                }

                return core[index];
            }
        }


        public long Cycle;


    }

}
