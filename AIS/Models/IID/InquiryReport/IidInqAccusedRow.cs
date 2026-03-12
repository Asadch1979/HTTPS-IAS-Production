using System;
using AIS.Validation;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqAccusedRow
        {
        public long AccusedRowId { get; set; }
        public long ComplaintId { get; set; }
        [PlainText]
        public string PersonName { get; set; }
        [PlainText]
        public string FatherName { get; set; }
        [PlainText]
        public string Designation { get; set; }
        [PlainText]
        public string RoleType { get; set; }

        [PlainText]
        public string StatementType { get; set; }
        [PlainText]
        public string PpnoNumber { get; set; }
        [PlainText]
        public string Cnic { get; set; }
        [PlainText]
        public string PostingPlace { get; set; }
        [PlainText]
        public string Remarks { get; set; }
        public int SortOrder { get; set; }
        [PlainText]
        public string Status { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }
