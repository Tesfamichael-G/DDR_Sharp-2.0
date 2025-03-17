using System;

namespace SimulatorLib.Common
{
    public class RowAddress
    {
        public int Rank;
        public int Bank;
        public int BankGroup;
        public int Row;

        public RowAddress(int rank, int bankG, int bank, int row)
        {
            Rank = rank;
            BankGroup = bankG;
            Bank = bank;
            Row = row;
        }

        public override bool Equals(object obj)
        {
            return obj is RowAddress address &&
                   Rank == address.Rank &&
                   BankGroup == address.BankGroup &&
                   Bank == address.Bank &&
                   Row == address.Row;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Rank, BankGroup, Bank, Row);
        }

        public override string ToString()
        {
            return $"{Rank}{BankGroup}{Bank}-{Row}"; ;
        }
        public static bool operator ==(RowAddress Left, RowAddress Right) => (Left is RowAddress) ? Left.Equals(Right) : Right is null;
        public static bool operator !=(RowAddress Left, RowAddress Right) => !(Left == Right);

    }

}