using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImmatureBackend.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum Grade
{
    Pr,
    G1,
    G2,
    G3,
    [EnumMember(Value = "Below Standard")] BelowStandard
}