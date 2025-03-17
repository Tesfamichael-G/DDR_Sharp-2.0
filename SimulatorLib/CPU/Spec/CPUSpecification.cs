namespace SimulatorLib.CPU.Spec
{
    public partial class CPUSpecification
    {


        //public bool DEBUG_CACHE { get; set; }

        public int NUMCORES { get; set; }
        public int FERQUENCY { get; set; }
        public int CLK_MULTIPLIER { get; set; }
        public int ROB_SIZE { get; set; }
        public int MAX_RETIRE { get; set; }
        public int MAX_FETCH { get; set; }
        public int PIPELINE_DEPTH { get; set; }
        public int MSHR_MAX { get; set; } = 32;
        public int WriteBackQ_MAX { get; set; } = 32;
        public int RDWRQ_MAX { get; set; } = 32;
        public bool WriteBack { get; set; } = true;

        public bool ISSUE_DUPLICATE_REQUEST { get; set; } = true;

        public bool CACHE_ENABLED { get; set; }        // Turn on cache?= true;         
        public int BLOCK_SIZE_BITS { get; set; }       // Cache block size in power of 2= 6;  


        /// <summary>
        /// L1 Cache
        /// </summary>
        public bool SHARED_LLC_CACHE_ONLY { get; set; }// If the cache is one, is it a shared LLC?= false;// If the cache is one, is it a shared LLC?
        public uint L1CACHE_SIZE_BITS { get; set; } = 15;//L1 cache size in power of 2. Ex: 15->32KB
        public uint L1CACHE_ASSOCIATIVITY { get; set; } = 8;
        public uint L1CACHE_HIT_LATENCY { get; set; } = 4;



        /// <summary>
        /// L2 Cache
        /// Is L2 shared by all cores
        /// </summary>
        public bool SHARED_L2CACHE { get; set; } = true;
        public uint L2CACHE_SIZE_BITS { get; set; } = 22;  //L2 cache size in power of 2.
        public uint L2CACHE_ASSOCIATIVITY { get; set; } = 8;
        public uint L2CACHE_HIT_LATENCY { get; set; } = 20;


        /// <summary>
        /// L3 Cache
        /// </summary>
        public bool SHARED_L3CACHE { get; set; } = false;
        public uint L3CACHE_SIZE_BITS { get; set; } = 32;
        public uint L3CACHE_ASSOCIATIVITY { get; set; } = 8;
        public uint L3CACHE_HIT_LATENCY { get; set; } = 32;
        public int ADDRESS_BITS { get; set; }
    }

}

