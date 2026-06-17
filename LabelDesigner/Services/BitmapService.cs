using PDFtoZPL;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp.Views.WPF;

namespace LabelDesigner.Services
{
    public static class BitmapService
    {
        public static string WPFToZpl(FrameworkElement element, double dpi, double width, double height)
        {
            RenderTargetBitmap source = GetBitmap(element, dpi, width, height);
            ZplOptions options = new()
            {
                EncodingKind = BitmapEncodingKind.Base64Compressed,
                DitheringKind = DitheringKind.None,
                Threshold = 128
            };

            return Conversion.ConvertBitmap(source.ToSKBitmap(), options);
        }
        public static ImageBrush GetAlphaMask(FrameworkElement element, double dpi)
        {
            RenderTargetBitmap source = GetBitmap(element, dpi);

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

        private static RenderTargetBitmap GetBitmap(FrameworkElement element, double dpi = 96, double? width = null, double? height = null)
        {
            Transform originalT = element.RenderTransform;
            element.RenderTransform = Transform.Identity;
            element.UpdateLayout();

            int pixelWidth = (int)Math.Floor((width ?? element.DesiredSize.Width) * dpi / 96);
            int pixelHeight = (int)Math.Floor((height ?? element.DesiredSize.Height) * dpi / 96);

            RenderTargetBitmap rtb = new(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(element);

            element.RenderTransform = originalT;
            element.UpdateLayout();

            return rtb;
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
