using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectTicketMain.Models.Ticket
{
    public class TicketInfo
    {
        public static int MAX_TICKET_NUM = 100;

        public int id { get; set; }
        public int start { get; set; }
        public int terminal { get; set; }
        public bool isSold { get; set; }
        public int owner { get; set; }

        public TicketInfo(int id, int start, int terminal, bool isSold, int owner = 0)
        {
            this.id = id;
            this.start = start;
            this.terminal = terminal;
            this.isSold = isSold;
            this.owner = owner;
        }
    }

    // Only used in MemoryNappedFile
    public struct TicketInfoStruct
    {
        public int id { get; set; }
        public int start { get; set; }
        public int terminal { get; set; }
        public bool isSold { get; set; }
        public int owner { get; set; }

        public TicketInfoStruct(int id, int start, int terminal, bool isSold, int owner = 0)
        {
            this.id = id;
            this.start = start;
            this.terminal = terminal;
            this.isSold = isSold;
            this.owner = owner;
        }
    }
}
