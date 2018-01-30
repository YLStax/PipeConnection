using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PipeConnection.ObjectIO
{
    public class ObjectJsonBinaryReaderWriter : IObjectReaderWriter
    {
        public Encoding Encoding { get; }

        public bool LeaveOpen { get; }

        public ILoggerFactory LoggerFactory { get; }

        private ILogger Logger { get; }

        public ObjectJsonBinaryReaderWriter(bool leaveOpen = true)
            : this(Encoding.UTF8, new LoggerFactory(), leaveOpen) { }

        public ObjectJsonBinaryReaderWriter(Encoding encoding, bool leaveOpen = true)
            : this(encoding, new LoggerFactory(), leaveOpen) { }

        public ObjectJsonBinaryReaderWriter(ILoggerFactory loggerFactory, bool leaveOpen = true)
            : this(Encoding.UTF8, loggerFactory, leaveOpen) { }

        public ObjectJsonBinaryReaderWriter(Encoding encoding, ILoggerFactory loggerFactory, bool leaveOpen = true)
        {
            Encoding = encoding;
            LeaveOpen = leaveOpen;

            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<ObjectJsonBinaryReaderWriter>();
        }

        public T ReadObject<T>(Stream stream)
        {
            using (Logger.BeginScope($"{nameof(ReadObject)}"))
            {
                using (var reader = new BinaryReader(stream, Encoding, LeaveOpen))
                {
                    var json = reader.ReadString();
                    Logger.LogDebug($"Read stream.\r\nJson : {json}");
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
        }

        public void WriteObject<T>(Stream stream, T obj)
        {
            using (Logger.BeginScope($"{nameof(WriteObject)}"))
            {
                using (var writer = new BinaryWriter(stream, Encoding, LeaveOpen))
                {
                    var json = JsonConvert.SerializeObject(obj, Formatting.None);
                    writer.Write(json);
                    Logger.LogDebug($"Write stream.\r\nJson : {json}");
                }
            }
        }
    }
}
