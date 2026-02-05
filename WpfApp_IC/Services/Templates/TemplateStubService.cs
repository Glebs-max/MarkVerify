using System.IO;

namespace WpfApp_IC.Services.Templates
{
    /// <summary>
    /// Заглушка шаблонов.
    /// Пока грузим шаблон из файла.
    /// </summary>
    public class TemplateStubService : ITemplateService
    {
        public string LoadTemplateForGtin(string gtin)
        {
            // Позже — загрузка из БД
            return File.ReadAllText("Assets/template.txt");
        }
    }
}
