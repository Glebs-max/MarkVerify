using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class aggregation
{
    public ulong id { get; set; }

    public string? code { get; set; }

    public ulong? gtin_id { get; set; }

    public byte aggregation_type { get; set; }

    public sbyte level { get; set; }

    public ulong? parent_id { get; set; }

    public ulong? export_id { get; set; }

    public DateTime? printing_date { get; set; }

    public string? operator_name { get; set; }

    /// <summary>
    /// Description:Выведен ли из оборота
    /// </summary>
    public bool withdrawaled { get; set; }

    public virtual ICollection<aggregation> Inverseparent { get; set; } = new List<aggregation>();

    public virtual gtin? gtin { get; set; }

    public virtual ICollection<main> mains { get; set; } = new List<main>();

    public virtual aggregation? parent { get; set; }

    public virtual ICollection<tmp_main> tmp_mains { get; set; } = new List<tmp_main>();
}
