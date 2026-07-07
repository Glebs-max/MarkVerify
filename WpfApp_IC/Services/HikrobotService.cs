using MvCodeReaderSDKNet;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using WpfApp_IC.Models.Devices;

namespace WpfApp_IC.Services
{
    public class HikrobotService
    {
        public static List<HikrobotDeviceInfo> ScanAvailableDevices()
        {
            List<HikrobotDeviceInfo> devices = [];
            MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST list = new()
            {
                pDeviceInfo = new IntPtr[MvCodeReader.MV_CODEREADER_MAX_DEVICE_NUM]
            };

            try
            {
                if (MvCodeReader.MV_CODEREADER_EnumDevices_NET(ref list, MvCodeReader.MV_CODEREADER_GIGE_DEVICE) != MvCodeReader.MV_CODEREADER_OK)
                    return devices;
            }
            catch
            {
                return devices;
            }

            for (int i = 0; i < list.nDeviceNum; i++)
                devices.Add(new(Marshal.PtrToStructure<MvCodeReader.MV_CODEREADER_DEVICE_INFO>(list.pDeviceInfo[i])));

            return devices;
        }
        public static string? ExtractCode(byte[]? data)
        {
            if (data == null)
                return null;

            int zero = Array.IndexOf(data, (byte)0, 8);

            if(zero < 1) 
                return null;

            return Encoding.UTF8.GetString(data, 8, zero - 8);
        }
        public static BitmapSource DecodeFrame(nint pData, int frameLength)
        {
            byte[] jpeg = new byte[frameLength];
            Marshal.Copy(pData, jpeg, 0, jpeg.Length);

            using MemoryStream stream = new(jpeg);
            JpegBitmapDecoder decoder = new(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            BitmapFrame bmp = decoder.Frames[0];

            bmp.Freeze();
            return bmp;
        }
    }
}
