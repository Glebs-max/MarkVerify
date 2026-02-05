using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models;

/// <summary>
/// Подзаказы
/// </summary>
public partial class suborder
{
    public int id { get; set; }

    /// <summary>
    /// Description:FK на orders_ex.id
    /// </summary>
    public int order_id { get; set; }

    /// <summary>
    /// Description:Статус подзаказа
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// Description:Кол-во заказанных КМ
    /// </summary>
    public int total_codes { get; set; }

    /// <summary>
    /// Description:Валидность подзаказа
    /// </summary>
    public bool is_valid { get; set; }

    /// <summary>
    /// Description:FK на errors.id
    /// </summary>
    public int? error_id { get; set; }

    /// <summary>
    /// FK на gtin.GtinId
    /// </summary>
    public ulong gtin_id { get; set; }

    public virtual ICollection<block> blocks { get; set; } = new List<block>();

    public virtual error? error { get; set; }

    public virtual gtin gtin { get; set; } = null!;

    public virtual orders_ex order { get; set; } = null!;
}
