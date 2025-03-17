using SimulatorLib;
using SimulatorLib.Common;
using SimulatorLib.MemCtrl;
using SimulatorLib.DRAM;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.Memory.Refresh
{
    public class AutoRefresh : IRefresh
    {
        private MemoryController MC;
        Parameters t;

        public bool IsBusy { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }

        private int EightREFI;
        private int EightRFC_RP;
        public AutoRefresh(MemoryController MemCtrl)
        {

            //RefreshDeadline = (int)NextRefresh_CompletionDeadline - Param.RP - 8 * Param.RFC;
            //RefresheCount = 0;

            MC = MemCtrl;
            t = MemCtrl.PARAM;

            EightREFI = 8 * t.REFI ;
            EightRFC_RP = 8 * t.RFC + t.RP;

            IsBusy = false;

            EndTime   = EightREFI ;
            StartTime = EndTime - EightRFC_RP;
        }

        public void Tick()
        {
            
            if (MC.Cycle == StartTime) 
            {
                IsBusy = true;
                InjectRefresh();
                return;
            }

            if (MC.Cycle == EndTime) 
            {
                EndTime = MC.Cycle + EightREFI;
                StartTime = EndTime - EightRFC_RP;
                IsBusy = false;
            }

        }


        //public void InjectRefresh() => MC.CHANNEL.FORCE_REFRESH(EndTime);
        public void InjectRefresh() => MC.CHANNEL.FORCE_REFRESH(EightREFI);

        bool DisAllows(COMMAND cmd, bool fast_powerup) => cmd switch
        {
            COMMAND.ACT => (MC.Cycle + t.RAS) > StartTime,
            COMMAND.RD => (MC.Cycle + t.RTP) > StartTime,
            COMMAND.PRE => (MC.Cycle + t.RP) > StartTime,
            COMMAND.WR => (MC.Cycle + t.CWL + t.BL + t.WR) > StartTime,

            COMMAND.RDA => (MC.Cycle + t.RTP+ t.RP) > StartTime,
            COMMAND.WRA => (MC.Cycle + t.CWL + t.BL + t.WR + t.RP) > StartTime,
            

            COMMAND.PDE => (MC.Cycle + t.XP_DLL + t.PD) > StartTime,
            //COMMAND.PDNF => (MC.Cycle + t.XP + t.PD) > StartTime,

            COMMAND.PDX => (fast_powerup) ? (MC.Cycle + t.XP) > StartTime : (MC.Cycle + t.XP_DLL) > StartTime,
            _ => false,
        };



    }

}
