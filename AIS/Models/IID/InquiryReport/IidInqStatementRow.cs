using System;
using AIS.Validation;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqStatementRow
        {
        public long StatementId { get; set; }
        public long ComplaintId { get; set; }
        [PlainText]
        public string PersonName { get; set; }
        [PlainText]
        public string RoleType { get; set; }
        [PlainText]
        public string PpnoNumber { get; set; }
        [PlainText]
        public string Cnic { get; set; }
        public DateTime? StatementDatetime { get; set; }
        [PlainText]
        public string Place { get; set; }
        [PlainText]
        public string ModeType { get; set; }
        [PlainText]
        public string KeyPoints { get; set; }
        [PlainText]
        public string Status { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }
