using AIS.Validation;
namespace AIS.Models
    {
    public class RiskRatingModelForBranchesWorking
        {

        [PlainText]
        public string MainProcessRiskSequence { get; set; }
        [PlainText]
        public string MainProcess { get; set; }
        [PlainText]
        public string MainProcessWeightAssigned { get; set; }
        [PlainText]
        public string SubProcessRiskSequence { get; set; }
        [PlainText]
        public string SubProcess { get; set; }
        [PlainText]
        public string SubProcessWeightAssigned { get; set; }
        [PlainText]
        public string High { get; set; }
        [PlainText]
        public string Medium { get; set; }
        [PlainText]
        public string Low { get; set; }
        [PlainText]
        public string TotalNoOfTest { get; set; }
        [PlainText]
        public string AvailableWeightedScore { get; set; }
        [PlainText]
        public string AvailableProcessScore { get; set; }
        [PlainText]
        public string TotalHigh { get; set; }
        [PlainText]
        public string TotalMedium { get; set; }
        [PlainText]
        public string TotalLow { get; set; }
        [PlainText]
        public string TotalObservations { get; set; }
        [PlainText]
        public string TotalScoreSubProcess { get; set; }
        [PlainText]
        public string WeightedAverageScore { get; set; }
        [PlainText]
        public string TotalScoreProcess { get; set; }
        [PlainText]
        public string WeightedAverageScoreOverall { get; set; }



        }
    }
