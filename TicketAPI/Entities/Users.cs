namespace TicketAPI.Entities
{
    public class Users
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Surname { get; set; }
        public Guid AuthLevel { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public DateTime SysDate { get; set; }
        public DateTime SysUpdate {  get; set; }
        public bool SysVisible { get; set; }
        public bool SysEnabled { get; set; }
    }
}
