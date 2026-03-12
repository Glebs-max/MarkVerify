using System;
using System.IO;
using System.Windows.Media.Imaging;
using WpfApp_IC.Services.SettingsD;

namespace WpfApp_IC.Services
{
    public interface IImageSaverService
    {
        void SaveReject(BitmapSource frame, string? code);
        void UpdatePath(string path);
    }   

    public class ImageSaverService : IImageSaverService
    {
        private string _basePath;


        public ImageSaverService(string basePath)
        {
            _basePath = string.IsNullOrWhiteSpace(basePath) ? "RejectImages" : basePath;
            Directory.CreateDirectory(_basePath);
        }

        public void UpdatePath(string newPath)
        {
            _basePath = string.IsNullOrWhiteSpace(newPath) ? "RejectImages" : newPath;
            Directory.CreateDirectory(_basePath);
        }


        /// <summary>
        /// Сохраняет кадр в папку RejectImages.
        /// Имя файла: дата_время_код.jpg
        /// </summary>

        public void SaveReject(BitmapSource frame, string? code)
        { 
            try
            {
                string safeCode = string.IsNullOrWhiteSpace(code)
                    ? "NOREAD"
                    : code.Replace("/", "-").Replace("\\", "-")
                           .Replace(":", "-").Replace(" ", "_");

                string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{safeCode}.jpg";
                string fullPath = Path.Combine(_basePath, fileName);

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
