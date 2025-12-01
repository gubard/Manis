using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Gaia.Errors;
using Manis.Contract.Errors;

namespace Manis.Contract.Models;

[JsonSerializable(typeof(ManisGetRequest))]
[JsonSerializable(typeof(ManisGetResponse))]
[JsonSerializable(typeof(ManisPostRequest))]
[JsonSerializable(typeof(ManisPostResponse))]
[JsonSerializable(typeof(CreateUser))]
[JsonSerializable(typeof(AlreadyExistsValidationError))]
[JsonSerializable(typeof(NotFoundValidationError))]
public partial class ManisJsonContext : JsonSerializerContext
{
    public static readonly IJsonTypeInfoResolver Resolver;

    static ManisJsonContext()
    { 
        Resolver = Default.WithAddedModifier(typeInfo =>
        {
            if (typeInfo.Type == typeof(ValidationError))
            {
                typeInfo.PolymorphismOptions = new()
                {
                    TypeDiscriminatorPropertyName = "$type",
                    DerivedTypes =
                    {
                        new(typeof(AlreadyExistsValidationError), typeof(AlreadyExistsValidationError).FullName!),
                        new(typeof(NotFoundValidationError), typeof(NotFoundValidationError).FullName!),
                    },
                };
            }
        });
    }
}