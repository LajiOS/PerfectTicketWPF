using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerfectTicketClient.Models.User;

namespace PerfectTicketClient.Models.PriorityQueue
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
            UserRequest front = userRequestSS.Min();
            userRequestSS.Remove(front);
            return front;
        }
    }

    public class ByUserPriority : IComparer<UserRequest>
    {
        public int Compare(UserRequest x, UserRequest y)
        {

            // Higher priority is at front
            if (x.userPriority > y.userPriority)
            {
                return -1;
            }
            else if (x.userPriority < y.userPriority)
            {
                return 1;
            }
            else
            {
                if (x.userID <= y.userID)
                    return -1;
                else return 1;
            }
        }
    }
}
