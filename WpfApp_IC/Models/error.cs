using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models;

/// <summary>
/// Ошибки
/// </summary>
public partial class error
{
    public int id { get; set; }

    /// <summary>
    /// Description:Сообщение об ошибке
    /// </summary>
    public string message { get; set; } = null!;

    public virtual ICollection<orders_ex> orders_exes { get; set; } = new List<orders_ex>();

    public virtual ICollection<suborder> suborders { get; set; } = new List<suborder>();
}
