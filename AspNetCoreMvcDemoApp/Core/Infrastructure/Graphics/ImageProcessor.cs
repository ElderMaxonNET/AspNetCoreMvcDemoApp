namespace AspNetCoreMvcDemoApp.Core.Infrastructure.Graphics
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.Formats.Webp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SadLib.Web.Upload;
    using Abstractions;
    using SadLib.Web.Upload.Abstractions;
    using SadLib.Web.Upload.Models;

    public class ImageProcessor(int width, int height, ResizeMode mode = ResizeMode.Pad, bool convertToWebp = true, int quality = 80) : IImageProcessor
    {
        private const string WebpExtension = ".webp";
        private readonly ImageOptions options = new(Width: width, Height: height, Mode: mode, ConvertToWebp: convertToWebp, Quality: quality);

        private static IImageEncoder GetEncoder(IImageFormat format, ImageOptions options) => format switch
        {
            JpegFormat => new JpegEncoder
            {
                Quality = options.Quality,
                SkipMetadata = true
            },
            PngFormat => new PngEncoder
            {
                ColorType = PngColorType.RgbWithAlpha,
                TransparentColorMode = PngTransparentColorMode.Preserve,
                CompressionLevel = PngCompressionLevel.BestCompression,
                SkipMetadata = true
            },
            WebpFormat => new WebpEncoder
            {
                Quality = options.Quality,
                FileFormat = WebpFileFormatType.Lossy,
                SkipMetadata = true
            },
            _ => throw new NotSupportedException($"Unsupported image format: {format.GetType().Name}")
        };


        public async Task<(Stream Stream, string Extension)> ResizeAsync(Stream imageStream, FileDescriptor fileInfo)
        {
            if (imageStream.CanSeek)
                imageStream.Position = 0;

            using var image = await Image.LoadAsync<Rgba32>(imageStream);

            var format = image.Metadata.DecodedImageFormat ??
                throw new NotSupportedException($"The file content for '{fileInfo.Extension}' could not be recognized as a valid image format.");

            var resizeOptions = new ResizeOptions
            {
                Size = new Size(options.Width, options.Height),
                Mode = options.Mode,
                PadColor = format is PngFormat or WebpFormat ? Color.Transparent : Color.White
            };

            image.Mutate(x => x.AutoOrient());
            image.Mutate(x => x.Resize(resizeOptions));

            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IccProfile = null;
            image.Metadata.CicpProfile = null;

            MemoryStream? outputStream = null;
            try
            {
                outputStream = new MemoryStream();

                string finalExtension;

                if (options.ConvertToWebp)
                {
                    await image.SaveAsWebpAsync(outputStream, new WebpEncoder
                    {
                        Quality = options.Quality,
                        FileFormat = WebpFileFormatType.Lossy
                    });

                    finalExtension = WebpExtension;
                }
                else
                {
                    IImageEncoder encoder = GetEncoder(format, options);
                    finalExtension = format.FileExtensions.FirstOrDefault(ext => ext.Equals(fileInfo.Extension, StringComparison.OrdinalIgnoreCase))
                        ?? throw new NotSupportedException($"File extension {fileInfo.Extension} does not match the detected image format.");

                    await image.SaveAsync(outputStream, encoder);
                }

                outputStream.Position = 0;

                return (outputStream, finalExtension);
            }
            catch
            {
                outputStream?.Dispose();
                throw;
            }
        }

    }
}
