using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectTicketMain.Models.User
{
    public class UserLab
    {
        private List<UserInfo> userLab;

        public UserLab()
        {
            userLab = new List<UserInfo>();
        }

        public void addUserToLab(UserInfo user)
        {
            userLab.Add(user);
        }

        public List<UserInfo> getUserList()
        {
            return userLab;
        }

    }
}
