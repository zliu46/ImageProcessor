using Grpc.Net.Client;
using Google.Protobuf;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to Pikaso Image Processor");
        using var channel = GrpcChannel.ForAddress("http://localhost:5161");
        var client = new ImageProcessor.ImageProcessorClient(channel);

        Console.Write("Enter the path to the image you wish to process: ");
        var inputPath = Console.ReadLine();
        if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
        {
            Console.WriteLine("File does not exist.");
            return;
        }

        var imageBytes = await File.ReadAllBytesAsync(inputPath);
        var request = new ImageProcessRequest
        {
            Image = ByteString.CopyFrom(imageBytes),
        };

        bool processing = true;
        while (processing)
        {
            Console.WriteLine("\nChoose an operation to perform:");
            Console.WriteLine("1. Convert to grayscale");
            Console.WriteLine("2. Flip image horizontally");
            Console.WriteLine("3. Flip image vertically");
            Console.WriteLine("4. Rotate image");
            Console.WriteLine("5. Resize image");
            Console.WriteLine("6. Generate a thumbnail");
            Console.WriteLine("7. Finish processing");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    request.ConvertToGrayscale = true;
                    break;
                case "2":
                    request.FlipHorizontal = true;
                    break;
                case "3":
                    request.FlipVertical = true;
                    break;
                case "4":
                    request.RotateDegrees = AskUserForFloat("Enter degrees to rotate: ");
                    break;
                case "5":
                    request.ResizeWidth = AskUserForInt("Enter new width: ");
                    request.ResizeHeight = AskUserForInt("Enter new height: ");
                    break;
                case "6":
                    request.GenerateThumbnail = true;
                    break;
                case "7":
                    processing = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please select a valid operation.");
                    break;
            }
        }

        try
        {
            var reply = await client.ProcessImageAsync(request);
            var directoryPath = Path.GetDirectoryName(inputPath) ?? Directory.GetCurrentDirectory();
            var outputPath = Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(inputPath) + "_processed" + Path.GetExtension(inputPath));
            await File.WriteAllBytesAsync(outputPath, reply.Image.ToByteArray());
            Console.WriteLine($"Processed image saved to: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        //try
        //{
        //    var reply = await client.ProcessImageAsync(request);
        //    var tempFilePath = Path.GetTempFileName() + ".jpg"; // Ensure extension matches expected format
        //    await File.WriteAllBytesAsync(tempFilePath, reply.Image.ToByteArray());

        //    // Determine the action based on the operating system
        //    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        //    {
        //        // Windows: Use 'cmd' to execute the 'start' command which opens the file with the default application
        //        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {tempFilePath}") { CreateNoWindow = true });
        //    }
        //    else
        //    {
        //        // For other operating systems, adjust accordingly. This is an example for Windows.
        //        Console.WriteLine("Automatic opening is not configured for this OS.");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"Error: {ex.Message}");
        //}
        //try
        //{
        //    var reply = await client.ProcessImageAsync(request);
        //    var tempFilePath = Path.GetTempFileName() + ".jpg"; // Make sure the extension is appropriate for the image format
        //    await File.WriteAllBytesAsync(tempFilePath, reply.Image.ToByteArray());
        //    Console.WriteLine("Opening processed image...");

        //    // Open the processed image using the default viewer on macOS
        //    System.Diagnostics.Process.Start("open", tempFilePath);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"Error: {ex.Message}");
        //}
    }

    static float AskUserForFloat(string message)
    {
        Console.Write(message);
        if (float.TryParse(Console.ReadLine(), out float result))
        {
            return result;
        }
        return 0; // Default to 0 if parsing fails
    }

    static int AskUserForInt(string message)
    {
        Console.Write(message);
        if (int.TryParse(Console.ReadLine(), out int result))
        {
            return result;
        }
        return 0; // Default to 0 if parsing fails
    }
}
