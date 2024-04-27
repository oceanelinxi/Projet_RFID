namespace ML1.Models
{
    public class SaveResultModel
    {
        public int IdSauvegarde { get; set; }
        public float Accuracy { get; set; }
        public DateTime Heure { get; set; }
        public int IdMethode { get; set; }
        public List<SRHParam>? ListHParam { get; set; }
    }
    public class SRHParam
    {
        public int Id { get; set; }
        public string? Value { get; set; }
    }
}
