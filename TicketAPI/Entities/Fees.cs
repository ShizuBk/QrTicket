using Org.BouncyCastle.Bcpg.OpenPgp;

namespace TicketAPI.Entities
{
    public class Fees
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public decimal Fee { get; set; }
        public DateTime SysDate { get; set; }
        public DateTime SysUpdate {  get; set; }
        public bool SysVisible { get; set; }
        public bool SysEnabled { get; set; }
    }
}
