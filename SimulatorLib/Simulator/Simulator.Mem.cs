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

    public List<ReqHistory> Histories;

    private void FinishPending()
    {
        DEBUG.Print($"SIM.PENDING ...");

        while (true)
        {
            bool completed = true;
            for (int c = 0; c < Param.NUM_CHANNELS; c++)
            {
                completed = completed & MemSystem.MemControllers[c].FinishPending();

                //DEBUG.Print($"\t\t CHANNEL({c}) {completed} & *** {cmp} *** ");
            }

            Cycle += Param.ClockFactor;

            MemSystem.Cycle = Cycle;

            //DEBUG.Print($"SIM.Completed: *** {completed} *** ");

            if (completed)
            {
                //for (int c = 0; c < Param.NUM_CHANNELS; c++)
                //{
                //    MemSystem.MemControllers[c].BUS.WriteToFile();
                //}

                break;
            }
        }

        WriteDebugInfo();

    }

    private void WriteDebugInfo()
    {
        //MemSystem.MemControllers[0].SCHEDULER.CloseFile();

        string fn = $"sharp_his{DateTime.Now.Day}{DateTime.Now.Minute}-{DateTime.Now.Second}.csv";
        //string h = @"C:\Users\tmik\Desktop\DeActivator\Workspace\output\" + fn;
        //System.IO.StreamWriter sw = new System.IO.StreamWriter(h);

        foreach (var line in Histories)
            Console.WriteLine(line.ToString());

        //sw.Write(sb.ToString());
        //sw.Close();

    }

    private bool WriteReadTrace()
    {
        string fn = $"rt({DateTime.Now.Day}{DateTime.Now.Minute}-{DateTime.Now.Second})";

        return TraceReader.WriteTrace(fn);
    }

    public async Task<bool> RunMemAsync(CancellationToken token, Action<long> handler) =>
        await Task.Run(() =>
        {
            if (token.IsCancellationRequested) return false;
            progress = new Progress<long>(handler);

            Initialize();

            //if (WriteReadTrace()) //    return true;
            MemSystem.FetchRequest();

            while (true)
            {

                //if (token.IsCancellationRequested) return false;

                if (TRACE_EOF)
                {
                    FinishPending();
                    break;
                }

                //if (Cycle == 1000000)
                //{
                //    WriteDebugInfo();
                //    break;
                //}

                Cycle += Param.ClockFactor;

                MemSystem.Cycle = Cycle;

                MemSystem.Tick();

                for (int c = 0; c < Param.NUM_CHANNELS; c++)
                    MemSystem.MemControllers[c].Tick();


                //if (Cycle % 1000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 10000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 100000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 1000000 == 0) progress.Report((int)Cycle);
                if (Cycle % 5000000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 10000000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 15000000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 20000000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 25000000 == 0) progress.Report((int)Cycle);
                //if (Cycle % 50000000 == 0) progress.Report((int)Cycle);

            }

            return true;
        }
        );

    public async Task<bool> RunMemAsync() =>
        await Task.Run(() =>
        {

            int M = 0;

            Initialize();

            //if (WriteReadTrace()) //    return true;
            MemSystem.FetchRequest();

            while (true)
            {

                if (TRACE_EOF)
                {
                    Console.WriteLine($"TRACE_EOF: finishing pending requests on queue...");

                    FinishPending();
                    break;
                }

                Cycle += Param.ClockFactor;

                MemSystem.Cycle = Cycle;

                MemSystem.Tick();

                for (int c = 0; c < Param.NUM_CHANNELS; c++)
                    MemSystem.MemControllers[c].Tick();

                if (Cycle % 250_000_000 == 0)
                {
                    M += 250;
                    Console.WriteLine($"Cycle: {M}M");
                }


            }
            return true;
        }
        );


}


