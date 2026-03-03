namespace AIS.Models.Security
{
    public class CspViolationDto
    {
        public string Disposition { get; set; }
        public string DocumentUri { get; set; }
        public string Referrer { get; set; }
        public string BlockedUri { get; set; }
        public string EffectiveDirective { get; set; }
        public string ViolatedDirective { get; set; }
        public string OriginalPolicy { get; set; }
        public string SourceFile { get; set; }
        public long? LineNumber { get; set; }
        public long? ColumnNumber { get; set; }
        public long? StatusCode { get; set; }
        public string ScriptSample { get; set; }
        public string UserAgent { get; set; }
        public string ClientIp { get; set; }
        public string RawJson { get; set; }
    }
}
