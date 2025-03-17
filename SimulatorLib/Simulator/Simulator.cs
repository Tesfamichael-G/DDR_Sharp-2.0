using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimulatorLib.Common;
using SimulatorLib.CPU;

namespace SimulatorLib;

public partial class Simulator
{
    //StringBuilder sb = new StringBuilder();

    public string ID;
    public Random random = new Random(0);

    public long Cycle = 0;
    public long MAX_Cycles = 10000;

    public bool DEACTIVATOR_ENABLED = false;

    public Parameters Param;
    public Parameters_DeActivator Param_DeACT;

    public Core[] Cores;


    public MemorySystem MemSystem;

    public ITraceReader TraceReader;

    public TraceRecord Record;

    public IAddressTranslator Translator;
     
    public IPageConverter PageConverter;

    public string Workload;
    public string TRACE_NAME;

    public Stat STAT;

    public Xbar XBAR;
    public bool openpolicy;
    public bool TRACE_EOF = false;

    public Simulator() { }

    public Simulator(Parameters param)
    {
        Param = param;
    }

    //public StringBuilder sb;
    private void Initialize()
    {
        //ThreadPool.SetMinThreads(4, 4);
        //************************ sb = new StringBuilder();

        STAT = new Stat(this);

        //CacheReplacementPolicy = (new LRU() as IReplacementPolicy);
        Translator = new MemoryMap(Param);
        //Translator = new RamulatorMemoryMap(Param);

        const long page_size = 4 * 1024;

        PageConverter = Param.MAPPING_METHOD switch
        {
            PageMappingMethod.CONTIGIOUS => new ContiguousAllocator(page_size, Param.NUM_ROWS),
            PageMappingMethod.RANDOM => new PageRandomizer(this, page_size),
            _ => new PageRandomizer(this, page_size)
        };

        //if (Param.CACHE_ENABLED) 
        //    Caches = new CacheHierarchy(Param.NUMCORES, this);

        Cores = new Core[Param.NUMCORES];
        XBAR = new Xbar(this);

        for (int Core_ID = 0; Core_ID < Param.NUMCORES; Core_ID++)
        {
            Core core = new Core(Core_ID, this);
            
            //if (Param.CACHE_ENABLED) core = new Core(Core_ID, this, Caches);
            
            core.ID = Core_ID;
            Cores[Core_ID] = core;
        }

        MemSystem = new MemorySystem(this);

        MemSystem.Initialize();
        Histories = new List<ReqHistory>();

        //TraceReader.NextTraceRecord(0);

    }


    public bool IsCancelled = false;
    public bool TraceEnding = false;
    public bool ROBStalled = false;

    public static void SetPoolSize()
    {
        int minWorker, minIOC;
        // Get the current settings.
        ThreadPool.GetMinThreads(out minWorker, out minIOC);
        // Change the minimum number of worker threads to four, but
        // keep the old setting for minimum asynchronous I/O 
        // completion threads.
        if (ThreadPool.SetMinThreads(4, minIOC))
        {
            // The minimum number of threads was set successfully.
        }
        else
        {
            // The minimum number of threads was not changed.
        }
    }

    IProgress<long> progress;

    void printInfo()
    {
        DEBUG.Print($"********************************** DEACT.ENABLED: {DEACTIVATOR_ENABLED} **********************************");
        DEBUG.Print($"********** CHANNEL => {MemSystem.MemControllers[0].CHANNEL.GetType().FullName}.");
        DEBUG.Print($"********** SCHED'R => {MemSystem.MemControllers[0].SCHEDULER.GetType().FullName}.");
        if (DEACTIVATOR_ENABLED)
        {
            string s = $"{Param_DeACT.MAX_COUNTER_SIZE}  x " +
                       $"{Param_DeACT.MAX_ROW_BUFFERS_PER_BANK} x " +
                       $"{Param_DeACT.MAX_ACTIVATIONS_PERMITTED} x " +
                       $"{Param_DeACT.VALIDATION_TIME})";

            DEBUG.Print($"********** {s} **********");
        }

    }
   
