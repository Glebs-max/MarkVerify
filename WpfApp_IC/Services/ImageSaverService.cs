using System;
using System.IO;
using System.Windows.Media.Imaging;
using WpfApp_IC.Services;

namespace WpfApp_IC.Services
{
    public class ImageSaverService
    {
        public string SavePath { get; set; } = "RejectImages";

        public void SaveReject(BitmapSource frame, string? code)
        { 
            try
            {
                string safeCode = string.IsNullOrWhiteSpace(code)
                    ? "NOREAD"
                    : code.Replace("/", "-").Replace("\\", "-").Replace(":", "-").Replace(" ", "_");

                string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{safeCode}.jpg";
                string fullPath = Path.Combine(SavePath, fileName);

                var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
                encoder.Frames.Add(BitmapFrame.Create(frame));

                using var stream = new FileStream(fullPath, FileMode.Create);
                encoder.Save(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения изображения: {ex.Message}");
            }
        }
    }
}
