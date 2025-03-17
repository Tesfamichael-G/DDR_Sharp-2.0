//using System;
//using System.Linq;
//using SimulatorLib.Memory.Refresh;
//using SimulatorLib.Common;
//using SimulatorLib.DDR;

//namespace SimulatorLib.DE_ACT;

//public partial class DeActivator_0
//{

//    public Channel CHANNEL;
//    private Parameters Param => CHANNEL.Param;
//    private int BANK_MAX;
//    private long Cycle => CHANNEL.Cycle;
//    private Parameters_DeActivator CONFIG;
//    private Counter COUNTER;
//    public IRefresh REFRESHER;
//    public DeActivator_0(Channel channel)
//    {
//        CHANNEL = channel;
//        COUNTER = new Counter(channel);
//        REFRESHER = channel.MC.REFRESHER;
//    }

//    long VALIDATION_PERIOD = 0;
//    public void Initialize(Parameters_DeActivator config)
//    {
//        CONFIG = config;
//        COUNTER.Initialize(CONFIG);

//        BANK_MAX = (Param.DDR_TYPE == MemoryType.DDR4) ? Param.NUM_BANKS * Param.NUM_BANK_GROUPS : Param.NUM_BANKS;

//        for (int r = 0; r < Param.NUM_RANKS; r++)
//        {
//            for (int i = 0; i < BANK_MAX; i++)
//            {
//                Bank BANK = CHANNEL.Ranks[r].Banks[i];
//                Bank.BankDeactivator deactivator = new Bank.BankDeactivator(BANK, config);
//                BANK.DE_ACT = deactivator;
//                BANK.DE_ACT.Initialize();
//                CHANNEL.Ranks[r].Banks[i].InitializeDeActivator();
//            }
//        }

//        VALIDATION_PERIOD = (int)(config.VALIDATION_TIME * 1000000 * Param.ClockFactor / Param.TCK);
//    }

//    public void Tick()
//    {
//        if (Cycle % VALIDATION_PERIOD == 0)
//            COUNTER.Reset();
//    }

//    private void AddHotRowToRowBuffer(RowAddress Row)
//    {

//        Bank BANK = CHANNEL.Ranks[Row.Rank].Banks[Row.Bank];

//        var evicted = BANK.DE_ACT.AddToRowBuffer(Row);
//        DEBUG.Print($"{Cycle}: BANK.AddToRowBuffer({Row}) Evict({evicted})");
//        DEBUG.Print($"{Cycle}: BANK.AddToRowBuffer({Row}) Evict({evicted})");
//        COUNTER.Remove(Row);

//        if (evicted.IsNotNull && ((Cycle - evicted.TimeStamp) < (0.75 * CONFIG.VALIDATION_TIME)))
//        {
//            uint significance = (uint)(1 - (Cycle - evicted.TimeStamp) / CONFIG.VALIDATION_TIME);
//            uint AdjustedActivations = (uint)CONFIG.MAX_ACTIVATIONS_PERMITTED * significance;

//            RowAddress row = evicted.Row;
//            RowInfo info = new RowInfo(row, AdjustedActivations);
//            COUNTER.Add(info);
//            DEBUG.Print($"{Cycle}: Added Evicted({row}) to counter.");
//        }

//    }

//    public bool Update(Request REQ)
//    {

//        var Row = REQ.MemAddr.RowAddress;
//        Bank BANK = CHANNEL.Ranks[Row.Rank].Banks[Row.Bank];
//        if (BANK.IsHit(Row))
//        {
//            BANK.Update(Row);
//        }
//        var (exists, row_index) = COUNTER.RowExists(Row);

//        if (exists)
//        {
//            if (COUNTER[row_index].Activations > CONFIG.MAX_ACTIVATIONS_PERMITTED)
//            {
//                if (REQ.BlockAddress == 76713089990)
//                    DEBUG.Print($"{Cycle}: DeActivator.AddHotRowToRowBuffer({REQ.BlockAddress}, {Row})");
//                AddHotRowToRowBuffer(Row);
//            }
//            else
//            {
//                COUNTER[row_index].Activations++;
//                DEBUG.Print($"{Cycle}: |({Row}).Activation => {COUNTER[row_index].Activations}");
//                if (Row.ToString() == "0-3-185128")
//                    DEBUG.Print($"{Cycle}: Row(0-3-185128).Acstivation => {COUNTER[row_index].Activations}");
//            }
//        }
//        else
//            COUNTER.Add(new RowInfo(Row));

