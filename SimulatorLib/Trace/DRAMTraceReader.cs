using System;
using System.Globalization;
using System.IO;
using SimulatorLib.Common;

namespace SimulatorLib
{

    public class DRAMTraceReader : ITraceReader
    {

        private Simulator Sim;
        private StreamReader Reader;

        public long cur_inst_count;
        char[] cmapping;
        string[] MappingArray;

        public DRAMTraceReader(string filename)
        {
            Reader= new StreamReader(filename);
        }

        public DRAMTraceReader(string filename, Simulator sim)
        {

            Sim = sim;

            MappingArray = sim.Param.ADDRESS_MAPPING.ToString().Split("_");
            cmapping = new char[MappingArray.Length];
            for (int i = 0; i < MappingArray.Length; i++)
            {
                cmapping[i] = CharValue(MappingArray[i]);
            }

            Reader = new StreamReader(filename);
             
        }

        char CharValue(string s)
        {
            switch (s.ToUpper())
            {
                case "ROW": return 'r';
                case "RANK": return 'R';
                case "BANKG": return 'G';
                case "BANK": return 'B';
                case "CHAN": return 'C';
                case "COL": return 'c';
            }
            return '0';
        }

        ReadOnlySpan<char> NextRD()
        {
            ReadOnlySpan<char> line = Reader.ReadLine().AsSpan();
            
            if (line == null)
                return null;

            int index = line.IndexOf(' ');
            char cmd = line.Slice(index + 1)[0];
            ReadOnlySpan<char> addr = line.Slice(0, index).Trim();
            long ReadAddress = Convert.ToInt64(addr.ToString(), 16);

            //DEBUG.Print($"*** {line.ToString()} => {ReadAddress} [{ReadAddress>>6}] | '{cmd}'");

            if (cmd != 'R')
            {
                //DEBUG.Print($"*** SKIP {line.ToString()} => {ReadAddress} [{ReadAddress>>6}]");
                line = NextRD();
            }

            //DEBUG.Print($"*** *** {line.ToString()} => {ReadAddress} [{ReadAddress>>6}]");
            return line;
        }

        private TraceRecord GetDRAMTrace2(int CoreID)
        {
            ReadOnlySpan<char> line = Reader.ReadLine().Trim();

            if (line == null)
                return null;

            Request REQ = new Request(Sim);  

            long ReadAddress = Convert.ToInt64(line.ToString(), 16);
             
            REQ.TYPE = Operation.READ;

            REQ.CMD = COMMAND.NOP;
            REQ.CanBeIssued = false;

            REQ.PhysicalAddress = ReadAddress;
            REQ.Update(CoreID, ReadAddress);

            var record = new TraceRecord();
            record.Request = REQ;
            record.NumCPU_Ins = 0;

            return record;

        }

        private TraceRecord GetDRAMTrace(int CoreID)
        {
            string LINE = Reader.ReadLine();

            ReadOnlySpan<char> line = LINE.AsSpan(); // NextRD(); //

            if (line == null)
                return TraceRecord.NULL;

            Request REQ = new Request(Sim);  

            int index = line.IndexOf(' ');
            ReadOnlySpan<char> addr = line.Slice(0, index).Trim();
            char cmd = line.Slice(index+1)[0];

            long ReadAddress = Convert.ToInt64(addr.ToString(), 16);
             
            REQ.TYPE = (cmd == 'W')? Operation.WRITE:Operation.READ;

            REQ.CMD = COMMAND.NOP;
            REQ.CanBeIssued = false;

            REQ.PhysicalAddress = ReadAddress;
            REQ.Update(0, ReadAddress);

            var record = new TraceRecord();
            record.Request = REQ;
            record.NumCPU_Ins = 0;

            DEBUG.Print($"-----({addr}, {cmd}) => ({REQ.PhysicalAddress}|{REQ.BlockAddress}, {REQ.TYPE}) \n");
            DEBUG.Print($"-----(record.BlockAddress: {record.Request.BlockAddress} \n");

            return record;

        }

        public void Close()
        {
            Reader.Close();
        }
        public TraceRecord NextTraceRecord() => GetDRAMTrace(0);

