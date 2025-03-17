using System.IO;
using static System.Diagnostics.Debug;

namespace SimulatorLib;

public class FileReader : IFileReader
{
    int line_no = 0;

    private string FILE_NAME;
    private StreamReader Reader;

    public FileReader(string filename)
    {
        FILE_NAME = filename;
        Reader = new StreamReader(filename);
    }

    public string NextLine()
    {
        //if (line_no > 37000)
        //    Print($"=====> TRACE : ENDING .... LINE: {line_no}");

        string line = Reader.ReadLine();
        line_no++;

        if (Reader.EndOfStream || string.IsNullOrWhiteSpace(line))
        {
            line_no = 0;
            Print("=====> TRACE EOF: Rewinding .... ");
            Reader.Close();
            Reader= new StreamReader(FILE_NAME);
            return Reader.ReadLine();
        }
            
        //if (line_no > 37000)
        //    Print($"=====> TRACE : NOT NULL: {line_no}");

        return line;
  }

    public void Close()
    {
        Reader.Close();
    }

    public bool EOF()
    {
        return Reader.EndOfStream;
    }
}




