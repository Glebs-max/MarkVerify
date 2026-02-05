using MvCodeReaderSDKNet;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;

namespace WpfApp_IC.Services.Camera
{
    /// <summary>
    /// Реализация работы с камерой через SDK  MvCoderReader
    /// Всю логику работы с unmanaged-кодом изолируем тут
    /// </summary>
    public class CameraService : ICameraService
    {
        private MvCodeReader? _reader;
        private bool _isOpened;

        /// <summary>
        /// Инициализация камеры: поиск, создание handle (дескриптор), настройка параметров
        /// </summary>
        public void Open()
        {
            if (_isOpened) return;

            // Получаем список устройств
            var list = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST
            {
                pDeviceInfo = new IntPtr[MvCodeReader.MV_CODEREADER_MAX_DEVICE_NUM]
            };

            int ret = MvCodeReader.MV_CODEREADER_EnumDevices_NET(
                ref list,
                MvCodeReader.MV_CODEREADER_GIGE_DEVICE);

            if (ret != MvCodeReader.MV_CODEREADER_OK || list.nDeviceNum == 0)
                throw new Exception("Камера не найдена");

            // Берём первую камеру (в этом месте нужно будет исправить на несколько камер кода-нибудь)
            var devInfo = Marshal.PtrToStructure<MvCodeReader.MV_CODEREADER_DEVICE_INFO>(
                list.pDeviceInfo[0] );

            _reader = new MvCodeReader();
            _reader.MV_CODEREADER_CreateHandle_NET( ref devInfo );
            _reader.MV_CODEREADER_OpenDevice_NET();

            _reader.MV_CODEREADER_SetEnumValueByString_NET("TriggerMode", "On");
            _reader.MV_CODEREADER_SetEnumValueByString_NET("TriggerSource", "Software");
            _reader.MV_CODEREADER_SetEnumValueByString_NET("TriggerActivation", "RisingEdge");
            _reader.MV_CODEREADER_SetEnumValueByString_NET("CodeType", "DataMatrix");



            _reader.MV_CODEREADER_StartGrabbing_NET();

            _isOpened = true;
        }

        /// <summary>
        /// Закрытие камеры 
        /// </summary>
        
        public void Close()
        {
            if(!_isOpened || _reader == null) return;
            try
            {
                _reader.MV_CODEREADER_StopGrabbing_NET();
                _reader.MV_CODEREADER_CloseDevice_NET();
                _reader.MV_CODEREADER_DestroyHandle_NET();
            }
            catch { Console.WriteLine($"Ошибка при закрытии камеры "); }

            _reader = null;
            _isOpened= false;
        }

        /// <summary>
        /// используем софт-триггер, получаем кадр и пытаемся извлечь DataMatrix.
        /// </summary>
        public  (DataMatrixResult? dm, BitmapSource? frame) TriggerAndRead()
        {
            if (!_isOpened || _reader == null)
                throw new InvalidOperationException("Камера не открыта");

            // Софт-триггер 

            _reader.MV_CODEREADER_SetCommandValue_NET("TriggerSoftware");

            IntPtr pData = IntPtr.Zero;
            var info = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO();
            IntPtr pInfo = Marshal.AllocHGlobal(Marshal.SizeOf(info));

            try
            {
                // Получаем кадр 
                int ret = _reader.MV_CODEREADER_GetOneFrameTimeout_NET(ref pData, pInfo, 1000);
                if (ret != MvCodeReader.MV_CODEREADER_OK) 
                    return (null, null);

                info = Marshal.PtrToStructure<MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO>(pInfo); 
                
                BitmapSource? frame = DecodeFrame(pData, info);


                // Если код не считан — возвращаем только кадр
                if (!info.bIsGetCode || info.chResult == null || info.chResult.Length <= 8) 
                    return (null, frame);

                var dm = ExtractDM(info.chResult);
                return (dm, frame);
            }
            finally
            {
                Marshal.FreeHGlobal(pInfo);
            }
        }

        /// <summary>
        /// Извлекает DataMatrix из байтов SDK.
        /// </summary>
        private DataMatrixResult? ExtractDM(byte[] data)
        {
            int start = 8;
            int zero = Array.IndexOf(data, (byte)0, start);
            if(zero < 1) 
                return null;
            string raw = Encoding.UTF8.GetString(data, start, zero - start);
            string normalized = raw.Replace(((char)0x1D).ToString(), "");

            return new DataMatrixResult { Raw = raw, Normalized = normalized };
        }

        /// <summary>
        /// Декодирует JPEG-кадр в BitmapSource.
        /// </summary>
        private BitmapSource DecodeFrame (IntPtr pData, MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO info)
        {
            byte[] jpeg = new byte[(int)info.nFrameLen];
            Marshal.Copy(pData, jpeg, 0, jpeg.Length);

            using var ms = new MemoryStream(jpeg);
            var decoder = new JpegBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var bmp = decoder.Frames[0];
            bmp.Freeze();
            return bmp;
        }
        public void Dispose() => Close();
    }
}
