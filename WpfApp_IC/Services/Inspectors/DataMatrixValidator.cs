using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WpfApp_IC.Data;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Реализация проверки DataMatrix-кодов через EF Core.
    /// </summary>
    public class DataMatrixValidator : IDataMatrixValidator
    {
        private readonly AppDbContext _db;

        public DataMatrixValidator(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Проверяет код в таблице printer_bases.
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(string? dm, ulong gtinId)
        {
            if (string.IsNullOrWhiteSpace(dm))
                return ValidationResult.NoRead();

            var code = await _db.printer_bases
                .FirstOrDefaultAsync(c => c.Code == dm && c.GtinId == gtinId && c.StatusId == 1);

            if (code == null)
                return ValidationResult.NotFound();

            code.StatusId = 77;
            await _db.SaveChangesAsync();

            return ValidationResult.Ok();
        }
    }
}
