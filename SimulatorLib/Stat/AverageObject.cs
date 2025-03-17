using System;

namespace SimulatorLib
{
    public class AverageObject
    {

        public string ID;
        public long VALUE;
        public uint COUNT;

        public AverageObject()
        {
            ID = string.Empty;
            VALUE = 0;
            COUNT = 0;
        }

        public AverageObject(string name)
        {
            ID = name;
            VALUE = 0;
            COUNT = 0;
        }


        public static AverageObject operator ++(AverageObject stat)
        {
            stat.VALUE++;
            stat.COUNT++;

            return stat;
        }
        public void INC()
        {
            VALUE++;
            COUNT++;
        }

        public void Add(long value)
        {
            VALUE += value;
            COUNT += 1;
        }

        public double Average
        {
            get
            {
                if (COUNT == 0)
                {
                    return 0.0;
                }

                return Math.Round((double)VALUE / COUNT, 2);
            }
        }
        public override string ToString() => $"{Math.Round(Average, 2)}";


    }



}
