using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LabelDesigner.Services
{
    public static class BitmapService
    {
        public static WriteableBitmap GetBitmap(FrameworkElement element, double dpi = 96, Size? size = null)
        {
            Transform t = element.RenderTransform;
            element.RenderTransform = Transform.Identity;

            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));
            element.UpdateLayout();

            int width = (int)Math.Floor((size?.Width ?? element.DesiredSize.Width) * dpi / 96);
            int height = (int)Math.Floor((size?.Height ?? element.DesiredSize.Height) * dpi / 96);
            
            RenderTargetBitmap rtb = new(width, height, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(element);
            
            element.RenderTransform = t;
            return new(rtb);
        }
        public static ImageBrush GetAlphaMask(BitmapSource source)
        {
            int stride = source.PixelWidth * 4;
            int bytesPerPixel = source.Format.BitsPerPixel / 8;
            int length = source.PixelHeight * stride;
            byte[] pixels = new byte[length];

            source.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < length; i += bytesPerPixel)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];

                byte gray = (byte)((r + g + b) / 3);
                pixels[i + 3] = (byte)(255 - gray);
            }

            WriteableBitmap result = new(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), pixels, stride, 0);
            return new(result);
        }
        public static WriteableBitmap Monochrome(BitmapSource source, byte threshold = 128)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            byte[] pixels = new byte[width * height * 4];
            source.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte r = pixels[i + 2];
                byte g = pixels[i + 1];
                byte b = pixels[i + 0];
                byte brightness = (byte)((r + g + b) / 3);
                byte color = brightness < threshold ? (byte)0 : (byte)255;

                pixels[i + 0] = color;
                pixels[i + 1] = color;
                pixels[i + 2] = color;
                pixels[i + 3] = 255;
            }

            WriteableBitmap result = new(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
            return result;
        }
        public static WriteableBitmap Dither(BitmapSource source, byte threshold = 200)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];

            source.CopyPixels(pixels, stride, 0);

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int i = y * stride + x * 4;
                    byte oldPixel = (byte)(0.299 * pixels[i + 2] + 0.587 * pixels[i + 1] + 0.114 * pixels[i]);
                    byte newPixel = oldPixel < threshold ? (byte)0 : (byte)255;
                    int error = oldPixel - newPixel;

                    pixels[i] = pixels[i + 1] = pixels[i + 2] = newPixel;

                    DistributeError(pixels, i + 4, error, 7.0 / 16.0);
                    DistributeError(pixels, i - 4 + stride, error, 3.0 / 16.0);
                    DistributeError(pixels, i + stride, error, 5.0 / 16.0);
                    DistributeError(pixels, i + 4 + stride, error, 1.0 / 16.0);
                }
            }

            WriteableBitmap result = new(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return result;
        }
        public static string ConvertToZpl(BitmapSource bitmap)
        {
            ArgumentNullException.ThrowIfNull(bitmap);

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int bytesPerRow = (width + 7) / 8;
            int stride = width * 4;

            byte[] pixels = new byte[height * stride];
            bitmap.CopyPixels(pixels, stride, 0);

            StringBuilder hexBuilder = new();

            for (int y = 0; y < height; y++)
            {
                int bitPos = 0;
                int currentByte = 0;

                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = y * stride + x * 4;
                    byte b = pixels[pixelIndex + 0];
                    byte g = pixels[pixelIndex + 1];
                    byte r = pixels[pixelIndex + 2];

                    double luminance = 0.299 * r + 0.587 * g + 0.114 * b;
                    bool isBlack = luminance < 128;

                    currentByte <<= 1;
                    if (isBlack)
                        currentByte |= 1;

                    bitPos++;

                    if (bitPos == 8)
                    {
                        hexBuilder.Append(currentByte.ToString("X2"));
                        bitPos = 0;
                        currentByte = 0;
                    }
                }

                if (bitPos > 0)
                {
                    currentByte <<= 8 - bitPos;
                    hexBuilder.Append(currentByte.ToString("X2"));
                }
            }

            int totalBytes = hexBuilder.Length / 2;

            return new($"^XA\n^GFA,{totalBytes},{totalBytes},{bytesPerRow},{hexBuilder}\n^FS\n^XZ");
        }

        private static void DistributeError(byte[] pixels, int index, int error, double factor)
        {
            if (index < 0 || index + 2 >= pixels.Length)
                return;

            int delta = (int)(error * factor);

            for (int k = 0; k < 3; k++)
            {
                int val = pixels[index + k] + delta;
                pixels[index + k] = (byte)Math.Clamp(val, 0, 255);
            }
        }
    }
}
