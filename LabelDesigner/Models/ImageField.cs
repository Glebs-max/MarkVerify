using LabelDesigner.DesignerItemVisuals;
using LabelDesigner.Services;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LabelDesigner.Models
{
    public class ImageField : Field
    {
        private string _source = "Icons/example.bmp";

        public ImageField()
        {
            DataType = DataType.Fixed;
            Source = _source;
        }

        public new ImageVisual Visual => (ImageVisual)base.Visual;

        public override string FieldType => "Изображение";
        public string Source
        {
            get => _source;
            set
            {
                Set(ref _source, value, () =>
                {
                    Inverted = false;
                    KeepAspectRatio = false;
                    Visual.Content.Source = LoadFromSource(value);
                    InitialSize = new(Visual.Content.Source.Width, Visual.Content.Source.Height);
                    KeepAspectRatio = true;
                });
            }
        }

        protected override ImageVisual InitializeVisual() => new(LoadFromSource(_source));
        protected override void InvertField()
        {
            WriteableBitmap? bitmap = new(Visual.Content.Source as BitmapSource ?? null);

            if (bitmap == null)
                return;

            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = bitmap.Format.BitsPerPixel / 8;
            int length = bitmap.PixelHeight * stride;
            byte[] pixels = new byte[length];

            bitmap.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += bytesPerPixel)
            {
                pixels[i] = (byte)(255 - pixels[i]);
                pixels[i + 1] = (byte)(255 - pixels[i + 1]);
                pixels[i + 2] = (byte)(255 - pixels[i + 2]);
            }

            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0);
            Visual.Content.Source = bitmap;
        }
        private static WriteableBitmap LoadFromSource(string source)
        {
            BitmapImage bitmap = new();

            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(source, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return BitmapService.Dither(bitmap);
            }
            catch
            {
                return LoadFromSource("Icons/NoImage.png");
            }
        }
    }
}
