#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SyncfusionHelpDeskClient.Shared
{
    public class HelpDeskTicket
    {
        public int Id { get; set; }
        [Required]
        public string TicketStatus { get; set; }
        [Required]
        public DateTime TicketDate { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage =
            "Description must be a minimum of 2 and maximum of 50 characters.")]
        public string TicketDescription { get; set; }
        [Required]
        [EmailAddress]
        public string TicketRequesterEmail { get; set; }
        public string TicketGuid { get; set; }

        public List<HelpDeskTicketDetail> HelpDeskTicketDetails { get; set; }
    }

    public class HelpDeskTicketDetail
    {
        public int Id { get; set; }
        public int HelpDeskTicketId { get; set; }
        public DateTime TicketDetailDate { get; set; }
        public string TicketDescription { get; set; }
    }
}