using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using PerfectTicketMain.Models.Ticket;
namespace PerfectTicketMain.Models.SSemaphore
{
    public class SemaphoreManager
    {
        private static int STATION_NUM = 10;
        private static int PQ_SEM_MAX_VALUE = 10;
        private static int TICKET_MAX_VALUE = 30;
        private static String PQ_FULL_SEM_NAME = "PQ_FULL";
        private static String PQ_EMPTY_SEM_NAME = "PQ_EMPTY";
        private static String TICKET_FULL_SEM_NAME = "TICKET_FULL";
        private static String TICKET_EMPTY_SEM_NAME = "TICKET_EMPTY";

        // reference to engine ticket left
        // use to create semaphores
        private int[,] ticketLeft;

        // TODO semaphore
        private Semaphore priorityQueueSemFull;
        private Semaphore priorityQueueSemEmpty;
        private Semaphore[] ticketLeftFullSems = new Semaphore[STATION_NUM*STATION_NUM];
        private Semaphore[] ticketLeftEmptySems = new Semaphore[STATION_NUM*STATION_NUM];


        public SemaphoreManager(int [,] ticketLeft)
        {
            // TODO semaphore access right
            priorityQueueSemFull = new Semaphore(0, PQ_SEM_MAX_VALUE, PQ_FULL_SEM_NAME);
            priorityQueueSemEmpty = new Semaphore(PQ_SEM_MAX_VALUE, PQ_SEM_MAX_VALUE, PQ_EMPTY_SEM_NAME);
            this.ticketLeft = ticketLeft;
            createTicketLeftSems();
        }

        public void P_PQSemFull()
        {
            priorityQueueSemFull.WaitOne();
        }

        public void V_PQSemFull()
        {
            priorityQueueSemFull.Release();
        }

        public void P_PQSemEmpty()
        {
            priorityQueueSemEmpty.WaitOne();
        }

        public void V_PQSemEmpty()
        {
            priorityQueueSemEmpty.Release();
        }

        private void createTicketLeftSems()
        {
            int remain;
            String ticketLeftFullSemName = String.Empty;
            ticketLeftFullSemName = TICKET_FULL_SEM_NAME;
            String ticketLeftEmptySemName = String.Empty;
            ticketLeftEmptySemName = TICKET_EMPTY_SEM_NAME;
            for (int start = 0; start < STATION_NUM; start++)
            {
                for (int terminal = 0; terminal < STATION_NUM; terminal++)
                {
                    remain = ticketLeft[start, terminal];
                    ticketLeftFullSemName += "#";
                    ticketLeftFullSemName += start;
                    ticketLeftFullSemName += "#";
                    ticketLeftFullSemName += terminal;
                    

                    ticketLeftEmptySemName += "#";
                    ticketLeftEmptySemName += start;
                    ticketLeftEmptySemName += "#";
                    ticketLeftEmptySemName += terminal;

                    ticketLeftFullSems[start*STATION_NUM+terminal] = new Semaphore(remain, TICKET_MAX_VALUE, ticketLeftFullSemName);
                    ticketLeftEmptySems[start*STATION_NUM+terminal] = new Semaphore(TICKET_MAX_VALUE-remain, TICKET_MAX_VALUE, ticketLeftEmptySemName);

                    ticketLeftFullSemName = TICKET_FULL_SEM_NAME;
                    ticketLeftEmptySemName = TICKET_EMPTY_SEM_NAME;
                }
            }
            return;
        }

        public void P_TicketSemFull(int start, int terminal)
        {
            ticketLeftFullSems[start * STATION_NUM + terminal].WaitOne();
        }

        public void V_TicketSemFull(int start, int terminal)
        {
            ticketLeftFullSems[start * STATION_NUM + terminal].Release();
        }

        public void P_TicketSemEmpty(int start, int terminal)
        {
            ticketLeftEmptySems[start * STATION_NUM + terminal].WaitOne();
        }

        public void V_TicketSemEmpty(int start, int terminal)
        {
            ticketLeftEmptySems[start * STATION_NUM + terminal].Release();
        }
    }
}
