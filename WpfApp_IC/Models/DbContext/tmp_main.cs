using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class tmp_main
{
    public string Code { get; set; } = null!;

    public byte? StatusId { get; set; }

    public DateTime? DatePrint { get; set; }

    public DateTime? DateImport { get; set; }

    public DateTime? DateVerify { get; set; }

    public ulong GtinId { get; set; }

    public DateTime? DateExport { get; set; }

    public uint? PartyId { get; set; }

    public ulong? FAgg { get; set; }

    public DateTime? FAggDatePrint { get; set; }

    public ulong? SAgg { get; set; }

    public DateTime? SAggDatePrint { get; set; }

    public int? VerEx { get; set; }

    public int? VerFirstAgg { get; set; }

    public int? VerSecondAgg { get; set; }

    public int? PutIntoUsing { get; set; }

    public string? OperatorName { get; set; }

    public uint? Typograth { get; set; }

    public string? OrderID { get; set; }

    public ulong? exportId { get; set; }

    public DateTime? TAggDatePrint { get; set; }

    public ulong? TAgg { get; set; }

    public int? VerThirdAgg { get; set; }

    public int? ATK_Flag { get; set; }

    public int? Mass { get; set; }

    public ulong? AggregationCode { get; set; }

    public virtual aggregation? AggregationCodeNavigation { get; set; }

    public virtual gtin Gtin { get; set; } = null!;
}
