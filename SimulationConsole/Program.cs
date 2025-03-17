//// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using static System.Console;

using SimulatorLib;
using SimulatorLib.Common;
using System.ComponentModel;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

//using System.Linq;

namespace SimulatorConsole;


class Program
{

    //public Stat STAT => SIM.STAT;
    //public string WorkloadPath;
    //public string Workload;
    //public string Unit;


    static long Cycles = 1;
    public Simulator SIM;
    static Parameters PARAM;

    static long initialTicks;
    static DateTime TimeStarted;

    static long TotalCycles;

    public static async Task Main(string[] args)
    {
        Stopwatch sw = new Stopwatch(); 
        sw.Start();
        DefaultSettings settings = new DefaultSettings();
        settings.Initialize();
        PARAM = settings.Param;
        
        //Print( PARAM);

        //Console.ReadKey();


        PARAM.PIPELINEDEPTH = 0;
        SimulatorLib.Simulator SIM = new SimulatorLib.Simulator(PARAM);


        await StartAsync(SIM, PARAM);
        sw.Stop();
        Console.WriteLine($"total seconds: {sw.Elapsed.TotalSeconds}");
        Console.ReadKey();
    }

    private static async Task<bool> StartAsync( Simulator SIM, Parameters PARAM)
    {

        WriteLine($"\nIntializing ... ");
        InitializeUI(SIM, PARAM);

        WriteLine($"Simulating {SIM.ID} ... ");

        try
        {
            TimeStarted = DateTime.Now;
            initialTicks = TimeStarted.Ticks;

            bool finished = await SIM.RunMemAsync();
            long endTime =  DateTime.Now.Ticks;

            if (finished)
            {
                var elapsed = endTime - initialTicks;
                TimeSpan span = TimeSpan.FromTicks(elapsed);

                WriteLine($"Simulation Completed. {span.TotalSeconds} | {span.Minutes}:{span.Seconds}.{span.Milliseconds}");

                //CONTAINER.REPORT = ReportGenerator.GetReport(SIM, Workload);
                return true;
            }
            WriteLine("Simulation not completed.");
            return false;


        }
        catch (AggregateException ex)
        {
            WriteLine("\nAggregateException thrown with the following inner exceptions:");
            foreach (var v in ex.InnerExceptions)
            {
                WriteLine($"   Exception: {v.GetType().Name}");
            }
            return false;
        }
    }

    private static void InitializeUI(Simulator SIM, Parameters Param)
    {
        long TotalCycles = Cycles * 1000000;
        SIM.MAX_Cycles = TotalCycles;// * 1000 * 1000;
        Param.TRACE_TYPE = TraceType.DRAM;


        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        ////string projectDirectory = Directory.GetCurrentDirectory();

        // Construct the relative path to the trace file
        string trace = Path.Combine(projectDirectory, "Workspace", "input", "rt.1m");

        SIM.TraceReader = new DRAMTraceReader(trace, SIM);

    }

}

