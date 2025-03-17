using System;
using System.Collections.Generic;
using SimulatorLib.Common;
using SimulatorLib.CPU;

namespace SimulatorLib.DDR;

public partial class Rank
{
    #region CAS

    protected int SIBLING_RD_RD ; 
    protected int SIBLING_RD_WR ; 
    protected int SIBLING_RDA_RD; 
    protected int SIBLING_RDA_WR; 
    protected int SIBLING_WR_RD;  

    public void SIBLING_READ(Request request)
    {
        NextRD = Math.Max(cycle + SIBLING_RD_RD, NextRD);
        NextWR = Math.Max(cycle + SIBLING_RD_WR, NextWR);

        Stat.NUM_READS++;
    }

    public void SIBLING_READ_AP(Request request)
    {
        NextRD = Math.Max(cycle +  SIBLING_RDA_RD, NextRD);
        NextWR = Math.Max(cycle + SIBLING_RDA_WR, NextWR);
    }

    public void SIBLING_WRITE(Request request)
    {
        NextRD = Math.Max(cycle + SIBLING_WR_RD, NextRD);
    }

    public void SIBLING_WRITE_AP(Request request)
    {
        NextRD  = Math.Max(cycle + SIBLING_WR_RD, NextRD);
    }


    #endregion

}


