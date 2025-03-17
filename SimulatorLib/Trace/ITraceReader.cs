namespace SimulatorLib
{
    public interface ITraceReader
    {
        public TraceRecord NextTraceRecord();
        public TraceRecord NextTraceRecord(int CoreID);
        public DDR_Request NextDRAM_TraceRecord();
        public bool WriteTrace(string outputfile);

        public void Close();
        public void TestReaderState();
    }

}
