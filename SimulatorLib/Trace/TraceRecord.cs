using SimulatorLib.Common;

namespace SimulatorLib
{

    public class TraceRecord
    {
        //public long RAW;
        public int NumCPU_Ins { get; set; }

        public Request Request { get; set; }

        //public string Text => $"CPU: {NumCPU_Ins.ToString().PadLeft(0, ' ')} " +
        //                      $"MEM {MemoryRequest.TYPE.ToString().PadLeft(5)} " +
        //                      $"Address: {MemoryRequest.PhysicalAddress.ToString().PadLeft(10, '0')}|{MemoryRequest.BlockAddress.ToString().PadLeft(10, '0')}";

        public bool IsNotModified = true;

        public static TraceRecord NULL = new TraceRecord
        {
            NumCPU_Ins = -100,
        };

    }

    public class DDR_Request
    {
        Simulator Sim;

        internal IPageConverter PageConverter => Sim.PageConverter;
        internal IAddressTranslator Translator => Sim.Translator;

        public DDR_Request(Simulator sim)
        {
            Sim = sim;
        }

        public long PhysicalAddress;
        public long BlockAddress;

        public int Channel;
        public int Rank;
        public int BankGroup;
        public int Bank;
        public long Row;
        public long Column;

        public long TsArrival;
        public long TsQueued;
        public long TsCompletion;
        public long Latency;

        public COMMAND CMD;
        public bool CanBeIssued;
        public Operation TYPE;

        long Paddr;
        public int WordOffset;

        public bool IsServed;

        //public void Update(long paddr)
        //{
        //    TsCompletion = -100;
        //    Latency = -100;
        //    IsServed = false;

        //    PhysicalAddress = paddr;
        //    Paddr = paddr;

        //    //if (PageConverter != null)
        //    //    Paddr = PageConverter.ScanPage(paddr);

        //    BlockAddress = Paddr >> Sim.Param.CACHE_BLOCK_SIZE_BITS;

        //    MemAddr = Translator.Translate(Paddr);

        //    //Sim.STAT.CORE[CoreID].allocated_physical_pages++;

        //}


    }


}
