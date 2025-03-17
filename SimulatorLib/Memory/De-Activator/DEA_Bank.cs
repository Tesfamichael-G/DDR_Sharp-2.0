using SimulatorLib.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.DeActivator
{
    public class DEA_Bank : DDR.Bank
    {

        public DEA_Bank(Parameters param, int rank, int group, int id) : base(param, rank, group, id)
        {
            t = param;
            RankID = rank;
            GroupID = group;
            ID = id;
            Index = group * param.NUM_BANK_GROUPS + id;
            Stat = new BankStat();
            //Initialize();

        }
        public DEA_Bank(Parameters param, int rank, int group, int id, Parameters_DeActivator config) : this(param, rank, group, id)
        {
            InitializeDEA(config);

        }

        public List<RowBufferData> RowBuffers;

        RowBufferLRU LRU;
        private int ROWBUFFER_MAX;
        private Parameters_DeActivator CONFIG;

        public override bool CanRead(int row) // => ActiveRow == row && cycle >= NextRD;
        {
            if (ID == 6)
                DEBUG.Print($"\t*** {cycle}: [NextRD: {NextRD}] Bank.CanRead => {cycle >= NextRD}");

            return IsHit(row) && cycle >= NextRD;
        }

        public override bool CanWrite(int row) 
        {
            if (ID == 6)
                DEBUG.Print($"\t*** {cycle}: [NextWR: {NextWR}] Bank.CanRead => {cycle >= NextWR}");
            
            return IsHit(row) && cycle >= NextWR;
        } 

        public void InitializeDEA(Parameters_DeActivator config)
        {

            CONFIG = config;

            ROWBUFFER_MAX = CONFIG.MAX_ROW_BUFFERS_PER_BANK;

            LRU = new RowBufferLRU();
            RowBuffers = new List<RowBufferData>(ROWBUFFER_MAX);

            for (int i = 0; i < ROWBUFFER_MAX; i++)
            {
                RowBuffers.Add(new RowBufferData());
            }

        }

        public bool IsHit(int row)
        {
            if (ActiveRow == row)
                return true;

            for (int i = 0; i < ROWBUFFER_MAX; i++)
            {
                if (RowBuffers[i].Row == row)
                {

                    return true;
                }
            }
            return false;
        }


        public bool IsHit(Request REQ)
        {
            if (ActiveRow == REQ.MemAddr.Row)
                return true;

            for (int i = 0; i < ROWBUFFER_MAX; i++)
            {
                if (RowBuffers[i].Row == REQ.MemAddr.Row)
                {

                    return true;
                }
            }

            return false;

        }

        public bool DEBUG_IsHit(int Row)
        {
            if (ActiveRow == Row)
                return true;

            int iCnt = 0;
            int iRow = -1;
            //int eCount = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append($"[ ");
            for (int i = 0; i < ROWBUFFER_MAX; i++)
            {

                var r = RowBuffers[i];

                //if (r.ISNULL)
                //{
                //    eCount++;
                //    continue;
                //}


                if (r.Row == Row)
                {
                    iCnt++;
                    iRow = i;
                    sb.Append($"{i} ");
                }
            }
            sb.Append($"]");

            //if (eCount < ROWBUFFER_MAX)
            //    DEBUG.Print($"*** BANK({RankID}{GroupID}{ID}.RowBuffers.Empty => {eCount}/{ROWBUFFER_MAX}");

            if (iCnt == 0) return false;

            DEBUG.PrintAndAssert(iCnt == 1, $"DeActivator Error: {iCnt} duplicate buffering for {Row} found in bank {ID} at Index/s {sb.ToString()}.");
            RowBuffers[iRow].TimeStamp = cycle;
            //DEBUG.Print($"*** BANK({RankID}{GroupID}{ID} Is Hit at Row[{iRow}] for {Row.Row}");
            return true;
            //[<0-5-33054> (0, 1, 2)]
        }

        public bool AlreadyBuffered(int row)
        {

            for (int i = 0; i < RowBuffers.Count; i++)
            {
                if (RowBuffers[i].Row == row)
                    return true;
            }

            return false;
        }

        public bool TryAddToRowBuffer(int row, out RowBufferData? EvictedRow)
        {
            if (AlreadyBuffered(row))
            {
                EvictedRow = null;
                return false;
            }
            return LRU.TryAdd(RowBuffers, row, cycle, out EvictedRow);
        }

        public bool EmptyRowBufferExists()
        {
            foreach (var rData in RowBuffers)
            {
                if (rData.Row == -1)
                    return true;
            }
            return false;
        }

        public string HotRows
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"*************** Bank({RankID}{GroupID}{ID}) **********************\n");

                for (int i = 0; i < RowBuffers.Count; i++)
                {
                    if (RowBuffers[i].Row == -1)
                        continue;

                    sb.Append($"\t{cycle} => [{RowBuffers[i].Row}, {RowBuffers[i].TimeStamp}]\n");//[{Row},{TimeStamp}]
                }

                return sb.ToString();

            }
        }

        public override void READ(MemoryAddress mAddr)
        {
            DEBUG.Assert(mAddr.BankGroup == GroupID && mAddr.Bank == ID);
            //DEBUG.Assert(STATE == State.ACTIVE);
            DEBUG.Assert(CanRead(mAddr.Row));

            CountDown = t.CL;
            IsBusy = true;
            CAS_Issued = true;

            NextPRE = Math.Max(cycle + t.RTP, NextPRE);
            STATE = State.READING;
            NextState = State.ACTIVE;
            Stat.READ++;

        }

        public override bool WRITE(MemoryAddress mAddr)
        {
            DEBUG.Assert(mAddr.BankGroup == GroupID && mAddr.Bank == ID);
            //DEBUG.Assert(STATE == State.ACTIVE);
            DEBUG.Assert(CanWrite(mAddr.Row));

            CountDown = t.CWL;
            IsBusy = true;
            CAS_Issued = true;

            NextPRE = Math.Max(cycle + WR_TO_PRE, NextPRE); //t[WR<->PRE]  = t.CWL + t.BL + t.WR});
            Stat.WRITE++;
            STATE = State.WRITING;
            NextState = State.ACTIVE;

            return true;
        }



    }


}
