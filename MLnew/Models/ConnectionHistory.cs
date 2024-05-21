namespace MLnew.Models
{
    public class ConnectionHistory
    {
        public int Id { get; set; }
        public string? UserEmail { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? Action { get; set; }
    }
}
