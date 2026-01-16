using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class CircularDocumentModel
        {
        public int DocId { get; set; }
        public int CircularId { get; set; }
        [PlainText]
        public string FileName { get; set; }
        [PlainText]
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public byte[] FileBlob { get; set; }
        [PlainText]
        public string UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        }

    }
