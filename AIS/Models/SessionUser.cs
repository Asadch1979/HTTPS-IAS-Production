using AIS.Validation;
namespace AIS.Models
    {
    /// <summary>
    ///     Represents the authenticated user stored in the session.
    ///     The following fields are guaranteed to be populated immediately after a successful login:
    ///     <list type="bullet">
    ///         <item><description><see cref="ID"/></description></item>
    ///         <item><description><see cref="SessionId"/></description></item>
    ///         <item><description><see cref="PPNumber"/></description></item>
    ///         <item><description><see cref="UserRoleID"/></description></item>
    ///         <item><description><see cref="UserEntityID"/></description></item>
    ///     </list>
    ///     Other properties are populated where available from the identity provider.
    /// </summary>
    public class SessionUser
        {
        public int ID { get; set; }
        [PlainText]
        public string SessionId { get; set; }
        [PlainText]
        public string IPAddress { get; set; }
        [PlainText]
        public string MACAddress { get; set; }
        [PlainText]
        public string FirstMACCardAddress { get; set; }
        [PlainText]
        public string Name { get; set; }
        [PlainText]
        public string PPNumber { get; set; }
        [RichTextSanitize]
        public string Email { get; set; }
        [PlainText]
        public string IsActive { get; set; }
        [PlainText]
        public string UserLocationType { get; set; }
        public int? UserPostingAuditZone { get; set; }
        public int? UserPostingDiv { get; set; }
        public int? UserPostingDept { get; set; }
        public int? UserPostingBranch { get; set; }
        public int? UserPostingZone { get; set; }
        public int UserGroupID { get; set; }
        public int UserRoleID { get; set; }
        public int? UserEntityID { get; set; }
        public int? UserParentEntityID { get; set; }
        public int? UserEntityCode { get; set; }
        public int? UserParentEntityCode { get; set; }
        [PlainText]
        public string UserEntityName { get; set; }
        [PlainText]
        public string UserParentEntityName { get; set; }
        [PlainText]
        public string UserRoleName { get; set; }
        public int? UserEntityTypeID { get; set; }
        public int? UserParentEntityTypeID { get; set; }
        }
    }
