using System.Text.Json.Serialization;

namespace Manis.Contract.Models;

[JsonSerializable(typeof(ManisGetRequest))]
[JsonSerializable(typeof(ManisGetResponse))]
[JsonSerializable(typeof(ManisPostRequest))]
[JsonSerializable(typeof(ManisPostResponse))]
[JsonSerializable(typeof(CreateUser))]
public partial class ManisJsonContext : JsonSerializerContext;