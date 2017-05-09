using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectTicketClient.Models.Ticket
{
    public class TicketRemain
    {
        public int start { get; set; }
        public int terminal { get; set; }
        public int count { get; set; }

        public TicketRemain(int start, int terminal, int count)
        {
            this.start = start;
            this.terminal = terminal;
            this.count = count;
        }
    }
}
