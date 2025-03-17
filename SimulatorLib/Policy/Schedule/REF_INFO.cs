using SimulatorLib.Common;

namespace SimulatorLib.Memory.Schedule;

public struct REF_INFO
{
    public COMMAND CMD;
    public int RANK;
    public int GROUP;
    public int BANK;

    public bool NEEDS_REFRESH;
    public bool BANK_IS_OPEN;
    public bool CAN_ISSUE;
    public REF_INFO()
    {
        CAN_ISSUE = false;
        NEEDS_REFRESH = false;
        BANK_IS_OPEN = false;
        CMD = COMMAND.NOP;
        RANK = GROUP = BANK = -1;
    }

    public static REF_INFO NULL
    {
        get
        {
            REF_INFO info = new REF_INFO();
            DEBUG.Print($"\t - NULL => [{info.RANK}{info.GROUP}{info.BANK}] \n" +
                        $"\t\tNeedsREF:[{info.NEEDS_REFRESH}] \n" +
                        $"\t\tIsOpen: [{info.BANK_IS_OPEN}]\n" +
                        $"\t\tCanIssue: [{info.CAN_ISSUE}]");
            return info;
        }
    }

}

