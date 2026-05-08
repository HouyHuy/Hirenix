using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

class Program
{
    static async Task Main()
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hirenix", ""));
            message.To.Add(MailboxAddress.Parse("legiahuy070705@gmail.com"));
            message.Subject = "Test Email";
            message.Body = new TextPart("plain") { Text = "This is a test" };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("", "nsne sjbe mzdy nfbt");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
