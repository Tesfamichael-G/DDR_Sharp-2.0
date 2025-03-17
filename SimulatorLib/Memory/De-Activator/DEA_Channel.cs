using SimulatorLib.Common;
using System.Collections.Generic;
using System.Linq;

namespace SimulatorLib.DeActivator;

public class DEA_Channel : DDR.Channel
{

    private Parameters_DeActivator CONFIG;
    long VALIDATION_PERIOD = 0;

    public List<RowInfo> Rows;

    public DEA_Channel(MemCtrl.MemoryController controller)
    {

        MC = controller;
        Param = MC.PARAM;
        CONFIG = MC.SIM.Param_DeACT;

        Rows = new List<RowInfo>(CONFIG.MAX_COUNTER_SIZE);

        VALIDATION_PERIOD = (int)(CONFIG.VALIDATION_TIME * 1000000 * Param.ClockFactor / Param.TCK);
        DisableReset = (CONFIG.VALIDATION_TIME == 0);

        Ranks = new DEA_Rank[Param.NUM_RANKS];
        for (int i = 0; i < Param.NUM_RANKS; i++)
        {
            Ranks[i] = new DEA_Rank(Param, i, CONFIG);
        }

        //HTML_TOOL.SetChannel(MC.SIM.MAX_Cycles, MC.SIM.TRACE_NAME);
        //HTML = HTML_TOOL.channel;


    }
    public new void CloseStatFile()
    {
        //HTML.CloseFile();
    }

    private long TIME;
    private bool DisableReset = false;
    public override void Update()
    {
        if (DisableReset)
            return;

        TIME += Param.ClockFactor;

        if (TIME < VALIDATION_PERIOD)
            return;

        TIME = 0;

        Reset();

    }

    public override bool ACTIVATE(Request REQ)
    {
        Ranks[REQ.MemAddr.Rank].ACTIVATE(REQ.MemAddr, REQ.TYPE);
        REQ.NUM_ACTS++;

        Stat.ACTS++;
        if (REQ.TYPE == Operation.READ)
        {
            Stat.ACTS_RD++;
        }
        else
        {
            Stat.ACTS_WR++;
        }

        //HTML.WriteLine($"<p> - Cycle: {Cycle}| ACT({REQ.BlockAddress}], [{REQ.MemAddr.ToString()}]) ACTS: {Stat.ACTS} ({Stat.ACTS_RD},{Stat.ACTS_WR})</p>");

        UpdateCounter(REQ.MemAddr); //COUNTER.Update(request.MemAddr);
        return true;
    }


    //************************* COUNTER


    public (bool, int) RowExists(MemoryAddress addr)
    {
        List<int> indexes = new List<int>();
        bool exists = false;
        int iCnt = 0;
        int row_index = -1;

        for (int i = 0; i < Rows.Count; i++)
        {
            if ((Rows[i].Rank, Rows[i].BankGroup, Rows[i].Bank, Rows[i].Row) == (addr.Rank, addr.BankGroup, addr.Bank, addr.Row))
            {
                exists = true;
                row_index = i;
                indexes.Add(i);
                iCnt++;
            }
        }
        DEBUG.Assert(iCnt <= 1);
        return (exists, row_index);

    }

    public List<int> Locations(MemoryAddress addr)
    {
        var indexes = new List<int>();

        for (int i = 0; i < Rows.Count; i++)
        {
            if ((Rows[i].Rank, Rows[i].BankGroup, Rows[i].Bank, Rows[i].Row) == (addr.Rank, addr.BankGroup, addr.Bank, addr.Row))
                indexes.Add(i);
        }
        return indexes;

    }

    public void Add(RowInfo info)
    {
        if (Rows.Count >= CONFIG.MAX_COUNTER_SIZE)
            RemoveLeatActivatedRow();

        Rows.Add(info);

        //if (info.Row.ToString() == "0-3-185128")
        //{
        //    string list = Locations(info.Row).Info();
        //    //DEBUG.Print($"\t\t Row(0-3-185128) added at indexs ... {list}");
        //}

    }

    private void RemoveLeatActivatedRow()
    {
        var oldItems = Rows.GetRange(0, CONFIG.MAX_COUNTER_SIZE / 2);
        uint minActivation = oldItems.Min(r => r.Activations);
        var leastActivated = Rows.Where(r => r.Activations == minActivation).ToList();

        if (leastActivated.Count() > 0)
        {
            Remove(leastActivated[0]);
        }
    }

    public void Remove(RowInfo addr)
    {
        Rows.RemoveAll(r => (r.Rank, r.BankGroup, r.Bank, r.Row) == (addr.Rank, addr.BankGroup, addr.Bank, addr.Row));
    }
    public void Remove(MemoryAddress addr)
    {
        Rows.RemoveAll(r => (r.Rank, r.BankGroup, r.Bank, r.Row) == (addr.Rank, addr.BankGroup, addr.Bank, addr.Row));
    }

    public void Reset()
    {
        Rows.Clear();

        //uint minActivation = Rows.Min(r => r.Activations);
        //var leastActivated = Rows.Where(r => r.Activations == minActivation).ToList();

        //if (leastActivated.Count() > 0)
        //{
        //    Rows.RemoveAll(r => r.Activations == minActivation);
        //}

    }

    public void UpdateCounter(MemoryAddress mAddr)
    {

        var (exists, row_index) = RowExists(mAddr);

        if (!exists)
        {
            Add(new RowInfo(mAddr));
            return;
        }

        if (Rows[row_index].Activations <= CONFIG.MAX_ACTIVATIONS_PERMITTED)
        {
            Rows[row_index].Activations++;
            return;
        }

        AddHotRowToRowBuffer(mAddr);

    }

    private void AddHotRowToRowBuffer(MemoryAddress addr)
    {

        DEA_Rank rank = (DEA_Rank)Ranks[addr.Rank];
        DEA_BankGroup group = (DEA_BankGroup)rank.Groups[addr.BankGroup];
        DEA_Bank BANK = (DEA_Bank)group.Banks[addr.Bank];

        RowBufferData? EvictedRow;
        bool OK = BANK.TryAddToRowBuffer(addr.Row, out EvictedRow);  //DEBUG.Print($"{Cycle}: BANK.AddToRowBuffer({Row}) Evict({evicted})");

        if (OK)
        {
            Remove(addr);

            if (EvictedRow == null)
                return;

            if (((Cycle - EvictedRow.TimeStamp) < (0.75 * CONFIG.VALIDATION_TIME)))
            {
                uint significance = (uint)(1 - (Cycle - EvictedRow.TimeStamp) / CONFIG.VALIDATION_TIME);
                uint AdjustedActivations = (uint)CONFIG.MAX_ACTIVATIONS_PERMITTED * significance;

                int row = EvictedRow.Row;
                RowInfo info = new RowInfo(MemoryAddress.CreateRowAddress(BANK.RankID, BANK.GroupID, BANK.ID, row), AdjustedActivations);
                Add(info);
            }

        }
    }




}

