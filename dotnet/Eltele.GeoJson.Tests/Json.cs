using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eltele.GeoJson.Tests;

internal static class Json
{
    public static void AssertEqual(string expected, string actual) =>
        Assert.IsTrue(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)),
            $"Expected {expected}{Environment.NewLine}but was {actual}");
}
