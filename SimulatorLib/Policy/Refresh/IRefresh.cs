using SimulatorLib.DRAM;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.Memory.Refresh
{

    public enum RefreshStatus
    {
        Idle,
        Ready,
        Busy
    }
    public interface IRefresh
    {

        public void Tick();

        //public bool DisAllows(Common.COMMAND cmd, bool fast_powerup = true);

        //public bool IsBusy { get; set; }

        //public long StartTime { get; set; }
        //public long EndTime { get; set; }

        //public bool ConflictsWithPrecharge { get; }  

        //public bool ConflictsWithRead { get; }
        //public bool ConflictsWithWrite { get; }

        //public bool ConflictsWithActivate { get; }

        //public bool ConflictsWithFastPowerUp { get; }
        //public bool ConflictsWithFastPowerDown { get; }

        //public bool ConflictsWithSlowPowerUp { get; }
        //public bool ConflictsWithSlowPowerDown { get; }



    }


}
