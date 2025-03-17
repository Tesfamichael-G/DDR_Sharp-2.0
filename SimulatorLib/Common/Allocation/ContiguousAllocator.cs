using System.Collections.Generic;

namespace SimulatorLib.Common
{

    public class ContiguousAllocator : IPageConverter
    {
        private long PAGE_SIZE;
        private Dictionary<long, long> PAGE_TABLE;  
        private long MAX_MEM_ROWS; 

        private long PHY_PAGE_NUM;
        public ContiguousAllocator(long pageSize, long MaxRows)
        {
            PHY_PAGE_NUM = 0;
            PAGE_SIZE = pageSize;
            PAGE_TABLE = new Dictionary<long, long>();
            MAX_MEM_ROWS = MaxRows;
        }


        public long SCAN(long address)
        {
            long PAGE_ID = address / PAGE_SIZE;
            long PAGE_MODE = address % PAGE_SIZE;

            if (PAGE_TABLE.ContainsKey(PAGE_ID))
                return PAGE_TABLE[PAGE_ID] * PAGE_SIZE + PAGE_MODE;

            PAGE_TABLE.Add(PAGE_ID, PHY_PAGE_NUM);
            
            PHY_PAGE_NUM++;

            long PHYSICAL_ADDRESS = PHY_PAGE_NUM * PAGE_SIZE + PAGE_MODE;
            
            DEBUG.Assert(PHY_PAGE_NUM < MAX_MEM_ROWS);

            return PHYSICAL_ADDRESS;
        }


    }

}