using System;
using System.Linq;

using SimulatorLib.DDR;

namespace SimulatorLib
{
    public partial class ChannelStat
    {

        //public Channel CHANNEL { get; set; }

        public int ID { get; set; }
        //public long X_ACTS { get; set; }
        //public long X_ACTS_RD { get; set; }
        //public long X_ACTS_WR { get; set; }

        public string NAME => $"CHNL-{ID}";

        public long PRE { get; set; }
        public long ACTS { get; set; }
        public long ACTS_RD { get; set; }
        public long ACTS_WR { get; set; }
        public long READS { get; set; }
        public long WRITES { get; set; }
        public long RD_HITS { get; set; }
        public long WR_HITS { get; set; }

        public AverageObject AVG_ReadLatency { get; set; }
        public AverageObject AVG_ReadQue_Latency { get; set; }
        public AverageObject AVG_Write_Latency { get; set; }
        public AverageObject AVG_WriteQue_Latency { get; set; }

        public float READ_HitRate { get; set; }
        public float READ_MissRate { get; set; }

        public float WRITE_HitRate { get; set; }
        public float WRITE_MissRate { get; set; }


        /*   POWER   */
        public double BackgroundPower { get; set; }
        public double ActPower { get; set; }
        public double ReadPower { get; set; }
        public double WritePower { get; set; }


        public double ReadTerminatePower { get; set; }
        public double WriteTerminatePower { get; set; }

        public double ReadTerminateOtherODTPower { get; set; }
        public double WriteTerminateOtherODTPower { get; set; }

        public double RefreshPower { get; set; }
        public double TotalPower { get; set; }

        /*   ENERGY   */
        public double BackgroundEnergy { get; set; }
        public double ActEnergy { get; set; }
        public double ReadEnergy { get; set; }
        public double WriteEnergy { get; set; }

        public double ReadTerminateEnergy { get; set; }
        public double WriteTerminateEnergy { get; set; }

        public double ReadTerminateOtherODTEnergy { get; set; }
        public double WriteTerminateOtherODTEnergy { get; set; }

        public double RefreshEnergy { get; set; }
        public double TotalEnergy { get; set; }

        public void Intialize()
        {
            READS = 0;
            WRITES = 0;

            AVG_ReadLatency = new AverageObject("AVG_ReadLatency");

            AVG_ReadQue_Latency = new AverageObject();
            AVG_Write_Latency = new AverageObject();
            AVG_WriteQue_Latency = new AverageObject();

            READ_HitRate = 0;
            ACTS_WR = 0;
            ACTS_RD = 0;
        }

        bool FileHiits = false;
        public void Compile(Channel CHANNEL)
        {
            long acts = ACTS;// - X_ACTS;
            long actsForRead = ACTS_RD;// - X_ACTS_RD;
            long actsForWrite = ACTS_WR;// - X_ACTS_WR;

            long readHitCount = READS - actsForRead;
            long writeHitCount = WRITES - actsForWrite;

            READ_HitRate = (READS == 0) ? 0 : (float)readHitCount / READS;  // (READS == 0) ? 0 : (float)RD_HITS / READS;
            READ_MissRate = (float)actsForRead /READS;

            WRITE_HitRate = (WRITES == 0) ? 0 : (float)writeHitCount / WRITES; //(float)WR_HITS / WRITES;
            WRITE_MissRate = (float)actsForWrite/WRITES;

            if (FileHiits)
            {
                string hdr = $"ACT READ   = {ACTS_RD}";
                string hdw = $"ACT READ   = {ACTS_WR}";
                string hda = $"ACT        = {ACTS}";

                string rdh = $"READ_HitRate   = (READS == 0)  ? 0 : (READS:{READS} - ACTS_RD{ACTS_RD}) / READS:{READS} => {READ_HitRate}";
                string rdm = $"READ_MissRate  = (READS == 0)  ? 0 : (ACTS_RD{ACTS_RD}) / READS:{READS} => {READ_HitRate}";

                string wdh = $"WRITE_HitRate  = (WRITES == 0) ? 0 : WRITES: {WRITES} - ACTS_WR: {ACTS_WR} / WRITES: {WRITES}";
                string wdm = $"WRITE_MissRate = (WRITES == 0) ? 0 : ACTS_WR: {ACTS_WR} / WRITES: {WRITES}";

                System.IO.StreamWriter sw;
                string fn = $"hits{DateTime.Now.Minute}-{DateTime.Now.Second}.txt";
                string f = @"C:\Users\tmik\Desktop\DeActivator\Workspace\output\" + fn;
                sw = new System.IO.StreamWriter(f);
                sw.WriteLine(hdr);
                sw.WriteLine(hdw);
                sw.WriteLine("----------------------------------------------------");
                sw.WriteLine(hda);
                sw.WriteLine("\n");

                sw.WriteLine(rdh);
                sw.WriteLine(rdm);
                sw.WriteLine(wdh);
                sw.WriteLine(wdm);

                sw.Close();
            }


            for (int i = 0; i < CHANNEL.Ranks.Length; i++)
            {
                CHANNEL.Ranks[i].CalculatePower();
            }

            BackgroundPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.BackgroundPower), 2);
            ActPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ActPower), 2);
            ReadPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadPower), 2);
            WritePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WritePower), 2);
            ReadTerminatePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminatePower), 2);
            WriteTerminatePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminatePower), 2);
            ReadTerminateOtherODTPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateOtherPower), 2);
            WriteTerminateOtherODTPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateOtherPower), 2);
            RefreshPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.RefreshPower), 2);
            TotalPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.TotalRankPower), 2);

            BackgroundEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.BackgroundEnergy), 2);
            ActEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ActEnergy), 2);
            ReadEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadEnergy), 2);
            WriteEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteEnergy), 2);
            ReadTerminateEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateEnergy), 2);
            WriteTerminateEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateEnergy), 2);
            ReadTerminateOtherODTEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateOtherEnergy), 2);
            WriteTerminateOtherODTEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateOtherEnergy), 2);
            RefreshEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.RefreshEnergy), 2);
            TotalEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.TotalRankEnergy), 2);

        }


    }

}


