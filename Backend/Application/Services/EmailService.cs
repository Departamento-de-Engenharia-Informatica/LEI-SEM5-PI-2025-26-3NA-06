using System.Net;
using System.Net.Mail;
using ProjArqsi.Domain.UserAggregate;

namespace ProjArqsi.Application.Services
{
    public class EmailService
    {
        public async Task SendConfirmationEmailAsync(User user, string email, string token)
        {
            // Change link to correct frontend confirmation page
            var confirmationLink = $"http://localhost:5173/confirm-email?token={token}";
            var subject = "Confirm Your Registration | ProjArqsi";
            
            var body = $@"
            <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0px 0px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center;'>
                            <img src='../Backend/assets/image.png' alt='ProjArqsi Logo' style='max-width: 150px;' />
                        </div>
                        <h2 style='text-align: center; color: #333;'>Confirm Your Registration</h2>
                        <p style='font-size: 16px; color: #333;'>Dear {user.Username},</p>
                        <p style='font-size: 16px; color: #333;'>We received your registration request at ProjArqsi. To confirm your account, please click the link below:</p>
                        <p style='text-align: center;'>
                            <a href='{confirmationLink}' style='font-size: 16px; color: #ffffff; text-decoration: none; background-color: #007bff; padding: 10px 20px; border-radius: 5px; display: inline-block;'>Confirm Registration</a>
                        </p>
                        <p style='font-size: 16px; color: #333;'>If you didn’t request this, please ignore this email or contact ProjArqsi support immediately.</p>
                        <p style='font-size: 16px; color: #333;'>Thank you for joining ProjArqsi!</p>
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
