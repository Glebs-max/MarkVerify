using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class status
{
    public byte StatusId { get; set; }

    public string StatusName { get; set; } = null!;
}
