namespace SimulatorLib;

public interface IFileReader
{
    public string NextLine();

    public void Close();

    public bool EOF();

}




