using FluentNHibernate.AspNetCore.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityUserTokenMap : ClassMapBase<IdentityUserToken<int>>
{
    /// <inheritdoc />
    public IdentityUserTokenMap()
    {
        Table("aspnet_user_tokens");
        
        CompositeId()
            .KeyProperty(x => x.UserId)
            .KeyProperty(x => x.LoginProvider)
            .KeyProperty(x => x.Name);
        
        Map(x => x.Value)
            .Length(256)
            .Not.Nullable();
    }
}