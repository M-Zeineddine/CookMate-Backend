namespace CookMateBackend.Models.OutputModels
{
    public class DetectionModel
    {
        public string Class { get; set; }
        public double Confidence { get; set; }
        public string DetectionId { get; set; }
    }

}
