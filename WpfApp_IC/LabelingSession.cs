using Observable;
using System.Collections.ObjectModel;
using WpfApp_IC.Models;

namespace WpfApp_IC
{
    public class LabelingSession : ObservableObject
    {
        private string _machineName = "Машина 1";
        private gtin _gtin = new();
        private printer_task _currentTask = new();
        private int _verified = 0, _rejected = 0, _count = 0;

        public string MachineName
        {
            get => _machineName;
            set => Set(ref _machineName, value);
        }
        public gtin GTIN
        {
            get => _gtin;
            set => Set(ref _gtin, value);
        }
        public printer_task CurrentTask
        {
            get => _currentTask;
            set => Set(ref _currentTask, value);
        }
        public int Verified
        {
            get => _verified;
            set => Set(ref _verified, value);
        }
        public int Rejected
        {
            get => _rejected;
            set => Set(ref _rejected, value);
        }
        public int Count
        {
            get => _count;
            set => Set(ref _count, value);
        }
        public ObservableCollection<string> VerifiedCodes = [
            "0104630007402529215Q7a-En&yYq0V",
            "0104630007402529215Q8Dr-fhifehs",
            "0104630007402529215Q8HMQZHNYoaJ",
            "0104630007402529215Q8N-ONZNac!L",
            "0104630007402529215sWA:Xm1XTp2_"
        ];

        public void Reset()
        {
            Verified = 0;
            Rejected = 0;
            Count = 0;
            GTIN = new();
            CurrentTask = new();
            VerifiedCodes.Clear();
        }
    }
}
