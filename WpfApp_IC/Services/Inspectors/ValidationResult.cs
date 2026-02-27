    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace WpfApp_IC.Services.Inspectors
    {
        /// <summary>
        /// Результат проверки DataMatrix-кода.
        /// Содержит флаг успешности и описание ошибки (если есть).
        /// </summary>
        public sealed class ValidationResult
        {
            /// <summary>
            /// Признак успешной проверки.
            /// </summary>
            public bool IsOk { get; }

            /// <summary>
            /// Код ошибки (например: NO_READ, NOT_FOUND).
            /// </summary>
            public string? ErrorCode { get; }

            /// <summary>
            /// Человекочитаемое описание ошибки.
            /// </summary>
            public string? ErrorMessage { get; }

            public ValidationResult(bool isOk, string? errorCode = null, string? errorMessage = null)
            {
                IsOk = isOk;
                ErrorCode = errorCode;
                ErrorMessage = errorMessage;
            }

            public static ValidationResult Ok() => new(true);
            public static ValidationResult NoRead() => new(false, "NO_READ", "DataMatrix не считан");
            public static ValidationResult NotFound() => new(false, "NOT_FOUND", "Код не найден в БД");
        }
    }

