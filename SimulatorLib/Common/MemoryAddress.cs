namespace SimulatorLib.Common
{
    public class MemoryAddress
    {
        public long BlockAddress;
        public int Channel;
        public int Rank;
        public int BankGroup;
        public int Bank;
        public int Row;
        public int Column;

        public static MemoryAddress CreateRowAddress(int rnk, int group, int bnk, int row)
        {
            MemoryAddress ma = new MemoryAddress();
            (ma.Rank, ma.BankGroup, ma.Bank, ma.Row) = (rnk, group, bnk, row);
            return ma;
        }

        //public override bool Equals(object obj)
        //{
        //    return obj is MemoryAddress address &&
        //           Rank == address.Rank &&
        //           BankGroup == address.BankGroup &&
        //           Bank == address.Bank &&
        //           Row == address.Row;
        //}

        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(Rank, BankGroup, Bank, Row);
        //}

        public override string ToString()
        {
            return $"{Channel} {Rank}{BankGroup}{Bank}-{Row}"; ;
        }

        internal int said;

        //public static bool operator ==(MemoryAddress Left, MemoryAddress Right) => (Left is MemoryAddress) ? Left.Equals(Right) : Right is null;
        //public static bool operator !=(MemoryAddress Left, MemoryAddress Right) => !(Left == Right);
    }


}