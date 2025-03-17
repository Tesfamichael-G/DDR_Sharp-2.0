namespace SimulatorLib.Memory.Schedule
{
    public interface IScheduler
    {

        public void Tick();
        //void CloseFile();

        public void RobStalled(long addr)
        {
            int x;
        }

    }

}
