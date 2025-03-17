using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using System;

namespace SimulatorLib.CPU
{

    [Flags]
    public enum ROBState
    {
        Ready = 0,
        Busy = 1,
        Full = 2,
        Stalled = 4,
    }

    public partial class ReorderBuffer
    {
        Core core;
        Parameters Param;
        Simulator SIM;

        private long cycle;
        public long Cycle
        {
            get => cycle;
            set
            {
                cycle = value++;
                Issued = 0;
            }
        }

        public int Issued
        {
            get => issued;
            set
            {
                issued = value;
                if (value == Param.MAX_FETCH || Inflight == Param.ROBSIZE)
                {
                    IsReady = false;
                    return;
                }
                IsReady = true;
            }
        }

        public int Head;
        public int Tail;

        public int Inflight;
        public Instruction[] Instructions;

        public long fetched = 0;
        private int issued = 0;
        public long committed = 0;
        public long stall;

        //public ROBState STATE;
        public bool IsReady;//=> STATE != ROBState.Ready;
        public bool MemIssued = false;
        public ReorderBuffer(Core CORE)
        {

            core = CORE;
            Param = core.Param;
            SIM = core.SIM;

            Instructions = new Instruction[Param.ROBSIZE];

            for (int i = 0; i < Param.ROBSIZE; i++)
                Instructions[i] = new Instruction();

            Head = 0;
            Tail = 0;
            Inflight = 0;
            //STATE = ROBState.Ready;
            IsReady = true;

        }

        #region Text

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //public string Text
        //{
        //    get
        //    {
        //        if (Tail == -1)
        //            return "";

        //        sb.Append($"***{Cycle}: ROB DATA (Head, Tail, Inflight) => ({Head}, {Tail}, {Inflight}) ******\n");
        //        //string s = $"*************************** ROB DATA (Head, Tail, Inflight) => ({Head}, {Tail}, {Inflight}) ****************************\n";
        //        //s += $"Head: {Head} | Tail: {Tail} | Inflight: {Inflight}\n";

        //        int iCnt = Tail;

        //        //s += "mem_address : { ";
        //        //for (int i = Head; i != Tail; i = (i + 1) % Param.ROBSIZE)
        //        //    s += $"{i}, ";
        //        //s += $"{iCnt}" + "}\n";

        //        char r;
        //        sb.Append("ETA : { ");
        //        //s += "CT : { ";
        //        for (int i = Head; i != Tail; i = (i + 1) % Param.ROBSIZE)
        //        {
        //            r = (Instructions[i].IsReady) ? 'R' : 'N';
        //            sb.Append($"{Instructions[i].CompletetionTime.ToString().PadLeft(10, r)}, ");
        //            //s += $"{Instructions[i].CompletetionTime.ToString().PadLeft(10, r)}, ";
        //        }
        //        r = (Instructions[iCnt].IsReady) ? 'R' : 'N';
        //        sb.Append($"{Instructions[iCnt].CompletetionTime.ToString().PadLeft(10, r)}" + "}\n");
        //        //s += $"{Instructions[iCnt].CompletetionTime.ToString().PadLeft(10, r)}" + "}\n";
        //        sb.Append("ADR : { ");
        //        //s += "MA : { ";
        //        for (int i = Head; i != Tail; i = (i + 1) % Param.ROBSIZE)
        //            sb.Append($"{Instructions[i].MemAddress.ToString().PadLeft(10, '0')}, ");
        //        //s += $"{Instructions[i].MemAddress.ToString().PadLeft(10, '0')}, ";

        //        sb.Append($"{Instructions[iCnt].MemAddress.ToString().PadLeft(10, '0')}" + "}\n\n");
        //        //s += $"{Instructions[iCnt].MemAddress.ToString().PadLeft(10, '0')}" + "}\n";

        //        //s += "OT : { ";
        //        //for (int i = Head; i != Tail; i = (i + 1) % Param.ROBSIZE)
        //        //    s += $"{Instructions[i].TYPE}, ";
        //        //s += $"{Instructions[iCnt].TYPE}" + "}\n";

        //        //s += "instrpc : { ";
        //        //for (int i = Head; i != Tail; i = (i + 1) % Param.ROBSIZE)
        //        //    s += $"{Instructions[i].PC}, ";
        //        //s += $"{Instructions[iCnt].PC}" + "}\n";

        //        //sb.Append("******************************************************************************************************");
        //        //s += "******************************************************************************************************";

        //        return sb.ToString();

