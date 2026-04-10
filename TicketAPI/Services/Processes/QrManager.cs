using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Net.Codecrete.QrCodeGenerator;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using TicketAPI.Dto;

namespace TicketAPI.Services.Processes
{
    public class QrManager
    {
        public byte[] GetQrCodeBytes(string url, TicketDetailsDto ticketDetails)
        {
            var file = GenFile(ticketDetails);
            var fullString = $"{url}/file/{file}";

            var qr = QrCode.EncodeText(fullString, QrCode.Ecc.Medium);

            var image = qr.ToBmpBitmap();

            return image;
        }

        private string GenFile(TicketDetailsDto ticketDetails)
        {
            ticketDetails.TitularName.ToUpper();
            ticketDetails.TitularLastName.ToUpper();
            ticketDetails.TitularSurname.ToUpper();

            var fecha = ticketDetails.PurchaseDate.ToString("yyMMdd");
            var nombre = ticketDetails.TitularName[0];
            var apellidoP = ticketDetails.TitularLastName[0];
            char apellidoM = 'X';
            if (ticketDetails.TitularSurname is not null or "")
                apellidoM = ticketDetails.TitularSurname[0];

            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes($"{nombre}{apellidoM}{apellidoP}")))
                .Substring(0, 4);
            var random = new Random().Next(0, 9999);
            return $"{nombre}{apellidoP}-{hash}{apellidoM}-{random:0000}";
        }
    }
}
