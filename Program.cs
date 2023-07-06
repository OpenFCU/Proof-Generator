using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using ProofGenerator;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

FontManager.RegisterFontWithCustomName("Noto Sans TC", File.OpenRead("fonts/NotoSansTC-Medium.otf"));
FontManager.RegisterFontWithCustomName("TW-Kai", File.OpenRead("fonts/TW-Kai-98_1.ttf"));

var serializer = new SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>((JsonSerializerOptions options) =>
{
    options.Converters.Append(new DateOnlyConverter());
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var BadRequest = new APIGatewayHttpApiV2ProxyResponse
{
    StatusCode = 400,
    Body = "400 Bad Request?"
};

var handler = async Task<APIGatewayHttpApiV2ProxyResponse> (APIGatewayHttpApiV2ProxyRequest raw, ILambdaContext context) =>
{
    var ms = new MemoryStream(Encoding.UTF8.GetBytes(raw.Body));
    var request = serializer.Deserialize<Request>(ms);
    var imageBytes = await GenerateProofImage(request.Student, request.Icon, request.Stamp);
    if (imageBytes == null) return BadRequest;
    var payload = Convert.ToBase64String(imageBytes);
    var payloadLength = Encoding.UTF8.GetByteCount(payload);
    return new APIGatewayHttpApiV2ProxyResponse
    {
        StatusCode = (int)HttpStatusCode.OK,
        Body = payload,
        Headers = new Dictionary<string, string> {
            {"Content-Type", "image/webp" },
            {"Content-Length", $"{payloadLength}" },
            {"Content-Disposition", """inline; filename="proof.webp"; filename*=utf-8''proof.webp"""},
        },
        IsBase64Encoded = true
    };
};

await LambdaBootstrapBuilder.Create(handler, serializer)
        .Build()
        .RunAsync();


async Task<byte[]?> GenerateProofImage(Student student, string iconFileName, string stampFileName)
{
    if (!IdPattern().IsMatch(student.Id)) return null;
    if (!Enum.IsDefined<Degree>(student.Degree)) return null;

    byte[]? icon = null, stamp = null;

    using (var _client = new HttpClient())
    {
        Task<byte[]>? iconTask = null, stampTask = null;
        if (ImgurPattern().IsMatch(iconFileName))
            try { iconTask = _client.GetByteArrayAsync($"https://i.imgur.com/{iconFileName}"); } catch (HttpRequestException) { }
        if (ImgurPattern().IsMatch(stampFileName))
            try { stampTask = _client.GetByteArrayAsync($"https://i.imgur.com/{stampFileName}"); } catch (HttpRequestException) { }
        if (iconTask != null)
            icon = await iconTask.WaitAsync(new TimeSpan(0, 0, 5));
        if (stampTask != null)
            stamp = await stampTask.WaitAsync(new TimeSpan(0, 0, 5));
    }

    var doc = new StudentProofDocument(student, icon, stamp);
    return await GenerateImageAsync(doc, ImageFormat.Webp);
}

async Task<byte[]> GenerateImageAsync(IDocument doc, ImageFormat format = ImageFormat.Webp)
{
    var settings = new ImageGenerationSettings()
    {
        ImageFormat = format,
        RasterDpi = 144
    };
    return await Task.Run(() => doc.GenerateImages(settings).First());
}

public static partial class Program
{
    [GeneratedRegex(@"^[dm]\d{7}$", RegexOptions.IgnoreCase)]
    public static partial Regex IdPattern();
    [GeneratedRegex(@"^[a-z0-9]+\.(avif|bmp|gif|heif|ico|jpg|jpeg|png|webp)$", RegexOptions.IgnoreCase)]
    public static partial Regex ImgurPattern();
}