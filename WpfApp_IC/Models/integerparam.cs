using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models;

public partial class integerparam
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ulong Value { get; set; }
}
