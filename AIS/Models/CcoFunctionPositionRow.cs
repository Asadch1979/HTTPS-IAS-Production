namespace AIS.Models
{
    public sealed class CcoFunctionPositionRow
    {
        public string RowKind { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public long OpeningBalance { get; set; }
        public long ParasAdded { get; set; }
        public long TotalParas { get; set; }
        public long SettledParas { get; set; }
        public long OutstandingParas { get; set; }
        public decimal? SettlementPercentage { get; set; }
    }
}
