using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PerfectTicketClient.Models.SSemaphore
{
    public class SemaphoreManager
    {
        private static int STATION_NUM = 10;
        private static int PQ_SEM_MAX_VALUE = 10;
        private static String PQ_FULL_SEM_NAME = "PQ_FULL";
        private static String PQ_EMPTY_SEM_NAME = "PQ_EMPTY";
        private static String TICKET_FULL_SEM_NAME = "TICKET_FULL";
        private static String TICKET_EMPTY_SEM_NAME = "TICKET_EMPTY";

        // reference to engine ticket left
        // use to create semaphores

        // TODO semaphore
        private Semaphore priorityQueueSemFull;
        private Semaphore priorityQueueSemEmpty;
        private Semaphore[] ticketLeftFullSems = new Semaphore[STATION_NUM * STATION_NUM];
        private Semaphore[] ticketLeftEmptySems = new Semaphore[STATION_NUM * STATION_NUM];


        public SemaphoreManager()
        {
            // TODO semaphore access right
            try
            {
                priorityQueueSemFull = Semaphore.OpenExisting(PQ_FULL_SEM_NAME);
                priorityQueueSemEmpty = Semaphore.OpenExisting(PQ_EMPTY_SEM_NAME);
            } catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            openTicketLeftSems();
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

        public void openTicketLeftSems()
        {
            String ticketLeftFullSemName = String.Empty;
            ticketLeftFullSemName = TICKET_FULL_SEM_NAME;
            String ticketLeftEmptySemName = String.Empty;
            ticketLeftEmptySemName = TICKET_EMPTY_SEM_NAME;
            for (int start = 0; start < STATION_NUM; start++)
            {
                for (int terminal = 0; terminal < STATION_NUM; terminal++)
                {
                    ticketLeftFullSemName += "#";
                    ticketLeftFullSemName += start;
                    ticketLeftFullSemName += "#";
                    ticketLeftFullSemName += terminal;

                    ticketLeftEmptySemName += "#";
                    ticketLeftEmptySemName += start;
                    ticketLeftEmptySemName += "#";
                    ticketLeftEmptySemName += terminal;
                    try
                    {
                        ticketLeftFullSems[start * STATION_NUM + terminal] = Semaphore.OpenExisting(ticketLeftFullSemName);
                        ticketLeftEmptySems[start * STATION_NUM + terminal] = Semaphore.OpenExisting(ticketLeftEmptySemName);
                        Console.WriteLine(start + "#" + terminal + "sem opened----------------");
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }

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
