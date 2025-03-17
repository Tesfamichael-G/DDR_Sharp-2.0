using SimulatorLib.Common;
using System.Collections.Generic;
using System.Linq;


namespace SimulatorLib.DeActivator
{

    //public class Counter
    //{

    //    //public List<RowInfo> Rows;

    //    //private DEA_Channel CHANNEL;
    //    //private Parameters_DeActivator CONFIG;

    //    //int Max_Size;
    //    //int Max_Activations;
    //    //private Parameters_DeActivator CONFIG;

    //    //public Counter(DEA_Channel channel)//,   Parameters_DeActivator config)
    //    //{
    //    //    //CONFIG = config;
    //    //    CHANNEL = channel;
    //    //    //Max_Size = config.MAX_COUNTER_SIZE;
    //    //    //Max_Activations = config.MAX_ACTIVATIONS_PERMITTED;
    //    //    //Rows = new List<RowInfo>(Max_Size);
    //    //}

    //    //public Counter(DEA_Channel channel, Parameters_DeActivator config) : this(channel)
    //    //{
    //    //    CHANNEL = channel;
    //    //    CONFIG = config;

    //    //    Max_Size = config.MAX_COUNTER_SIZE;
    //    //    Max_Activations = config.MAX_ACTIVATIONS_PERMITTED;
    //    //    Rows = new List<RowInfo>(Max_Size);
    //    //}

    //    //public (bool, int) RowExists(RowAddress Row)
    //    //{
    //    //    List<int> indexes = new List<int>();
    //    //    bool exists = false;
    //    //    int iCnt = 0;
    //    //    int row_index = -1;

    //    //    for (int i = 0; i < Rows.Count; i++)
    //    //    {
    //    //        if (Rows[i].Row == Row)
    //    //        {
    //    //            exists = true;
    //    //            row_index = i;
    //    //            indexes.Add(i);
    //    //            iCnt++;
    //    //        }
    //    //    }
    //    //    if (iCnt > 1)
    //    //    {
    //    //        DEBUG.Print($"Counter: {iCnt} Entries for row {Row} at {indexes.Info()}");
    //    //    }
    //    //    return (exists, row_index);

    //    //}

    //    //public List<int> Locations(RowAddress Row)
    //    //{
    //    //    var indexes = new List<int>();

    //    //    for (int i = 0; i < Rows.Count; i++)
    //    //    {
    //    //        //DEBUG.Print($"\t\t if (Rows[{i}].Row: {Rows[i].Row} == Row: {Row}) => {(Rows[i].Row == Row)}");

    //    //        if (Rows[i].Row == Row)
    //    //            indexes.Add(i);
    //    //    }
    //    //    return indexes;

    //    //}
 
    //    //public void Add(RowInfo info)
    //    //{
    //    //    if (Rows.Count >= Max_Size)
    //    //        RemoveLeatActivatedRows();

    //    //    Rows.Add(info);

    //    //    if (info.Row.ToString() == "0-3-185128")
    //    //    {
    //    //        string list = Locations(info.Row).Info();
    //    //        //DEBUG.Print($"\t\t Row(0-3-185128) added at indexs ... {list}");
    //    //    }

    //    //}

    //    //private void RemoveLeatActivatedRows()
    //    //{
    //    //    var oldItems = Rows.GetRange(0, Max_Size / 2);
    //    //    uint minActivation = oldItems.Min(r => r.Activations);
    //    //    var leastActivated = Rows.Where(r => r.Activations == minActivation).ToList();

    //    //    if (leastActivated.Count() > 0)
    //    //    {
    //    //        Remove(leastActivated[0].Row);
    //    //    }
    //    //}

    //    //public void Remove(RowAddress row)
    //    //{
    //    //    Rows.RemoveAll(r => r.Row == row);
    //    //    //DEBUG.Print($"\tRemoveed({row})");
    //    //}
 
    //    //public void Reset()
    //    //{
    //    //    Rows.Clear();
    //    //}


    //    //public void Update(MemoryAddress mAddr)
    //    //{

    //    //    var Row = mAddr.RowAddress;

    //    //    var (exists, row_index) = RowExists(Row);

    //    //    if (!exists)
    //    //    {
    //    //        //DEBUG.Print($"{CHANNEL.Cycle}: COUNTER.Add.New({mAddr.BlockAddress}, {Row})");
    //    //        Add(new RowInfo(Row));
    //    //        return;
    //    //    }

    //    //    if (Rows[row_index].Activations < CONFIG.MAX_ACTIVATIONS_PERMITTED)
    //    //    {
    //    //        Rows[row_index].Activations++;
    //    //        //DEBUG.Print($"{CHANNEL.Cycle}: COUNTER[({mAddr.BlockAddress}, {Row}++)] => {Rows[row_index].Activations}/{CONFIG.MAX_ACTIVATIONS_PERMITTED}");
    //    //        return;
    //    //    }

    //    //    //DEBUG.Print($"{CHANNEL.Cycle}: COUNTER[({mAddr.BlockAddress}, {Row})] is HOT.");
    //    //    AddHotRowToRowBuffer(Row);

    //    //}

    //    //private void AddHotRowToRowBuffer(RowAddress Row)
    //    //{

    //    //    DEA_Rank rank = (DEA_Rank)CHANNEL.Ranks[Row.Rank];
    //    //    DEA_BankGroup group = (DEA_BankGroup)rank.Groups[Row.BankGroup];
    //    //    DEA_Bank BANK = (DEA_Bank)group.Banks[Row.Bank];

    //    //    var evicted = BANK.AddToRowBuffer(Row);//DEBUG.Print($"{Cycle}: BANK.AddToRowBuffer({Row}) Evict({evicted})");

    //    //    Remove(Row);

    //    //    if (evicted.IsNotNull && ((CHANNEL.Cycle - evicted.TimeStamp) < (0.75 * CONFIG.VALIDATION_TIME)))
    //    //    {
    //    //        uint significance = (uint)(1 - (CHANNEL.Cycle - evicted.TimeStamp) / CONFIG.VALIDATION_TIME);
    //    //        uint AdjustedActivations = (uint)CONFIG.MAX_ACTIVATIONS_PERMITTED * significance;

    //    //        RowAddress row = evicted.Row;
    //    //        RowInfo info = new RowInfo(row, AdjustedActivations);
    //    //        Add(info);
    //    //        //DEBUG.Print($"{Cycle}: Added Evicted({row}) to counter.");
    //    //    }

    //    //}


    //    ////************************************

    //    //public List<RowInfo> HOTRows => Rows.Where(info => info.Activations >= Max_Activations).ToList();

    //    //public RowInfo this[int index]
    //    //{
    //    //    get { return Rows[index]; }
    //    //    set { Rows[index] = value; }
    //    //}
      
    //    //public bool IsHot(RowAddress Row)
    //    //{

    //    //    var (exists, row_index) = RowExists(Row);

    //    //    if (exists)
    //    //    {
    //    //        if (Rows[row_index].Activations > CONFIG.MAX_ACTIVATIONS_PERMITTED)
    //    //            return true;
    //    //    }

    //    //    return false;
    //    //}

    //    //private bool HasReachedMinimumActivation(RowInfo info) => (info.Activations >= Max_Activations) ? true : false;


    //}

}





