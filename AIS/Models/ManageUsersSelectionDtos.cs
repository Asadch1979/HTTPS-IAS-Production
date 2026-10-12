namespace AIS.Models
{
    public sealed class EntityTypeSelectionDto
    {
        public int AUTID { get; set; }
        public string ENTITYCODE { get; set; }
        public string ENTITYTYPEDESC { get; set; }
        public int? DISPLAY_ORDER { get; set; }
    }
    public sealed class ReportingOfficeSelectionDto
    {
        public int ENTITY_ID { get; set; }
        public string DESCRIPTION { get; set; }
        public int? PARENT_TYPE_ID { get; set; }
    }
    public sealed class PostingSelectionDto
    {
        public int ENTITY_ID { get; set; }
        public string C_NAME { get; set; }
        public int TYPEID { get; set; }
        public int? PARENT_ID { get; set; }
    }
}
