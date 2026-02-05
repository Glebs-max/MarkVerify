using System.Collections.Generic;

namespace WpfApp_IC.Services.Products
{
    /// <summary>
    /// Сервис продуктов.
    /// Позже будет работать с БД.
    /// </summary>
    public interface IProductService
    {
        IEnumerable<string> GetAllGtins();
    }
}
