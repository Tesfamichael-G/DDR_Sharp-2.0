//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace TMikMemSim
//{
//    public partial class Parameters
//    {


//        public bool DEBUG_CACHE { get;  set; }
//        public bool CACHE_ENABLED { get; set; }        // Turn on cache?= false;        // Turn on cache?

//        int bs;
//        public int CACHE_BLOCK_SIZE_BITS 
//        { 
//            get
//            {
//                return bs;
//            }
//            set
//            {
//                DEBUG.Print($" *** *** *** BLOCK_SIZE_BITS: changed from {bs} to {value}");
//                bs = value;
//            }
//        }       // Cache block size in power of 2= 6;  

//        /// <summary>
//        /// L1 Cache
//        /// </summary>
//        public bool SHARED_LLC_CACHE_ONLY { get; set; }// If the cache is one, is it a shared LLC?= false;// If the cache is one, is it a shared LLC?
//        public uint L1CACHE_SIZE_BITS { get; set; } = 15;//L1 cache size in power of 2. Ex: 15->32KB
//        public uint L1CACHE_ASSOCIATIVITY { get; set; } = 8;
//        public uint L1CACHE_HIT_LATENCY { get; set; } = 4;

//        /// <summary>
//        /// L2 Cache
//        /// Is L2 shared by all cores
//        /// </summary>
//        public bool SHARED_L2CACHE { get; set; } = true;
//        public uint L2CACHE_SIZE_BITS { get; set; } = 22;  //L2 cache size in power of 2.
//        public uint L2CACHE_ASSOCIATIVITY { get; set; } = 8;
//        public uint L2CACHE_HIT_LATENCY { get; set; } = 20;


//        /// <summary>
//        /// L3 Cache
//        /// </summary>
//        public bool SHARED_L3CACHE { get; set; } = false;
//        public uint L3CACHE_SIZE_BITS { get; set; } = 32;
//        public uint L3CACHE_ASSOCIATIVITY { get; set; } = 8;
//        public uint L3CACHE_HIT_LATENCY { get; set; } = 32;


//    }



//}
