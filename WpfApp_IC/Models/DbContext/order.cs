using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class order
{
    public string ID { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Status { get; set; } = null!;

    public string ProdType { get; set; } = null!;

    public DateTime? DateLastUpdate { get; set; }

    public string? Note { get; set; }
}
