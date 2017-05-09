﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerfectTicketMain.Models.User;
using PerfectTicketMain.Models.Ticket;

namespace PerfectTicketMain.Models.File
{

    class FileManager
    {
        // TODO: USER TICKET Table Name
        private static String USER_TABLE_FILENAME = "C:\\Users\\Gerry\\Desktop\\Perfetct Ticket Project\\user_table_max.txt";
        private static String TICKET_TABLE_FILENAME = "C:\\Users\\Gerry\\Desktop\\Perfetct Ticket Project\\ticket_table_max.txt";
        private static int USER_ATTRIBUTE_NUM = 5;
        private static int TICKET_ATTRIBUTE_NUM = 5;

        private TicketLab ticketLab;
        private UserLab userLab;

        public FileManager()
        {
            ticketLab = new TicketLab();
            userLab = new UserLab();
        }

        private UserInfo parseStringToUser(String consumerLine)
        {
            string[] stringSeparators = new string[] { "    " };      // 4 spaces
            String[] strs = consumerLine.Split(stringSeparators, USER_ATTRIBUTE_NUM, StringSplitOptions.None);
            int id = Int32.Parse(strs[0]);
            String name = strs[1];
            String password = strs[2];
            int priority = Int32.Parse(strs[3]);
            int balance = Int32.Parse(strs[4]);
            UserInfo user = new UserInfo(id, name, password, priority, balance);
            return user;
        }

        private TicketInfo parseStringToTicket(String ticketLine)
        {
            string[] stringSeparators = new string[] { "    " };      // 4 spaces
            String[] strs = ticketLine.Split(stringSeparators, TICKET_ATTRIBUTE_NUM, StringSplitOptions.None);
            int id = Int32.Parse(strs[0]);
            int start = Int32.Parse(strs[1]);
            int terminal = Int32.Parse(strs[2]);
            bool sold = Int32.Parse(strs[3]) == 1 ? true : false;
            int owner = Int32.Parse(strs[4]);
            TicketInfo ticket = new TicketInfo(id, start, terminal, sold, owner);
            return ticket;
        }


        // Read files
        public UserLab readUserFile()
        {
            String userLine;
            try
            {
                StreamReader sr = new StreamReader(USER_TABLE_FILENAME);
                userLine = sr.ReadLine();
                while (userLine != null)
                {
                    // write the lie to console window
                    Console.WriteLine(userLine);
                    UserInfo user = parseStringToUser(userLine);
                    // add user to user lab
                    userLab.addUserToLab(user);
                    // Read the next line
                    userLine = sr.ReadLine();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return userLab;
        }

        public TicketLab readTicketFile()
        {
            String ticketLine;
            try
            {
                StreamReader sr = new StreamReader(TICKET_TABLE_FILENAME);
                ticketLine = sr.ReadLine();
                while (ticketLine != null)
                {
                    // write the lie to console window
                    Console.WriteLine(ticketLine);
                    // parse string to ticket
                    TicketInfo ticket = parseStringToTicket(ticketLine);
                    // add ticket to ticket lab
                    ticketLab.addTicketToLab(ticket);
                    // Read the next line
                    ticketLine = sr.ReadLine();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return ticketLab;
        }

        // No deed write user file
        public bool writeUserFile(UserLab userLabToBeWrite)
        {
            this.userLab = userLabToBeWrite;
            //
            return false;
        }

        private String parseTicketToString(TicketInfo ticket)
        {
            String ticketString = String.Empty;
            ticketString += ticket.id;
            ticketString += "    ";
            ticketString += ticket.start;
            ticketString += "    ";
            ticketString += ticket.terminal;
            ticketString += "    ";
            ticketString += ticket.isSold ? "1" : "0";
            ticketString += "    ";
            ticketString += ticket.owner;
            return ticketString;
        }

        // write ticket file
        public bool writeTicketFile(TicketLab ticketLabToBeWrite)
        {
            this.ticketLab = ticketLabToBeWrite;
            List<TicketInfo> ticketList = ticketLabToBeWrite.getTicketList();
            String[] allTicketLines = new String[ticketList.Count];
            int i = 0;
            foreach (TicketInfo ticket in ticketList)
            {
                allTicketLines[i++] = parseTicketToString(ticket);
            }
            try
            {
                System.IO.File.WriteAllLines(@TICKET_TABLE_FILENAME, allTicketLines);
                return true;
            } catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return false;
            }
        }
    }
}