        //    }
        //}
        public string VerticalText
        {
            get
            {
                if (Tail == -1)
                    return "";


                int iCnt = Tail;

                char r;
                sb.Append("ETA : { ");
                for (int i = Head; i != Tail; i = (i + 1) % Param.ROBSIZE)
                {
                    sb.Append($"***{Cycle}: ROB DATA (Head, Tail, Inflight) => ({Head}, {Tail}, {Inflight}) ******\n");
                    r = (Instructions[i].IsReady) ? 'R' : 'N';
                    sb.Append($"{sb.Append($"{Instructions[i].MemAddress.ToString()}, ")}\t\tETA : {Instructions[i].CompletetionTime.ToString()}\n");
                }
                r = (Instructions[Tail].IsReady) ? 'R' : 'N';
                sb.Append($"{sb.Append($"{Instructions[Tail].MemAddress.ToString()}, ")}\t\tETA : {Instructions[Tail].CompletetionTime.ToString()}\n\n");

                return sb.ToString();

            }
        }

        //public string Info
        //{
        //    get
        //    {
        //        string s = $"\t\t\t*************************** ROB DATA (Head, Tail, Inflight) => ({Head}, {Tail}, {Inflight}) ****************************\n\t\t\t";
        //        //s += $"Head: {Head} | Tail: {Tail} | Inflight: | {Inflight} | TraceIsDone | {IsTraceDone}\n\t\t\t";

        //        int iCnt = Instructions.Length - 1;

        //        s += "comptime : { ";
        //        for (int i = 0; i < iCnt; i++)
        //            s += $"{Instructions[i].CompletetionTime}, ";
        //        s += $"{Instructions[iCnt].CompletetionTime}" + "}\n\t\t\t";

        //        s += "mem_address : { ";
        //        for (int i = 0; i < iCnt; i++)
        //            s += $"{Instructions[i].MemAddress}, ";
        //        s += $"{Instructions[iCnt].MemAddress}" + "}\n\t\t\t";

        //        s += "optype : { ";
        //        for (int i = 0; i < iCnt; i++)
        //            s += $"{Instructions[i].TYPE}, ";
        //        s += $"{Instructions[iCnt].TYPE}" + "}\n\t\t\t";

        //        s += "instrpc : { ";
        //        for (int i = 0; i < iCnt; i++)
        //            s += $"{Instructions[i].PC}, ";
        //        s += $"{Instructions[iCnt].PC}" + "}\n\t\t\t";

        //        s += "**********************************";

        //        return s;

        //    }
        //}



        #endregion

        private long LastIssued = -1;

