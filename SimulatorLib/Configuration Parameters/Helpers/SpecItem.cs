namespace SimulatorLib
{
    public class KeyValue_Item
    {
        public object Name { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return $"{Name} {Value}";
        }

    }
}
