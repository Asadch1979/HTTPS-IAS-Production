namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqComplaintRequest
        {
        public long ComplaintId { get; set; }
        }

    public class IidInqDeleteRequest
        {
        public long Id { get; set; }
        public long UserId { get; set; }
        }

    public class IidEmployeeInfoRequest
        {
        public long PpNo { get; set; }
        }
    }
