namespace SimulatorLib
{
    public partial class Core_Stat
    {

        public Core_Stat()
        {
            Initialize();
        }

        //public Proc_Stat PROC;

        public long cycle;

        public uint ipc;  //total instructions, mem and non-mem, retired (executed) by instruction window

        // Memory inst //AccumRateStat
        public double rmpc, wmpc, cmpc;

        public uint read_misses, write_misses, copy_misses;

        //stall
        public uint stall_inst_wnd, stall_read_mctrl, stall_write_mctrl, stall_copy_mctrl, stall_mshr;

        //memory request issued (sent to memory scheduler)
        public uint req;          //total memory requests issued

        public uint read_req;     //read (load) requests issued
        public uint write_req;    //write (store) requests issued
        public uint copy_req;     //copy requests issued
        public uint allocated_physical_pages;

        // Memory request served
        public uint read_req_served, write_req_served, copy_req_served;

        // Per-quantum stats PerQuantumStat 
        public double insts_per_quantum, reads_per_megainst, writes_per_megainst;

        // Writeback hit
        public uint WriteBack_Hit;

        // Row-buffer related stats
        public uint row_hit_read, row_miss_read, row_hit_write, row_miss_write;

        public AverageObject row_hit_rate_read, row_hit_rate_write; //SamplePercentAvgStat 

        // Latency (time between when a request is issued and served) SampleAvgStat 
        public AverageObject read_avg_latency, write_avg_latency, copy_avg_latency;


        // Bank-level parallelism SampleAvgStat 
        public AverageObject service_blp;

        // Queueing latency SampleAvgStat 
        public AverageObject read_queue_latency_perproc;

        public double read_queue_latency_per_quantum; //every 100K cycles PerQuantumStat 

        // Copy range
        public uint chan_copy, rank_copy, bank_copy, inter_sa_copy, intra_sa_copy;




    }

}
