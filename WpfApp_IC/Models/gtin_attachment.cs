using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models;

public partial class gtin_attachment
{
    /// <summary>
    /// ГТИН набора/групповой упаковки
    /// </summary>
    public ulong parent_gtin { get; set; }

    /// <summary>
    /// ГТИН вложения
    /// </summary>
    public ulong child_gtin { get; set; }

    /// <summary>
    /// Количество КМ данного вложения
    /// </summary>
    public int quantity { get; set; }

    public virtual gtin child_gtinNavigation { get; set; } = null!;

    public virtual gtin parent_gtinNavigation { get; set; } = null!;
}
