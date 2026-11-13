using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImmatureBackend.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ReplicateQuality
{
    [EnumMember(Value = "bad")] Bad,
    [EnumMember(Value = "good")] Good
}