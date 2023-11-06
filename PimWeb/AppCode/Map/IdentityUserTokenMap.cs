using PimWeb.AppCode.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityUserTokenMap : ClassMapBase<AppUserToken>
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