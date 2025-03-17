namespace SimulatorLib
{
    public interface IAddressTranslator
    {
        public Common.MemoryAddress Translate(long physical_address);

    }

}
