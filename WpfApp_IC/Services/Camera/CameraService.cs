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
        public void Open(string cameraIp)
        {
            if (_isOpened) return;

            if (string.IsNullOrWhiteSpace(cameraIp))
                throw new Exception("IP камеры не задан. Укажите IP в настройках.");

            var list = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST
            {
                pDeviceInfo = new IntPtr[MvCodeReader.MV_CODEREADER_MAX_DEVICE_NUM]
            };

            int ret = MvCodeReader.MV_CODEREADER_EnumDevices_NET(
                ref list, MvCodeReader.MV_CODEREADER_GIGE_DEVICE);

            if (ret != MvCodeReader.MV_CODEREADER_OK)
                throw new Exception($"Ошибка поиска камер: код {ret}");

            for (int i = 0; i < list.nDeviceNum; i++)
            {
                var devInfo = Marshal.PtrToStructure<MvCodeReader.MV_CODEREADER_DEVICE_INFO>(
                    list.pDeviceInfo[i]);

                byte[] g = devInfo.SpecialInfo.stGigEInfo;
                string foundIp = $"{g[11]}.{g[10]}.{g[9]}.{g[8]}";

                if (foundIp == cameraIp)
                {
                    _reader = new MvCodeReader();
                    _reader.MV_CODEREADER_CreateHandle_NET(ref devInfo);
                    _reader.MV_CODEREADER_OpenDevice_NET();
                    ConfigureCamera();
                    _isOpened = true;
                    return;
                }
            }

            throw new Exception($"Камера {cameraIp} не найдена в сети");
        }

        /// <summary>
        /// Получение информации с камеры
        /// </summary>
        public IReadOnlyList<CameraDeviceInfo> GetAvailableDevices()
        {
            var result = new List<CameraDeviceInfo>();

            var list = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST
            {
                pDeviceInfo = new IntPtr[MvCodeReader.MV_CODEREADER_MAX_DEVICE_NUM]
            };

            int ret = MvCodeReader.MV_CODEREADER_EnumDevices_NET(
                ref list,
                MvCodeReader.MV_CODEREADER_GIGE_DEVICE);

            if (ret != MvCodeReader.MV_CODEREADER_OK)
                return result;

            for (int i = 0; i < list.nDeviceNum; i++)
            {
                var devInfo = Marshal.PtrToStructure<MvCodeReader.MV_CODEREADER_DEVICE_INFO>(
                    list.pDeviceInfo[i]);

                // stGigEInfo — сырой byte[540], разбираем вручную
                byte[] g = devInfo.SpecialInfo.stGigEInfo;

                result.Add(new CameraDeviceInfo
                {
                    Index = i,
                    IP = ParseIp(g, 8),    // текущий IP
                    StaticIP = ParseIp(g, 196),  // статический IP
                    Gateway = ParseIp(g, 16),   // шлюз
                    Manufacturer = ParseString(g, 20, 32),  // "Hikrobot"
                    Model = ParseString(g, 52, 32),  // "MV-ID3013PM-06M-SENSOTEC"
                    Firmware = ParseString(g, 84, 32),  // "V3.1.4.C 250519"
                    SerialNumber = ParseString(g, 164, 32), // "02DA6864011"
                });
            }

            return result;
        }

        // IP хранится в формате: byte[offset]=last, byte[offset+1]=..., обратный порядок
        private static string ParseIp(byte[] data, int offset)
        {
            if (offset + 3 >= data.Length) return "";
            // Порядок байт: [offset+3].[offset+2].[offset+1].[offset]
            return $"{data[offset + 3]}.{data[offset + 2]}.{data[offset + 1]}.{data[offset]}";
        }

        private static string ParseString(byte[] data, int offset, int maxLen)
        {
            if (offset >= data.Length) return "";
            int end = offset;
            int limit = Math.Min(offset + maxLen, data.Length);
            while (end < limit && data[end] != 0) end++;
            return Encoding.UTF8.GetString(data, offset, end - offset);
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

        /// <summary>
        /// Настройка параметров камеры после подключения.
        /// Вынесено отдельно чтобы не дублировать в Open() и OpenByIp().
        /// </summary>
        private void ConfigureCamera()
        {
            if (_reader == null) return;

            _reader.MV_CODEREADER_SetEnumValueByString_NET("TriggerMode", "On");
            _reader.MV_CODEREADER_SetEnumValueByString_NET("TriggerSource", "Software");
            _reader.MV_CODEREADER_SetEnumValueByString_NET("TriggerActivation", "RisingEdge");
            _reader.MV_CODEREADER_SetEnumValueByString_NET("CodeType", "DataMatrix");

            _reader.MV_CODEREADER_StartGrabbing_NET();
        }
    }

    /// <summary>
    /// Модель данных камеры
    /// </summary>
    public class CameraDeviceInfo
    {
        public int Index { get; set; }
        public string IP { get; set; } = "";
        public string StaticIP { get; set; } = "";
        public string Gateway { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Model { get; set; } = "";
        public string Firmware { get; set; } = "";
        public string SerialNumber { get; set; } = "";

        public override string ToString() => $"[{Index}] {Model} — {IP}";
    }
}
