using FluentNHibernate.AspNetCore.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityUserRoleMap : ClassMapBase<IdentityUserRole<int>>
{
    public IdentityUserRoleMap()
    {
        Table("aspnet_user_roles");

        CompositeId().KeyProperty(x => x.UserId).KeyProperty(x => x.RoleId);
    }
}