//        return true;
//    }

//    public bool IsHot(RowAddress Row)
//    {

//        var (exists, row_index) = COUNTER.RowExists(Row);

//        if (exists)
//        {
//            if (COUNTER[row_index].Activations > CONFIG.MAX_ACTIVATIONS_PERMITTED)
//                return true;
//        }

//        return false;
//    }

//    internal bool CanReadWrite(Request request, Operation operation)
//    {
//        int bank = request.MemAddr.Bank;
//        int rank = request.MemAddr.Rank;

//        Rank RANK = CHANNEL.Ranks[rank];
//        Bank BANK = RANK.Banks[bank];

//        bool YesNo = (operation == Operation.READ) ? CanRead(request) : (operation == Operation.WRITE) ? CanWrite(request) : false;

//        if (request.MemAddr.BlockAddress == 100886550675)
//            DEBUG.Print($"{Cycle}: Channel.Can_{operation}(100886550675) at Bank[{bank}] | State: {BANK.STATE}|{BANK.DE_ACT.state} => {YesNo}");

//        return YesNo;
//    }

//    internal bool CanRead(Request request)
//    {

//        int bank = request.MemAddr.Bank;
//        int rank = request.MemAddr.Rank;

//        Rank RANK = CHANNEL.Ranks[rank];
//        Bank BANK = RANK.Banks[bank];

//        DEBUG.Assert(BANK.DE_ACT.IsHit(request.MemAddr));

//        request.CMD = COMMAND.READ;

//        int GroupID = request.MemAddr.BankGroup;
//        int CCD = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.CCDL : (GroupID == BANK.BankGroupID) ? BANK.Param.CCDL : BANK.Param.CCDS;
//        int WTR = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.WTRL : (GroupID == BANK.BankGroupID) ? BANK.Param.WTRL : BANK.Param.WTRS;
//        long block_addr = request.MemAddr.BlockAddress;
//        long row = request.MemAddr.Row;

//        if (REFRESHER.DisAllows(COMMAND.READ))//RANK.CanNotRead)
//        {
//            //DEBUG.Print($"{Cycle}: RANK.CanNotRead({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = False");
//            return false;
//        }
//        if (Cycle < BANK.Next_READ)
//        {
//            DEBUG.Print($"{Cycle}: De'Act.CanRead({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = False");
//            return false;
//        }
//        DEBUG.Print($"{Cycle}: De'Act.CantRead({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = True");
//        return true;

//        switch (BANK.PreviousCommand)
//        {
//            case COMMAND.COL_READ:
//                {
//                    if ((Cycle - BANK.LastRead) > Math.Max(CCD, BANK.Param.BL))
//                        DEBUG.Print($"{Cycle}: P'Read({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = true ");
//                    else
//                        DEBUG.Print($"{Cycle}: P'Read({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = false ");

//                    return (Cycle - BANK.LastRead) > Math.Max(CCD, BANK.Param.BL);
//                }

//            case COMMAND.COL_WRITE:
//                {
//                    if ((Cycle - BANK.LastRead) > (BANK.Param.CWD + BANK.Param.BL + WTR))
//                        DEBUG.Print($"{Cycle}: P'Write({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = true ");
//                    else
//                        DEBUG.Print($"{Cycle}: P'Write({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = false ");

//                    return (Cycle - BANK.LastRead) > (BANK.Param.CWD + BANK.Param.BL + WTR);
//                }

//            case COMMAND.PRE:

//                DEBUG.Print($"{Cycle}: DeActivator.CanRead[{BANK.Next_READ}]({block_addr})|[{row}] => True [PRE():{BANK.LastCycle}] ");
//                return true;

//            case COMMAND.ACT:
//                {
//                    if ((Cycle - BANK.LastCycle) > BANK.Param.RCD)
//                        DEBUG.Print($"{Cycle}: P'Act({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = true ");
//                    else
//                        DEBUG.Print($"{Cycle}: P'Act({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = false ");

//                    return (Cycle - BANK.LastCycle) > (BANK.Param.RCD);

//                }

