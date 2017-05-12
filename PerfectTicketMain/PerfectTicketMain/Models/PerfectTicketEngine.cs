using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using PerfectTicketMain.Models.MMemoryMappedFile;
using PerfectTicketMain.Models.File;
using PerfectTicketMain.Models.User;
using PerfectTicketMain.Models.Ticket;
using PerfectTicketMain.Models.PrioriryQueue;
using PerfectTicketMain.Models.SSemaphore;

namespace PerfectTicketMain.Models
{
    // PerfectTicketEngine
    public class PerfectTicketEngine
    {
        // sub wpf file name
        //private static string SUB_WPF_FILENAME = "C:\\Users\\Gerry\\Documents\\Visual Studio 2017\\Projects\\PerfectTicketClient\\PerfectTicketClient\\bin\\Debug\\PerfectTicketClient.exe";
        private static string SUB_WPF_FILENAME = "PerfectTicketClient.exe";

        // Station number
        private static int STATION_NUM = 10;

        private static int TICKET_UNIT_PRICE = 10;

        // ONLY used in semaphore manager MAIN
        // use semaphore to control this
        private int[,] ticketLeft;  

        private MemoryMappedFileManager memoryMappedFileManager;
        private FileManager fileManager;
        private SemaphoreManager semaphoreManager;

        private UserLab userLab;
        private TicketLab ticketLab;

        private UserRequestPQ requestPQ;

        private Thread requestHandleThread;
        

        public PerfectTicketEngine()
        {
            memoryMappedFileManager = new MemoryMappedFileManager();
            fileManager = new FileManager();
            requestPQ = new UserRequestPQ();

            ticketLeft = new int[STATION_NUM, STATION_NUM];
        }

        public void readFiles()
        {
            userLab = fileManager.readUserFile();
            ticketLab = fileManager.readTicketFile();
        }
        
        public UserLab getUserLab()
        {
            return userLab;
        }

        public TicketLab getTicketLab()
        {
            return ticketLab;
        }

        public UserRequestPQ getUserReqPQ()
        {
            return requestPQ;
        }

        private void calculateRemainTicket()
        {
            List<TicketInfo> ticketList = ticketLab.getTicketList();
            foreach (TicketInfo ticket in ticketList)
            {
                ticketLeft[ticket.start, ticket.terminal]++;
            }
        }

        private void firstMapLabsAndPQ()
        {
            memoryMappedFileManager.mapUserLabToMMF(userLab);
            memoryMappedFileManager.firstMapTicketLabToMMF(ticketLab);
            memoryMappedFileManager.firstMapUserReqPQToMMF(requestPQ);
        }

        private void mapLabsAndPQChanges()
        {
            memoryMappedFileManager.mapTicketLabChangesToMMF(ticketLab);
            memoryMappedFileManager.mapUserReqPQChangesToMMF(requestPQ);
        }

        private HashSet<UserInfo> randomChooseUsers(int userNum)
        {
            List<UserInfo> userList = userLab.getUserList();
            HashSet<UserInfo> randomUserList = new HashSet<UserInfo>();
            Random rand = new Random();
            while(randomUserList.Count < userNum)
            {
                int userID = rand.Next(1, userList.Count+1);
                UserInfo randomUser = userList.Find(u => u.id == userID);
                randomUserList.Add(randomUser);
            }
            return randomUserList;
        }

        private void callWpf(UserInfo user, bool isAutoMode)
        {
            var args = string.Empty;
            // Convert values
            try
            {
                args = Convert.ToString(isAutoMode);
                args += "#";
                args += Convert.ToString(user.id);
                args += "#";        // argument sepearater
                args += user.name;
                args += "#";
                args += user.password;
                args += "#";
                args += Convert.ToString(user.priority);
                args += "#";
                args += Convert.ToString(user.balance);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }

            // String[] args = new String();
            // args[1] = user.id + user.name + user.password + user.priority + user.balance;
            ProcessStartInfo BOM = new ProcessStartInfo();
            BOM.FileName = @SUB_WPF_FILENAME;
            BOM.Arguments = args;
            Process.Start(BOM);

            // TODO wait sub process? no..
            // HANLDE waitOne() method to do it
        }

