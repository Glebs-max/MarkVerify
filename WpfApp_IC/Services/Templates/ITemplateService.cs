namespace WpfApp_IC.Services.Templates
{
    /// <summary>
    /// Сервис шаблонов этикеток.
    /// Позже будет брать шаблоны из БД.
    /// </summary>
    public interface ITemplateService
    {
        string LoadTemplateForGtin(string gtin);
    }
}
