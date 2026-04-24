using System;
using AIS.Validation;

namespace AIS.Models
    {
    public class UserContextAssignmentModel
        {
        public int AssignmentId { get; set; }
        public int UserId { get; set; }
        [PlainText]
        public string PPNumber { get; set; }
        public int GroupId { get; set; }
        public int RoleId { get; set; }
        [PlainText]
        public string RoleName { get; set; }
        public int EntityId { get; set; }
        [PlainText]
        public string EntityName { get; set; }
        public int? ParentEntityId { get; set; }
        [PlainText]
        public string ParentEntityName { get; set; }
        public int? RelationshipTypeId { get; set; }
        [PlainText]
        public string RelationshipTypeName { get; set; }
        public int? EntityTypeId { get; set; }
        public int? ParentEntityTypeId { get; set; }
        public int? EntityCode { get; set; }
        public int? ParentEntityCode { get; set; }
        [PlainText]
        public string UserLocationType { get; set; }
        public int? UserPostingAuditZone { get; set; }
        public int? UserPostingDiv { get; set; }
        public int? UserPostingDept { get; set; }
        public int? UserPostingBranch { get; set; }
        public int? UserPostingZone { get; set; }
        [PlainText]
        public string IsDefault { get; set; }
        [PlainText]
        public string IsActive { get; set; }
        [PlainText]
        public string AssignmentType { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        [PlainText]
        public string Remarks { get; set; }
        }
    }
