using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class buffer
{
    public ulong GtinId { get; set; }

    public string ID { get; set; } = null!;

    public int Remains { get; set; }

    public int All { get; set; }

    public string Status { get; set; } = null!;

    public string LastBlockId { get; set; } = null!;

    public virtual order IDNavigation { get; set; } = null!;
}
