using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectTicketClient.Models.User
{
    public class UserRequest
    {
        public int userID { get; set; }
        public int userPriority { get; set; }
        public int startStation { get; set; }
        public int terminalStation { get; set; }

        // order or refound
        public bool isBuy { get; set; }
        // if request is refund ticket id will be usefull
        public int ticketID { get; set; }

        public UserRequest(int userID, int priority, int start, int terminal, bool buy, int ticketID)
        {
            this.userID = userID;
            userPriority = priority;
            startStation = start;
            terminalStation = terminal;
            isBuy = buy;
            this.ticketID = ticketID;
        }
    }

    public struct UserRequestStruct
    {
        public int userID { get; set; }
        public int userPriority { get; set; }
        public int startStation { get; set; }
        public int terminalStation { get; set; }
        public bool isBuy { get; set; }
        public int ticketID { get; set; }

        public UserRequestStruct(int userID, int priority, int start, int terminal, bool buy, int ticketID)
        {
            this.userID = userID;
            userPriority = priority;
            startStation = start;
            terminalStation = terminal;
            isBuy = buy;
            this.ticketID = ticketID;
        }
    }
}
