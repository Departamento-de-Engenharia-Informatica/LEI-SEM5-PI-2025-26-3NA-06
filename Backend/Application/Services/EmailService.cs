using System.Net;
using System.Net.Mail;
using ProjArqsi.Domain.UserAggregate;

namespace ProjArqsi.Application.Services
{
    public class EmailService
    {
        public async Task SendConfirmationEmailAsync(User user, string email, string token)
        {
            // Direct link to Backend activation endpoint
            var confirmationLink = $"http://localhost:5218/api/Activation/confirm?token={token}";
            var subject = "Activate Your Account | ProjArqsi";
            
            var body = $@"
            <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0px 0px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center;'>
                            <img src='../Backend/assets/image.png' alt='ProjArqsi Logo' style='max-width: 150px;' />
                        </div>
                        <h2 style='text-align: center; color: #333;'>Activate Your Account</h2>
                        <p style='font-size: 16px; color: #333;'>Dear {user.Username.Value},</p>
                        <p style='font-size: 16px; color: #333;'>An administrator has assigned you the role of <strong>{user.Role.Value}</strong> and activated your account!</p>
                        <p style='font-size: 16px; color: #333;'>To complete your activation and start using ProjArqsi, please click the link below:</p>
                        <p style='text-align: center;'>
                            <a href='{confirmationLink}' style='font-size: 16px; color: #ffffff; text-decoration: none; background-color: #28a745; padding: 12px 24px; border-radius: 5px; display: inline-block;'>Activate My Account</a>
                        </p>
                        <p style='font-size: 14px; color: #666; text-align: center; margin-top: 20px;'>This link will expire in 24 hours</p>
                        <p style='font-size: 16px; color: #333;'>If you didn't request this account, please ignore this email or contact support.</p>
                        <p style='font-size: 16px; color: #333;'>After activation, you can log in at: <a href='http://localhost:4200/login'>http://localhost:4200/login</a></p>
                        <p style='font-size: 16px; color: #333;'>Best regards,</p>
                        <p style='font-size: 16px; color: #333;'>The ProjArqsi Team</p>
                    </div>
                </body>
            </html>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.Credentials = new NetworkCredential("projarqsiemail@gmail.com", "zkab aurb dhty vgqj");
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("projarqsiemail@gmail.com"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true // Set to true if the body contains HTML content
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the email sending process
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }

    }
}
