namespace AIS.Models.IID
{
    public class ComplaintRowDto
    {
        public long ComplaintId { get; set; }
        public string ComplaintNo { get; set; }
        public string ComplainantName { get; set; }
        public string Nature { get; set; }
        public string Source { get; set; }
        public int? AssignedUnitId { get; set; }
        public string Status { get; set; }
        public string SubmittedOn { get; set; }
    }
}
