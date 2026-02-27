using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WpfApp_IC.Data;
using WpfApp_IC.Models;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Реализация проверки DataMatrix-кодов через EF Core.
    /// </summary>
    public class DataMatrixValidator(IDbContextFactory<AppDbContext> dbContextFactory) : IDataMatrixValidator
    {
        /// <summary>
        /// Проверяет код в таблице printer_bases.
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(string? dm, ulong gtinId)
        {
            if (string.IsNullOrWhiteSpace(dm))
                return ValidationResult.NoRead();

            await using var db = await dbContextFactory.CreateDbContextAsync();
            printer_base? code = await db.printer_bases.FirstOrDefaultAsync(c => c.Code == dm && c.GtinId == gtinId && c.StatusId == 1);

            if (code == null)
                return ValidationResult.NotFound();

            code.StatusId = 2;
            await db.SaveChangesAsync();

            return ValidationResult.Ok();
        }
    }
}
