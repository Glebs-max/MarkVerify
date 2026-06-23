using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class debug
{
    public uint id { get; set; }

    public DateTime timestamp { get; set; }

    public string type { get; set; } = null!;

    public string source { get; set; } = null!;

    public string? info { get; set; }

    public string machine { get; set; } = "Dev";
}
