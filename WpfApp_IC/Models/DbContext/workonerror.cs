using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class workonerror
{
    public string? OperatorName { get; set; }

    public DateTime? DateErr { get; set; }

    public string? TextOfExc { get; set; }

    public ulong? MarkGtin { get; set; }

    public byte[]? image { get; set; }

    public virtual gtin? MarkGtinNavigation { get; set; }
}
