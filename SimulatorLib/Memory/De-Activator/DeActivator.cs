using System;
using System.Linq;
using SimulatorLib.Memory.Refresh;
using SimulatorLib.Common;
using SimulatorLib.DDR;

namespace SimulatorLib.DeActivator;

//public partial class DeActivator
//{
//    public Channel CHANNEL;
//    private Parameters Param => CHANNEL.Param;
//    private long Cycle => CHANNEL.Cycle;
//    private Parameters_DeActivator CONFIG;
//    private Counter COUNTER;
//    public IRefresh REFRESHER;
//    public DeActivator(DEA_Channel channel)
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

//        for (int r = 0; r < Param.NUM_RANKS; r++)
//        {
//            for (int g = 0; g < Param.NUM_BANK_GROUPS; g++)
//            {
//                for (int b = 0; b < Param.NUM_BANKS; b++)
//                {
//                    DEA_Rank rank = (DEA_Rank)CHANNEL.Ranks[r];
//                    DEA_BankGroup group = (DEA_BankGroup)rank.Groups[g];
//                    DEA_Bank BANK = (DEA_Bank)group.Banks[b];

//                    BANK.InitializeDEA(config);

//                    //DeActBank.BankDeactivator deactivator = new DeActBank.BankDeactivator(BANK, config);
//                    //BANK.DE_ACT = deactivator;
//                    //BANK.DE_ACT.Initialize();
//                }
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

//        //Bank BANK = CHANNEL.Ranks[Row.Rank].Banks[Row.Bank][Row.Bank];
//        //Bank BANK = CHANNEL.Ranks[Row.Rank].Groups[Row.BankGroup].Banks[Row.Bank];

//        DEA_Rank rank = (DEA_Rank)CHANNEL.Ranks[Row.Rank];
//        DEA_BankGroup group = (DEA_BankGroup)rank.Groups[Row.BankGroup];
//        DEA_Bank BANK = (DEA_Bank)group.Banks[Row.Bank];

//        var evicted = BANK.AddToRowBuffer(Row);
//        //DEBUG.Print($"{Cycle}: BANK.AddToRowBuffer({Row}) Evict({evicted})");
//        COUNTER.Remove(Row);

//        if (evicted.IsNotNull && ((Cycle - evicted.TimeStamp) < (0.75 * CONFIG.VALIDATION_TIME)))
//        {
//            uint significance = (uint)(1 - (Cycle - evicted.TimeStamp) / CONFIG.VALIDATION_TIME);
//            uint AdjustedActivations = (uint)CONFIG.MAX_ACTIVATIONS_PERMITTED * significance;

//            RowAddress row = evicted.Row;
//            RowInfo info = new RowInfo(row, AdjustedActivations);
//            COUNTER.Add(info);
//            //DEBUG.Print($"{Cycle}: Added Evicted({row}) to counter.");
//        }

//    }

//    public bool NotifyActivation(MemoryAddress mAddr)
//    {

//        var Row = mAddr.RowAddress;

//        var (exists, row_index) = COUNTER.RowExists(Row);

//        if (exists)
//        {
//            if (COUNTER[row_index].Activations > CONFIG.MAX_ACTIVATIONS_PERMITTED)
//            {
//                if (mAddr.BlockAddress == 76713089990)
//                    DEBUG.Print($"{Cycle}: DeActivator.AddHotRowToRowBuffer({mAddr.BlockAddress}, {Row})");
//                AddHotRowToRowBuffer(Row);
//            }
//            else
//            {
//                COUNTER[row_index].Activations++;
//                //DEBUG.Print($"{Cycle}: |({Row}).Activation => {COUNTER[row_index].Activations}");
//                //if (Row.ToString() == "0-3-185128")
//                //    DEBUG.Print($"{Cycle}: Row(0-3-185128).Acstivation => {COUNTER[row_index].Activations}");
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


//}


//internal bool CanReadWrite(Request request, Operation operation)
//{
//    int bank = request.MemAddr.Bank;
//    int rank = request.MemAddr.Rank;
//    int BankG = request.MemAddr.BankGroup;
//    Rank RANK = CHANNEL.Ranks[rank];
//    Bank BANK = RANK.Groups[BankG].Banks[bank];

//    bool YesNo = (operation == Operation.READ) ? CanRead(request) : (operation == Operation.WRITE) ? CanWrite(request) : false;

//    //if (request.MemAddr.BlockAddress == 100886550675)
//    //    DEBUG.Print($"{Cycle}: Channel.Can_{operation}(100886550675) at Bank[{bank}] | State: {BANK.STATE}|{BANK.DE_ACT.state} => {YesNo}");

//    return YesNo;
//}

//internal bool CanRead(Request request)
//{

//    int bank = request.MemAddr.Bank;
//    int rank = request.MemAddr.Rank;
//    int bankG = request.MemAddr.BankGroup;

//    DeActRank RANK = (DeActRank)CHANNEL.Ranks[rank];
//    DeActBank BANK = (DeActBank)RANK.Groups[bankG].Banks[bank];

//    DEBUG.Assert(BANK.DE_ACT.IsHit(request.MemAddr));

//    request.CMD = COMMAND.READ;

//    int GroupID = request.MemAddr.BankGroup;
//    int CCD = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.CCDL : (GroupID == BANK.BankGroupID) ? BANK.Param.CCDL : BANK.Param.CCDS;
//    int WTR = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.WTRL : (GroupID == BANK.BankGroupID) ? BANK.Param.WTRL : BANK.Param.WTRS;
//    long block_addr = request.MemAddr.BlockAddress;
//    long row = request.MemAddr.Row;

//    if (Cycle < BANK.Next_READ)
//    {
//        return false;
//    }

//    return true;

//}

//internal bool CanWrite(Request request)
//{
//    int bank = request.MemAddr.Bank;
//    int rank = request.MemAddr.Rank;
//    int bankG = request.MemAddr.BankGroup;

//    Rank RANK = CHANNEL.Ranks[rank];
//    Bank BANK = RANK.Groups[bankG].Banks[bank];

//    DEBUG.Assert(BANK.DE_ACT.IsHit(request.MemAddr));

//    request.CMD = COMMAND.WRITE;

//    //int GroupID = request.MemAddr.BankGroup;
//    //int CCD = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.CCD : (GroupID == BANK.BankGroupID) ? BANK.Param.CCDL : BANK.Param.CCDS;
//    //int WTR = (BANK.Param.DDR_TYPE == MemoryType.DDR3) ? BANK.Param.WTR : (GroupID == BANK.BankGroupID) ? BANK.Param.WTRL : BANK.Param.WTRS;
//    long block_addr = request.MemAddr.BlockAddress;
//    long row = request.MemAddr.Row;

//    //if (REFRESHER.DisAllows(COMMAND.WRITE))// RANK.CanNotWrite)
//    //{
//    //    //DEBUG.Print($"{Cycle}: RANK.CanNotWrite({BANK.LastAddress}, {BANK.LastCycle}, N'Write:{BANK.Next_WRITE}) DeActivator.CanWrite({block_addr}, {row}) = false ");
//    //    return false;
//    //}

//    if (Cycle < BANK.Next_WRITE)
//    {
//        //DEBUG.Print($"{Cycle}: De'Act.CanWrite({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = False");
//        return false;
//    }
//    //DEBUG.Print($"{Cycle}: De'Act.CanWrite({BANK.LastAddress}, {BANK.LastCycle}, N'Read:{BANK.Next_READ}) DeActivator.CanRead({block_addr}, {row}) = True");
//    return true;

//}


