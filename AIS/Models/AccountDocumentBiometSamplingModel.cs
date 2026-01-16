using AIS.Validation;
namespace AIS.Models
    {
    public class AccountDocumentBiometSamplingModel
        {
        [PlainText]
        public string OldAccountNo { get; set; }
        [PlainText]
        public string PageNo { get; set; }
        [PlainText]
        public string Name { get; set; }
        public byte[] DocImage { get; set; } // Assuming document image is stored as binary data
        [PlainText]
        public string DocRemarks { get; set; }
        }
    }
