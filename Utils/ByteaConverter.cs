using Newtonsoft.Json;

namespace ImmatureBackend.Utils;

public class ByteaConverter : JsonConverter<byte[]>
{
    public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = reader.Value?.ToString();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        
        if (value.StartsWith("\\x"))
        {
            var hex = value[2..];
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }
        
        return Convert.FromBase64String(value);
    }

    public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteValue("\\x" + Convert.ToHexString(value).ToLowerInvariant());
    }
}