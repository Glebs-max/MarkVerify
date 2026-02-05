using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models;

/// <summary>
/// Заказы
/// </summary>
public partial class orders_ex
{
    public int id { get; set; }

    /// <summary>
    /// Description:UUID заказа
    /// </summary>
    public Guid uuid { get; set; }

    /// <summary>
    /// Description:Дата создания заказа
    /// </summary>
    public DateTime creation_at { get; set; }

    /// <summary>
    /// Description:Дата ожидаемого завершения подготовки КМ
    /// </summary>
    public DateTime complete_at { get; set; }

    /// <summary>
    /// Description:Статус заказа
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// Description:Дата годности заказа
    /// </summary>
    public DateTime expiration_at { get; set; }

    /// <summary>
    /// Description:Валидность заказа
    /// </summary>
    public bool is_valid { get; set; }

    /// <summary>
    /// Description:FK на errors.id
    /// </summary>
    public int? error_id { get; set; }

    public virtual error? error { get; set; }

    public virtual ICollection<suborder> suborders { get; set; } = new List<suborder>();
}
