using Cmmunication;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessMessages;
using Utilities;

namespace TestExecutive
{
    class test
    {

        public int repoPort = 8085;
        public int clientPort = 8082;
        public int THport = 8081;
        public string url = "http://localhost";
        public ICommunicator Repochannel = null;
        public Comm<Client> objComm = new Comm<Client>();
        public void createRepoChannel()
        {
            string senderEndPoint1 = Comm<Client>.makeEndPoint(url, repoPort);
            EndpointAddress baseAddress = new EndpointAddress(senderEndPoint1);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, senderEndPoint1);
            Repochannel = factory.CreateChannel();
        }
        public void rcvThreadProc()
        {
            List<Task> listTasks = new List<Task>();
            while (true)
            {
                Message msg = new Message();
                msg = objComm.rcvr.GetMessage();
                // string name = objComm.rcvr.name;
                Console.WriteLine();
            }

        }
        public void getlogs()
        {
            LogRequest objlogRequest = new LogRequest();
            objlogRequest.author = "Karthik";
            objlogRequest.TestRequestName = "TestRequest1";
            Messages.Message logMessage = new Messages.Message();
            logMessage.author = "karu";
            logMessage.body = objlogRequest.ToXml();
            logMessage.to = Comm<Client>.makeEndPoint(url, repoPort);
            logMessage.from = Comm<Client>.makeEndPoint(url, clientPort);
            logMessage.type = "LogRequest";
            logMessage.time = DateTime.Now;
            objComm.sndr.PostMessage(logMessage);

        }
        public void hostClient()
        {
            string rcvrEndPoint = Comm<Client>.makeEndPoint(url, clientPort);
            try
            {
                objComm.rcvr.CreateRecvChannel(rcvrEndPoint);
                Thread rcvThread = objComm.rcvr.start(rcvThreadProc);
                rcvThread.IsBackground = true;
                Console.Write("\n  rcvr thread id = {0}", rcvThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                
            }
        }

        static void Main(string[] args)
        {
            test objtest = new  test();
            objtest.createRepoChannel();
            objtest.hostClient();
            objtest.getlogs();

        }


    }


    class Client
{

}
}