        public TraceRecord NextTraceRecord(int CoreID) => GetDRAMTrace(CoreID);

        public DDR_Request NextDRAM_TraceRecord()
        {
            ReadOnlySpan<char> line = Reader.ReadLine().AsSpan();

            if (line == null)
                return null;

            DDR_Request REQ = new DDR_Request(Sim);

            int index = line.IndexOf(' ');
            ReadOnlySpan<char> addr = line.Slice(0, index).Trim();
            char cmd = line.Slice(index + 1)[0];

            long ReadAddress = Convert.ToInt64(addr.ToString(), 16); 

            REQ.TYPE = (cmd == 'W') ? Operation.WRITE : Operation.READ;

            REQ.CMD = COMMAND.NOP;
            REQ.CanBeIssued = false;

            REQ.PhysicalAddress = ReadAddress;
            //REQ.Update(ReadAddress);
            REQ.TsCompletion = -100;
            REQ.Latency = -100;
            REQ.IsServed = false;

            //REQ.Paddr = paddr;

            //if (Sim.PageConverter != null)
            //    REQ.PhysicalAddress =  Sim.PageConverter.ScanPage(ReadAddress);//Paddr = Sim.PageConverter.ScanPage(paddr);

            REQ.PhysicalAddress = ReadAddress;

            int byteOffset = Sim.Param.tx_bits;
            REQ.BlockAddress = ReadAddress >> byteOffset;//>> Sim.Param.CACHE_BLOCK_SIZE_BITS;

            long ADDR = (int)ReadAddress >> byteOffset;

            for (int m = cmapping.Length - 1; m >= 0; m--)
            {
                switch (cmapping[m])
                {
                    case 'r':
                        REQ.Row = (int)ADDR & ((1 << Sim.Param.rowBits) - 1);
                        ADDR >>= Sim.Param.rowBits;
                        break;
                    case 'R':
                        REQ.Rank = (int)ADDR & ((1 << Sim.Param.rankBits) - 1);
                        ADDR >>= Sim.Param.rankBits;
                        break;
                    case 'G':
                        REQ.BankGroup = (int)ADDR & ((1 << Sim.Param.bankGBits) - 1);
                        ADDR >>= Sim.Param.bankGBits;
                        break;
                    case 'B':
                        REQ.Bank = (int)ADDR & ((1 << Sim.Param.bankBits) - 1);
                        ADDR >>= Sim.Param.bankBits;
                        break;
                    case 'C':
                        REQ.Channel = (int)ADDR & ((1 << Sim.Param.channelBits) - 1);
                        ADDR >>= Sim.Param.channelBits;
                        break;
                    case 'c':
                        REQ.Column = (int)ADDR & ((1 << Sim.Param.colBits) - 1);
                        ADDR >>= Sim.Param.colBits;
                        break;

                    default:
                        break;
                }
            }

            //REQ.MemAddr = Translator.Translate(Paddr);

            //Sim.STAT.CORE[CoreID].allocated_physical_pages++;


            //var record = new TraceRecord();
            //record.Request = REQ;
            //record.NumCPU_Ins = 0;

            //DEBUG.Print($"-----({addr}, {cmd}) => ({REQ.PhysicalAddress}{REQ.BlockAddress}, {REQ.TYPE}) \n");

            return REQ;
        }


        string path = @"C:\Users\tmik\Desktop\DeActivator\Workspace\input\";
        public bool WriteTrace(string outputfile)
        {
            string ofile = path + outputfile;
            StreamWriter sw = new StreamWriter(ofile);

            ReadOnlySpan<char> line = Reader.ReadLine().AsSpan();

            while (line != null)
            {
                int index = line.IndexOf(' ');
                ReadOnlySpan<char> addr = line.Slice(0, index).Trim();
                long ReadAddress = Convert.ToInt64(addr.ToString(), 16); 
                sw.WriteLine(ReadAddress);

                line = Reader.ReadLine().AsSpan();
            }
            sw.Close();
            return true;
        }

        public void TestReaderState()
        {
            throw new NotImplementedException();
        }
    }




}
