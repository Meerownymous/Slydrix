using System.Text.Json;
using Tonga;
using Tonga.Scalar;

namespace Eluvion.Misc;

/// <summary>A scalar whose value is the given JSON text deserialized as <typeparamref name="TResult"/>.</summary>
public sealed class DeserializedJson<TResult>(TextMorph txt) : ScalarEnvelope<TResult>(() =>
    JsonSerializer.Deserialize<TResult>(txt.Str(), new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    })
)
{
    /// <summary>A scalar whose value is the given text interpreted as JSON and deserialized as <typeparamref name="TResult"/>.</summary>
    public DeserializedJson(IText text) : this(new TextMorph(text))
    { }
}