        int x = 0;
        public void Tick()
        {

            //if (SIM.TraceEnding)
            //{
            //    DEBUG.Print($"\tROB: STALL: {stall} ");
            //}

            Cycle++;

            int num_ret = RetireInstructions();

            //STATE =
            IsReady = (Inflight < Param.ROBSIZE) ? true : false;
            MemIssued = LastIssued==REQUEST.BlockAddress;

            if (stall > 1000000)
            {
                SIM.ROBStalled = true;
                x++;

                if (x % 4096 == 0)
                {

                    string str = $"Cycles= {Cycle}  -- Inst Window stalled for too long: window head = {Head}";
                    //DEBUG.AssertPrint(false, str);
                    DEBUG.Print(str);

                    int t = (Tail - 1 < 0) ? Tail + 127 : Tail - 1;
                    char rh = (Instructions[Head].IsReady) ? 'R' : 'N';
                    char rt = (Instructions[t].IsReady) ? 'R' : 'N';
                    long? th = Instructions[Head].CompletetionTime - Cycle;
                    long? tt = Instructions[t].CompletetionTime - Cycle;
                    Instruction insh = Instructions[Head];
                    Instruction inst = Instructions[t];
                    char p = (insh.TYPE == Operation.READ) ? 'R' : 'W';

                    //MemoryController mc = core.SIM.MemSystem.MemControllers[0];
                    //bool fromReadQueue = (insh.TYPE == Operation.READ) ? true : false;
                    //(Request req, bool isHit) = mc.QUEUE.GetRequest(insh.MemAddress, fromReadQueue);
                    //if (req != null)
                    //{
                    //    DEBUG.Print($"\t\t Request({req.BlockAddress}).IsHit => {isHit}");
                    //}

                    DEBUG.Print($"***{Cycle}: ROB({Inflight}) |(Head:{Head}, Tail:{Tail}) | \tH: {rh}\t{p}({insh.MemAddress})\tETA : {th} |  T': {rt}\t{inst.MemAddress}\tETA : {tt}");
                    SIM.MemSystem.RobStalled(insh.MemAddress);
                    x = 1;
                }

            }



            #region Commented
            //if (num_ret > 0) DEBUG.Print($"Retired {num_ret}");

            //DEBUG.Print($"ROB.Update | Tail: {Tail}");

            //Update();


            //DEBUG.Print($"Cycle: {Cycle} | TRACE: CPU Ins.:{TRACE.NumCPU_Ins}, Type: {REQUEST.TYPE}, Mem Address: {REQUEST.BlockAddress}\n{Text}");
            //core.PrintIf($"Cycle: {Cycle} | TRACE: CPU Ins.:{TRACE.NumCPU_Ins}, Type: {REQUEST.TYPE}, Mem Address: {REQUEST.BlockAddress}\n{Text}", Cycle > 1200);

            //if (Cycle > 855900)
            //{
            //    int t =(Tail-1 <0) ? Tail+128:Tail - 1;
            //    char rh = (Instructions[Head].IsReady) ? 'R' : 'N';
            //    char rt = (Instructions[t].IsReady) ? 'R' : 'N';
            //    long? th = Instructions[Head].CompletetionTime - Cycle;
            //    long? tt = Instructions[t].CompletetionTime - Cycle;
            //    Instruction insh = Instructions[Head];
            //    Instruction inst = Instructions[t];

            //    DEBUG.Print($"***{Cycle}: ROB({Inflight}) |(Head:{Head}, Tail:{Tail}) | \tH: {rh}\t{insh.MemAddress}\tETA : {th} |  T': {rt}\t{inst.MemAddress}\tETA : {tt}");
            //}

            //if (Cycle >= 477200)
            //{
            //    DEBUG.Print($"***{Cycle}: ROB DATA (Head, Tail, Inflight) => ({Head}, {Tail}, {Inflight}) ******");
            //    char r;
            //    for (int i = Head; i != Tail; i = (i + 1) % Param.ROBSIZE)
            //    {
            //        r = (Instructions[i].IsReady) ? 'R' : 'N';
            //        DEBUG.Print($"\t\t{r}\t\t{Instructions[i].MemAddress.ToString()}\t\tETA : {(Instructions[i].CompletetionTime-Cycle).ToString()}");
            //    }
            //    r = (Instructions[Tail].IsReady) ? 'R' : 'N';
            //    DEBUG.Print($"\t\t{r}\t\t{Instructions[Tail].MemAddress.ToString()}\t\tETA : {(Instructions[Tail].CompletetionTime-Cycle).ToString()}\n");
            //}

            #endregion        
        }

        public void ConsumeInstructions()
        {
            //if (issued == 0)
            //    P($"{Cycle}: IssueInstructions()");

            if (!IsReady)
            {
                //P($"\t*  ([{REQUEST.BlockAddress}], {TRACE.NumCPU_Ins}) => ROB Not Ready");
                if (issued == 0)
                {
                    stall++;
                }
                return;
            }

            while (Issued < Param.MAX_FETCH)
            {

                if (TRACE.NumCPU_Ins == 0)
                {
                    break;
                }

                if (Inflight == Param.ROBSIZE)
                {
                    //P($"\t* Issued {Issued}: ([{REQUEST.BlockAddress}], {TRACE.NumCPU_Ins})  ROB.IsFull return...");
                    IsReady = false;
                    break;
                }

                //P($"\t* Issued {Issued}: ([{REQUEST.BlockAddress}], {TRACE.NumCPU_Ins})");

                Consume(Operation.CPU);

            }

            if (!IsReady)
                return;

            if (MemIssued)
            {
                //P($"\t* Memory request already Issued. return...");
                return;
            }

            ////P($"\t* Issued {Issued}: Mem([{REQUEST.BlockAddress}]) | remaing cpu ins: {TRACE.NumCPU_Ins}");
            Consume(REQUEST.TYPE);

            void Consume(Operation type)
            {
                Instruction ins = Instructions[Tail];
                ins.TYPE = type;
                ins.IsReady = false;
                ins.CompletetionTime = Cycle + Param.PIPELINEDEPTH;
                Inflight++;
                fetched++;
                Issued++;

                if (type == Operation.CPU)
                {
                    ins.MemAddress = 0;
                    ins.IsMem = false;
                    TRACE.NumCPU_Ins--;
                    Tail = (Tail + 1) % Param.ROBSIZE;
                    return;
                }

                ins.MemAddress = REQUEST.BlockAddress;
                ins.IsMem = true;
                MemIssued = true;
                LastIssued = REQUEST.BlockAddress;

                REQUEST.InstructionID = Tail;

                if (ins.TYPE == Operation.READ)
                {
                    MemoryController MC = SIM.MemSystem.MemControllers[REQUEST.MemAddr.Channel];
                    if (MC.WriteQueueContains(REQUEST.BlockAddress))
                        ins.CompletetionTime = Cycle + Param.WQ_LOOKUP_LATENCY + Param.PIPELINEDEPTH;
                    else if (MC.ReadQueueContains(REQUEST.BlockAddress))
                        ins.CompletetionTime = Cycle + 1 + Param.PIPELINEDEPTH;
                    else
                        ins.CompletetionTime = null;

                  if (ins.MemAddress == 43209711458)
                    DEBUG.Print($"{Cycle}: Consume(43209711458) => @[{Tail}]");  //|{TRACE.RAW}| 

                  //string tm = ins.CompletetionTime == null ? "NULL" : ins.CompletetionTime.Value.ToString();
                    //P($"\t\t >>>> CompletionTime({REQUEST.TYPE}[{REQUEST.BlockAddress}]) => {tm}");
                }


                Tail = (Tail + 1) % Param.ROBSIZE;


            }

        }

