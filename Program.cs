using System.Security.Cryptography;
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

int outInt;
var imgurTimeLimitSec = int.TryParse(Environment.GetEnvironmentVariable("IMGUR_TIMELIMIT"), out outInt) ? outInt : 5;
var imgurTimeLimit = new TimeSpan(0, 0, imgurTimeLimitSec);

Directory.CreateDirectory("/tmp/generated");

var PreflightResponse = new APIGatewayHttpApiV2ProxyResponse
{
    StatusCode = 200,
    Headers = new Dictionary<string, string> {
            {"Access-Control-Allow-Headers", "Content-Type"},
            {"Access-Control-Allow-Origin", "*"},
            {"Access-Control-Allow-Methods", "GET,POST"},
        }
};

var BadRequestResponse = new APIGatewayHttpApiV2ProxyResponse
{
    StatusCode = 400,
    Body = "400 Bad Request"
};

var NotFoundResponse = new APIGatewayHttpApiV2ProxyResponse
{
    StatusCode = 404,
    Body = "404 Not Found"
};

var md5 = MD5.Create();

var serializer = new SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>((JsonSerializerOptions options) =>
{
    options.Converters.Append(new DateOnlyConverter());
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var handler = async Task<APIGatewayHttpApiV2ProxyResponse> (APIGatewayHttpApiV2ProxyRequest raw, ILambdaContext context) =>
{
    var http = raw.RequestContext.Http;

    if (http.Method == "GET" && http.Path.StartsWith("/generated/"))
    {
        return await GetGeneratedFileAsync(http.Path);
    }

    if (http.Method == "OPTIONS" && http.Path == "/")
    {
        return PreflightResponse;
    }

    if (http.Method == "POST" && http.Path == "/")
    {
        var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(raw.Body));
        var request = serializer.Deserialize<Request>(requestStream);
        requestStream.Seek(0, SeekOrigin.Begin);
        var hash = Convert.ToHexString(await md5.ComputeHashAsync(requestStream));
        var generatedPath = await GenerateProofImage(request, hash);
        if (generatedPath == null) return BadRequestResponse;

        var responseStream = new MemoryStream(64);
        serializer.Serialize<Response>(new Response(generatedPath), responseStream);

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> {
                {"Content-Type", "application/json"},
            },
            Body = Encoding.UTF8.GetString(responseStream.ToArray()),
        };
    }

    return NotFoundResponse;
};

await LambdaBootstrapBuilder.Create(handler, serializer)
        .Build()
        .RunAsync();

FileInfo? CheckGeneratedFilePath(string httpPath)
{
    if (!GeneratedFilePattern().IsMatch(httpPath)) return null;
    var info = new FileInfo($"/tmp{httpPath}");
    if (!info.FullName.StartsWith("/tmp/generated/")) return null;
    return info;
}

async Task<string?> GenerateProofImage(Request request, string hash)
{
    var httpPath = $"/generated/{hash}.webp";
    var info = CheckGeneratedFilePath(httpPath);
    if (info == null) return null;

    if (!Enum.IsDefined<Degree>(request.Student.Degree)) return null;

    byte[]? icon = null, stamp = null;

    using (var _client = new HttpClient())
    {
        Task<byte[]>? iconTask = null, stampTask = null;
        if (ImgurPattern().IsMatch(request.Icon))
            try { iconTask = _client.GetByteArrayAsync($"https://i.imgur.com/{request.Icon}"); } catch (HttpRequestException) { }
        if (ImgurPattern().IsMatch(request.Stamp))
            try { stampTask = _client.GetByteArrayAsync($"https://i.imgur.com/{request.Stamp}"); } catch (HttpRequestException) { }
        if (iconTask != null)
            icon = await iconTask.WaitAsync(imgurTimeLimit);
        if (stampTask != null)
            stamp = await stampTask.WaitAsync(imgurTimeLimit);
    }

    ProofDocument? doc = request.Lang switch
    {
        "en" => new EnglishProofDocument(request.Title, request.Student, icon, stamp),
        "zh" => new ChineseProofDocument(request.Title, request.Student, icon, stamp),
        _ => null,
    };
    if (doc == null) return null;
    var settings = new ImageGenerationSettings()
    {
        ImageFormat = ImageFormat.Webp,
        RasterDpi = 144
    };
    await Task.Run(() => doc.GenerateImages(x => (x == 0) ? info.FullName : "/dev/null", settings));
    return $".{httpPath}";
}

async Task<APIGatewayHttpApiV2ProxyResponse> GetGeneratedFileAsync(string path)
{
    var info = CheckGeneratedFilePath(path);
    if (info == null || !info.Exists) return NotFoundResponse;
    var filename = info.Name;
    return new APIGatewayHttpApiV2ProxyResponse
    {
        StatusCode = 200,
        Headers = new Dictionary<string, string> {
            {"Content-Type", "image/webp" },
            {"Content-Disposition", $"""inline; filename="{filename}"; filename*=utf-8''{filename}"""},
        },
        Body = Convert.ToBase64String(await File.ReadAllBytesAsync(info.FullName)),
        IsBase64Encoded = true
    };
}

public static partial class Program
{
    [GeneratedRegex(@"^[a-z0-9]+\.(avif|bmp|gif|heif|ico|jpg|jpeg|png|webp)$", RegexOptions.IgnoreCase)]
    public static partial Regex ImgurPattern();
    [GeneratedRegex(@"^/generated/[a-z0-9]{32}\.webp$", RegexOptions.IgnoreCase)]
    public static partial Regex GeneratedFilePattern();
}
