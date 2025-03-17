using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimulatorLib
{


    public class CPU_Parameters
    {

        public int NUMCORES { get; set; }
        public int PROCESSOR_FREQ { get; set; }
        public int ROBSIZE { get; set; }
        public int MAX_RETIRE { get; set; }
        public int MAX_FETCH { get; set; }
        public int PIPELINEDEPTH { get; set; }
        public int MSHR_MAX { get; set; } = 32;
        public int WriteBackQ_MAX { get; set; } = 32;
        public int RDWRQ_MAX { get; set; } = 32;
        public bool WriteBack { get; set; } = true;
        public bool ISSUE_DUPLICATE_REQUEST { get; set; } = true;

        // total number of address bits (i.e. indicates size of memory)
        public int ADDRESS_BITS { get; set; }  = 32;

        //public int ADDRESS_BITS = 32;

        // Traces with cloning and setting calls
        public bool b_read_rc_traces { get; set; }  = false;
        public bool stats_exclude_cpy { get; set; }  = false;

        //public bool DEBUG_CACHE ;
        public bool CACHE_ENABLED { get; set; }       // Turn on cache?= false;        // Turn on cache?

        public int CACHE_BLOCK_SIZE_BITS { get; set; }    // Cache block size in power of 2^6; 

        public bool SHARED_LLC_CACHE_ONLY  { get; set; }     // If the cache is one, is it a shared LLC?= false;// If the cache is one, is it a shared LLC?
        public int L1CACHE_SIZE_BITS  { get; set; } = 15;   //L1 cache size in power of 2. Ex: 15->32KB
        public int L1CACHE_ASSOCIATIVITY  { get; set; } = 8;
        public int L1CACHE_HIT_LATENCY  { get; set; } = 4;

        public bool SHARED_L2CACHE   { get; set; }= true;
        public int L2CACHE_SIZE_BITS  { get; set; } = 22;  //L2 cache size in power of 2.
        public int L2CACHE_ASSOCIATIVITY  { get; set; } = 8;
        public int L2CACHE_HIT_LATENCY  { get; set; } = 20;


        public bool SHARED_L3CACHE  { get; set; } = false;
        public int L3CACHE_SIZE_BITS  { get; set; } = 32;
        public int L3CACHE_ASSOCIATIVITY  { get; set; } = 8;
        public int L3CACHE_HIT_LATENCY  { get; set; } = 32;


        private SpecInfo spec;
        public SpecInfo Spec
        {
            get
            {
                if (spec == null)
                    spec = new SpecInfo(this);

                return spec;
            }
        }

        public class SpecInfo
        {

            //string[] Arch_names = {"ADDRESS_BITS", "ISSUE_DUPLICATE_REQUEST", "WriteBack", "RDWRQ_MAX", "WriteBackQ_MAX", "MSHR_MAX",
            //                           "PIPELINEDEPTH", "MAX_FETCH", "MAX_RETIRE", "ROBSIZE", "PROCESSOR_CLK_MULTIPLIER", "PROCESSOR_FERQ", "NUMCORES"};

            string[] Cache_spec_names = { "CACHE_ENABLED", "CACHE_BLOCK_SIZE_BITS", "SHARED_LLC_CACHE_ONLY",
                                          "L1CACHE_SIZE_BITS", "L1CACHE_ASSOCIATIVITY", "L1CACHE_HIT_LATENCY",
                                          "SHARED_L2CACHE", "L2CACHE_SIZE_BITS", "L2CACHE_ASSOCIATIVITY", "L2CACHE_HIT_LATENCY",
                                          "SHARED_L3CACHE", "L3CACHE_SIZE_BITS", "L3CACHE_ASSOCIATIVITY", "L3CACHE_HIT_LATENCY"};

            //string[] MemCtrlr_names = { "MAPPING_METHOD", "ADDRESS_MAPPING", "WQ_LOOKUP_LATENCY", "xbar_latency", "READQ_MAX", "WRITEQ_MAX" };

            public List<KeyValue_Item> Arch;
            public List<KeyValue_Item> MemCtrlr;
            public List<KeyValue_Item> Cache;
            PropertyInfo[] properties;

            public SpecInfo(CPU_Parameters param)
            {
                if (Arch == null)
                    Arch = new List<KeyValue_Item>();

                if (Cache == null)
                    Cache = new List<KeyValue_Item>();

                //if (MemCtrlr == null)
                //    MemCtrlr = new List<KeyValue_Item>();

                properties = param.GetType().GetProperties();

                System.Diagnostics.Debug.Print($"properties.count = {properties.Count()}");

                foreach (var prop in properties)
                {
                    if (prop.Name == "Spec" ||
                        prop.Name == "PROCESSOR_CLK_MULTIPLIER" ||
                        prop.Name == "b_read_rc_traces" ||
                        prop.Name == "stats_exclude_cpy" ||
                        prop.Name == "ADDRESS_BITS" ||
                        prop.Name == "ISSUE_DUPLICATE_REQUEST")
                        continue;

                    var val = prop.GetValue(param);
                    var name = prop.Name + ": ";

                    var line = new KeyValue_Item { Name = name.ToUpper(), Value = val };

                    if (Cache_spec_names.Contains(prop.Name))
                        Cache.Add(line);
                    //else if (MemCtrlr_names.Contains(prop.Name))
                    //    MemCtrlr.Add(line);
                    else
                        Arch.Add(line);

                }

            }


        }




    }

}
