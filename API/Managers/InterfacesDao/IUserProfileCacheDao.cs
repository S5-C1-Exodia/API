namespace Api.Managers.InterfacesDao
{
    public interface IUserProfileCacheDao
    {
        Task DeleteByProviderUserAsync(string providerUserId);
    }
}