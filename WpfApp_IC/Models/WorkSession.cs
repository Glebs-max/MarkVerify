using Observable;
using WpfApp_IC.Models.DbContext;

namespace WpfApp_IC.Models
{
    public enum WorkMode
    {
        Default,
        NoPrint,
        SkipDuplicates
    }
    public enum ScannerMode
    {
        Verify,
        Reject
    }

    public class WorkSession : ObservableObject
    {
        private string _machineName = "";
        public string MachineName
        {
            get => _machineName;
            set => Set(ref _machineName, value);
        }

        private gtin? _gtin;
        public gtin? GTIN
        {
            get => _gtin;
            set => Set(ref _gtin, value);
        }

        private printer_task? _currentTask;
        public printer_task? CurrentTask
        {
            get => _currentTask;
            set => Set(ref _currentTask, value);
        }

        private int _verified = 0;
        public int Verified
        {
            get => _verified;
            set => Set(ref _verified, value);
        }

        private int _rejected = 0;
        public int Rejected
        {
            get => _rejected;
            set => Set(ref _rejected, value);
        }

        private int _printCount = 0;
        public int PrintCount
        {
            get => _printCount;
            set => Set(ref _printCount, value);
        }

        private WorkMode _workMode = WorkMode.Default;
        public WorkMode WorkMode
        {
            get => _workMode;
            set => Set(ref _workMode, value);
        }

        private ScannerMode _scannerMode = ScannerMode.Verify;
        public ScannerMode ScannerMode
        {
            get => _scannerMode;
            set => Set(ref _scannerMode, value);
        }

        public Dictionary<string, bool> Codes = [];

        public void Reset()
        {
            _verified = _rejected = _printCount = 0;
            _workMode = WorkMode.Default;
            _scannerMode = ScannerMode.Verify;
            _gtin = null;
            _currentTask = null;
            Codes.Clear();
        }
    }
}