//public void Compile2(Channel CHANNEL)
//{
//    //long Num_Activates_ForReads = 0;
//    //long Num_Activates_ForWrites = 0;

//    //long Num_Reads = 0;
//    //long Num_Writess = 0;

//    //for (int r = 0; r < CHANNEL.Param.NUM_RANKS; r++)
//    //{
//    //    Rank rank = CHANNEL.Ranks[r];
//    //    for (int G = 0; G < CHANNEL.Param.NUM_BANK_GROUPS; G++)
//    //    {
//    //        Num_Activates_ForWrites += rank.Groups[G].Banks.Sum(bnk => bnk.Stat.ACT_WRITE);
//    //        Num_Activates_ForReads += rank.Groups[G].Banks.Sum(bnk => bnk.Stat.ACT_READ);

//    //        Num_Reads += rank.Groups[G].Banks.Sum(bnk => bnk.Stat.READ);
//    //        Num_Writess += rank.Groups[G].Banks.Sum(bnk => bnk.Stat.WRITE);
//    //    }
//    //}

//    READ_HitRate = (READS == 0) ? 0 : (float)RD_HITS / READS;
//    READ_MissRate = (READS == 0) ? 0 : (1 - RD_HITS) / READS;
//    //READ_HitRate = (Num_Reads == 0) ? 0 : (float)((Num_Reads - Num_Activates_ForReads)) / Num_Reads;
//    //READ_MissRate = (Num_Reads == 0) ? 0 : (float)(Num_Activates_ForReads) / Num_Reads;

//    WRITE_HitRate = (WRITES == 0) ? 0 : (float)WR_HITS / WRITES;
//    WRITE_MissRate = (WRITES == 0) ? 0 : (float)(1 - WR_HITS) / WRITES;
//    //WRITE_HitRate = (Num_Writess == 0) ? 0 : (float)((Num_Writess - Num_Activates_ForWrites)) / Num_Writess;
//    //WRITE_MissRate = (Num_Writess == 0) ? 0 : (float)(Num_Activates_ForWrites) / Num_Writess;

//    string fn = $"hits{DateTime.Now.Minute}-{DateTime.Now.Second}.txt";
//    string f = @"C:\Users\tmik\Desktop\DeActivator\Workspace\output\" + fn;
//    System.IO.StreamWriter sw = new System.IO.StreamWriter(f);

//    string hdr = $"ACT READ   = {ACTS_RD}";
//    string hdw = $"ACT READ   = {ACTS_WR}";
//    string hda = $"ACT        = {ACTS}";

//    string rdh = $"READ_HitRate   = (READS == 0)  ? 0 : (READS:{READS} - ACTS_RD{ACTS_RD}) / READS:{READS} => {READ_HitRate}";
//    string rdm = $"READ_MissRate  = (READS == 0)  ? 0 : (ACTS_RD{ACTS_RD}) / READS:{READS} => {READ_HitRate}";

//    string wdh = $"WRITE_HitRate  = (WRITES == 0) ? 0 : WRITES: {WRITES} - ACTS_WR: {ACTS_WR} / WRITES: {WRITES}";
//    string wdm = $"WRITE_MissRate = (WRITES == 0) ? 0 : ACTS_WR: {ACTS_WR} / WRITES: {WRITES}";

//    sw.WriteLine(hdr);
//    sw.WriteLine(hdw);
//    sw.WriteLine("----------------------------------------------------");
//    sw.WriteLine(hda);
//    sw.WriteLine("\n");

//    sw.WriteLine(rdh);
//    sw.WriteLine(rdm);
//    sw.WriteLine(wdh);
//    sw.WriteLine(wdm);