    public async Task<bool> RunAllAsync(CancellationToken token, Action<long> handler) =>
        await Task.Run(() =>
            {

                if (token.IsCancellationRequested) return false;

                progress = new Progress<long>(handler);

                Initialize();
                printInfo();

                if (Param.INCLUDE_XBAR_LATENCY)
                    Param.PIPELINEDEPTH = 0;

                //DEBUG.Print($"B1. Sim.Run  => Cycle => {Cycle}");
                //for (int i = 0; i < Param.NUMCORES; i++)
                //{
                //    Cores[i].FetchRequest();
                //}
                  //DEBUG.Print($"B2. Cores[i].FetchRequest() => Cycle => {Cycle}");
                while (true)
                {

                    if (token.IsCancellationRequested) return false;

                    MemSystem.Cycle = Cycle;

                    #region Randomize
                    //Cores[0].Tick();
                    //int RANDOM_ID = random.Next(Param.NUMCORES);
                    //for (int i = 0; i < Param.NUMCORES; i++)
                    //{
                    //    //DEBUG.Print($"Cores[{RANDOM_ID}].Tick()");
                    //    Cores[RANDOM_ID].Tick();
                    //    RANDOM_ID = (RANDOM_ID + 1) % Param.NUMCORES;
                    //}
                    #endregion

                    //DEBUG.Print($"A1. Sim.Cycle++ => {Cycle}");
                    //MemSystem.tick();

                    for (int i = 0; i < Param.NUMCORES; i++)
                        Cores[i].Tick();

                    //DEBUG.Print($"A2.  Cores[i].Tick()  => {Cycle}");
                    //if (Cycle % Param.ClockFactor == 0)
                    //{
                    //    for (int c = 0; c < Param.NUM_CHANNELS; c++)
                    //        MemSystem.MemControllers[c].Tick();
                    //}

                    MemSystem.tick();

                    XBAR.Tick();

                    Cycle++;

                    if (Cycle % 5000000 == 0) progress.Report((int)Cycle);

                    if (Cycle > MAX_Cycles)
                    {
                        TraceReader.Close();
                        //WriteDebugInfo();
                        MemSystem.MemControllers.ForEach(mc => mc.CHANNEL.CloseStatFile());
                        return true;
                    }

                    //if (TRACE_EOF) return true;

                }
            }
        );

    
    #region UN-USED"

    public async Task<bool> Run_AllAsync(CancellationToken token, Action<long> handler) =>
        await Task.Run(() =>
            {
                if (token.IsCancellationRequested) return false;
                progress = new Progress<long>(handler);

                Initialize();

                while (true)
                {

                    if (token.IsCancellationRequested) return false;

                    if (TRACE_EOF) return true;

                    Cycle++;
                    MemSystem.Cycle = Cycle;

                    #region Randomize
                    //Cores[0].Tick();
                    //int RANDOM_ID = random.Next(Param.NUMCORES);
                    //for (int i = 0; i < Param.NUMCORES; i++)
                    //{
                    //    //DEBUG.Print($"Cores[{RANDOM_ID}].Tick()");
                    //    Cores[RANDOM_ID].Tick();
                    //    RANDOM_ID = (RANDOM_ID + 1) % Param.NUMCORES;
                    //}
                    #endregion

                    for (int i = 0; i < Param.NUMCORES; i++)
                        Cores[i].Tick();


                    if (Cycle % Param.ClockFactor == 0)
                    {
                        for (int c = 0; c < Param.NUM_CHANNELS; c++)
                            MemSystem.MemControllers[c].Tick();
                    }


                    XBAR.Tick();

                    if (Cycle % 1000000 == 0) progress.Report((int)Cycle);

                    //if (Cycle > MAX_Cycles) return true;

                }
            }
        );
  
    public bool StartNow()
    {
        Initialize();

        while (true)
        {

            Cycle++;
            MemSystem.Cycle = Cycle;


            for (int i = 0; i < Param.NUMCORES; i++)
                Cores[i].Tick();

            if (Cycle % Param.ClockFactor == 0)
            {
                for (int c = 0; c < Param.NUM_CHANNELS; c++)
                    MemSystem.MemControllers[c].Tick();
            }


            XBAR.Tick();

            if (Cycle > MAX_Cycles) return true;

        }

    }
 
    
    #endregion


}


