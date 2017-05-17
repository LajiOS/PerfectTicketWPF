using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using PerfectTicketClient.Models.MMemoryMappedFile;
using PerfectTicketClient.Models.SSemaphore;
using PerfectTicketClient.Models.Ticket;
using PerfectTicketClient.Models.PriorityQueue;
using PerfectTicketClient.Models.User;

namespace PerfectTicketClient.Models
{
    public class PerfectEngineClient
    {
        private static int STATION_NUM = 10;
        private static int TICKET_UNIT_PRICE = 10;

        private MemoryMappedFileManager memoryMappedFileManager;
        private SemaphoreManager semaphoreManager;

        private TicketLab ticketLab;
        private UserRequestPQ requestPQ;

        private TicketLab remainTicketLab;
        private TicketLab myTicketLab;
        private TicketRemainLab remainsLab;


        //private Thread requestHandleThread;

        private UserInfo currentUser;

        bool isAutoMode;

        public PerfectEngineClient(UserInfo user, bool isAutoMode)
        {
            memoryMappedFileManager = new MemoryMappedFileManager();
            // requestPQ = new UserRequestPQ();
            semaphoreManager = new SemaphoreManager();
            remainsLab = new TicketRemainLab();
            currentUser = user;
            this.isAutoMode = isAutoMode;
        }
        public TicketLab getMyTicketLab()
        {
            return myTicketLab;
        }
        public TicketLab getRemainTicketLab()
        {
            return remainTicketLab;
        }

        public TicketRemainLab getRemainLab()
        {
            return remainsLab;
        }

        private void refreshTicketLab()
        {
            ticketLab = memoryMappedFileManager.readTicketLabMMFChanges();
        }
        private void refreshPQ()
        {
            requestPQ = memoryMappedFileManager.readReqPQMMFChanges();
        }

        public void startEngine()
        {
            refreshTicketLab();
            refreshPQ();

            scanTicketLab();
        }

        public void autoModeScript()
        {
            Random rand = new Random();
            Thread.Sleep(rand.Next(800, 1005));

            // Refund a ticket
            addRequest(myTicketLab.getTicketList().First(), null, false);

            Thread.Sleep(rand.Next(1800, 3505));

            // Buy a ticket
            addRequest(null, remainsLab.getTicketRemainList().First(), true);

            Thread.Sleep(rand.Next(1000, 5000));
        }

        private void scanTicketLab()
        {
            myTicketLab = new TicketLab();
            remainTicketLab = new TicketLab();
            remainsLab = new TicketRemainLab();
            List<TicketInfo> ticketList = ticketLab.getTicketList();
            List<TicketRemain> remainList = remainsLab.getTicketRemainList();
            int start = 0;
            int terminal = 0;
            foreach (TicketInfo ticket in ticketList)
            {
                start = ticket.start;
                terminal = ticket.terminal;
                if (!ticket.isSold)
                {
                    TicketRemain ticketOld = remainList.Find(t => t.start == start && t.terminal == terminal);
                    TicketRemain ticketRemain = new TicketRemain(start, terminal, 1);
                    if (ticketOld != null)
                    {
                        ticketOld.count++;
                    } else
                    {
                        remainsLab.addTicketToLab(ticketRemain);
                    }
                    remainTicketLab.addTicketToLab(ticket);
                }
                else if (ticket.owner == currentUser.id)
                {
                    myTicketLab.addTicketToLab(ticket);
                }
            }
        }

        private bool judgeUseCanAfford(int start, int terminal)
        {
            int ticketPrice = Math.Abs(start - terminal) * TICKET_UNIT_PRICE;
            if (currentUser.balance >= ticketPrice)
            {
                currentUser.balance -= ticketPrice;
                // and then sell it
                return true;
            }
            else
            {
                return false;
            }
        }

        // Add Request
        public bool addRequest(TicketInfo refundTicket, TicketRemain buyTicket, bool buy)
        {
            if (buy)
            {
                if (judgeUseCanAfford(buyTicket.start, buyTicket.terminal))
                {
                    semaphoreManager.P_TicketSemFull(buyTicket.start, buyTicket.terminal);
                    UserRequest request = new UserRequest(currentUser.id, currentUser.priority, buyTicket.start, buyTicket.terminal, buy, 0);
                    requestPQ.addToPQ(request);
                    semaphoreManager.P_PQSemEmpty();
                    memoryMappedFileManager.mapUserReqPQToMMF(requestPQ);
                    semaphoreManager.V_PQSemFull();
                    semaphoreManager.V_TicketSemEmpty(buyTicket.start, buyTicket.terminal);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                semaphoreManager.P_TicketSemEmpty(refundTicket.start, refundTicket.terminal);
                UserRequest request = new UserRequest(currentUser.id, currentUser.priority, refundTicket.start, refundTicket.terminal, buy, refundTicket.id);
                requestPQ.addToPQ(request);
                semaphoreManager.P_PQSemEmpty();
                memoryMappedFileManager.mapUserReqPQToMMF(requestPQ);
                semaphoreManager.V_PQSemFull();
                semaphoreManager.V_TicketSemFull(refundTicket.start, refundTicket.terminal);
                int ticketPrice = Math.Abs(refundTicket.start - refundTicket.terminal) * TICKET_UNIT_PRICE;
                currentUser.balance += ticketPrice;
                return true;
            }
        }
    }
}
