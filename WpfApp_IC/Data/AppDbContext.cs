using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using WpfApp_IC.Models.DbContext;

namespace WpfApp_IC.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext() {}

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public virtual DbSet<aggregation> aggregations { get; set; }

    public virtual DbSet<allowed_document> allowed_documents { get; set; }

    public virtual DbSet<block> blocks { get; set; }

    public virtual DbSet<buffer> buffers { get; set; }

    public virtual DbSet<debug> debugs { get; set; }

    public virtual DbSet<error> errors { get; set; }

    public virtual DbSet<gtin> gtins { get; set; }

    public virtual DbSet<gtin_attachment> gtin_attachments { get; set; }

    public virtual DbSet<integerparam> integerparams { get; set; }

    public virtual DbSet<main> mains { get; set; }

    public virtual DbSet<order> orders { get; set; }

    public virtual DbSet<orders_ex> orders_exes { get; set; }

    public virtual DbSet<param> _params { get; set; }

    public virtual DbSet<printer_base> printer_bases { get; set; }

    public virtual DbSet<printer_task> printer_tasks { get; set; }

    public virtual DbSet<report> reports { get; set; }

    public virtual DbSet<status> statuses { get; set; }

    public virtual DbSet<suborder> suborders { get; set; }

    public virtual DbSet<tmp_main> tmp_mains { get; set; }

    public virtual DbSet<versioninfo> versioninfos { get; set; }

    public virtual DbSet<workonerror> workonerrors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<aggregation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("aggregation");

            entity.HasIndex(e => e.parent_id, "FK_aggregation_aggregation");

            entity.HasIndex(e => e.gtin_id, "FK_aggregation_gtin");

            entity.HasIndex(e => e.code, "code");

            entity.HasIndex(e => e.export_id, "export_id");

            entity.HasIndex(e => e.operator_name, "operator_name");

            entity.Property(e => e.id).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.aggregation_type).HasColumnType("tinyint(3) unsigned");
            entity.Property(e => e.code)
                .HasMaxLength(100)
                .UseCollation("utf8mb4_bin");
            entity.Property(e => e.export_id).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.gtin_id).HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.level).HasColumnType("tinyint(3)");
            entity.Property(e => e.operator_name).HasMaxLength(50);
            entity.Property(e => e.parent_id).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.printing_date).HasColumnType("datetime");
            entity.Property(e => e.withdrawaled).HasComment("Description:Выведен ли из оборота");

            entity.HasOne(d => d.gtin).WithMany(p => p.aggregations)
                .HasForeignKey(d => d.gtin_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_aggregation_gtin");

            entity.HasOne(d => d.parent).WithMany(p => p.Inverseparent)
                .HasForeignKey(d => d.parent_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_aggregation_aggregation");
        });

        modelBuilder.Entity<allowed_document>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.GtinId, "GtinId");

            entity.Property(e => e.id).HasColumnType("int(11)");
            entity.Property(e => e.GtinId).HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.date_doc).HasDefaultValueSql("current_timestamp()");
            entity.Property(e => e.number_doc)
                .HasMaxLength(100)
                .HasDefaultValueSql("'-'");
            entity.Property(e => e.type_doc)
                .HasMaxLength(100)
                .HasDefaultValueSql("'-'");
            entity.Property(e => e.well_number)
                .HasMaxLength(100)
                .HasDefaultValueSql("'-'");

            entity.HasOne(d => d.Gtin).WithMany(p => p.allowed_documents)
                .HasForeignKey(d => d.GtinId)
                .HasConstraintName("allowed_documents_ibfk_1");
        });

        modelBuilder.Entity<block>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Пакеты КМ"));

            entity.HasIndex(e => e.suborder_id, "FK_blocks_suborder_ex");

            entity.Property(e => e.id).HasColumnType("int(11)");
            entity.Property(e => e.creation_at)
                .HasDefaultValueSql("utc_timestamp()")
                .HasComment("Description:Дата создания пакета КМ")
                .HasColumnType("datetime");
            entity.Property(e => e.quantity)
                .HasComment("Description:Количество КМ в пакете КМ.")
                .HasColumnType("int(11)");
            entity.Property(e => e.suborder_id)
                .HasComment("Description:FK на suborders.id")
                .HasColumnType("int(11)");
            entity.Property(e => e.uuid).HasComment("Description:ID пакета с кодами");

            entity.HasOne(d => d.suborder).WithMany(p => p.blocks)
                .HasForeignKey(d => d.suborder_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_blocks_suborder_ex");
        });

        modelBuilder.Entity<buffer>(entity =>
        {
            entity
                .HasNoKey()
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ID, "iOrder");

            entity.Property(e => e.All).HasColumnType("int(11)");
            entity.Property(e => e.GtinId).HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.ID).HasMaxLength(256);
            entity.Property(e => e.LastBlockId)
                .HasMaxLength(100)
                .HasDefaultValueSql("'0'");
            entity.Property(e => e.Remains).HasColumnType("int(11)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.IDNavigation).WithMany()
                .HasForeignKey(d => d.ID)
                .HasConstraintName("buffers_ibfk_1");
        });

        modelBuilder.Entity<debug>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("debug");

            entity.Property(e => e.id)
                .HasColumnType("int(10) unsigned");
            entity.Property(e => e.info)
                .HasColumnType("text");
            entity.Property(e => e.source)
                .HasMaxLength(64);
            entity.Property(e => e.timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.type)
                .HasMaxLength(64);
            entity.Property(e => e.machine)
                .HasMaxLength(64);
        });

        modelBuilder.Entity<error>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Ошибки"));

            entity.Property(e => e.id).HasColumnType("int(11)");
            entity.Property(e => e.message)
                .HasMaxLength(255)
                .HasComment("Description:Сообщение об ошибке")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<gtin>(entity =>
        {
            entity.HasKey(e => e.GtinId).HasName("PRIMARY");

            entity.ToTable("gtin");

            entity.Property(e => e.GtinId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.Agg).HasColumnType("int(11)");
            entity.Property(e => e.Articul)
                .HasMaxLength(100)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CountAviable).HasColumnType("int(10) unsigned");
            entity.Property(e => e.ExpirationDate).HasColumnType("int(10) unsigned");
            entity.Property(e => e.GtinName).HasMaxLength(256);
            entity.Property(e => e.Tara).HasDefaultValueSql("'0'");
            entity.Property(e => e.TemplateFAGG).HasMaxLength(256);
            entity.Property(e => e.TemplateLabel).HasMaxLength(256);
            entity.Property(e => e.TemplateSAGG).HasMaxLength(256);
            entity.Property(e => e.TemplateTAGG).HasMaxLength(256);
            entity.Property(e => e.Using).HasColumnType("int(11)");
            entity.Property(e => e.VerEx).HasColumnType("int(11)");
            entity.Property(e => e.Weight).HasColumnType("float unsigned");
            entity.Property(e => e.alcohol_volume).HasComment("Description:Объем алкоголя (фактическое содержание этилового спирта)");
            entity.Property(e => e.cer_type)
                .HasMaxLength(64)
                .HasDefaultValueSql("'CONFORMITY_DECLARATION'");
            entity.Property(e => e.certificate_date)
                .HasComment("Description:Дата выдачи разрешительного документа")
                .HasColumnType("datetime");
            entity.Property(e => e.certificate_number)
                .HasMaxLength(256)
                .HasComment("Description:Номер разрешительного документа")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.cis_type)
                .HasComment("Description:Тип кода маркировки")
                .HasColumnType("int(11)");
            entity.Property(e => e.description)
                .HasMaxLength(1024)
                .HasDefaultValueSql("''' '''");
            entity.Property(e => e.exp_date_conditions)
                .HasDefaultValueSql("'[]'")
                .HasComment("Description:Условия истечения срока годности")
                .HasColumnType("json");
            entity.Property(e => e.expiration_days)
                .HasComment("Description:Срок годности в днях")
                .HasColumnType("int(11)");
            entity.Property(e => e.expiration_hours)
                .HasComment("Description:Срок годности в часах")
                .HasColumnType("int(11)");
            entity.Property(e => e.expiration_months)
                .HasComment("Description:Срок годности в месяцах")
                .HasColumnType("int(11)");
            entity.Property(e => e.expiration_years)
                .HasComment("Description:Срок годности в годах")
                .HasColumnType("int(11)");
            entity.Property(e => e.group)
                .HasComment("Description:Товарная группа")
                .HasColumnType("int(11)");
            entity.Property(e => e.gtin_alias)
                .HasMaxLength(100)
                .HasComment("Description:GTIN алиас")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.has_vetis_guid).HasComment("Description:Сырьё подлежит ветеринарному контролю");
            entity.Property(e => e.is_variable_exp_date).HasComment("Description:Переменный срок годности (true/false)");
            entity.Property(e => e.licence_date).HasComment("Description:Дата лицензии на пользование недрами");
            entity.Property(e => e.licence_number)
                .HasMaxLength(256)
                .HasComment("Description:Номер лицензии на пользование недрами")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.owner_inn)
                .HasMaxLength(12)
                .HasComment("Description:ИНН собственника")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.producer_inn)
                .HasMaxLength(12)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.productCode).HasMaxLength(256);
            entity.Property(e => e.template_type)
                .HasComment("Description:Шаблон кода маркировки")
                .HasColumnType("int(11)");
            entity.Property(e => e.tnved)
                .HasMaxLength(10)
                .HasDefaultValueSql("''")
                .HasComment("Description:ТН ВЭД 10 Код")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.well_number)
                .HasMaxLength(512)
                .HasComment("Description:Номер скважины")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<gtin_attachment>(entity =>
        {
            entity.HasKey(e => new { e.parent_gtin, e.child_gtin })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.HasIndex(e => e.child_gtin, "child_gtin");

            entity.Property(e => e.parent_gtin)
                .HasComment("ГТИН набора/групповой упаковки")
                .HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.child_gtin)
                .HasComment("ГТИН вложения")
                .HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.quantity)
                .HasComment("Количество КМ данного вложения")
                .HasColumnType("int(11)");

            entity.HasOne(d => d.child_gtinNavigation).WithMany(p => p.gtin_attachmentchild_gtinNavigations)
                .HasForeignKey(d => d.child_gtin)
                .HasConstraintName("gtin_attachments_ibfk_2");

            entity.HasOne(d => d.parent_gtinNavigation).WithMany(p => p.gtin_attachmentparent_gtinNavigations)
                .HasForeignKey(d => d.parent_gtin)
                .HasConstraintName("gtin_attachments_ibfk_1");
        });

        modelBuilder.Entity<integerparam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Name).HasMaxLength(45);
            entity.Property(e => e.Value).HasColumnType("bigint(24) unsigned");
        });

        modelBuilder.Entity<main>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("main");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.HasIndex(e => e.FAgg, "FAgg");

            entity.HasIndex(e => e.AggregationCode, "FK_main_aggregation");

            entity.HasIndex(e => new { e.GtinId, e.StatusId }, "GtinId");

            entity.HasIndex(e => e.SAgg, "SAgg");

            entity.HasIndex(e => new { e.StatusId, e.Code }, "StatusId");

            entity.HasIndex(e => e.OrderID, "iOrder");

            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .UseCollation("utf8mb4_bin");
            entity.Property(e => e.ATK_Flag)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.AggregationCode).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.DateExport).HasColumnType("datetime");
            entity.Property(e => e.DateImport)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.DatePrint).HasColumnType("datetime");
            entity.Property(e => e.DateVerify).HasColumnType("datetime");
            entity.Property(e => e.FAgg).HasMaxLength(20);
            entity.Property(e => e.FAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.GtinId).HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.Mass).HasColumnType("int(6)");
            entity.Property(e => e.OperatorName)
                .HasMaxLength(50)
                .HasDefaultValueSql("'no_name'");
            entity.Property(e => e.OrderID).HasMaxLength(100);
            entity.Property(e => e.PartyId).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PutIntoUsing)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.SAgg).HasMaxLength(20);
            entity.Property(e => e.SAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'0'")
                .HasColumnType("tinyint(3) unsigned");
            entity.Property(e => e.TAgg).HasMaxLength(20);
            entity.Property(e => e.TAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.Typograth)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11) unsigned");
            entity.Property(e => e.VerEx)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerFirstAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerSecondAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerThirdAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(8)");
            entity.Property(e => e.exportId).HasColumnType("bigint(20) unsigned");

            entity.HasOne(d => d.AggregationCodeNavigation).WithMany(p => p.mains)
                .HasForeignKey(d => d.AggregationCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_main_aggregation");

            entity.HasOne(d => d.Gtin).WithMany(p => p.mains)
                .HasForeignKey(d => d.GtinId)
                .HasConstraintName("main_ibfk_4");
        });

        modelBuilder.Entity<order>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ID).HasMaxLength(256);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DateLastUpdate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Note)
                .HasMaxLength(256)
                .HasDefaultValueSql("' '");
            entity.Property(e => e.ProdType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<orders_ex>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("orders_ex", tb => tb.HasComment("Заказы"));

            entity.HasIndex(e => e.error_id, "FK_orders_ex_errors");

            entity.Property(e => e.id).HasColumnType("int(11)");
            entity.Property(e => e.complete_at)
                .HasDefaultValueSql("utc_timestamp()")
                .HasComment("Description:Дата ожидаемого завершения подготовки КМ")
                .HasColumnType("datetime");
            entity.Property(e => e.creation_at)
                .HasDefaultValueSql("utc_timestamp()")
                .HasComment("Description:Дата создания заказа")
                .HasColumnType("datetime");
            entity.Property(e => e.error_id)
                .HasComment("Description:FK на errors.id")
                .HasColumnType("int(11)");
            entity.Property(e => e.expiration_at)
                .HasComment("Description:Дата годности заказа")
                .HasColumnType("datetime");
            entity.Property(e => e.is_valid).HasComment("Description:Валидность заказа");
            entity.Property(e => e.status)
                .HasComment("Description:Статус заказа")
                .HasColumnType("int(11)");
            entity.Property(e => e.uuid).HasComment("Description:UUID заказа");

            entity.HasOne(d => d.error).WithMany(p => p.orders_exes)
                .HasForeignKey(d => d.error_id)
                .HasConstraintName("FK_orders_ex_errors");
        });

        modelBuilder.Entity<param>(entity =>
        {
            entity.HasKey(e => e.name).HasName("PRIMARY");

            entity.ToTable("params");

            entity.Property(e => e.name).HasMaxLength(45);
            entity.Property(e => e.value).HasMaxLength(200);
        });

        modelBuilder.Entity<printer_base>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("printer_base");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.HasIndex(e => e.FAgg, "FAgg");

            entity.HasIndex(e => e.task_id, "FK_printer_base_task_id_printer_tasks_id");

            entity.HasIndex(e => new { e.GtinId, e.StatusId }, "GtinId");

            entity.HasIndex(e => e.SAgg, "SAgg");

            entity.HasIndex(e => new { e.StatusId, e.Code }, "StatusId");

            entity.HasIndex(e => e.OrderID, "iOrder");

            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .UseCollation("utf8mb4_bin");
            entity.Property(e => e.DateExport).HasColumnType("datetime");
            entity.Property(e => e.DateImport)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.DatePrint).HasColumnType("datetime");
            entity.Property(e => e.DateVerify).HasColumnType("datetime");
            entity.Property(e => e.FAgg).HasColumnType("bigint(8) unsigned zerofill");
            entity.Property(e => e.FAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.GtinId).HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.OperatorName).HasMaxLength(50);
            entity.Property(e => e.OrderID).HasMaxLength(100);
            entity.Property(e => e.PartyId).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PutIntoUsing)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.SAgg).HasColumnType("bigint(8) unsigned zerofill");
            entity.Property(e => e.SAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'0'")
                .HasColumnType("tinyint(3) unsigned");
            entity.Property(e => e.Typograth)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11) unsigned");
            entity.Property(e => e.VerEx)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerFirstAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerSecondAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.code_number)
                .HasComment("Description:Номер КМ в рамках задачи печати")
                .HasColumnType("bigint(20)");
            entity.Property(e => e.task_id)
                .HasComment("Description:Идентификатор связанной задачи печати")
                .HasColumnType("bigint(20)");

            entity.HasOne(d => d.task).WithMany(p => p.printer_bases)
                .HasForeignKey(d => d.task_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_printer_base_task_id_printer_tasks_id");
        });

        modelBuilder.Entity<printer_task>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Задачи печати"));

            entity.Property(e => e.id)
                .HasComment("Description:id")
                .HasColumnType("bigint(20)");
            entity.Property(e => e.created_at)
                .HasComment("Description:Дата создания задачи")
                .HasColumnType("datetime");
            entity.Property(e => e.last_used_at)
                .HasComment("Description:Дата крайнего использования задачи")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<report>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ID)
                .HasMaxLength(100)
                .HasDefaultValueSql("'0'");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Note)
                .HasMaxLength(256)
                .HasDefaultValueSql("'-'");
            entity.Property(e => e.ProdType)
                .HasMaxLength(45)
                .HasDefaultValueSql("'-'");
            entity.Property(e => e.Status)
                .HasMaxLength(64)
                .HasDefaultValueSql("'PENDING'");
            entity.Property(e => e.TypeOperation).HasMaxLength(20);
            entity.Property(e => e.file_name)
                .HasMaxLength(260)
                .HasDefaultValueSql("''")
                .HasComment("Description:Путь к файлу с отчетом")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.last_request_status)
                .HasComment("Description:Последнее время запроса статуса.")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<status>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("status");

            entity.Property(e => e.StatusId).HasColumnType("tinyint(3) unsigned");
            entity.Property(e => e.StatusName).HasMaxLength(64);
        });

        modelBuilder.Entity<suborder>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Подзаказы"));

            entity.HasIndex(e => e.error_id, "FK_suborders_errors");

            entity.HasIndex(e => e.gtin_id, "FK_suborders_gtin");

            entity.HasIndex(e => e.order_id, "FK_suborders_orders_ex");

            entity.Property(e => e.id).HasColumnType("int(11)");
            entity.Property(e => e.error_id)
                .HasComment("Description:FK на errors.id")
                .HasColumnType("int(11)");
            entity.Property(e => e.gtin_id)
                .HasComment("FK на gtin.GtinId")
                .HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.is_valid).HasComment("Description:Валидность подзаказа");
            entity.Property(e => e.order_id)
                .HasComment("Description:FK на orders_ex.id")
                .HasColumnType("int(11)");
            entity.Property(e => e.status)
                .HasComment("Description:Статус подзаказа")
                .HasColumnType("int(11)");
            entity.Property(e => e.total_codes)
                .HasComment("Description:Кол-во заказанных КМ")
                .HasColumnType("int(11)");

            entity.HasOne(d => d.error).WithMany(p => p.suborders)
                .HasForeignKey(d => d.error_id)
                .HasConstraintName("FK_suborders_errors");

            entity.HasOne(d => d.gtin).WithMany(p => p.suborders)
                .HasForeignKey(d => d.gtin_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_suborders_gtin");

            entity.HasOne(d => d.order).WithMany(p => p.suborders)
                .HasForeignKey(d => d.order_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_suborders_orders_ex");
        });

        modelBuilder.Entity<tmp_main>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("tmp_main");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.HasIndex(e => e.FAgg, "FAgg");

            entity.HasIndex(e => e.AggregationCode, "FK_main_aggregation");

            entity.HasIndex(e => new { e.GtinId, e.StatusId }, "GtinId");

            entity.HasIndex(e => e.SAgg, "SAgg");

            entity.HasIndex(e => new { e.StatusId, e.Code }, "StatusId");

            entity.HasIndex(e => e.OrderID, "iOrder");

            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .UseCollation("utf8mb4_bin");
            entity.Property(e => e.ATK_Flag)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.AggregationCode).HasColumnType("bigint(20) unsigned");
            entity.Property(e => e.DateExport).HasColumnType("datetime");
            entity.Property(e => e.DateImport)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.DatePrint).HasColumnType("datetime");
            entity.Property(e => e.DateVerify).HasColumnType("datetime");
            entity.Property(e => e.FAgg).HasColumnType("bigint(8) unsigned zerofill");
            entity.Property(e => e.FAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.GtinId).HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.Mass).HasColumnType("int(6)");
            entity.Property(e => e.OperatorName).HasMaxLength(50);
            entity.Property(e => e.OrderID).HasMaxLength(100);
            entity.Property(e => e.PartyId).HasColumnType("int(10) unsigned");
            entity.Property(e => e.PutIntoUsing)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.SAgg).HasColumnType("bigint(8) unsigned zerofill");
            entity.Property(e => e.SAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'0'")
                .HasColumnType("tinyint(3) unsigned");
            entity.Property(e => e.TAgg).HasColumnType("bigint(8) unsigned zerofill");
            entity.Property(e => e.TAggDatePrint).HasColumnType("datetime");
            entity.Property(e => e.Typograth)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11) unsigned");
            entity.Property(e => e.VerEx)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerFirstAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerSecondAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.VerThirdAgg)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(8)");
            entity.Property(e => e.exportId).HasColumnType("bigint(20) unsigned");

            entity.HasOne(d => d.AggregationCodeNavigation).WithMany(p => p.tmp_mains)
                .HasForeignKey(d => d.AggregationCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_tmp_main_aggregation");

            entity.HasOne(d => d.Gtin).WithMany(p => p.tmp_mains)
                .HasForeignKey(d => d.GtinId)
                .HasConstraintName("tmp_main_ibfk_4");
        });

        modelBuilder.Entity<versioninfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("versioninfo");

            entity.HasIndex(e => e.Version, "UC_Version").IsUnique();

            entity.Property(e => e.AppliedOn).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(1024)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Version).HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<workonerror>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("workonerror")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.MarkGtin, "FK_workonerror_gtin");

            entity.Property(e => e.DateErr).HasColumnType("datetime");
            entity.Property(e => e.MarkGtin).HasColumnType("bigint(14) unsigned zerofill");
            entity.Property(e => e.OperatorName).HasMaxLength(100);
            entity.Property(e => e.TextOfExc).HasMaxLength(100);
            entity.Property(e => e.image).HasColumnType("mediumblob");

            entity.HasOne(d => d.MarkGtinNavigation).WithMany()
                .HasForeignKey(d => d.MarkGtin)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_workonerror_gtin");
        });
        modelBuilder.HasSequence("faggit_seq")
            .HasMin(1L)
            .HasMax(9999999L)
            .IsCyclic();
        modelBuilder.HasSequence("generator_seq")
            .HasMin(1L)
            .HasMax(999999999999999999L)
            .IsCyclic();
        modelBuilder.HasSequence("saggit_seq")
            .HasMin(1L)
            .HasMax(9999999L)
            .IsCyclic();
        modelBuilder.HasSequence("taggit_seq")
            .HasMin(1L)
            .HasMax(9999999L)
            .IsCyclic();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
