using SixLabors.ImageSharp.Processing;

namespace AspNetCoreMvcDemoApp.Core.Infrastructure.Graphics.Abstractions
{
    public record ImageOptions
    (
            int Width,
            int Height,
            ResizeMode Mode = ResizeMode.Pad,
            bool ConvertToWebp = true,
            int Quality = 80
     );
}
