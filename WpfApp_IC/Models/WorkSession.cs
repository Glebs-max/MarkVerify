using Observable;
using WpfApp_IC.Models.DbContext;

namespace WpfApp_IC.Models
{
    public enum WorkMode
    {
        Default,
        SkipDuplicates
    }
    public enum ScannerMode
    {
        Verify,
        Reject
    }

    public class WorkSession : ObservableObject
    {
        private string? _machineName;
        private gtin _gtin = new();
        private printer_task _currentTask = new();
        private int _verified = 0, _rejected = 0, _printCount = 0, _totalCount = 0;
        private WorkMode _workMode;
        private ScannerMode _scannerMode;

        public string? MachineName
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
        public int PrintCount
        {
            get => _printCount;
            set => Set(ref _printCount, value);
        }
        public int TotalCount
        {
            get => _totalCount;
            set => Set(ref _totalCount, value);
        }
        public WorkMode WorkMode
        {
            get => _workMode;
            set => Set(ref _workMode, value);
        }
        public ScannerMode ScannerMode
        {
            get => _scannerMode;
            set => Set(ref _scannerMode, value);
        }
        public Dictionary<string, bool> Codes = [];

        public void Reset()
        {
            Verified = 0;
            Rejected = 0;
            PrintCount = 0;
            TotalCount = 0;
            WorkMode = WorkMode.Default;
            ScannerMode = ScannerMode.Verify;
            GTIN = new();
            CurrentTask = new();
            Codes.Clear();
        }
    }
}
