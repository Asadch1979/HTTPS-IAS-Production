namespace AIS.Session
{
    public static class SessionKeys
    {
        public const string User = "session:user";
        public const string Token = "session:token";
        public const string PageId = "session:page";
        public const string AllowedViewIds = "session:allowed-view-ids";
        public const string AllowedApiPaths = "session:allowed-api-paths";
        public const string Bootstrapped = "session:bootstrapped";
        public const string SbpAccessGranted = "session:sbp:access";
        public const string SessionStamp = "session:stamp";
        public const string UserRole = "session:user-role";
        public const string IsSuperUser = "session:is-super-user";
        public const string ActiveEngagementId = "session:active-engagement-id";
        public const string MustChangePassword = "session:must-change-password";
    }
}
