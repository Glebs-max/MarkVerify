using MvCodeReaderSDKNet;
using Observable;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp_IC.Services;

namespace WpfApp_IC.Models.Devices
{
    public readonly struct HikrobotDeviceInfo(MvCodeReader.MV_CODEREADER_DEVICE_INFO info)
    {
        public MvCodeReader.MV_CODEREADER_DEVICE_INFO DeviceInfo { get; } = info;
        public string IP { get; } = ParseIp(info.SpecialInfo.stGigEInfo, 8);
        public string StaticIP { get; } = ParseIp(info.SpecialInfo.stGigEInfo, 196);
        public string Gateway { get; } = ParseIp(info.SpecialInfo.stGigEInfo, 16);
        public string Manufacturer { get; } = ParseString(info.SpecialInfo.stGigEInfo, 20, 32);
        public string Model { get; } = ParseString(info.SpecialInfo.stGigEInfo, 52, 32);
        public string Firmware { get; } = ParseString(info.SpecialInfo.stGigEInfo, 84, 32);
        public string SerialNumber { get; } = ParseString(info.SpecialInfo.stGigEInfo, 164, 32);

        public override string ToString() => $"[{IP}] {Manufacturer} - {Model}";

        private static string ParseIp(byte[] data, int offset)
        {
            if (offset + 3 >= data.Length)
                return "";

            return $"{data[offset + 3]}.{data[offset + 2]}.{data[offset + 1]}.{data[offset]}";
        }
        private static string ParseString(byte[] data, int offset, int maxLen)
        {
            if (offset >= data.Length)
                return "";

            int end = offset, limit = Math.Min(offset + maxLen, data.Length);

            while (end < limit && data[end] != 0)
                end++;

            return Encoding.UTF8.GetString(data, offset, end - offset);
        }
    }

    public class HikrobotCamera : ObservableObject
    {
        private MvCodeReader? _mvCodeReader;
        private bool _triggered;

        public event Action<BitmapSource>? FrameReceived;

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        private string _ip = "";
        public string IP
        {
            get => _ip;
            set => Set(ref _ip, value);
        }

        private uint _frameTimeout = 200;
        public uint FrameTimeout
        {
            get => _frameTimeout;
            set => Set(ref _frameTimeout, value);
        }

        public void Connect()
        {
            if (Connected)
                return;

            try
            {
                List<HikrobotDeviceInfo> devices = HikrobotService.ScanAvailableDevices();

                foreach (HikrobotDeviceInfo device in devices)
                {
                    if (device.IP == IP)
                    {
                        MvCodeReader.MV_CODEREADER_DEVICE_INFO deviceInfo = device.DeviceInfo;

                        _mvCodeReader = new();
                        _mvCodeReader.MV_CODEREADER_CreateHandle_NET(ref deviceInfo);
                        _mvCodeReader.MV_CODEREADER_OpenDevice_NET();
                        _mvCodeReader.MV_CODEREADER_SetEnumValueByString_NET("TriggerMode", "On");
                        _mvCodeReader.MV_CODEREADER_SetEnumValueByString_NET("TriggerSource", "Software");
                        _mvCodeReader.MV_CODEREADER_SetEnumValueByString_NET("TriggerActivation", "RisingEdge");
                        _mvCodeReader.MV_CODEREADER_SetEnumValueByString_NET("CodeType", "DataMatrix");
                        _mvCodeReader.MV_CODEREADER_StartGrabbing_NET();

                        Connected = true;

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка при попытке подключения к камере:\n\n{e.Message}\n\n{e.InnerException}");
            }
        }
        public void Disconnect()
        {
            if (!Connected || _mvCodeReader == null)
                return;

            try
            {
                _mvCodeReader.MV_CODEREADER_StopGrabbing_NET();
                _mvCodeReader.MV_CODEREADER_CloseDevice_NET();
                _mvCodeReader.MV_CODEREADER_DestroyHandle_NET();

                _mvCodeReader = null;
                Connected = false;
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка при попытке отключения от камеры:\n\n{e.Message}\n\n{e.InnerException}");
            }
        }
        public string? TriggerSnapshot()
        {
            if (!Connected || _mvCodeReader == null || _triggered)
                throw new Exception("Не удалось выполнить триггер камеры");

            _triggered = true;

            _mvCodeReader.MV_CODEREADER_SetCommandValue_NET("TriggerSoftware");

            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO info = new();
            nint pData = nint.Zero, pInfo = Marshal.AllocHGlobal(Marshal.SizeOf(info));

            if (_mvCodeReader.MV_CODEREADER_GetOneFrameTimeout_NET(ref pData, pInfo, _frameTimeout) != MvCodeReader.MV_CODEREADER_OK)
                throw new Exception("Не удалось получить изображение с камеры");

            _triggered = false;
            
            info = Marshal.PtrToStructure<MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO>(pInfo);
            Marshal.FreeHGlobal(pInfo);

            FrameReceived?.Invoke(HikrobotService.DecodeFrame(pData, (int)info.nFrameLen));
            return HikrobotService.ExtractCode(info.chResult);
        }
    }
}
