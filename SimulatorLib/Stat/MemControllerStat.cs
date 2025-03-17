namespace SimulatorLib
{
    public partial class MemControllerStat
    {
        public uint cid;

        // load
        public uint[] rbinaryloadtick_per_proc, rloadtick_per_proc, wbinaryloadtick_per_proc, wloadtick_per_proc;

        // writeback
        public int wbmode_fraction;

        public AverageObject rds_per_wb_mode, wbs_per_wb_mode, wbmode_blp, wbmode_length, wbmode_distance;

        // Refresh
        public AverageObject read_queue_latency_perchan;

        public uint stall_on_refresh;

        // ChargeCache
        public uint cc_hit, cc_miss;

    }

}
