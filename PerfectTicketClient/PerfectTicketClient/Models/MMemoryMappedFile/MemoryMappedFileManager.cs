using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

using PerfectTicketClient.Models.PriorityQueue;
using PerfectTicketClient.Models.Ticket;
using PerfectTicketClient.Models.User;

namespace PerfectTicketClient.Models.MMemoryMappedFile
{
    public class MemoryMappedFileManager
    {
        private static String TICKET_MUTEX = "TicketMutex";
        private static String REQ_PQ_MUTEX = "PQMutext";
        private static String TICKET_MAP = "TicketMap";
        private static String REQ_PQ_MAP = "PQMap";

        private static long TICKET_MAP_SIZE = 1024 * 1024;
        private static long REQ_PQ_MAP_SIZE = 1024;

        // use it determine the end of mmf
        private static int REQ_PQ_MMF_OFFSET = 4;

        // use it determine the end of mmf
        private static int TICKET_MMF_OFFSET = 4;

        private Mutex ticketLabMMFMutex;
        private Mutex userRequestPQMMFMutex;
        private MemoryMappedFile ticketLabMMF;
        private MemoryMappedFile userReqPQMMF;
        private MemoryMappedViewAccessor ticketLabMMVA;
        private MemoryMappedViewAccessor userReqPQMMVA;

        private int ticketCount;
        private int requestCount;

        public MemoryMappedFileManager()
        {
            openTicketMutex();
            openReqPQMutex();
            openTicketLabMMF();
            openUserReqPQMMF();
        }
        
        public Mutex getTicketMutex()
        {
            return ticketLabMMFMutex;
        }

        public Mutex getReqPQMutex()
        {
            return userRequestPQMMFMutex;
        }

        private bool openTicketMutex()
        {
            try
            {
                ticketLabMMFMutex = Mutex.OpenExisting(TICKET_MUTEX);
                return true;
            } catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return false;
        }

        private bool openReqPQMutex()
        {
            try
            {
                userRequestPQMMFMutex = Mutex.OpenExisting(REQ_PQ_MUTEX);
                return true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return false;
        }

        private bool openTicketLabMMF()
        {
            try
            {
                ticketLabMMF = MemoryMappedFile.OpenExisting(TICKET_MAP);
                return true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return false;
        }

        private bool openUserReqPQMMF()
        {
            try 
            {
                userReqPQMMF = MemoryMappedFile.OpenExisting(REQ_PQ_MAP);
                return true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return false;
        }

        // Clien can not use
        public bool mapTicketLabToMMF(TicketLab ticketLab)
        {
            List<TicketInfo> ticketList = ticketLab.getTicketList();
            TicketInfo ticket;
            TicketInfoStruct ticketStruct;
            long position = 0;
            if (ticketLabMMFMutex != null)
            {
                ticketLabMMFMutex.WaitOne();
                using (ticketLabMMVA = ticketLabMMF.CreateViewAccessor())
                {
                    ticketCount = ticketList.Count;
                    for (int i = 0; i < ticketCount; i++)
                    {
                        ticket = ticketList[i];
                        ticketStruct = new TicketInfoStruct(ticket.id, ticket.start, ticket.terminal, ticket.isSold, ticket.owner);
                        position = i * Marshal.SizeOf(typeof(TicketInfoStruct));
                        ticketLabMMVA.Write(position, ref ticketStruct);
                    }

                }

                // ReleaseMutex
                ticketLabMMFMutex.ReleaseMutex();
                return true;
            }
            return false;
        }

        public bool mapUserReqPQToMMF(UserRequestPQ requestPQ)
        {
            SortedSet<UserRequest> requestSet = requestPQ.getRequestSet();
            UserRequest request;
            UserRequestStruct requestStruct;
            long position = 0;
            int requestCount = requestSet.Count;

            if (userRequestPQMMFMutex != null)
            {
                userRequestPQMMFMutex.WaitOne();

                using (userReqPQMMVA = userReqPQMMF.CreateViewAccessor())
                {
                    userReqPQMMVA.Write(0, ref requestCount);

                    for (int i = 0; i < requestCount; i++)
                    {
                        request = requestSet.ElementAt(i);
                        requestStruct = new UserRequestStruct(
                            request.userID, request.userPriority, request.startStation, request.terminalStation, request.isBuy, request.ticketID);
                        position = i * Marshal.SizeOf(typeof(UserRequestStruct));
                        userReqPQMMVA.Write(position + REQ_PQ_MMF_OFFSET, ref requestStruct);
                    }

                    userRequestPQMMFMutex.ReleaseMutex();
                }
                return true;
            }
            return false;
        }


        // TODO read ticket lab finished
        public TicketLab readTicketLabMMFChanges()
        {
            ticketLabMMFMutex.WaitOne();

            TicketLab ticketLab = new TicketLab();
            TicketInfo ticket;
            long position = 0;
            int count = 0;

            using (ticketLabMMVA = ticketLabMMF.CreateViewAccessor())
            {
                TicketInfoStruct ticketStruct = new TicketInfoStruct();
                try
                {
                    ticketLabMMVA.Read(0, out ticketCount);
                    while (count < ticketCount)
                    {
                        position = count * Marshal.SizeOf(typeof(TicketInfoStruct));
                        ticketLabMMVA.Read(position+TICKET_MMF_OFFSET, out ticketStruct);
                        ticket = new TicketInfo(
                            ticketStruct.id, ticketStruct.start, ticketStruct.terminal, ticketStruct.isSold, ticketStruct.owner);
                        ticketLab.addTicketToLab(ticket);
                        count++;
                    }

                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }

            ticketLabMMFMutex.ReleaseMutex();
            return ticketLab;
        }

        //public UserLab readUserLabMMFChanges()
        //{
        //    // Only map ticketlab to shm
        //    return null;
        //}


        public UserRequestPQ readReqPQMMFChanges()    // get requestCount by semaphore
        {
            userRequestPQMMFMutex.WaitOne();

            UserRequestPQ requestPQ = new UserRequestPQ();
            UserRequest request;
            long position = 0;
            int count = 0;
            using (userReqPQMMVA = userReqPQMMF.CreateViewAccessor())
            {
                UserRequestStruct requestStruct = new UserRequestStruct();
                try
                {
                    // Read count
                    userReqPQMMVA.Read(0, out requestCount);
                    while (count < requestCount)
                    {
                        position = count * Marshal.SizeOf(typeof(UserRequestStruct));
                        userReqPQMMVA.Read(position + REQ_PQ_MMF_OFFSET, out requestStruct);
                        request = new UserRequest(
                            requestStruct.userID, requestStruct.userPriority, requestStruct.startStation, requestStruct.terminalStation, requestStruct.isBuy, requestStruct.ticketID);
                        requestPQ.addToPQ(request);
                        count++;
                    }

                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }

            userRequestPQMMFMutex.ReleaseMutex();

            return requestPQ;
        }
    }
}
