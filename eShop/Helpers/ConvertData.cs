using eShop.Models;
using System.Text.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace eShop.Helpers
{
    public static class ConvertData<T>
    {
        public static async IAsyncEnumerable<T> ByteArrayToObjectList(byte[] inputByteArray)
        {
            IAsyncEnumerable<T> deserializedList = JsonSerializer.DeserializeAsyncEnumerable<T>(new MemoryStream(inputByteArray));
            //return deserializedList;
            await foreach (T _item in deserializedList)
            {
                yield return _item;
            }
        }

        public static byte[] ObjectListToByteArray(List<T> inputList)
        {
            MemoryStream memoryStream = new MemoryStream();
            JsonSerializer.SerializeAsync(memoryStream, inputList);

            return memoryStream.ToArray();
        }

        public static T ByteArrayToObject(byte[] inputByteArray)
        {
            var deserializedList = JsonSerializer.Deserialize<T>(inputByteArray);
            return deserializedList;
        }

        public static byte[] ObjectToByteArray(T input)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(input);

            return bytes;
        }

        public static List<T> StringToObjectList(string inputString)
        {
            var deserializedList = JsonSerializer.Deserialize<List<T>>(inputString);
            return deserializedList;
        }

        public static string ObjectListToString(List<T> inputList)
        {
            var _returnString = JsonSerializer.Serialize(inputList);

            return _returnString;
        }

        public static T StringToObject(string inputString)
        {
            var deserializedList = JsonSerializer.Deserialize<T>(inputString);
            return deserializedList;
        }

        public static string ObjectToString(T input)
        {
            var _returnString = JsonSerializer.Serialize(input);

            return _returnString;
        }
    }
}
