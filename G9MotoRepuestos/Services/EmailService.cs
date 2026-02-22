using System.Net;
using System.Net.Mail;

namespace G9MotoRepuestos.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreo(string destino, string asunto, string mensaje)
        {
            var emailEmisor = _config["EmailSettings:EmailEmisor"];
            var password = _config["EmailSettings:Password"];
            var servidor = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var puerto = int.Parse(_config["EmailSettings:Port"] ?? "587");

            using (var clienteSmtp = new SmtpClient(servidor))
            {
                clienteSmtp.Port = puerto;
                clienteSmtp.Credentials = new NetworkCredential(emailEmisor, password);
                clienteSmtp.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailEmisor!, "Moto Repuestos Rojas"),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(destino);

                try
                {
                    await clienteSmtp.SendMailAsync(mailMessage);
                    Console.WriteLine("--> Correo enviado exitosamente a: " + destino);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--> Error al enviar correo: " + ex.Message);
                    throw; 
                }
            }
        }
    }
}