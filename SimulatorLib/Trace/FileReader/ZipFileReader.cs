using System.IO;
using System.IO.Compression;
using System.Text;

namespace SimulatorLib;

public class ZipFileReader : IFileReader
{
    
    private string FILE_NAME;

    private GZipStream Reader;

    public const int BUF_MAX = 1000;
    public ZipFileReader(string filename)
    {
        FILE_NAME = filename;
        Reader = new GZipStream(File.OpenRead(filename), CompressionMode.Decompress);
    }

    public string NextLine()
    {
        return ReadLine();
    }

    private string ReadBytes()
    {
        byte[] single_buf = new byte[1];

        // EOF check
        if (Reader.Read(single_buf, 0, 1) == 0)
            return null;

        byte[] buf = new byte[BUF_MAX];
        int n = 0;
        while (single_buf[0] != (byte)'\n')
        {
            buf[n++] = single_buf[0];
            Reader.Read(single_buf, 0, 1);
        }
        return Encoding.ASCII.GetString(buf, 0, n);
    }

    public string ReadLine()
    {
        string line = ReadBytes();

        if (line != null)
            return line;

        Reader.Close();
        Reader = new GZipStream(File.OpenRead(FILE_NAME), CompressionMode.Decompress);

        line = ReadLine();
        DEBUG.Assert(line != null);
        return line;
    }

    public void Close()
    {
        Reader.Close();
    }

    public bool EOF()
    {
        throw new System.NotImplementedException();
    }

}




