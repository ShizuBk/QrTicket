namespace TicketAPI.Dto
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Surname { get; set; }
        public string AuthLevel { get; set; }
    }
}
