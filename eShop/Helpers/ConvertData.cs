using eShop.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace eShop.Helpers
{
    public static class ConvertData<T>
    {
        public static List<T> ByteArrayToProductList(byte[] inputByteArray)
        {
            var deserializedList = JsonSerializer.Deserialize<List<T>>(inputByteArray);
            return deserializedList;
        }

        public static byte[] ProductListToByteArray(List<T> inputList)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(inputList);

            return bytes;
        }

        public static T ByteArrayToProduct(byte[] inputByteArray)
        {
            var deserializedList = JsonSerializer.Deserialize<T>(inputByteArray);
            return deserializedList;
        }

        public static byte[] ProductToByteArray(T input)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(input);

            return bytes;
        }
    }
}
