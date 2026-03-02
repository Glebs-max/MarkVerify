namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Результат проверки DataMatrix-кода.
    /// Содержит флаг успешности и описание ошибки (если есть).
    /// </summary>
    public sealed class ValidationResult(bool isOk, string? errorCode = null, string? errorMessage = null)
    {
        /// <summary>
        /// Признак успешной проверки.
        /// </summary>
        public bool IsOk { get; } = isOk;

        /// <summary>
        /// Код ошибки (например: NO_READ, NOT_FOUND).
        /// </summary>
        public string? ErrorCode { get; } = errorCode;

        /// <summary>
        /// Человекочитаемое описание ошибки.
        /// </summary>
        public string? ErrorMessage { get; } = errorMessage;

        public static ValidationResult Ok() => new(true);
        public static ValidationResult NoRead() => new(false, "NO_READ", "DataMatrix не считан");
        public static ValidationResult NotFound() => new(false, "NOT_FOUND", "Код не найден в БД");
    }
}