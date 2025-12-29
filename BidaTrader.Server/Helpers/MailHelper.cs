using RazorLight;
using System.Net;
using System.Net.Mail;

namespace BidaTrader.Server.Helpers
{
    public class MailHelper
    {
        private IWebHostEnvironment Environment { get; set; }
        public IConfiguration Configuration { get; set; }
        private readonly IConfiguration _configuration;

        private readonly IRazorLightEngine _razorLightEngine;

        public MailHelper(IConfiguration _configuration, IWebHostEnvironment environment, IConfiguration configuration, IConfiguration iConfiguration)
        {
            Configuration = _configuration;
            Environment = environment;

            try
            {
                string viewsPath = Path.Combine(environment.ContentRootPath, "Views");
                // Tạo RazorLightEngine
                _razorLightEngine = new RazorLightEngineBuilder()
                    .UseFileSystemProject(Path.Combine(viewsPath))
                    .UseMemoryCachingProvider()
                    .Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing RazorLightEngine: {ex.Message}");
            }
        }

        public string PopulateBody(string OTP)
        {
            //Lấy Template HTML
            string body = string.Empty;
            string path = Path.Combine(this.Environment.WebRootPath, "Template\\EmailTemplate.htm");
            using (StreamReader reader = new StreamReader(path))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{OTP}", OTP);
            return body;
        }

        public void SendHtmlFormattedEmail(string recepientEmail, string subject, string body)
        {
            string host = this.Configuration.GetValue<string>("Smtp:Server");
            int port = this.Configuration.GetValue<int>("Smtp:Port");
            string fromAddress = this.Configuration.GetValue<string>("Smtp:FromAddress");
            string userName = this.Configuration.GetValue<string>("Smtp:UserName");
            string password = this.Configuration.GetValue<string>("Smtp:Password");

            using (MailMessage mm = new MailMessage(fromAddress, recepientEmail))
            {
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = host;
                    smtp.EnableSsl = true;
                    NetworkCredential networkCred = new NetworkCredential(userName, password);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = networkCred;
                    smtp.Port = port;
                    smtp.Send(mm);
                }
            }
        }

    }
}
