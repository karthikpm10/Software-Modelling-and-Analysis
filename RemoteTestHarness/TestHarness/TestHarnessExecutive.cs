
/////////////////////////////////////////////////////////////////////////////
//  TestHarnessExecutive.cs -Implements repository functionalities         //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Platform:     Windows 10,                                              //
//  Application:  Test Harness Project                                     //
//  Author:       Karthik Palepally Muniyappa                              //
//                                                                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   TestHarnessExecutive.cs - this class hosts a wcf service that provides accepts test requests from  multiple  
 *   clients and runs the tests concurrently
 * 
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  TestHarnessExecutive.cs,CommService.cs,
 *   - Compiler command: csc   TestHarnessExecutive
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20th November 2016
 *     - first release
 * 
 */
//
using Cmmunication;
using Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessMessages;
using Utilities;
using ServerTH;
using System.ServiceModel;
namespace TestHarness
{
    
    class TestHarnessExecutive<T>
    {
        public Comm<T> objComm = new Comm<T>();
        public ICommunicator channel = null;
        public int repoPort = 8085;
        public int clientPort = 8082;
        public int THport = 8081;
        //create a channle to communicate with repository
        public ICommunicator createRepoChannel()
        {
            string senderEndPoint1 = Comm<Server>.makeEndPoint("http://localhost", repoPort);
            EndpointAddress baseAddress = new EndpointAddress(senderEndPoint1);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, senderEndPoint1);
            ICommunicator channel = factory.CreateChannel();
            return channel;
        }
        //start the test harness and hand over the test request to ut
        public Message initiateTesting(Message msg)
        {
            Console.WriteLine("*****Requirement 4 : running Test requests concurrently message ");
            Console.Write("\n test processing thread {0} for message{1}", Thread.CurrentThread.ManagedThreadId, msg.author);
            TestHarness objTH = new TestHarness();
            Console.WriteLine(" thread {0} Datetime for msgs{1} {2}", Thread.CurrentThread.ManagedThreadId,msg.author,DateTime.Now);
            Console.WriteLine("**************Requirement 2*********************");
            Console.WriteLine("Received message from client is");
            Console.WriteLine(msg.body);
            channel = (channel==null) ? createRepoChannel() : channel;
            Message resultMsg =  objTH.processTestRequest(msg, channel);
            return resultMsg;
        }
        //send the results back to the client
        public void sendTestResult(Message msg)
        {
            Console.Write("\n  test result thread {0} for message{1}", Thread.CurrentThread.ManagedThreadId, msg.author);
            Console.WriteLine("***************Requirement 6&7: Results sent to client using channel**********");
            Console.WriteLine("msg {0}",msg.body.shift());
            objComm.sndr.PostMessage(msg);
        }
        //thread which constantly monitors for messages on its receive queue
        public void rcvThreadProc()
        {
            List<Task> listTasks = new List<Task>();
            while (true)
            {
                Message msg = new Message();
                msg = objComm.rcvr.GetMessage();
               // string name = objComm.rcvr.name;
                Console.Write("\n  getting message on rcvThread {0}", Thread.CurrentThread.ManagedThreadId);
                if (msg.type == "TestRequest")
                {
                       // run each request on a separate task( which in turn runs on a seperate thread)
                    Action<Message> testResultProcess = (message) => sendTestResult(message);
                    Task objTask = Task<Message>.Factory.StartNew(() => initiateTesting(msg))
                        .ContinueWith(antecedent => testResultProcess(antecedent.Result));
                    listTasks.Add(objTask);
                    //Thread.Sleep(10000);
                }
                else
                {
                    Console.Write("\n  {0}\n  received message from:  {1}\n{2}", msg.to, msg.from, msg.body.shift());
                    if (msg.body == "quit")
                        break;
                }
               
            }
            Task.WaitAll(listTasks.ToArray());
            Console.Write("\n  receiver shutting down\n");
        }
        // host the service
        public void initiateServer()
        {
           
            string rcvrEndPoint1 = Comm<T>.makeEndPoint("http://localhost", THport);
            objComm.rcvr.CreateRecvChannel(rcvrEndPoint1);
            Thread rcvThread1 = objComm.rcvr.start(rcvThreadProc);
            Console.Write("\n  rcvr thread id = {0}", rcvThread1.ManagedThreadId);
            Console.WriteLine();
        }
        // make a test request( for testing)
        public string makeTestRequest()
        {
            TestElement te1 = new TestElement("FirstTest");
            te1.addDriver("TestDriver.dll");
            te1.addCode("CodeToTest1.dll");
            te1.addCode("CodeToTest2.dll");
            TestElement te2 = new TestElement("test2");
            te2.addDriver("TestDriver2.dll");
            te2.addCode("CodeToTest1.dll");
            te2.addCode("CodeToTest3.dll");
            TestRequest tr = new TestRequest();
            tr.author = "Rahul The Great";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            return tr.ToXml();
        }
        // make a test request( for testing)
        public string makeTestRequest2()
        {
            TestElement te1 = new TestElement("test2");
            te1.addDriver("TestDriver.dll");
            te1.addCode("CodeToTest1.dll");
            te1.addCode("CodeToTest2.dll");
            TestElement te2 = new TestElement("FirstTest");
            te2.addDriver("TestDriver2.dll");
            te2.addCode("CodeToTest1.dll");
            te2.addCode("CodeToTest3.dll");
            TestRequest tr = new TestRequest();
            tr.author = "Rahul The Great";
            tr.tests.Add(te2);
            tr.tests.Add(te1);
            return tr.ToXml();
        }
    }
    
    class Initiator
    {
        static void Main(string[] args)
        {
            Console.Title = "TestHarness";
            TestHarnessExecutive<Server> objTestHarnessExecutive = new TestHarnessExecutive<Server>();
           // TestHarnessExecutive<client> objTestHarnessExecutive1 = new TestHarnessExecutive<client>();
            objTestHarnessExecutive.initiateServer();
           // objTestHarnessExecutive1.initiateServer(8081);
           /* Message msg = null;
            string rcvrEndPoint;
            string sndrEndPoint1 = Comm<Server>.makeEndPoint("http://localhost", 8081);
            string rcvrEndPoint1 = Comm<Server>.makeEndPoint("http://localhost", 8080);
            for (int i = 0; i < 1; ++i)
            {
                if(i==0)
                {
                    msg = new Message(objTestHarnessExecutive.makeTestRequest());
                }
                else
                {
                    msg = new Message(objTestHarnessExecutive.makeTestRequest2());
                }
                msg.type = "TestRequest";
                msg.from = sndrEndPoint1;
                msg.author = "Mr Perfect Rahul" + i;
                if (i < 3)
                    msg.to = rcvrEndPoint = rcvrEndPoint1;
                else
                {
                    msg.to = rcvrEndPoint = rcvrEndPoint1;
                    msg.author = "karthik";
                }
                msg.time = DateTime.Now;
               // objTestHarnessExecutive1.objComm.sndr.PostMessage(msg);
                Console.Write("\n  {0}\n  posting message with body:\n{1}", msg.from, msg.body.shift());
                //Thread.Sleep(20);
            }*/
        }
    }
