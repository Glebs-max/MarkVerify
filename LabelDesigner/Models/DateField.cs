using LabelDesigner.Services;
using Observable;

namespace LabelDesigner.Models
{
    public class DateCalculation() : ObservableObject
    {
        private int _dayOffset, _monthOffset, _yearOffset;

        public int DayOffset
        {
            get => _dayOffset;
            set => Set(ref _dayOffset, value);
        }
        public int MonthOffset
        {
            get => _monthOffset;
            set => Set(ref _monthOffset, value);
        }
        public int YearOffset
        {
            get => _yearOffset;
            set => Set(ref _yearOffset, value);
        }

        public DateTime ResultDate => DateTime.Now.AddYears(YearOffset).AddMonths(MonthOffset).AddDays(DayOffset);
    }

    public class DateField : TextField
    {
        private DateTime _date = DateTime.Now;
        private string _dateFormat = $"dd$MM$yyyy";
        private char _separator = '.';
        private DateCalculation? _dateCalculation;

        public DateField()
        {
            DataType = DataType.Current;

            DisplayDate();

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(DataType):
                        if (DataType == DataType.Calculated)
                        {
                            DateCalculation = new();
                            DateCalculation.PropertyChanged += (s, e) => Date = DateCalculation.ResultDate;
                        }
                        else
                            DateCalculation = null;
                        break;
                }
            };
            TimerService.DesignerCanvasTimer.Tick += (s, e) =>
            {
                switch (DataType)
                {
                    case DataType.Current:
                        Date = DateTime.Now;
                        break;
                    case DataType.Calculated:
                        Date = DateCalculation?.ResultDate ?? DateTime.Now;
                        break;

                }     
            };
        }

        public override string FieldType => "Дата";
        public DateTime Date
        {
            get => _date;
            set => Set(ref _date, value, DisplayDate);
        }
        public string DateFormat
        {
            get => _dateFormat;
            set => Set(ref _dateFormat, value, DisplayDate);
        }
        public char Separator
        {
            get => _separator;
            set => Set(ref _separator, value, DisplayDate);
        }
        public DateCalculation? DateCalculation
        {
            get => _dateCalculation;
            set => Set(ref _dateCalculation, value, () => Date = value?.ResultDate ?? DateTime.Now);
        }

        private void DisplayDate()
        {
            if (_separator == '\\')
                Text = _date.Date.ToString(_dateFormat.Replace("$", "\\\\"));
            else if (_separator == '/')
                Text = _date.Date.ToString(_dateFormat.Replace("$", "\\/"));
            else if (_separator == '$')
                Text = _date.Date.ToString(_dateFormat.Replace("$", ""));
            else
                Text = _date.Date.ToString(_dateFormat.Replace('$', _separator));
        }
    }
}
