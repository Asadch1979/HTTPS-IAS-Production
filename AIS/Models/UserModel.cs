using AIS.Validation;
namespace AIS.Models
    {
    public class UserModel
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
        [PlainText]
        public string DivName { get; set; }
        public int? UserPostingDiv { get; set; }
        public int? RelationshipId { get; set; }
        [PlainText]
        public string DeptName { get; set; }
        public int? UserPostingDept { get; set; }
        [PlainText]
        public string BranchName { get; set; }
        public int? UserPostingBranch { get; set; }
        [PlainText]
        public string ZoneName { get; set; }
        public int? UserPostingZone { get; set; }
        [PlainText]
        public string UserRole { get; set; }
        [PlainText]
        public string UserGroup { get; set; }
        public int? UserGroupID { get; set; }
        public int? UserRoleID { get; set; }
        public int? UserEntityID { get; set; }
        public int? UserParentEntityID { get; set; }
        public int? UserEntityCode { get; set; }
        public int? UserParentEntityCode { get; set; }
        [PlainText]
        public string UserEntityName { get; set; }
        [PlainText]
        public string UserParentEntityName { get; set; }
        public int? UserEntityTypeID { get; set; }
        public int? UserParentEntityTypeID { get; set; }
        [PlainText]
        public string UserRoleName { get; set; }
        [PlainText]
        public string ErrorCode { get; set; }
        public int? RetryAfterSeconds { get; set; }
        [PlainText]
        public string ErrorTitle { get; set; }
        [PlainText]
        public string ErrorMsg { get; set; }
        public bool isAuthenticate { get; set; }
        public bool isAlreadyLoggedIn { get; set; }
        public bool passwordChangeRequired { get; set; }
        [PlainText]
        public string changePassword { get; set; }
        }
    }
