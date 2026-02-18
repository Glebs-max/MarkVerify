using DataMatrix.net;
using LabelDesigner.DesignerItemVisuals;
using LabelDesigner.Services;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace LabelDesigner.Models
{
    public class BarcodeField : Field
    {
        private string _barcodeData = "Default Data";
        private readonly DmtxImageEncoder _dataMatrix = new();
        private readonly DmtxImageEncoderOptions _options = new()
        {
            MarginSize = 0,
            ModuleSize = (int)UnitsService.FromMM(5),
            Scheme = DmtxScheme.DmtxSchemeAsciiGS1,
            SizeIdx = DmtxSymbolSize.DmtxSymbolSquareAuto
        };

        public BarcodeField()
        {
            DataType = DataType.Database;
        }

        public new ImageVisual Visual => (ImageVisual)base.Visual;

        public override string FieldType => "Штрихкод";
        public string BarcodeData
        {
            get => _barcodeData;
            set => Set(ref _barcodeData, value, () => Visual.Content.Source = BarcodeToBitmap());
        }

        protected override ImageVisual InitializeVisual() => new(BarcodeToBitmap());
        protected override void InvertField()
        {
            (_options.ForeColor, _options.BackColor) = (_options.BackColor, _options.ForeColor);
            Visual.Content.Source = BarcodeToBitmap();
        }
        private BitmapSource BarcodeToBitmap() => Imaging.CreateBitmapSourceFromHBitmap(_dataMatrix.EncodeImage(string.IsNullOrEmpty(BarcodeData) ? "Default Data" : BarcodeData, _options).GetHbitmap(), nint.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }
}
