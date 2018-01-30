using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace PipeConnection.ObjectIO
{
    public class ObjectBinaryReaderWriter : IObjectReaderWriter
    {
        public Encoding Encoding { get; }

        public bool LeaveOpen { get; }

        public ILoggerFactory LoggerFactory { get; }

        private ILogger Logger { get; }

        public ObjectBinaryReaderWriter(bool leaveOpen = true)
            : this(Encoding.UTF8, new LoggerFactory(), leaveOpen) { }

        public ObjectBinaryReaderWriter(Encoding encoding, bool leaveOpen = true)
            : this(encoding, new LoggerFactory(), leaveOpen) { }

        public ObjectBinaryReaderWriter(ILoggerFactory loggerFactory, bool leaveOpen = true)
            : this(Encoding.UTF8, loggerFactory, leaveOpen) { }

        public ObjectBinaryReaderWriter(Encoding encoding, ILoggerFactory loggerFactory, bool leaveOpen = true)
        {
            Encoding = encoding;
            LeaveOpen = leaveOpen;

            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<ObjectBinaryReaderWriter>();
        }

        public T ReadObject<T>(Stream stream)
        {
            using (Logger.BeginScope($"{nameof(ReadObject)}"))
            {
                using (var reader = new BinaryReader(stream, Encoding, LeaveOpen))
                {
                    var length = reader.ReadInt32();
                    var bytes = reader.ReadBytes(length);
                    Logger.LogDebug($"Read stream.\r\nBytes length : {length}\r\nBytes : {BytesLogString(bytes)}");
                    var converter = new ObjectBinaryConverter<T>();
                    return converter.FromByteArray(bytes);
                }
            }
        }

        public void WriteObject<T>(Stream stream, T obj)
        {
            using (Logger.BeginScope($"{nameof(WriteObject)}"))
            {
                using (var writer = new BinaryWriter(stream, Encoding, LeaveOpen))
                {
                    var converter = new ObjectBinaryConverter<T>();
                    var bytes = converter.ToByteArray(obj);
                    var length = bytes.Length;
                    writer.Write(length);
                    writer.Write(bytes);
                    Logger.LogDebug($"Write stream.\r\nBytes length : {length}\r\nBytes : {BytesLogString(bytes)}");
                }
            }
        }

        private string BytesLogString(byte[] bytes) =>
            $"[{string.Join(", ", bytes)}]";
    }
}
