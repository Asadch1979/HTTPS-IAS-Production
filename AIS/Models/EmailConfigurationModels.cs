using AIS.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class EmailConfigurationRecord
        {
        public long ConfigId { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string FromEmail { get; set; } = string.Empty;
        public string SmtpUsername { get; set; } = string.Empty;
        public byte[] EncryptedPassword { get; set; } = Array.Empty<byte>();
        public bool EnableSsl { get; set; } = true;
        public bool IsActive { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        }

    public class EmailConfigurationAdminModel
        {
        public long ConfigId { get; set; }

        [Required, PlainText]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [Required, EmailAddress, PlainText]
        public string FromEmail { get; set; } = string.Empty;

        [PlainText]
        public string SmtpUsername { get; set; } = string.Empty;

        [DataType(DataType.Password), PasswordText]
        public string NewPassword { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;
        public bool IsActive { get; set; }

        [EmailAddress, PlainText]
        public string TestRecipient { get; set; } = string.Empty;

        public bool HasSavedPassword { get; set; }
        }
    }
