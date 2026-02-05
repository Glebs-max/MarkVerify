using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models;

/// <summary>
/// Пакеты КМ
/// </summary>
public partial class block
{
    public int id { get; set; }

    /// <summary>
    /// Description:FK на suborders.id
    /// </summary>
    public int suborder_id { get; set; }

    /// <summary>
    /// Description:ID пакета с кодами
    /// </summary>
    public Guid uuid { get; set; }

    /// <summary>
    /// Description:Дата создания пакета КМ
    /// </summary>
    public DateTime creation_at { get; set; }

    /// <summary>
    /// Description:Количество КМ в пакете КМ.
    /// </summary>
    public int quantity { get; set; }

    public virtual suborder suborder { get; set; } = null!;
}