//    sw.Close();

//    for (int i = 0; i < CHANNEL.Ranks.Length; i++)
//    {
//        CHANNEL.Ranks[i].CalculatePower();
//    }
//    #region DEBUG
//    //DEBUG.Print($"************** Compiling {CHANNEL.Ranks.Count} ranks");
//    //double act = 0.0;
//    //for (int i = 0; i < CHANNEL.Ranks.Count; i++)
//    //{
//    //    act += CHANNEL.Ranks[i].Stat.ActPower;
//    //    DEBUG.Print($"************** {CHANNEL.Ranks[i].Stat.ActPower} | {act}");
//    //}
//    #endregion

//    BackgroundPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.BackgroundPower), 2);
//    ActPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ActPower), 2);
//    ReadPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadPower), 2);
//    WritePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WritePower), 2);
//    ReadTerminatePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminatePower), 2);
//    WriteTerminatePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminatePower), 2);
//    ReadTerminateOtherODTPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateOtherPower), 2);
//    WriteTerminateOtherODTPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateOtherPower), 2);
//    RefreshPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.RefreshPower), 2);
//    TotalPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.TotalRankPower), 2);

//    BackgroundEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.BackgroundEnergy), 2);
//    ActEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ActEnergy), 2);
//    ReadEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadEnergy), 2);
//    WriteEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteEnergy), 2);
//    ReadTerminateEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateEnergy), 2);
//    WriteTerminateEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateEnergy), 2);
//    ReadTerminateOtherODTEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateOtherEnergy), 2);
//    WriteTerminateOtherODTEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateOtherEnergy), 2);
//    RefreshEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.RefreshEnergy), 2);
//    TotalEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.TotalRankEnergy), 2);

//}

////public void Compile()
////{
////    long Num_Activates_ForReads = 0;
////    long Num_Activates_ForWrites = 0;

////    long Num_Reads = 0;
////    long Num_Writess = 0;

////    for (int r = 0; r < CHANNEL.Param.NUM_RANKS; r++)
////    {
////        Rank rank = CHANNEL.Ranks[r];

////        Num_Activates_ForWrites += rank.Banks.Sum(bnk => bnk.Stat.ACT_WRITE);
////        Num_Activates_ForReads += rank.Banks.Sum(bnk => bnk.Stat.ACT_READ);

////        Num_Reads += rank.Banks.Sum(bnk => bnk.Stat.READ);
////        Num_Writess += rank.Banks.Sum(bnk => bnk.Stat.WRITE);

////    }

////    READ_HitRate = (Num_Reads == 0) ? 0 : (float)((Num_Reads - Num_Activates_ForReads)) / Num_Reads;
////    READ_MissRate = (Num_Reads == 0) ? 0 : (float)(Num_Activates_ForReads) / Num_Reads;

////    WRITE_HitRate = (Num_Writess == 0) ? 0 : (float)((Num_Writess - Num_Activates_ForWrites)) / Num_Writess;
////    WRITE_MissRate = (Num_Writess == 0) ? 0 : (float)(Num_Activates_ForWrites) / Num_Writess;

////    //CHANNEL.Ranks.ForEach(r => r.CloseFile());
////    CHANNEL.Ranks.ForEach(r => r.CalculatePower());

////    #region DEBUG
////        //DEBUG.Print($"************** Compiling {CHANNEL.Ranks.Count} ranks");
////        //double act = 0.0;
////        //for (int i = 0; i < CHANNEL.Ranks.Count; i++)
////        //{
////        //    act += CHANNEL.Ranks[i].Stat.ActPower;
////        //    DEBUG.Print($"************** {CHANNEL.Ranks[i].Stat.ActPower} | {act}");
////        //}
////    #endregion            

////    BackgroundPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.BackgroundPower), 2);
////    ActPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ActPower), 2);
////    ReadPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadPower), 2);
////    WritePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WritePower), 2);
////    ReadTerminatePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminatePower), 2);
////    WriteTerminatePower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminatePower), 2);
////    ReadTerminateOtherODTPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateOtherPower), 2);
////    WriteTerminateOtherODTPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateOtherPower), 2);
////    RefreshPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.RefreshPower), 2);
////    TotalPower = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.TotalRankPower), 2);

////    BackgroundEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.BackgroundEnergy), 2);
////    ActEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ActEnergy), 2);
////    ReadEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadEnergy), 2);
////    WriteEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteEnergy), 2);
////    ReadTerminateEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateEnergy), 2);
////    WriteTerminateEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateEnergy), 2);
////    ReadTerminateOtherODTEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.ReadTerminateOtherEnergy), 2);
////    WriteTerminateOtherODTEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.WriteTerminateOtherEnergy), 2);
////    RefreshEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.RefreshEnergy), 2);
////    TotalEnergy = Math.Round(CHANNEL.Ranks.Sum(r => r.Stat.TotalRankEnergy), 2);

////}