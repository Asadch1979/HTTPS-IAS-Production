namespace AIS.Models
    {
    public class UserEntityModel
        {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string IsPrimary { get; set; }
        public string Status { get; set; }
        }
    }
