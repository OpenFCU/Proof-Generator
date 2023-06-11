using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace ProofGenerator;

[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class HttpApiJsonSerializerContext : JsonSerializerContext
{
}