//            case COMMAND.REF:
//                DEBUG.Print($"{Cycle}: DeActivator.CanRead[{BANK.Next_READ}]({block_addr})|[{row}] = True [REF():{BANK.LastCycle}]");
//                return true;
//            case COMMAND.PWR_DN_SLOW:
//            case COMMAND.PWR_DN_FAST:
//            case COMMAND.PWR_UP:
//            case COMMAND.NOP:
//                DEBUG.Print($"{Cycle}: DeActivator.CanRead[{BANK.Next_READ}]({block_addr})|[{row}] = true [PDN, NOP():{BANK.LastCycle}]");
//                return false;
//            default:
//                return false; ;
//        }
//    }

//    internal bool CanWrite(Request request)
//    {
//        int bank = request.MemAddr.Bank;
//        int rank = request.MemAddr.Rank;

//        Rank RANK = CHANNEL.Ranks[rank];
//        Bank BANK = RANK.Banks[bank];

//        DEBUG.Assert(BANK.DE_ACT.IsHit(request.MemAddr));

//        request.CMD = COMMAND.WRITE;

//        int GroupID = request.MemAddr.BankGroup;
//        int CCD = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.CCD : (GroupID == BANK.BankGroupID) ? BANK.Param.CCDL : BANK.Param.CCDS;
//        int WTR = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.WTR : (GroupID == BANK.BankGroupID) ? BANK.Param.WTRL : BANK.Param.WTRS;
//        long block_addr = request.MemAddr.BlockAddress;
//        long row = request.MemAddr.Row;

//        if (REFRESHER.DisAllows(COMMAND.WRITE))// RANK.CanNotWrite)
//        {
//            //DEBUG.Print($"{Cycle}: RANK.CanNotWrite({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = false ");
//            return false;
//        }

//        if (Cycle < BANK.Next_WRITE)
//        {
//            DEBUG.Print($"{Cycle}: De'Act.CanWrite({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = False");
//            return false;
//        }
//        DEBUG.Print($"{Cycle}: De'Act.CanWrite({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = True");
//        return true;



//        switch (BANK.PreviousCommand)
//        {
//            case COMMAND.COL_READ:
//                {
//                    if ((Cycle - BANK.LastRead) > (BANK.Param.CAS + BANK.Param.BL + BANK.Param.RTRS - BANK.Param.CWD))
//                        DEBUG.Print($"{Cycle}: P'Read({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = true ");
//                    else
//                        DEBUG.Print($"{Cycle}: P'Read({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = false ");

//                    return (Cycle - BANK.LastRead) > (BANK.Param.CAS + BANK.Param.BL + BANK.Param.RTRS - BANK.Param.CWD);
//                }

//            case COMMAND.COL_WRITE:
//                {
//                    if ((Cycle - BANK.LastWrite) > Math.Max(CCD, BANK.Param.BL))
//                        DEBUG.Print($"{Cycle}: P'Write({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = true ");
//                    else
//                        DEBUG.Print($"{Cycle}: P'Write({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = false ");

//                    return (Cycle - BANK.LastWrite) > Math.Max(CCD, BANK.Param.BL);
//                }

//            case COMMAND.PRE:

//                DEBUG.Print($"{Cycle}: DeActivator.CanWrite[{BANK.Next_WRITE}]({block_addr})|[{row}] = True [PRE():{BANK.LastCycle}] ");
//                return true;

//            case COMMAND.ACT:
//                {
//                    if ((Cycle - BANK.LastCycle) > BANK.Param.RCD)
//                        DEBUG.Print($"{Cycle}: P'Act({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = true ");
//                    else
//                        DEBUG.Print($"{Cycle}: P'Act({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = false ");

//                    return (Cycle - BANK.LastCycle) > (BANK.Param.RCD);

//                }

//            case COMMAND.REF:
//                DEBUG.Print($"{Cycle}: DeActivator.CanWrite[{BANK.Next_WRITE}]({block_addr})|[{row}] = True [REF():{BANK.LastCycle}]");
//                return true;
//            case COMMAND.PWR_DN_SLOW:
//            case COMMAND.PWR_DN_FAST:
//            case COMMAND.PWR_UP:
//            case COMMAND.NOP:
//                DEBUG.Print($"{Cycle}: DeActivator.CanWrite[{BANK.Next_WRITE}]({block_addr})|[{row}] = true [PDN, NOP():{BANK.LastCycle}]");
//                return false;
//            default:
//                return false; ;

//        }

//    }


//}


