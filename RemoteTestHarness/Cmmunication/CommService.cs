/////////////////////////////////////////////////////////////////////
// CommService.cs - Communicator Service                           //
// ver 1.0                                                         //
// Karthik Palepally Muniyappa                                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defindes a Sender class and Receiver class that
 * manage all of the details to set up a WCF channel.
 * 
 * Required Files:
 * ---------------
 * CommService.cs, ICommunicator, BlockingQueue.cs, Messages.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 20 Nov 2016
 * - first release
 */
using CS_BlockingQueue;
using Messages;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
namespace Cmmunication
{
    public class Receiver<T> : ICommunicator
    {
        static BlockingQueue<Message> rcvBlockingQ = null;
        ServiceHost service = null;
       public delegate Stream downloadFileDelegate(string filename);
        public delegate void uploadFileDelegate(FileTransferMessage msg);
       static public downloadFileDelegate downloadFileHandler = null;
       static public uploadFileDelegate uploadFileHandler = null;
        public string name { get; set; }
        public Receiver()
        {
            if (rcvBlockingQ == null)
                rcvBlockingQ = new BlockingQueue<Message>();
        }
        //download files from repository, only repository uses this 
        public Stream downLoadFile(string filename)
        {
            Stream fileStream = downloadFileHandler?.Invoke(filename);
            return fileStream;
        }
        //upload files from repository, only repository uses this 
        public void upLoadFile(FileTransferMessage msg)
        {
            uploadFileHandler?.Invoke(msg);
        }
        public void Close()
        {
            service.Close();
        }
        //  Create ServiceHost for Communication service
        public void CreateRecvChannel(string address)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(address);
            service = new ServiceHost(typeof(Receiver<T>), baseAddress);
            service.AddServiceEndpoint(typeof(ICommunicator), binding, baseAddress);
            service.Open();
            Console.Write("\n  Service is open listening on {0}", address);
        }
        // Implement service method to receive messages from other Peers
        public void PostMessage(Message msg)
        {
            Console.WriteLine("******Requirement 10 : Received msg {0} using wcf",msg.type);
            Console.WriteLine("*****Requirement 4 : service enquing message ");
            Console.WriteLine(msg.body);
            DateTime timeNow = DateTime.Now;
            double comnDelay = timeNow.Subtract(msg.time).Milliseconds;
            Console.WriteLine("******Requirement 12: Communication latency for msg {0} is {1} millisecond", msg.type, comnDelay.ToString());
            rcvBlockingQ.enQ(msg);
        }
        // Implement service method to extract messages from other Peers.
        // This will often block on empty queue, so user should provide
        // read thread.
        public Message GetMessage()
        {
            return rcvBlockingQ.deQ();
        }
        public Thread start(ThreadStart rcvThreadProc)
        {
            Thread rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            return rcvThread;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Sender is client of another Peer's Communication service

    public class Sender
    {
        public string name { get; set; }
        ICommunicator channel;
        string lastError = "";
        BlockingQueue<Message> sndBlockingQ = null;
        Thread sndThrd = null;
        int tryCount = 0, MaxCount = 10;
        string currEndpoint = "";
        //----< processing for send thread >-----------------------------
        void ThreadProc()
        {
            tryCount = 0;
            while (true)
            {
                Message msg = sndBlockingQ.deQ();
                if (msg.to != currEndpoint)
                {
                    currEndpoint = msg.to;
                    CreateSendChannel(currEndpoint);
                }
                while (true)
                {
                    try
                    {
                        Console.WriteLine("***************Requirement 10***********");
                        Console.WriteLine("Posting Message {0} using WCF",msg.type);
                        msg.time = DateTime.Now;
                        channel.PostMessage(msg);
                        Console.Write("\n  posted message from {0} to {1}", name, msg.to);
                        tryCount = 0;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n  connection failed", ex);
                        if (++tryCount < MaxCount)
                            Thread.Sleep(100);
                        else
                        {
                            Console.Write("\n  {0}", "can't connect\n");
                            currEndpoint = "";
                            tryCount = 0;
                            break;
                        }
                    }
                }
                if (msg.body == "quit")
                    break;
            }
        }
        //----< initialize Sender >--------------------------------------
        public Sender()
        {
            sndBlockingQ = new BlockingQueue<Message>();
            sndThrd = new Thread(ThreadProc);
            sndThrd.IsBackground = true;
            sndThrd.Start();
        }
        //----< Create proxy to another Peer's Communicator >------------
        public void CreateSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, address);
            channel = factory.CreateChannel();
            Console.Write("\n  service proxy created for {0}", address);
        }
        //----< posts message to another Peer's queue >------------------
        /*
         *  This is a non-service method that passes message to
         *  send thread for posting to service.
         */
        public void PostMessage(Message msg)
        {
            sndBlockingQ.enQ(msg);
        }
        public string GetLastError()
        {
            string temp = lastError;
            lastError = "";
            return temp;
        }
        //----< closes the send channel >--------------------------------
        public void Close()
        {
            ChannelFactory<ICommunicator> temp = (ChannelFactory<ICommunicator>)channel;
            temp.Close();
        }
        static void Main(string[] args)
        {
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Comm class simply aggregates a Sender and a Receiver
    //
    public class Comm<T>
    {
        public string name { get; set; } = typeof(T).Name;
        public Receiver<T> rcvr { get; set; } = new Receiver<T>();
        public Sender sndr { get; set; } = new Sender();
        public Comm()
        {
            rcvr.name = name;
            sndr.name = name;
        }
        public static string makeEndPoint(string url, int port)
        {
            string endPoint = url + ":" + port.ToString() + "/ICommunicator";
            return endPoint;
        }
        //----< this thrdProc() used only for testing, below >-----------
        public void thrdProc()
        {
            while (true)
            {
                Message msg = rcvr.GetMessage();
                // msg.showMsg();
                if (msg.body == "quit")
                {
                    break;
                }
            }
        }
    }
#if (TEST_COMMSERVICE)

  class Cat { }
  class TestComm
  {
    [STAThread]
    static void Main(string[] args)
    {
      Comm<Cat> comm = new Comm<Cat>();
      string endPoint = Comm<Cat>.makeEndPoint("http://localhost", 8080);
      comm.rcvr.CreateRecvChannel(endPoint);
      comm.rcvr.start(comm.thrdProc);
      comm.sndr = new Sender();
      comm.sndr.CreateSendChannel(endPoint);
      Message msg1 = new Message();
      msg1.body = "Message #1";
      comm.sndr.PostMessage(msg1);
      Message msg2 = new Message();
      msg2.body = "quit";
      comm.sndr.PostMessage(msg2);
      Console.Write("\n  Comm Service Running:");
      Console.Write("\n  Press key to quit");
      Console.ReadKey();
      Console.Write("\n\n");
    }
#endif
}

