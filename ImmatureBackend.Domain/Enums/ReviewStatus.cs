using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImmatureBackend.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ReviewStatus
{
    [EnumMember(Value = "review")] Review,
    [EnumMember(Value = "accepted")] Accepted,
    [EnumMember(Value = "rejected")] Rejected,
    [EnumMember(Value = "retraining")] Retraining
}