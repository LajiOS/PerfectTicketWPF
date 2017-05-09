using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerfectTicketMain.Models.User;
namespace PerfectTicketMain.Models.PrioriryQueue
{
    public class UserRequestPQ
    {
        private SortedSet<UserRequest> userRequestSS;

        public UserRequestPQ()
        {
            userRequestSS = new SortedSet<UserRequest>(new ByUserPriority());
        }
        public SortedSet<UserRequest> getRequestSet()
        {
            return userRequestSS;
        }

        public void addToPQ(UserRequest userRequest)
        {
            userRequestSS.Add(userRequest);
        }

        // Get the front request in the UserRequestPQ
        public UserRequest dequeue()
        {
            UserRequest front = userRequestSS.Max;
            userRequestSS.Remove(front);
            return front;
        }
    }

    internal class ByUserPriority : IComparer<UserRequest>
    {
        public int Compare(UserRequest x, UserRequest y)
        {
            int result = x.userPriority.CompareTo(y.userPriority);
            if (result == 0)
                result = 0-x.userID.CompareTo(y.userID);
            if (result == 0)
                result = 0-x.ticketID.CompareTo(y.ticketID);
            return result;
        }
    }
}
