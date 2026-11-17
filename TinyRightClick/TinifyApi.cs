using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TinyRightClick;

public record CompressResult(long OriginalSize, long CompressedSize, double SavedPercent);

/// <summary>
/// Calls the Tinify (TinyPNG/TinyJPG) API to compress an image in-place.
/// </summary>
public static class TinifyApi
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(5) };

    public static async Task<CompressResult> CompressAsync(string filePath, string apiKey)
    {
        var originalBytes = await File.ReadAllBytesAsync(filePath);
        var originalSize = originalBytes.LongLength;

        // POST the image to Tinify's shrink endpoint
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.tinify.com/shrink");
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        request.Content = new ByteArrayContent(originalBytes);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var response = await Http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            try
            {
                var errorJson = JsonDocument.Parse(errorBody);
                var msg = errorJson.RootElement.GetProperty("message").GetString();
                throw new Exception(msg ?? $"API error {(int)response.StatusCode}");
            }
            catch (JsonException)
            {
                throw new Exception($"API error {(int)response.StatusCode}");
            }
        }

        // Parse the response to get the output URL
        var responseBody = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(responseBody);
        var outputUrl = doc.RootElement.GetProperty("output").GetProperty("url").GetString()
            ?? throw new Exception("No output URL in API response");

        // Download the compressed image
        var compressedBytes = await Http.GetByteArrayAsync(outputUrl);

        // Replace the original file
        await File.WriteAllBytesAsync(filePath, compressedBytes);

        var compressedSize = compressedBytes.LongLength;
        var savedPercent = originalSize > 0
            ? (1.0 - (double)compressedSize / originalSize) * 100.0
            : 0.0;

        return new CompressResult(originalSize, compressedSize, savedPercent);
    }
}
