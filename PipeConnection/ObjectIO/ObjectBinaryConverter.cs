using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PipeConnection.ObjectIO
{
    public class ObjectBinaryConverter<T>
    {
        public IFormatter Formatter { get; }

        public ObjectBinaryConverter(IFormatter formatter = null)
        {
            Formatter = formatter ?? new BinaryFormatter();
        }

        public byte[] ToByteArray(T obj)
        {
            using (var stream = new MemoryStream())
            {
                Formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public T FromByteArray(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return (T)Formatter.Deserialize(stream);
            }
        }
    }
}
