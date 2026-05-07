namespace WebUI.Services;

public enum ImageFitMode
{
    Default = 0,
    CoverCenter = 1
}

public interface IImageUrlBuilder
{
    string Build(
        string? imageUrl,
        int? width = null,
        int? height = null,
        ImageFitMode fit = ImageFitMode.Default,
        string? gravity = null);
}
