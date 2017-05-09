using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectTicketMain.Models.Ticket
{
    public class TicketLab
    {
        private List<TicketInfo> ticketList;

        public TicketLab()
        {
            ticketList = new List<TicketInfo>();
        }

        public void addTicketToLab(TicketInfo ticket)
        {
            ticketList.Add(ticket);
        }

        public List<TicketInfo> getTicketList()
        {
            return ticketList;
        }
    }
}
