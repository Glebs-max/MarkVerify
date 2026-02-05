using System;
using System.Windows.Media.Imaging;

namespace WpfApp_IC.Services.Camera
{
    /// <summary>
    /// Интерфейс камеры. Абстракция над SDK.
    /// Позволяет легко заменить SDK или камеру без изменения остального кода.
    /// </summary>
    public interface ICameraService : IDisposable
    {
        /// <summary>Открывает соединение с камерой.</summary>
        void Open();

        /// <summary>Закрывает соединение.</summary>
        void Close();

        /// <summary>
        /// Делает софт-триггер и возвращает:
        /// - DataMatrix (если считан)
        /// - кадр (BitmapSource)
        /// </summary>
        (DataMatrixResult? dm, BitmapSource? frame) TriggerAndRead();
    }
}