        private void callUserClients(int userNum, bool isAutoMode)
        {
            List<UserInfo> userList = userLab.getUserList();
            if (userList.Count < userNum)
            {
                userNum = userList.Count;
            }

            HashSet<UserInfo> choosenUsers = randomChooseUsers(userNum);
            foreach(UserInfo user in choosenUsers)
            {
                callWpf(user, isAutoMode);
            }
        }
        
        // Create memorymappedfile and semaphore
        public void startEngine(int userNum, bool isAutoMode)
        {
            callUserClients(userNum, isAutoMode);

            // Create mmf
            firstMapLabsAndPQ();

            // Create semaphore
            calculateRemainTicket();
            semaphoreManager = new SemaphoreManager(ticketLeft);

            // TODO Create sub threading to listen request priority queue
            // And handle request
            // Maybe now is better
            requestHandleThread = new Thread(new ThreadStart(handleRueuestThreadMethod));
            requestHandleThread.Start();
        }

        public void closeEngine()
        {
            fileManager.writeTicketFile(ticketLab);
            fileManager.writeUserFile(userLab);
        }

        public void refreshTicketData()
        {
            ticketLab = memoryMappedFileManager.readTicketLabMMFChanges();
        }

        public bool producerAddRequest(int start, int terminal)
        {
            semaphoreManager.P_TicketSemEmpty(start, terminal);
            TicketInfo newTicket = new TicketInfo(ticketLab.getTicketList().Count+1, start, terminal, false, 0);
            ticketLab.addTicketToLab(newTicket);
            memoryMappedFileManager.mapTicketLabChangesToMMF(ticketLab);
            semaphoreManager.V_TicketSemFull(start, terminal);      
            return true;
        }

        // SUB THREADING |-----
        private bool sellChosenTicket(TicketInfo chosenTicket, int userID)
        {
            UserInfo user = userLab.getUserList().Find(u => u.id == userID);
            chosenTicket.isSold = true;
            chosenTicket.owner = userID;
            user.balance -= Math.Abs(chosenTicket.start - chosenTicket.terminal) * TICKET_UNIT_PRICE;
            return true;
        }
        private TicketInfo chooseTicketToSell(int start, int terminal)
        {
            TicketInfo chosenTicket;
            // ticketLeft[start, terminal]--;
            List<TicketInfo> ticketList = ticketLab.getTicketList();
            chosenTicket = ticketList.Find(
                e => e.isSold == false && e.start == start && e.terminal == terminal);
            return chosenTicket;
        }
        private bool refundTicket(int ticketID, int userID)
        {
            List<TicketInfo> ticketList = ticketLab.getTicketList();
            TicketInfo ticketToBeRefund;
            ticketToBeRefund = ticketList.Find(e =>
                e.id == ticketID && e.isSold == true && e.owner == userID);
            if (ticketToBeRefund != null)
            {
                ticketToBeRefund.isSold = false;
                ticketToBeRefund.owner = 0;
                UserInfo user = userLab.getUserList().Find(u => u.id == userID);
                user.balance += Math.Abs(ticketToBeRefund.start - ticketToBeRefund.terminal) * TICKET_UNIT_PRICE;
                return true;
            } else
            {
                return false;
            }
        }

        private int handleRequest(UserRequest request)
        {
            TicketInfo requsetedTicket; // buy or refund
            int start = request.startStation;
            int terminal = request.terminalStation;
            int userID = request.userID;
            bool buy = request.isBuy;
            int ticketID = request.ticketID;
            if (buy)
            {
                requsetedTicket = chooseTicketToSell(start, terminal);
                sellChosenTicket(requsetedTicket, userID);
            } else
            {
                refundTicket(ticketID, userID);
            }

            // call to change memory mapped file
            mapLabsAndPQChanges();

            return 0;
        }

        private void handleRueuestThreadMethod()
        {
            while (true)
            {
                // P Full
                semaphoreManager.P_PQSemFull();

                // get the highest priority request and handle
                requestPQ = memoryMappedFileManager.readReqPQMMFChanges();
                UserRequest requestToBeHanle = requestPQ.dequeue();
                handleRequest(requestToBeHanle);

                // V Empty
                semaphoreManager.V_PQSemEmpty();
            }
        }

        // ----|
    }
}
