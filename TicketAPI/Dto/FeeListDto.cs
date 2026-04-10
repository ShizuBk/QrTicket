namespace TicketAPI.Dto
{
    public class FeeListDto
    {
        // Agregamos 'public' e inicializamos con = new();
        public List<Fees> Fees { get; set; } = new();
    }

    public class Fees
    {
        // Inicializamos con string.Empty para evitar el warning de nulabilidad
        public string Name { get; set; } = string.Empty;
        public int Fee { get; set; }
    }
}