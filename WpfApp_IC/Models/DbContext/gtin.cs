using System;
using System.Collections.Generic;

namespace WpfApp_IC.Models.DbContext;

public partial class gtin
{
    public ulong GtinId { get; set; }

    public string GtinName { get; set; } = null!;

    public uint ExpirationDate { get; set; }

    public float Weight { get; set; }

    public uint CountAviable { get; set; }

    public string? TemplateLabel { get; set; }

    public int VerEx { get; set; }

    public int Agg { get; set; }

    public int Using { get; set; }

    public string? TemplateFAGG { get; set; }

    public string? TemplateSAGG { get; set; }

    public string? Articul { get; set; }

    /// <summary>
    /// Description:ТН ВЭД 10 Код
    /// </summary>
    public string tnved { get; set; } = null!;

    public string cer_type { get; set; } = null!;

    /// <summary>
    /// Description:Номер разрешительного документа
    /// </summary>
    public string? certificate_number { get; set; }

    /// <summary>
    /// Description:Дата выдачи разрешительного документа
    /// </summary>
    public DateTime? certificate_date { get; set; }

    public string? description { get; set; }

    public string? productCode { get; set; }

    public string? TemplateTAGG { get; set; }

    public double? Tara { get; set; }

    /// <summary>
    /// Description:Номер скважины
    /// </summary>
    public string? well_number { get; set; }

    /// <summary>
    /// Description:Номер лицензии на пользование недрами
    /// </summary>
    public string? licence_number { get; set; }

    /// <summary>
    /// Description:Дата лицензии на пользование недрами
    /// </summary>
    public DateOnly? licence_date { get; set; }

    /// <summary>
    /// Description:Срок годности в часах
    /// </summary>
    public int expiration_hours { get; set; }

    /// <summary>
    /// Description:Срок годности в днях
    /// </summary>
    public int expiration_days { get; set; }

    /// <summary>
    /// Description:Срок годности в месяцах
    /// </summary>
    public int expiration_months { get; set; }

    /// <summary>
    /// Description:Срок годности в годах
    /// </summary>
    public int expiration_years { get; set; }

    public string producer_inn { get; set; } = null!;

    /// <summary>
    /// Description:Тип кода маркировки
    /// </summary>
    public int cis_type { get; set; }

    /// <summary>
    /// Description:ИНН собственника
    /// </summary>
    public string owner_inn { get; set; } = null!;

    /// <summary>
    /// Description:Условия истечения срока годности
    /// </summary>
    public string exp_date_conditions { get; set; } = null!;

    /// <summary>
    /// Description:Переменный срок годности (true/false)
    /// </summary>
    public bool is_variable_exp_date { get; set; }

    /// <summary>
    /// Description:Объем алкоголя (фактическое содержание этилового спирта)
    /// </summary>
    public float alcohol_volume { get; set; }

    /// <summary>
    /// Description:Товарная группа
    /// </summary>
    public int group { get; set; }

    /// <summary>
    /// Description:GTIN алиас
    /// </summary>
    public string? gtin_alias { get; set; }

    /// <summary>
    /// Description:Сырьё подлежит ветеринарному контролю
    /// </summary>
    public bool has_vetis_guid { get; set; }

    /// <summary>
    /// Description:Шаблон кода маркировки
    /// </summary>
    public int template_type { get; set; }

    public virtual ICollection<aggregation> aggregations { get; set; } = new List<aggregation>();

    public virtual ICollection<allowed_document> allowed_documents { get; set; } = new List<allowed_document>();

    public virtual ICollection<gtin_attachment> gtin_attachmentchild_gtinNavigations { get; set; } = new List<gtin_attachment>();

    public virtual ICollection<gtin_attachment> gtin_attachmentparent_gtinNavigations { get; set; } = new List<gtin_attachment>();

    public virtual ICollection<main> mains { get; set; } = new List<main>();

    public virtual ICollection<suborder> suborders { get; set; } = new List<suborder>();

    public virtual ICollection<tmp_main> tmp_mains { get; set; } = new List<tmp_main>();
}
