using System.Collections.Generic;

namespace AIS.Models
{
    public class OrganizationMoveRequest
    {
        public int EntityId { get; set; }
        public int ExpectedParentId { get; set; }
        public int NewParentId { get; set; }
        public string Reason { get; set; }
    }

    public class OrganizationMovePreview
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int CurrentParentId { get; set; }
        public string CurrentParentName { get; set; }
        public int NewParentId { get; set; }
        public string NewParentName { get; set; }
        public string NewParentCode { get; set; }
        public int? NewParentTypeId { get; set; }
        public int? NewRelationTypeId { get; set; }
        public int DescendantEntities { get; set; }
    }
    public class OrganizationOpenParaCounts
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public long? OwnOpenParas { get; set; }
        public long? SubordinateOpenParas { get; set; }
        public long? TotalOpenParas { get; set; }
    }

    public class OrganizationNode
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int? EntityTypeId { get; set; }
        public string EntityCategory { get; set; }
        public int? ParentEntityId { get; set; }
        public string ParentEntityName { get; set; }
        public int? Reporting1 { get; set; }
        public int? Reporting2 { get; set; }
        public int? Reporting3 { get; set; }
        public int? Reporting4 { get; set; }
        public int? Reporting5 { get; set; }
        public int? Reporting6 { get; set; }
        public int DirectChildCount { get; set; }
        public bool IsRoot { get; set; }
        public bool IsOrphan { get; set; }
        public bool IsMissingName { get; set; }
        public bool HasTypeConflict { get; set; }
    }

    public class OrganizationPathEntry
    {
        public int LevelFromEntity { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int? ParentEntityId { get; set; }
        public string ParentEntityName { get; set; }
    }

    public class OrganizationSnapshot
    {
        public List<OrganizationNode> Nodes { get; set; }
        public bool IsRestricted { get; set; }
        public int? RootEntityId { get; set; }
    }
}
