using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class allowed_document
{
    public int id { get; set; }

    public string number_doc { get; set; } = null!;

    public DateOnly date_doc { get; set; }

    public string type_doc { get; set; } = null!;

    public string well_number { get; set; } = null!;

    public ulong GtinId { get; set; }

    public virtual gtin Gtin { get; set; } = null!;
}
