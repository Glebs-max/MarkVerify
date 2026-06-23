using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class report
{
    public string ID { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public string Status { get; set; } = null!;

    public string? TypeOperation { get; set; }

    public string Note { get; set; } = null!;

    public string ProdType { get; set; } = null!;

    /// <summary>
    /// Description:Последнее время запроса статуса.
    /// </summary>
    public DateTime? last_request_status { get; set; }

    /// <summary>
    /// Description:Путь к файлу с отчетом
    /// </summary>
    public string file_name { get; set; } = null!;
}
