using SimulatorLib.Common;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SimulatorLib;

public class CPUTraceReader : ITraceReader
{


    private Simulator SIM;

    public long cur_inst_count;
    public bool dirty_insert = false;

    IFileReader READER;


    public CPUTraceReader(string filename, Simulator sim)
    {
        READER = filename.Contains(".gz") ? new ZipFileReader(filename) : new FileReader(filename);
        SIM = sim; 
        
    }

    public TraceRecord GetRequest(int CoreID)
    {
        if (SIM.Param.CACHE_ENABLED)
            return Get_REQ(CoreID);
        else
            return Get_FilteredREQ(CoreID);

    }


    int line_no = 0;

    private TraceRecord Get_FilteredREQ(int CoreID)
    {

        string line = READER.NextLine(); //DEBUG.Print($"Line: {line}");
       
        Char[] delim = new Char[] { ' ' };
        string[] tokens = line.Split(delim);

        Request REQ = new Request(SIM);
        long ReadAddress = long.Parse(tokens[1]);
        ReadAddress = ReadAddress | (CoreID << 56);

        REQ.PhysicalAddress = ReadAddress;
        REQ.TYPE = Operation.READ;

        REQ.Update(CoreID, ReadAddress);

        REQ.CMD = COMMAND.NOP;
        REQ.CanBeIssued = false;

        REQ.DirtyInsert = dirty_insert;
        dirty_insert = !dirty_insert;
        REQ.CpyGenReq = SIM.Param.stats_exclude_cpy;

        var record = new TraceRecord();
        record.NumCPU_Ins = int.Parse(tokens[0]);
        //record.RAW = long.Parse(tokens[1]);
        record.Request = REQ;

        line_no++;

        if (line_no > 37250)
        {
            System.Diagnostics.Debug.Print($"=====>TRACE[{CoreID}]: {line}  : ENDING .... LINE: {line_no}");
            SIM.TraceEnding = true;
        }
        //DEBUG.Print($"{Sim.Cycle}| {line} | ({ReadAddress})=>{REQ.BlockAddress}");

        if (!SIM.Param.WriteBack || tokens.Length == 2)
        {
            return record;
        }

        DEBUG.Assert(tokens.Length == 3);
        long WriteBackAddress = long.Parse(tokens[2]);
        WriteBackAddress = WriteBackAddress | (((int)CoreID) << 56);

        Request WB_REQ = new Request(SIM);

        WB_REQ.PhysicalAddress = ReadAddress;
        WB_REQ.TYPE = Operation.WRITE;

        WB_REQ.Update(CoreID, WriteBackAddress);

        WB_REQ.CMD = COMMAND.NOP;
        WB_REQ.CanBeIssued = false;

        REQ.WriteBackReq = WB_REQ;

        //DEBUG.Print($"{Sim.Cycle}| {line} | ({WriteBackAddress})=>{REQ.WriteBackReq.BlockAddress}");

        return record;

    }

    private TraceRecord Get_REQ(int CoreID)
    {
        string line = READER.NextLine();
        Char[] delim = new Char[] { ' ' };
        string[] tokens = line.Split(delim);

        DEBUG.PrintAndAssert(tokens.Length == 6, "trace line = " + line);
        Operation req_type = (int.Parse(tokens[5]) == 0) ? Operation.READ : Operation.WRITE;

        // Read-only requests
        while (SIM.Param.WriteBack == false && req_type == Operation.WRITE)
        {
            line = READER.NextLine();
            tokens = line.Split(delim);
            req_type = (int.Parse(tokens[5]) == 0) ? Operation.READ : Operation.WRITE;
        }

        // Set instruction count b/w requests
        long icount = long.Parse(tokens[0]);
        int cpu_inst_cnt;
        if (cur_inst_count == 0)
            cpu_inst_cnt = 0;
        else
        {
            cpu_inst_cnt = (int)(icount - cur_inst_count);
            DEBUG.PrintAndAssert(cpu_inst_cnt >= 0, "Negative instruction count");
        }

        cur_inst_count = icount;

        var record = new TraceRecord();
        record.NumCPU_Ins = cpu_inst_cnt;

        // Parse virtual address
        long vaddr = long.Parse(tokens[2]);
        vaddr = vaddr + (((int)CoreID) << 48);


        Request REQ = new Request(SIM);
        REQ.TYPE = req_type;
        REQ.PhysicalAddress = vaddr;
        REQ.Update(CoreID, vaddr);

        REQ.CMD = COMMAND.NOP;
        REQ.CanBeIssued = false;

        record.Request = REQ;

        return record;
    }

    public void Close()
    {
        READER.Close();
    }

    public DDR_Request NextDRAM_TraceRecord()
    {
        throw new System.NotImplementedException();
    }

    public TraceRecord NextTraceRecord()
    {
        return GetRequest(0);
    }

    public TraceRecord NextTraceRecord(int CoreID)
    {
        //if (line_no > 37250)
        //{
        //    System.Diagnostics.Debug.Print($"=====>CPUTrace.NextTraceRecord({CoreID})[] <<ENDING>> @ LINE {line_no}");
        //    SIM.TraceEnding = true;
        //}

        return GetRequest(CoreID);

    }

    public bool WriteTrace(string outputfile)
    {
        throw new System.NotImplementedException();
    }

    public void TestReaderState()
    {
        //if (READER == null)
        //{
        //    DEBUG.Print("READER == null");
        //    return;
        //}
        //if (READER.EOF())
        //{
        //    DEBUG.Print("READER.EOF");
        //    return;
        //}

        //DEBUG.Print($"{SIM.Cycle}: TestReaderState().NextLine => {READER.NextLine()}");

    }

}




