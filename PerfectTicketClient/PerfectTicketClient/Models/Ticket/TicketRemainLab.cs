using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectTicketClient.Models.Ticket
{
    public class TicketRemainLab
    {
        private List<TicketRemain> ticketRemainList;

        public TicketRemainLab()
        {
            ticketRemainList = new List<TicketRemain>();
        }

        public void addTicketToLab(TicketRemain ticketRemain)
        {
            ticketRemainList.Add(ticketRemain);
        }

        public List<TicketRemain> getTicketRemainList()
        {
            return ticketRemainList;
        }
    }
}
