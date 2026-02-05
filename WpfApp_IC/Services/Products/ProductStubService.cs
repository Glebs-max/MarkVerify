using System.Collections.Generic;

namespace WpfApp_IC.Services.Products
{
    /// <summary>
    /// Заглушка списка продуктов.
    /// </summary>
    public class ProductStubService : IProductService
    {
        public IEnumerable<string> GetAllGtins()
        {
            return new[]
            {
                "04601234567890",
                "04601234567891",
                "04601234567892"
            };
        }
    }
}
