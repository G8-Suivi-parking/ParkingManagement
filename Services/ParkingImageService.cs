using System.Net.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ParkingManagement.API.Services;

public class ParkingImageService
{
    private readonly HttpClient _httpClient;

    public ParkingImageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GetParkingImageAsync(string imageUrl)
    {
        return await _httpClient.GetByteArrayAsync(imageUrl);
    }

    public bool IsImageValid(byte[] image)
    {
        return image != null && image.Length > 0;
    }

    public async Task<byte[]> PrepareImageAsync(byte[] imageBytes)
    {
        using var inputStream = new MemoryStream(imageBytes);

        using var image = await Image.LoadAsync(inputStream);

        image.Mutate(x => x.Resize(640, 480));

        using var outputStream = new MemoryStream();

        await image.SaveAsJpegAsync(outputStream);

        return outputStream.ToArray();
    }
}