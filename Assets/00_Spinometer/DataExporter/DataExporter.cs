using System;
using System.IO;

namespace GetBack.Spinometer.DataExporter
{
  public class DataExporter : IDisposable
  {
    private readonly string _path;

    public DataExporter(string path)
    {
      _path = path;
    }

    public void Dispose()
    {
      // nothing to do at this moment
    }

    public void Write(string data)
    {
      File.AppendAllText(_path, data);
    }
  }
}
