using System.Linq;

namespace SimulatorLib.DDR
{
    public partial class Rank
    {
        public class ScheduledPower
        {

            private Parameters Param;
            private RankStat Stat;
            private Rank RANK;

            DataSheetPower DS_P;

            private Power power;
            private Energy energy;

            public ScheduledPower(Parameters param)
            {
                Param = param;
                
            }

            public ScheduledPower(Parameters param, Rank rank)
            {
                Param = param;
                RANK = rank;
                Stat = RANK.Stat;

                power = new Power(param);
                energy = new Energy(param);
            }

            public (Energy, Power) Compute()
            {

                DS_P = new DataSheetPower();
                DS_P.Initialize(Param);

                CalcPower();

                CalcEnergy();

                return (energy, power);
            }

            private Power CalcPower()
            {

                long Cycle = RANK.Cycle;
                int nChips = Param.CHIPS_PER_RANK;

                double ACT_GAP = RANK.GAP_between_Activates.Average;
                power.ACT = (ACT_GAP > 0) ? DS_P.ACT * Param.RC * nChips / ACT_GAP : 0;

                //DEBUG.Print($"\t\t\t PSCH_ACT ({power.ACT}) => DS_P.ACT ({DS_P.ACT}) * tRC ({Param.RC}) * nChips ({nChips}) / ACT_GAP({ACT_GAP})");

                #region Background
                if (Param.DDR_TYPE == MemoryType.DDR4)
                {
                    power.ACT_PDN = DS_P.ACT_PDN * nChips * ((double)Stat.TS_ACT_PDN / Cycle);
                }
                else
                {
                    power.ACT_PDNF = DS_P.ACT_PDNF * nChips * ((double)Stat.TS_ACT_PDNF / Cycle);
                    power.ACT_PDNS = DS_P.ACT_PDNS * nChips * ((double)Stat.TS_ACT_PDNS / Cycle);
                }

                power.PRE_PDN_SLOW = DS_P.PRE_PDN_SLOW * nChips * ((double)Stat.TS_PRE_PDNS / Cycle);
                power.PRE_PDN_FAST = DS_P.PRE_PDN_FAST * nChips * ((double)Stat.TS_PRE_PDNF / Cycle);

                power.ACT_STBY = DS_P.ACT_STBY * nChips * ((double)Stat.TS_ACT_Standby / Cycle);

                double TS_PRE_STBY = ((double)(Cycle - Stat.TS_ACT_Standby - Stat.TS_PRE_PDNS - Stat.TS_PRE_PDNF - Stat.TS_ACT_PDN)) / Cycle;
                power.PRE_STBY = DS_P.PRE_STBY * nChips * TS_PRE_STBY;//(Cycle - Stat.TS_ACT_Standby - Stat.TS_PRE_PDNS - Stat.TS_PRE_PDNF - Stat.TS_ACT_PDN) / Cycle;
                #endregion


                power.WRITE = DS_P.WRITE * nChips * (Stat.NUM_WRITES * Param.BL) / Cycle;
                power.READ = DS_P.READ * nChips * (Stat.NUM_READS * Param.BL) / Cycle;

                power.REF = DS_P.REF * nChips * Param.RFC / Param.REFI;

                power.DQ = DS_P.DQ * nChips * (Stat.NUM_READS * Param.BL) / Cycle;
                power.TERM_WRITE = DS_P.TERM_WRITE * nChips * (Stat.NUM_WRITES * Param.BL) / Cycle;

                power.TERM_READ_OTHER = DS_P.TERM_READ_OTHER * nChips * ((double)Stat.TS_TERM_READS_FROM_OTHER_RANKS / Cycle);
                power.TERM_WRITE_OTHER = DS_P.TERM_WRITE_OTHER * nChips * ((double)Stat.TS_TERM_WRITES_TO_OTHER_RANKS / Cycle);

                return power;

            }

            private Energy CalcEnergy()
            {

                long Cycle = RANK.Cycle;
                int nChips = Param.CHIPS_PER_RANK;
                double tCK = Param.TCK;

                energy.ACT = (
                                 Stat.ACT * (Param.IDD0 * Param.RC
                                          - Param.IDD3N * Param.RAS
                                          - Param.IDD2N * (Param.RC - Param.RAS))
                             ) * Param.VDD * tCK * nChips/1000000;

                #region Background

                if (Param.DDR_TYPE == MemoryType.DDR4)
                {
                    energy.ACT_PDN = DS_P.ACT_PDN * nChips * Stat.TS_ACT_PDN * tCK / 1000000;
                }
                else
                {
                    energy.ACT_PDNF = DS_P.ACT_PDNF * nChips * Stat.TS_ACT_PDNF * tCK / 1000000;
                    energy.ACT_PDNS = DS_P.ACT_PDNS * nChips * Stat.TS_ACT_PDNS * tCK / 1000000;
                }

                energy.PRE_PDN_SLOW = DS_P.PRE_PDN_SLOW * nChips * Stat.TS_PRE_PDNS * tCK / 1000000;
                energy.PRE_PDN_FAST = DS_P.PRE_PDN_FAST * nChips * Stat.TS_PRE_PDNF * tCK / 1000000;

                energy.ACT_STBY = DS_P.ACT_STBY * nChips * Stat.TS_ACT_Standby * tCK / 1000000;

                double TS_PRE_STBY = Cycle - Stat.TS_ACT_Standby - Stat.TS_PRE_PDNS - Stat.TS_PRE_PDNF - Stat.TS_ACT_PDN;
                energy.PRE_STBY = DS_P.PRE_STBY * nChips * TS_PRE_STBY * tCK / 1000000;
                 #endregion

                energy.WRITE = DS_P.WRITE * nChips * Stat.NUM_WRITES * Param.BL * tCK / 1000000;
                energy.READ = DS_P.READ * nChips * Stat.NUM_READS * Param.BL * tCK / 1000000;

                energy.REF = DS_P.REF * nChips * Param.RFC * tCK / 1000000;

                energy.DQ = DS_P.DQ * nChips * Stat.NUM_READS * Param.BL * tCK / 1000000;
                energy.TERM_WRITE = DS_P.TERM_WRITE * nChips * Stat.NUM_WRITES * Param.BL * tCK / 1000000;

                energy.TERM_READ_OTHER = DS_P.TERM_READ_OTHER * nChips * Stat.TS_TERM_READS_FROM_OTHER_RANKS * tCK / 1000000;
                energy.TERM_WRITE_OTHER = DS_P.TERM_WRITE_OTHER * nChips * Stat.TS_TERM_WRITES_TO_OTHER_RANKS * tCK / 1000000;

                return energy;
            }

        }

    }



}

