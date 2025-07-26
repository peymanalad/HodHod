using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Abp.OpenIddict.Applications;
using Abp.OpenIddict.Authorizations;
using Abp.OpenIddict.EntityFrameworkCore;
using Abp.OpenIddict.Scopes;
using Abp.OpenIddict.Tokens;
using Abp.Timing;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using HodHod.Authorization.Delegation;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;
using HodHod.Categories;
using HodHod.Chat;
using HodHod.Editions;
using HodHod.ExtraProperties;
using HodHod.Friendships;
using HodHod.Geo;
using HodHod.MultiTenancy;
using HodHod.MultiTenancy.Accounting;
using HodHod.MultiTenancy.Payments;
using HodHod.Storage;
using HodHod.Reports;

namespace HodHod.EntityFrameworkCore;

public class HodHodDbContext : AbpZeroDbContext<Tenant, Role, User, HodHodDbContext>, IOpenIddictDbContext
{
    /* Define an IDbSet for each entity of the application */

    public virtual DbSet<OpenIddictApplication> Applications { get; }

    public virtual DbSet<OpenIddictAuthorization> Authorizations { get; }

    public virtual DbSet<OpenIddictScope> Scopes { get; }

    public virtual DbSet<OpenIddictToken> Tokens { get; }

    public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

    public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

    public virtual DbSet<SubscriptionPaymentProduct> SubscriptionPaymentProducts { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<UserDelegation> UserDelegations { get; set; }

    public virtual DbSet<RecentPassword> RecentPasswords { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<SubCategory> SubCategories { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportFile> ReportFiles { get; set; }

    public virtual DbSet<PhoneReportLimit> PhoneReportLimits { get; set; }
    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<ReportNote> ReportNotes { get; set; }

    public virtual DbSet<ReportStar> ReportStars { get; set; }
    public HodHodDbContext(DbContextOptions<HodHodDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BinaryObject>(b => { b.HasIndex(e => new { e.TenantId }); });

        modelBuilder.Entity<SubscriptionPayment>(x =>
        {
            x.Property(u => u.ExtraProperties)
                .HasConversion(
                    d => JsonSerializer.Serialize(d, new JsonSerializerOptions()
                    {
                        WriteIndented = false
                    }),
                    s => JsonSerializer.Deserialize<ExtraPropertyDictionary>(s, new JsonSerializerOptions()
                    {
                        WriteIndented = false
                    })
                );
        });

        modelBuilder.Entity<SubscriptionPaymentProduct>(x =>
        {
            x.Property(u => u.ExtraProperties)
                .HasConversion(
                    d => JsonSerializer.Serialize(d, new JsonSerializerOptions()
                    {
                        WriteIndented = false
                    }),
                    s => JsonSerializer.Deserialize<ExtraPropertyDictionary>(s, new JsonSerializerOptions()
                    {
                        WriteIndented = false
                    })
                );
        });

        modelBuilder.Entity<ChatMessage>(b =>
        {
            b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
            b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
            b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
            b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
        });

        modelBuilder.Entity<Friendship>(b =>
        {
            b.HasIndex(e => new { e.TenantId, e.UserId });
            b.HasIndex(e => new { e.TenantId, e.FriendUserId });
            b.HasIndex(e => new { e.FriendTenantId, e.UserId });
            b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
        });

        modelBuilder.Entity<Tenant>(b =>
        {
            b.HasIndex(e => new { e.SubscriptionEndDateUtc });
            b.HasIndex(e => new { e.CreationTime });
        });

        modelBuilder.Entity<SubscriptionPayment>(b =>
        {
            b.HasIndex(e => new { e.Status, e.CreationTime });
            b.HasIndex(e => new { PaymentId = e.ExternalPaymentId, e.Gateway });
        });

        modelBuilder.Entity<UserDelegation>(b =>
        {
            b.HasIndex(e => new { e.TenantId, e.SourceUserId });
            b.HasIndex(e => new { e.TenantId, e.TargetUserId });
        });

        modelBuilder.Entity<SubCategory>(b =>
        {
            b.HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HodHod.Reports.Report>(b =>
        {
            b.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(r => r.SubCategory)
                .WithMany()
                .HasForeignKey(r => r.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HodHod.Reports.ReportFile>(b =>
        {
            b.HasOne(rf => rf.Report)
                .WithMany(r => r.Files)
                .HasForeignKey(rf => rf.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HodHod.Reports.ReportNote>(b =>
        {
            b.HasOne(n => n.Report)
                .WithMany(r => r.Notes)
                .HasForeignKey(n => n.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HodHod.Reports.ReportStar>(b =>
        {
            b.HasIndex(s => new { s.ReportId, s.UserId }).IsUnique();
            b.HasOne(s => s.Report)
                .WithMany()
                .HasForeignKey(s => s.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<City>(b =>
        {
            b.HasOne(c => c.Province)
                .WithMany(p => p.Cities)
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.ConfigureOpenIddict();
    }

    private void UpdatePersianModificationTimes()
    {
        foreach (var entry in ChangeTracker.Entries<Report>())
        {
            if (entry.State == EntityState.Modified &&
                entry.Property("LastModificationTime").IsModified)
            {
                entry.Entity.PersianLastModificationTime =
                    PersianDateTimeHelper.ToCompactPersianNumber(Clock.Now);
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdatePersianModificationTimes();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdatePersianModificationTimes();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdatePersianModificationTimes();
        return base.SaveChangesAsync(cancellationToken);
    }
}

