using Newtonsoft.Json;

namespace Firepuma.Payments.Core.Entities;

public abstract class BaseEntity
{
    [JsonProperty(PropertyName = "id")]
    public virtual string Id { get; set; }

    [JsonProperty(PropertyName = "_etag")]
    public virtual string ETag { get; set; }

    [JsonProperty(PropertyName = "_ts")]
    public virtual long Timestamp { get; set; }
}