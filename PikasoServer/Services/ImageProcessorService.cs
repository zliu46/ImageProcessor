using Google.Protobuf;
using Grpc.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PikasoServer.Services
{
    public class ImageProcessorService : ImageProcessor.ImageProcessorBase
    {
        public override async Task<ImageProcessResponse> ProcessImage(ImageProcessRequest request, ServerCallContext context)
        {
            using var image = Image.Load(request.Image.ToByteArray());
            if (request.ConvertToGrayscale)
            {
                image.Mutate(x => x.Grayscale());
            }

            // Flip horizontally if requested
            if (request.FlipHorizontal)
            {
                image.Mutate(x => x.Flip(FlipMode.Horizontal));
            }

            // Flip vertically if requested
            if (request.FlipVertical)
            {
                image.Mutate(x => x.Flip(FlipMode.Vertical));
            }

            // Rotate the image by the specified degrees
            if (request.RotateDegrees != 0)
            {
                image.Mutate(x => x.Rotate(request.RotateDegrees));
            }

            // Resize the image if requested
            if (request.ResizeWidth > 0 && request.ResizeHeight > 0)
            {
                image.Mutate(x => x.Resize(request.ResizeWidth, request.ResizeHeight));
            }

            // Generate a thumbnail if requested
            if (request.GenerateThumbnail)
            {
                // Define thumbnail size, you can adjust these values
                const int thumbnailWidth = 200;
                const int thumbnailHeight = 200;
                image.Mutate(x => x.Resize(thumbnailWidth, thumbnailHeight));
            }

            // Save the processed image to a memory stream
            using (var ms = new MemoryStream())
            {
                image.SaveAsJpeg(ms);
                ms.Seek(0, SeekOrigin.Begin);

                // Return the processed image
                return new ImageProcessResponse
                {
                    Image = ByteString.CopyFrom(ms.ToArray())
                };
            }
        }
    }
}
