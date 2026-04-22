namespace TicketAPI.Dto
{
    public class NewUserDto
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Surname { get; set; }
        public Guid AuthLevel { get; set; }
        public string Password { get; set; }
    }
}
