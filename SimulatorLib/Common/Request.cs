using SimulatorLib.Memory.Refresh;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimulatorLib.Common;


public partial class Request
{

    Simulator Sim;

    internal IPageConverter PageConverter => Sim.PageConverter;
    internal IAddressTranslator Translator => Sim.Translator;

    public Request(Simulator sim)
    {
        Sim = sim;
    }


    public RequestData REQ_Data;
    public void CreateData(long cycle)
    {
        REQ_Data = new RequestData(PhysicalAddress, BlockAddress, MemAddr, cycle);
    }

    //public List<(long, COMMAND)> ACT_CYCLES = new List<(long, COMMAND)>();
    public int NUM_ACTS = 0;

    public long ID = -1;

    #region commented
    //public long TS_Created = -1;
    //public long ts_started = -1;
    //public long ts_queued = -1;
    //public long ts_Issued = -1;
    //public long ts_endeded = -1;

    //public long duration => ts_endeded - ts_started;

    ////public string History => $"Core[{CoreID}]: {ts_endeded} |  {BlockAddress}, {TS_Created}, {ts_started}, {ts_endeded}, {duration}";
    //public string History => $"{PhysicalAddress} {BlockAddress} |({MemAddr.Channel}{MemAddr.Rank}{MemAddr.Bank}, {MemAddr.Row}, {MemAddr.Column}) | C: {TS_Created}, S: {ts_started}, E: {ts_endeded} | {duration} |";

    #endregion

    public int CoreID;

    //public long PhysicalAddress;
    public long BlockAddress;

    public bool RdWr = false;
    public Request WriteBackReq;

    // Used to indicate if a particular read or write request is generated due to a copy request
    public bool CpyGenReq;

    public MemoryAddress MemAddr;

    public long TsArrival;
    public long TsIssued;
    public long TsDeparture;
    public long TsCompletion;
    public long Latency;
    public long ServiceLatency;

    //public long TsBus=0;

    public COMMAND CMD;
    public bool CanBeIssued;
    public Operation TYPE;

    public int InstructionID;
    public long PC;

    public long PhysicalAddress;

    public bool CacheHit;
    public bool DirtyInsert;

    public CallBack CALLBACK;

    long Paddr;
    public int WordOffset;

    public bool IsServed;
    public void Update(int pid, long paddr)
    {
        CoreID = pid;

        TsDeparture = -100;
        TsCompletion = -100;
        Latency = -100;
        IsServed = false;

        PhysicalAddress = paddr;
        Paddr = paddr;

        Debug.Print($"*** tx-bits: {Sim.Param.tx_bits}; {Paddr}");
        //if (PageConverter != null)
        //    Paddr = PageConverter.SCAN(paddr);

        BlockAddress = Paddr >> Sim.Param.tx_bits;
        Debug.Print($"*** tx-bits: {Sim.Param.tx_bits} | {Paddr} => {BlockAddress}");
        MemAddr = Translator.Translate(Paddr);

        //Sim.STAT.CORE[CoreID].allocated_physical_pages++;

    }

    public override string ToString()
    {
        var s = (IsServed == true) ? 'S' : 'W';
        var I = (CanBeIssued == true) ? 'T' : 'W';
        return $"{BlockAddress}[{s}|{I}]";
    }

}

public partial class RequestData
{

    public long PhysicalAddress;
    public long BlockAddr;
    public MemoryAddress Addr;

    public long TS_Issued { get; set; } = -1;

    public Dictionary<COMMAND, long> IssuedList;

    public RequestData(long physicalAddress, long blockAddr, MemoryAddress addr, long cycle)
    {
        PhysicalAddress = physicalAddress;
        BlockAddr = blockAddr;
        Addr = addr;
        ID = $"{cycle}: {BlockAddr}";
        Cycle = cycle;
        TS_Created = cycle;
        IssuedList = new Dictionary<COMMAND, long>();
        if (BlockAddr == 99814114965)
            DEBUG.Print($"\t\t\t => 99814114965: created called at cycle {cycle}");

    }

    public string ID { get; set; }
    public long Cycle { get; set; }
    public long TS_Created { get; set; } = -1;

    public long TS_Queued { get; set; } = -1;

    public long TS_Endeded { get; set; } = -1;

    public long Duration => TS_Queued - TS_Queued;

    public string History => Header + (string.IsNullOrWhiteSpace(IssueHistory) ? "" : $"{IssueHistory}");

    public string Header =>
        $"{ID}[{Addr.Rank}{Addr.BankGroup}{Addr.Bank}, {Addr.Row}, {Addr.Column}] | {TS_Queued}, {TS_Endeded}";

    private string IssueHistory
    {
        get
        {
            string s = string.Empty;
            foreach (var cmd in IssuedList)
            {
                s += $"\n\t|{cmd.Value}: {cmd.Key}  ";
            }
            return s;
        }
    }



}



public partial class MemRequest
{

    Simulator Sim;

    internal IPageConverter PageConverter => Sim.PageConverter;
    internal IAddressTranslator Translator => Sim.Translator;

    public MemRequest(Simulator sim)
    {
        Sim = sim;
    }

    public long BlockAddress;

    public long TsArrival;
    public long TsIssued;
    public long TsDeparture;
    public long TsCompletion;
    public long Latency;
    public long ServiceLatency;

    public long PhysicalAddress;

    public void Update(int pid, long paddr)
    {

        TsDeparture = -100;
        TsCompletion = -100;
        Latency = -100;

        PhysicalAddress = paddr;

        //if (PageConverter != null)
        //    Paddr = PageConverter.ScanPage(paddr);

        BlockAddress = paddr >> Sim.Param.CACHE_BLOCK_SIZE_BITS;

        /////////////////////////////MemAddr = Translator.Translate(paddr);

    }

    public COMMAND CMD;
    public bool CanBeIssued;
    public Operation TYPE;

    public int Channel;
    public int Rank;
    public int BankGroup;
    public int Bank;
    public long Row;
    public long Column;


}

public class ReqHistory
{

    public long Cycle { get; set; }
    public long Arrival { get; set; }
    public long Block { get; set; }
    public long Row { get; set; }

    public string B1 { get; set; }
    public string B2 { get; set; }
    public string B3 { get; set; }
    public string B4 { get; set; }
    public string B5 { get; set; }
    public string B6 { get; set; }
    public string B7 { get; set; }
    public string B8 { get; set; }

    public long NextPre { get; set; }
    public long NextAct { get; set; }
    public long NextRD { get; set; }
    public long NextWR { get; set; }

    public override string ToString()
    {
        return $"{Cycle}, {Arrival}, {Block}, {Row}, " +
            $"{B1}, {B2}, {B3}, {B4}, {B5}, {B6}, {B7}, {B8}, " +
            $" {NextPre}, {NextAct}, {NextRD}, {NextWR}";
    }
}