#if (TEST_TESTHARNESSEXECUTIVE)
  class Program
  {
    static void Main(string[] args)
    {
        Console.Title = "TestHarness";
            TestHarnessExecutive<Server> objTestHarnessExecutive = new TestHarnessExecutive<Server>();
           // TestHarnessExecutive<client> objTestHarnessExecutive1 = new TestHarnessExecutive<client>();
            objTestHarnessExecutive.initiateServer();
           // objTestHarnessExecutive1.initiateServer(8081);
           /* Message msg = null;
            string rcvrEndPoint;
            string sndrEndPoint1 = Comm<Server>.makeEndPoint("http://localhost", 8081);
            string rcvrEndPoint1 = Comm<Server>.makeEndPoint("http://localhost", 8080);
            for (int i = 0; i < 1; ++i)
            {
                if(i==0)
                {
                    msg = new Message(objTestHarnessExecutive.makeTestRequest());
                }
                else
                {
                    msg = new Message(objTestHarnessExecutive.makeTestRequest2());
                }
                msg.type = "TestRequest";
                msg.from = sndrEndPoint1;
                msg.author = "Mr Perfect Rahul" + i;
                if (i < 3)
                    msg.to = rcvrEndPoint = rcvrEndPoint1;
                else
                {
                    msg.to = rcvrEndPoint = rcvrEndPoint1;
                    msg.author = "karthik";
                }
                msg.time = DateTime.Now;
               // objTestHarnessExecutive1.objComm.sndr.PostMessage(msg);
                Console.Write("\n  {0}\n  posting message with body:\n{1}", msg.from, msg.body.shift());
                //Thread.Sleep(20);
            }*/
    }
  }
#endif
}

