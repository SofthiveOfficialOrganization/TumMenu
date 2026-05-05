namespace WebUI.Services;

public interface IImageUrlBuilder
{
    string Build(string? imageUrl, int? width = null, int? height = null);
}
