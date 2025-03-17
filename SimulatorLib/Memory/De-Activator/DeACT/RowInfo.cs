using SimulatorLib.Common;

namespace SimulatorLib.DeActivator
{
    public class RowInfo
    {

        public uint Activations { get; set; }
        public int Rank;
        public int BankGroup;
        public int Bank;
        public int Row;

        public RowInfo(MemoryAddress mAddr, uint activations)
        {
            Rank = mAddr.Rank;
            BankGroup = mAddr.BankGroup;
            Bank = mAddr.Bank;
            Row = mAddr.Row;

            Activations = activations;
        }

        public RowInfo(MemoryAddress mAddr)
        {
            Rank = mAddr.Rank;
            BankGroup = mAddr.BankGroup;
            Bank = mAddr.Bank;
            Row = mAddr.Row;
            Activations = 1;
        }
    }


}
