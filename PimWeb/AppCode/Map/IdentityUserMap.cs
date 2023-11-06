using PimWeb.AppCode.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityUserMap : ClassMapBase<AppUser>
{
    /// <inheritdoc />
    public IdentityUserMap()
    {
        Id(x => x.Id).GeneratedBy.Sequence("aspnet_identity_id_seq");
        Table("aspnet_users");

        Map(x => x.UserName)
            .Length(64)
            .Not.Nullable()
            .Unique();
        
        Map(x => x.NormalizedUserName)
            .Length(64)
            .Not.Nullable()
            .Unique();
        
        Map(x => x.Email)
            .Length(256)
            .Not.Nullable();
        
        Map(x => x.NormalizedEmail)
            .Length(256)
            .Not.Nullable();
        
        Map(x => x.EmailConfirmed);
        
        Map(x => x.PhoneNumber)
            .Length(128);

        Map(x => x.PhoneNumberConfirmed);
        Map(x => x.LockoutEnabled);
        
        Map(x => x.LockoutEnd)
            .CustomType<PostgresqlDatetimeOffsetType>();
        
        Map(x => x.AccessFailedCount);
        Map(x => x.ConcurrencyStamp)
            .Length(36);
        Map(x => x.PasswordHash)
            .Length(256);
        Map(x => x.TwoFactorEnabled);
        Map(x => x.SecurityStamp)
            .Length(64);
        
        HasManyToMany(x => x.Roles)
            .Table("aspnet_user_roles")
            .ParentKeyColumn("user_id")
            .ChildKeyColumn("role_id")
            .Cascade.None()
            .Cache.ReadWrite();

        HasMany(x => x.Claims)
            .Inverse()
            .Cascade.AllDeleteOrphan()
            .KeyColumn("user_id")
            .Cache.ReadWrite();

        HasMany(x => x.Logins)
            .Inverse()
            .Cascade.AllDeleteOrphan()
            .KeyColumn("user_id")
            .Cache.ReadWrite();

        HasMany(x => x.Tokens)
            .Inverse()
            .Cascade.AllDeleteOrphan()
            .KeyColumn("user_id")
            .Cache.ReadWrite();
    }
}