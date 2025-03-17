namespace SimulatorLib
{

    public partial class Core_Stat
    {


        // L1 Cache
        public AverageObject L1C_Read_HitRate;
        public AverageObject L1C_Write_HitRate;
        public AverageObject L1C_HitRate;

        public uint L1C_Read_Hits;
        public uint L1c_Read_Misses;
        public uint L1C_Write_Hits;
        public uint L1C_Write_Misses;
        public uint L1C_Dirty_Eviction;

        public AverageObject L1C_AVG_Hit_Latency;

        // L2 Cache
        public AverageObject L2C_Read_HitRate;
        public AverageObject L2C_Write_HitRate;
        public AverageObject L2C_HitRate;

        public uint L2C_Read_Hits;
        public uint L2C_Read_Misses;
        public uint L2C_Write_Hits;
        public uint L2C_Write_Misses;
        public uint L2C_Dirty_Eviction;

        public AverageObject L2C_AVG_Hit_Latency;

        // LLC Cache
        public AverageObject LLC_Read_HitRate;
        public AverageObject LLC_Write_HitRate;
        public AverageObject LLC_HitRate;

        public uint LLC_Rread_Hits;
        public uint LLC_Read_Misses;
        public uint LLC_Write_Hits;
        public uint LLC_Write_Misses;
        public uint LLC_Dirty_Eviction;

        public AverageObject LLC_AVG_Hit_Latency;


        public void Initialize()
        {


            row_hit_rate_read = new AverageObject();
            row_hit_rate_write = new AverageObject(); //SamplePercentAvgStat 

            // Latency (time between when a request is issued and served) SampleAvgStat 
            read_avg_latency = new AverageObject();
            write_avg_latency = new AverageObject();
            copy_avg_latency = new AverageObject();

            // Bank-level parallelism SampleAvgStat 
            service_blp = new AverageObject();

            // Queueing latency SampleAvgStat 
            read_queue_latency_perproc = new AverageObject();



            L1C_Read_HitRate = new AverageObject();
            L1C_Write_HitRate = new AverageObject();
            L1C_HitRate = new AverageObject();

            L1C_Read_Hits = 0;
            L1c_Read_Misses = 0;
            L1C_Write_Hits = 0;
            L1C_Write_Misses = 0;
            L1C_Dirty_Eviction = 0;

            L1C_AVG_Hit_Latency = new AverageObject();

            // L1 Cache
            L2C_Read_HitRate = new AverageObject();
            L2C_Write_HitRate = new AverageObject();
            L2C_HitRate = new AverageObject();

            L2C_Read_Hits = 0;
            L2C_Read_Misses = 0;
            L2C_Write_Hits = 0;
            L2C_Write_Misses = 0;
            L2C_Dirty_Eviction = 0;

            L2C_AVG_Hit_Latency = new AverageObject();

            // L1 Cache
            LLC_Read_HitRate = new AverageObject();
            LLC_Write_HitRate = new AverageObject();
            LLC_HitRate = new AverageObject();

            LLC_Rread_Hits = 0;
            LLC_Read_Misses = 0;
            LLC_Write_Hits = 0;
            LLC_Write_Misses = 0;
            LLC_Dirty_Eviction = 0;

            LLC_AVG_Hit_Latency = new AverageObject();

        }

    }


}
