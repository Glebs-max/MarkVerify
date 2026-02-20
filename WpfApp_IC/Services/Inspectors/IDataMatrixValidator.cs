using System.Threading.Tasks;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Интерфейс сервиса проверки DataMatrix-кодов.
    /// Реализация может использовать БД, API или кэш.
    /// </summary>
    public interface IDataMatrixValidator
    {
        /// <summary>
        /// Проверяет DataMatrix-код для указанного GTIN.
        /// </summary>
        Task<ValidationResult> ValidateAsync(string? dm, ulong gtinId);
    }
}
