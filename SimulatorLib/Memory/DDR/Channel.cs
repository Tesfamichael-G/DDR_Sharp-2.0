using SimulatorLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulatorLib.MemCtrl;

namespace SimulatorLib.DDR;

public partial class Channel
{
    public int ID;

    private long cycle;
    public long Cycle
    {
        get => cycle;
        set
        {
            cycle = value;
            for (int i = 0; i < Param.NUM_RANKS; i++)
                Ranks[i].Cycle = value;

            ////////////////////////////////////HTML.Cycle = value;
        }
    }

    public RequestQueue QUEUE => MC.QUEUE;
    public Bus BUS => MC.BUS;

    internal MemoryController MC;

    internal Rank[] Ranks;
    internal Parameters Param;
    public ChannelStat Stat;

    public long NextRD = 0;
    public long NextWR = 0;

    //protected IHtmlLoger HTML;
    //protected Tools HTML_TOOL = Tools.Instance;

    protected Channel()
    {
    }

    public Channel(MemoryController controller)
    {

        MC = controller;
        Param = MC.PARAM;


        Ranks = new Rank[Param.NUM_RANKS];
        for (int i = 0; i < Param.NUM_RANKS; i++)
        {
            Ranks[i] = new Rank(Param, i);
        }

        //HTML_TOOL.SetChannel(MC.SIM.MAX_Cycles, MC.SIM.TRACE_NAME);

        //HTML = HTML_TOOL.channel;

    }

    public void CloseStatFile()
    {
        //HTML.CloseFile();
    }

    public void Intialize()
    {
        Stat = new ChannelStat();
        Stat.Intialize();
        for (int j = 0; j < Param.NUM_RANKS; j++)
        {
            Ranks[j].Initialize();
        }
    }

    private void SetBankCommand(Request REQ)
    {
        var adr = REQ.MemAddr;
        Bank B = Ranks[adr.Rank].Groups[adr.BankGroup].Banks[adr.Bank];
        ReqHistory history = new ReqHistory
        {
            Cycle = cycle,
            Block = REQ.BlockAddress,
            Arrival = REQ.TsArrival,
            Row = REQ.MemAddr.Row,
            NextPre = B.NextPRE,
            NextAct = B.NextACT,
            NextRD = B.NextRD,
            NextWR = B.NextWR
        };

        switch (adr.Bank + 1)
        {
            case 1:
                history.B1 = REQ.CMD.ToString();
                break;
            case 2:
                history.B2 = REQ.CMD.ToString();
                break;
            case 3:
                history.B3 = REQ.CMD.ToString();
                break;
            case 4:
                history.B4 = REQ.CMD.ToString();
                break;
            case 5:
                history.B5 = REQ.CMD.ToString();
                break;
            case 6:
                history.B6 = REQ.CMD.ToString();
                break;
            case 7:
                history.B7 = REQ.CMD.ToString();
                break;
            case 8:
                history.B8 = REQ.CMD.ToString();
                break;

            default:
                break;
        }

        MC.SIM.Histories.Add(history);
    }

    public bool IssueRequest(Request REQ)
    {

        //REQ.TsIssued = Cycle;
        //REQ.REQ_Data.IssuedList[REQ.CMD] = Cycle;
        //REQ.ACT_CYCLES.Add((cycle,REQ.CMD));

        bool issue = REQ.CMD switch
        {
            COMMAND.ACT => ACTIVATE(REQ),
            COMMAND.RD => READ(REQ),
            COMMAND.WR => WRITE(REQ),
            COMMAND.PRE => PRECHARGE(REQ.MemAddr),
            //COMMAND.PDX => POWER_DOWN_EXIT(REQ),
            _ => false
        };

        //SetBankCommand(REQ);

        return issue;

    }

    public virtual void Update() { }

    public void UpdateStat()
    {
        for (int i = 0; i < Param.NUM_RANKS; i++)
            Ranks[i].UpdateStat();
    }

}



