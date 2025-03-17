using SimulatorLib.Common;

namespace SimulatorLib.DeActivator
{

    public class RowBufferData
    {

        public int Row = -1;
        public long TimeStamp = -1;

        public RowBufferData()
        {
            Row = -1;
            TimeStamp = -1;
        }
        public RowBufferData(int row, long timestamp)
        {
            Row = row;
            TimeStamp = timestamp;
        }

        public void Update(long time_stamp, int row)
        {
            Row = row;
            TimeStamp = time_stamp;
        }

        //public bool ISNULL => ((Row == -1) && (TimeStamp == -1));
        //public bool IsNotNull => !ISNULL;

        //public static RowBufferData NULL => new RowBufferData(-1, -1);

        public override string ToString()
        {
            return $"[{Row},{TimeStamp}]";

        }

    }





}
