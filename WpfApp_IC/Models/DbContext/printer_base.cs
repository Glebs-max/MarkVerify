using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class printer_base
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

    /// <summary>
    /// Description:Идентификатор связанной задачи печати
    /// </summary>
    public long? task_id { get; set; }

    /// <summary>
    /// Description:Номер КМ в рамках задачи печати
    /// </summary>
    public long? code_number { get; set; }

    public virtual printer_task? task { get; set; }
}
