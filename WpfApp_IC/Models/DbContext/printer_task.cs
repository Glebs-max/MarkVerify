using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

/// <summary>
/// Задачи печати
/// </summary>
public partial class printer_task
{
    /// <summary>
    /// Description:id
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// Description:Дата создания задачи
    /// </summary>
    public DateTime created_at { get; set; }

    /// <summary>
    /// Description:Дата крайнего использования задачи
    /// </summary>
    public DateTime last_used_at { get; set; }

    public virtual ICollection<printer_base> printer_bases { get; set; } = new List<printer_base>();
}
