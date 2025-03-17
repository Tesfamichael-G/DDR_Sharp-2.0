using System;
using System.Collections.Generic;

namespace SimulatorLib.Common
{
    public class PageRandomizer : IPageConverter
    {
        public long PAGE_SIZE;
        public Dictionary<long, long> PAGE_TABLE;  
        public List<long> FRAME_TABLE;

        int PAGE_SIZE_bits = 12; //Math.Log(PAGE_SIZE, 2);
        public Random Rand = new Random(0);
        private Simulator Sim;

        private Parameters Param => Sim.Param;

        public PageRandomizer(Simulator sim, long pageSize)
        {
            Sim = sim;
            PAGE_SIZE = pageSize;
            PAGE_TABLE = new Dictionary<long, long>();
            FRAME_TABLE = new List<long>();
            PAGE_SIZE_bits = (int)Math.Log(pageSize, 2);

        }

        public long SCAN(long address)
        {

            //return 100;

            long PAGE_ID = address >> PAGE_SIZE_bits;
            //long PAGE_ID = address / PAGE_SIZE;
            long PAGE_MOD = address % PAGE_SIZE;

            if (PAGE_TABLE.ContainsKey(PAGE_ID))
            {
                return PAGE_TABLE[PAGE_ID] * PAGE_SIZE + PAGE_MOD;
            }

            long FRAME_ID;
            while (true)
            {
                FRAME_ID = Rand.Next();
                if (FRAME_TABLE.Contains(FRAME_ID)) continue;

                FRAME_TABLE.Add(FRAME_ID);
                break;
            }

            PAGE_TABLE.Add(PAGE_ID, FRAME_ID);
            return FRAME_ID * PAGE_SIZE + PAGE_MOD;
        }
  
    }


}