using Microsoft.Extensions.Logging;
using System.IO;

namespace PipeConnection.ObjectIO
{
    public interface IObjectReaderWriter
    {
        ILoggerFactory LoggerFactory { get; }

        T ReadObject<T>(Stream stream);

        void WriteObject<T>(Stream stream, T obj);
    }
}