        public int RetireInstructions()
        {
            int num_ret = 0;
            Instruction ins;
            bool test = false;

            while ((num_ret < Param.MAX_RETIRE) && Inflight != 0)
            {
                ins = Instructions[Head];

                if (ins.MemAddress == 1545293 && (stall > 10000) && (x % 10000 == 0))
                {
                    DEBUG.Print($"\t\t\t => HEAD[ROB](1545293) [CM:{ins.CompletetionTime} <=> CY{Cycle}] try retiring at cycle {Cycle}");
                    test = true;
                }

                if (ins.CompletetionTime.HasValue && ins.CompletetionTime < Cycle)
                {

                    //if (ins.MemAddress == 1545293 && (stall > 10000) && (x % 10000 == 0))
                    //    DEBUG.Print($"\t\t\t => HEAD[ROB] = 1545293 RETIRED at cycle {Cycle}");

                    //if (cycle < 1000)
                    //    DEBUG.Print($"\t\t\t => ROB.RETIRED {ins.MemAddress} at cycle {Cycle}");

                    Head = (Head + 1) % Param.ROBSIZE;
                    Inflight--;
                    committed++;
                    num_ret++;
                }
                else
                    break;
            }

            if (test) DEBUG.Print($"\t\t\t => HEAD[ROB] = 1545293  STALLED at cycle {Cycle}");


            stall = (num_ret > 0) ? 0 : stall++;
            return num_ret;
        }

        internal void UpdateInstruction(Request request)
        {
            Instruction ins = Instructions[request.InstructionID];
            DEBUG.Assert(ins.MemAddress==request.BlockAddress);

            if (request.BlockAddress == 1400289)
                DEBUG.Print($"{Cycle}: UpdateInstruction({request.BlockAddress}): @{request.InstructionID} => {ins.MemAddress}");

            if (request.BlockAddress == 43209711458)
                DEBUG.Print($"{Cycle}: UpdateInstruction({request.BlockAddress}): @{request.InstructionID} => {ins.MemAddress}");
            
            if (request.BlockAddress == ins.MemAddress)
                ins.CompletetionTime = Cycle + Param.PIPELINEDEPTH;

            //DEBUG.AssertPrint(request.BlockAddress == ins.MemAddress, $"{Cycle}: [{request.BlockAddress}, {ins.MemAddress}] ROB: Instruction contains different memoory address");
            //ins.CompletetionTime = Cycle + Param.PIPELINEDEPTH;
        }

        public bool ContainsWrite(long block_addr)
        {
            if (Tail == -1)
                return false;

            int i = Head;
            bool write = false;
            while (i != Tail)
            {
                Instruction ins = Instructions[i];
                if (ins.IsWrite && ins.IsMem && ins.MemAddress == block_addr)
                    write = true;
                i = (i + 1) % Param.ROBSIZE;
            }
            return write;
        }

        private TraceRecord TRACE => core.TRACE;
        private Request REQUEST => core.REQUEST;

    }

    //private bool c => Cycle < 1000;
    //void P(string txt)
    //{
    //    if (c)
    //    {
    //        DEBUG.Print($"{txt}");

    //    }
    //}


}

