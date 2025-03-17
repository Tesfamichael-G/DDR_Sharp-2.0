using SimulatorLib;
using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using SimulatorLib.DRAM;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.Memory.Refresh;

public class RankRefresh : IRefresh
{
    private MemoryController MC;
    private Parameters t;
    private int Time;
    public int[] BankCounters;


    public RankRefresh(MemoryController MemCtrl)
    {

        MC = MemCtrl;
        t = MemCtrl.PARAM;
        BankCounters = new int[t.NUM_RANKS];
        Time = 0;
    }


    public void Tick()
    {
        Time += t.ClockFactor;

        if (Time < t.REFI)
            return;

        InjectRefresh();

        Time = 0;
    }

    public void InjectRefresh()
    {
        //MC.CHANNEL.InjectRefresh();

        for (int r = 0; r < MC.PARAM.NUM_RANKS; r++)
        {
            MC.QUEUE.REFS[r].NEEDS_REFRESH = true; // RefreshRank(r);
        }
        MC.CHANNEL.refresh_waiting = true;
        MC.CHANNEL.Ranks_to_Refresh = MC.PARAM.NUM_RANKS;
    }


}
