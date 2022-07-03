#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SyncfusionHelpDesk.Data
{
    [ApiController]
    [Route("[controller]")]
    public class SyncfusionHelpDeskController : ControllerBase
    {
        private readonly SyncfusionHelpDeskContext _context;

        public SyncfusionHelpDeskController(
            SyncfusionHelpDeskContext context)
        {
            _context = context;
        }

        // Only an Administrator can query.
        [Authorize(Roles = "Administrators")]
        [HttpGet]
        public object Get()
        {
            StringValues Skip;
            StringValues Take;
            StringValues OrderBy;

            // Filter the data.
            var TotalRecordCount = _context.HelpDeskTickets.Count();

            int skip = (Request.Query.TryGetValue("$skip", out Skip))
                ? Convert.ToInt32(Skip[0]) : 0;

            int top = (Request.Query.TryGetValue("$top", out Take))
                ? Convert.ToInt32(Take[0]) : TotalRecordCount;

            string orderby =
                (Request.Query.TryGetValue("$orderby", out OrderBy))
                ? OrderBy.ToString() : "TicketDate";

            // Handle OrderBy direction.
            if (orderby.EndsWith(" desc"))
            {
                orderby = orderby.Replace(" desc", "");

                return new
                {
                    Items = _context.HelpDeskTickets
                    .OrderByDescending(orderby)
                    .Skip(skip)
                    .Take(top),
                    Count = TotalRecordCount
                };
            }
            else
            {
                System.Reflection.PropertyInfo prop =
                    typeof(HelpDeskTickets).GetProperty(orderby);

                return new
                {
                    Items = _context.HelpDeskTickets
                    .OrderBy(orderby)
                    .Skip(skip)
                    .Take(top),
                    Count = TotalRecordCount
                };
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public Task
            Post(HelpDeskTickets newHelpDeskTickets)
        {
            // Add a new Help Desk Ticket.
            _context.HelpDeskTickets.Add(newHelpDeskTickets);
            _context.SaveChanges();

            return Task.FromResult(newHelpDeskTickets);
        }

        [HttpPut]
        [AllowAnonymous]
        public Task
            PutAsync(HelpDeskTickets UpdatedHelpDeskTickets)
        {
            // Get the existing record.
            // Note: Caller must have the TicketGuid.
            var ExistingTicket =
                _context.HelpDeskTickets
                .Where(x => x.TicketGuid ==
                UpdatedHelpDeskTickets.TicketGuid)
                .FirstOrDefault();

            if (ExistingTicket != null)
            {
                ExistingTicket.TicketDate =
                    UpdatedHelpDeskTickets.TicketDate;

                ExistingTicket.TicketDescription =
                    UpdatedHelpDeskTickets.TicketDescription;

                ExistingTicket.TicketGuid =
                    UpdatedHelpDeskTickets.TicketGuid;

                ExistingTicket.TicketRequesterEmail =
                    UpdatedHelpDeskTickets.TicketRequesterEmail;

                ExistingTicket.TicketStatus =
                    UpdatedHelpDeskTickets.TicketStatus;

                // Insert any new TicketDetails.
                if (UpdatedHelpDeskTickets.HelpDeskTicketDetails != null)
                {
                    foreach (var item in
                        UpdatedHelpDeskTickets.HelpDeskTicketDetails)
                    {
                        if (item.Id == 0)
                        {
                            // Create new HelpDeskTicketDetails record.
                            HelpDeskTicketDetails newHelpDeskTicketDetails =
                                new HelpDeskTicketDetails();
                            newHelpDeskTicketDetails.HelpDeskTicketId =
                                UpdatedHelpDeskTickets.Id;
                            newHelpDeskTicketDetails.TicketDetailDate =
                                DateTime.Now;
                            newHelpDeskTicketDetails.TicketDescription =
                                item.TicketDescription;

                            _context.HelpDeskTicketDetails
                                .Add(newHelpDeskTicketDetails);
                        }
                    }
                }

                _context.SaveChanges();
            }
            else
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        // Only an Administrator can delete.
        [Authorize(Roles = "Administrators")]
        [HttpDelete]
        public Task
            Delete(
            string HelpDeskTicketGuid)
        {
            // Get the existing record.
            var ExistingTicket =
                _context.HelpDeskTickets
                .Include(x => x.HelpDeskTicketDetails)
                .Where(x => x.TicketGuid == HelpDeskTicketGuid)
                .FirstOrDefault();

            if (ExistingTicket != null)
            {
                // Delete the Help Desk Ticket.
                _context.HelpDeskTickets.Remove(ExistingTicket);
                _context.SaveChanges();
            }
            else
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }

    // From: https://bit.ly/30ypMCp
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(
            this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(
            this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> ToLambda<T>(
            string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }
    }
}