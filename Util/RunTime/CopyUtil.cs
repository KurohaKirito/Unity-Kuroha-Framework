using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kuroha.Util.RunTime
{
    public static class CopyUtil
    {
        public static T DeepCopy<T>(T source) where T : class
        {
            if (typeof(T).IsSerializable == false)
            {
                DebugUtil.LogError($"The type must be serializable. {typeof(T)}", null, "red");
            }

            if (source == null)
            {
                return default;
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream) as T;
            }
        }
    }
}
