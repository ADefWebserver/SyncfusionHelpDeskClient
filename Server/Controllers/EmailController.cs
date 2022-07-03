#nullable disable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using SyncfusionHelpDeskClient.Shared;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SyncfusionHelpDesk.Data
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly SyncfusionHelpDeskContext _context;

        public EmailController(
            IConfiguration Configuration,
            IHttpContextAccessor HttpContextAccessor,
            SyncfusionHelpDeskContext context)
        {
            configuration = Configuration;
            httpContextAccessor = HttpContextAccessor;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public object Get()
        {
            // Return only one Ticket.
            StringValues HelpDeskTicketGuidProperty;

            string HelpDeskTicketGuid =
                (Request.Query.TryGetValue("HelpDeskTicketGuid",
                out HelpDeskTicketGuidProperty))
                ? HelpDeskTicketGuidProperty.ToString() : "";

            var ExistingTicket = _context.HelpDeskTickets
                .Include(x => x.HelpDeskTicketDetails)
                .Where(x => x.TicketGuid == HelpDeskTicketGuid)
                .FirstOrDefault();

            return ExistingTicket;
        }

        [HttpPost]
        [AllowAnonymous]
        public Task Post(HelpDeskEmail objHelpDeskEmail)
        {
            try
            {
                // Email settings.
                SendGridMessage msg = new SendGridMessage();
                var apiKey = configuration["SENDGRID_APIKEY"];
                var senderEmail = configuration["SenderEmail"];
                var client = new SendGridClient(apiKey);

                var FromEmail = new EmailAddress(
                    senderEmail,
                    senderEmail
                    );

                // Format Email contents.
                string strPlainTextContent =
                    $"{objHelpDeskEmail.EmailType}: " +
                    $"{GetHelpDeskTicketUrl(objHelpDeskEmail.TicketGuid)}";

                string strHtmlContent =
                    $"<b>{objHelpDeskEmail.EmailType}:</b> ";
                strHtmlContent = strHtmlContent +
                    $"<a href='" +
                    $"{GetHelpDeskTicketUrl(objHelpDeskEmail.TicketGuid)}" +
                    $"'>";
                strHtmlContent = strHtmlContent +
                    $"{GetHelpDeskTicketUrl(objHelpDeskEmail.TicketGuid)}</a>";

                if (objHelpDeskEmail.EmailType == "Help Desk Ticket Created")
                {
                    msg = new SendGridMessage()
                    {
                        From = FromEmail,
                        Subject = objHelpDeskEmail.EmailType,
                        PlainTextContent = strPlainTextContent,
                        HtmlContent = strHtmlContent
                    };

                    // Created Email always goes to Administrator.
                    // Send to senderEmail configured in appsettings.json
                    msg.AddTo(
                        new EmailAddress(senderEmail, objHelpDeskEmail.EmailType)
                        );
                }

                if (objHelpDeskEmail.EmailType == "Help Desk Ticket Updated")
                {
                    // Must pass a valid GUID.
                    // Get the existing record.
                    if (_context.HelpDeskTickets
                        .Where(x => x.TicketGuid == objHelpDeskEmail.TicketGuid)
                        .FirstOrDefault() != null)
                    {
                        // See if the user is the Administrator.
                        if (!this.User.IsInRole("Administrators"))
                        {
                            // Always send email to Administrator.
                            objHelpDeskEmail.EmailAddress = senderEmail;
                        }

                        msg = new SendGridMessage()
                        {
                            From = FromEmail,
                            Subject = objHelpDeskEmail.EmailType,
                            PlainTextContent = strPlainTextContent,
                            HtmlContent = strHtmlContent
                        };

                        // Send Email.
                        msg.AddTo(new EmailAddress(
                            objHelpDeskEmail.EmailAddress,
                            objHelpDeskEmail.EmailType)
                            );
                    }
                    else
                    {
                        Task.FromResult("Error - Bad TicketGuid");
                    }
                }

                client.SendEmailAsync(msg);
            }
            catch
            {
                // Could not send email.
                // Perhaps SENDGRID_APIKEY not set in 
                // appsettings.json
            }

            return Task.FromResult("");
        }

        // Utility

        #region private string GetHelpDeskTicketUrl(string TicketGuid)
        private string GetHelpDeskTicketUrl(string TicketGuid)
        {
            var request = httpContextAccessor.HttpContext.Request;

            var host = request.Host.ToUriComponent();

            var pathBase = request.PathBase.ToUriComponent();

            var url = $@"{request.Scheme}://" +
                $@"{host}{pathBase}/emailticketedit/" +
                $@"{TicketGuid}";

            return url;
        }
        #endregion
    }
}