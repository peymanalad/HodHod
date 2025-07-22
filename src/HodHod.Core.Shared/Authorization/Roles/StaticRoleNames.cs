namespace HodHod.Authorization.Roles;

public static class StaticRoleNames
{
    public static class Host
    {
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";
        public const string ProvinceAdmin = "ProvinceAdmin";
        public const string CityAdmin = "CityAdmin";
    }

    public static class Tenants
    {
        public const string Admin = "Admin";

        public const string User = "User";
    }
}

