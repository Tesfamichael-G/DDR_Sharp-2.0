namespace SimulatorLib.DDR
{
    public partial class Rank
    {
        public void CalculatePower()
        {
            var Scheduled_Power = new ScheduledPower(t, this);
            var (E, P) = Scheduled_Power.Compute();

            Stat.BackgroundPower = P.BACKGROUND;
            Stat.ActPower = P.ACT;
            Stat.ReadPower = P.READ;
            Stat.WritePower = P.WRITE;
            Stat.ReadTerminatePower = P.DQ;
            Stat.WriteTerminatePower = P.TERM_WRITE;
            Stat.ReadTerminateOtherPower = P.TERM_READ_OTHER;
            Stat.WriteTerminateOtherPower = P.TERM_WRITE_OTHER;
            Stat.RefreshPower = P.REF;
            Stat.TotalRankPower = P.TOTAL;

            Stat.BackgroundEnergy = E.BACKGROUND;
            Stat.ActEnergy = E.ACT;
            Stat.ReadEnergy = E.READ;
            Stat.WriteEnergy = E.WRITE;
            Stat.ReadTerminateEnergy = E.DQ;
            Stat.WriteTerminateEnergy = E.TERM_WRITE;
            Stat.ReadTerminateOtherEnergy = E.TERM_READ_OTHER;
            Stat.WriteTerminateOtherEnergy = E.TERM_WRITE_OTHER;
            Stat.RefreshEnergy = E.REF;
            Stat.TotalRankEnergy = E.TOTAL;


        }


    }



}

