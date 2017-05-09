using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;


using PerfectTicketMain.Models.Ticket;
using PerfectTicketMain.Models.User;
using PerfectTicketMain.Models.PrioriryQueue;

// TODO: share memory
namespace PerfectTicketMain.Models.MMemoryMappedFile
{
    // TODO: map file to share memory
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

        public Mutex getTicketMutex()
        {
            return ticketLabMMFMutex;
        }

        public Mutex getReqPQMutex()
        {
            return userRequestPQMMFMutex;
        }

        private bool createTicketMutex()
        {
            bool mutexCreated;
            ticketLabMMFMutex = new Mutex(true, TICKET_MUTEX, out mutexCreated);
            return mutexCreated;
        }

        private bool createReqPQMutex()
        {
            bool mutexCreated;
            userRequestPQMMFMutex = new Mutex(true, REQ_PQ_MUTEX, out mutexCreated);
            return mutexCreated;
        }

        public bool mapUserLabToMMF(UserLab userLab)
        {
            // Only map ticket to shm
            return false;
        }
        
        public bool firstMapTicketLabToMMF(TicketLab ticketLab)
        {
            List<TicketInfo> ticketList = ticketLab.getTicketList();
            TicketInfo ticket;
            TicketInfoStruct ticketStruct;
            long position = 0;

            ticketLabMMF = MemoryMappedFile.CreateNew(TICKET_MAP, TICKET_MAP_SIZE);
            if (createTicketMutex())
            {
                using (ticketLabMMVA = ticketLabMMF.CreateViewAccessor())
                {
                    ticketCount = ticketList.Count;
                    ticketLabMMVA.Write<int>(0, ref ticketCount);

                    for (int i = 0; i < ticketCount; i++)
                    {
                        ticket = ticketList[i];
                        ticketStruct = new TicketInfoStruct(ticket.id, ticket.start, ticket.terminal, ticket.isSold, ticket.owner);
                        position = i * Marshal.SizeOf(typeof(TicketInfoStruct));
                        ticketLabMMVA.Write(position+TICKET_MMF_OFFSET,ref ticketStruct);
                    }

                }
                // ReleaseMutex
                ticketLabMMFMutex.ReleaseMutex();
                return true;
            }
            return false;
        }

        public bool firstMapUserReqPQToMMF(UserRequestPQ requestPQ)
        {
            SortedSet<UserRequest> requestSet = requestPQ.getRequestSet();
            UserRequest request;
            UserRequestStruct requestStruct;
            long position = 0;
            int requestCount = requestSet.Count;


            userReqPQMMF = MemoryMappedFile.CreateOrOpen(REQ_PQ_MAP, REQ_PQ_MAP_SIZE);
            if (createReqPQMutex())
            {
                using (userReqPQMMVA = userReqPQMMF.CreateViewAccessor())
                {

                    // Write first byte
                    userReqPQMMVA.Write<int>(0, ref requestCount);
                    
                    // at first time requestSet.Count will be 0
                    // every time every user main get or put all to mmf
                    // main will dequeue(process) one request
                    // and then put new priority queue back to mmf
                    for (int i = 0; i < requestSet.Count; i++)
                    {
                        request = requestSet.ElementAt(i);
                        requestStruct = new UserRequestStruct(
                            request.userID, request.userPriority, request.startStation, request.terminalStation, request.isBuy, request.ticketID);
                        position = i * Marshal.SizeOf(typeof(UserRequestStruct));
                        userReqPQMMVA.Write(position+REQ_PQ_MMF_OFFSET, ref requestStruct);
                    }
                }

                // Relase mutex
                userRequestPQMMFMutex.ReleaseMutex();
                return true;
            }
            return false;
        }

        public bool mapTicketLabChangesToMMF(TicketLab ticketLab)
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
                    ticketLabMMVA.Write(0, ref ticketCount);

                    for (int i = 0; i < ticketCount; i++)
                    {
                        ticket = ticketList[i];
                        ticketStruct = new TicketInfoStruct(ticket.id, ticket.start, ticket.terminal, ticket.isSold, ticket.owner);
                        position = i * Marshal.SizeOf(typeof(TicketInfoStruct));
                        ticketLabMMVA.Write(position+TICKET_MMF_OFFSET, ref ticketStruct);
                    }

                }

                // ReleaseMutex
                ticketLabMMFMutex.ReleaseMutex();
                return true;
            }
            return false;
        }

        public bool mapUserReqPQChangesToMMF(UserRequestPQ requestPQ)
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

                } catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            
            ticketLabMMFMutex.ReleaseMutex();
            return ticketLab;
        }

        public UserLab readUserLabMMFChanges()
        {
            // Only map ticketlab to shm
            return null;
        }


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
                        userReqPQMMVA.Read(position+REQ_PQ_MMF_OFFSET, out requestStruct);
                        request = new UserRequest(
                            requestStruct.userID, requestStruct.userPriority, requestStruct.startStation, requestStruct.terminalStation, requestStruct.isBuy, requestStruct.ticketID);
                        requestPQ.addToPQ(request);
                        count++;
                    }

                } catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }

            userRequestPQMMFMutex.ReleaseMutex();

            return requestPQ;
        }

        // engine have the responsibility to arrange which ticket
        // change the mmf
        private bool sellATicket(TicketInfo toBeSoldTicket )
        {
            ticketLabMMFMutex.WaitOne();

            TicketInfoStruct ticketStruct = new TicketInfoStruct(
                toBeSoldTicket.id, toBeSoldTicket.start, toBeSoldTicket.terminal, toBeSoldTicket.isSold, toBeSoldTicket.owner);
            // the ticket id is start from 1
            long position = (ticketStruct.id - 1) * Marshal.SizeOf(typeof(TicketInfoStruct));

            using (ticketLabMMVA = ticketLabMMF.CreateViewAccessor())
            {
                try
                {
                    ticketLabMMVA.Write(position+TICKET_MMF_OFFSET, ref ticketStruct);
                } catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                    ticketLabMMFMutex.ReleaseMutex();
                    return false;
                }

            }

            ticketLabMMFMutex.ReleaseMutex();
            return true;
        }  
    }
}
