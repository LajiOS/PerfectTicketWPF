using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectTicketMain.Models.User
{
    public class UserInfo
    {
        public static int HIGH_PRIORITY = 0x08;
        public static int MIDDLE_PRIORITY = 0x04;
        public static int LOW_PRIORITY = 0x02;

        public int id { get; set; }
        public String name { get; set; }
        public String password { get; set; }
        public int priority { get; set; }
        public int balance { get; set; }

        public UserInfo(int id, String name, String password, int priority, int balance)
        {
            this.id = id;
            this.name = name;
            this.password = password;
            this.priority = priority;
            this.balance = balance;
        }
    }

    public struct UserInfoStruct
    {
        public int id { get; set; }
        public String name { get; set; }
        public String password { get; set; }
        public int priority { get; set; }
        public int balance { get; set; }

        public UserInfoStruct(int id, String name, String password, int priority, int balance)
        {
            this.id = id;
            this.name = name;
            this.password = password;
            this.priority = priority;
            this.balance = balance;
        }
    }
}
