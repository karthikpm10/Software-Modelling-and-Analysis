
/////////////////////////////////////////////////////////////////////////////
//  TestExecutive.cs - demonstrate threads communicating via Queue          //
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
 *   This module  Dequeues the xml test requests from the queue, for each test request it creates a child app domain , 
 injects the loader into it ,passes the test request xml, and also creates a new instance of logger class for each test request 
 and passes this to the loader, this newly created logger object logs test request related execution details
 * 
 *  
 *   Public Interface
 *   ----------------
 *   TestExecutive objTestExecutive = new TestExecutive();
 *   objTestExecutive.getLog(String filename, Logger objLoggerTestDriver)
     objTestExecutive.BeginTestHarness(BlockingQueue<XDocument> objBlockingQueue,string [] testRequests, Logger objLoggerTH)
 * 
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   TestExecutive.cs,AppDomainManager.cs,CS-BlockingQueue.cs,FileManager.cs,Loader.cs,LoaderInterface.cs,Logger.cs,TestInterface.cs
 *   - Compiler command: csc  TestExecutive AppDomainManager CS-BlockingQueue FileManager Loader LoaderInterface Logger TestInterface 
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 4th October 2016
 *     - first release
 * 
 */
//

using AppDomainManager;
using SWTools;
using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml.Linq;
using Utilities;
namespace TestHarness
{
    class TestExecutive
    {
        private AppDomainMgr m_objAppDomainMgr = new AppDomainMgr();
        //fetches the test request log from the filesystem( supports client queries for the test request logs
        public void getLog(String filename, Logger objLoggerTestDriver)
        {
            if (filename != null && objLoggerTestDriver.LogCompleted == true)
            {
                if (System.IO.File.Exists(filename))
                {
                    String[] logLines = System.IO.File.ReadAllLines(filename);
                    foreach (string line in logLines)
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    Console.WriteLine("Log {0} doesnt exist", filename);
                    objLoggerTestDriver.log("Log " + filename + " doesnt exist");
                }
            }
            else
            {
                Console.WriteLine("Log  still not completed");
            }
        }
        /*
         *Dequeues the xml test requests from the queue, for each test request it creates a child app domain , 
          injects the loader into it ,passes the test request xml, and also creates a new instance of logger class for each test request 
          and passes this to the loader, this newly created logger object logs test request related execution details, calls getLog to display the logs of each Test Request
         * 
         * 
         * */
        public void BeginTestHarness(BlockingQueue<XDocument> objBlockingQueue, string[] testRequests, Logger objLoggerTH)
        {
            try
            {
                int queueSize = objBlockingQueue.size();
                if (queueSize > 0)
                {
                    Console.WriteLine("Test harness Log File name is {0} ", objLoggerTH.fileName);
                    Console.WriteLine("Started writing test harness logs into the file {0}", objLoggerTH.fileName);
                    Console.WriteLine("Dequeuing Test Requests");
                    objLoggerTH.log("Dequeuing Test Requests");
                    // dequeueing test requests
                    for (int i = 0; i < queueSize; i++)
                    {
                        string[] testRequestFName = testRequests[i].Split(new char[] { '\\' });
                        XDocument xmlRequest = objBlockingQueue.deQ();
                        Console.WriteLine(" ");
                        Console.WriteLine("********************Dequeued Test  request {0} , Executing it*****************", testRequestFName[1]);
                        objLoggerTH.log("**********************Dequeued request " + testRequestFName[1]+ " , Executing it*******************************");
                        Console.WriteLine("In parent domain :{0}", AppDomain.CurrentDomain.FriendlyName);
                        objLoggerTH.log("In parent domain :" + AppDomain.CurrentDomain.FriendlyName);
                        Console.WriteLine("Creating a child app domain for  test request : {0}", testRequestFName[1]);
                        objLoggerTH.log("Creating a child app domain for  test request : " + testRequestFName[1]);
                        // creating a child app domain for a test request
                        AppDomain childDomain = m_objAppDomainMgr.createAppDomain("child domain", "file:///" + System.Environment.CurrentDirectory);
                        Console.WriteLine("Child App domain {0} created", childDomain.FriendlyName);
                        objLoggerTH.log("Child App domain " + childDomain.FriendlyName + "  created");
                        //injecting the loader in the child app domain
                        Assembly assembly = childDomain.Load("Loader");
                        Console.WriteLine("Loader Injected into the domain: {0}", childDomain.FriendlyName);
                        objLoggerTH.log("Loader Injected into the domain: " + childDomain.FriendlyName);
                        childDomain.Load("TestInterface");
                        //childDomain.Load("LoaderInterface");
                        //creaating instance of loader
                        ObjectHandle objHandle = childDomain.CreateInstance("Loader", "THLoader.Loader");
                        LoaderInterface.Iloader objLoaderInterface = (LoaderInterface.Iloader)objHandle.Unwrap();
                        // is call back from the loader required? think
                        Logger objLoggerTestDriver = new Logger();
                        objLoggerTestDriver.LogCompleted = false;
                        // starting the loader
                        objLoaderInterface.startLoader(xmlRequest.ToString(), objLoggerTH, objLoggerTestDriver);
                        string testDriverLogFileName = objLoggerTestDriver.fileName;
                        // calling get log to display the test requests logs
                        Console.WriteLine("Calling get log to display test request logs");
                        getLog(testDriverLogFileName, objLoggerTestDriver);
                        //unload app domain
                        AppDomain.Unload(childDomain);
                    }
                }
                else
                {
                    Console.WriteLine("No requests in the queue");
                    objLoggerTH.log("No requests in the queue");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception in Test Executive {0}", ex);
                objLoggerTH.log("Caught exception in Test Executive " + ex.ToString());
            }
            objLoggerTH.LogCompleted = true;
        }
        static void Main(string[] args)
        {
            TestExecutive objTestExecutive = new TestExecutive();
            Logger objLoggerTestDriver = new Logger();
            Logger THDriver = new Logger("../../../Logs/TestHarnessLogs/TestHarnessLog_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt");
            string[] testRequests = { "TestRequest.xml" };
            objTestExecutive.getLog("filename", objLoggerTestDriver);
            BlockingQueue<XDocument> objBlockingQueue = new BlockingQueue<XDocument>();
            System.IO.FileStream xmlFile = new System.IO.FileStream("../../../XMLTestRequests/TestRequest", System.IO.FileMode.Open);
            XDocument  testRquestDoc = new XDocument();
            testRquestDoc = XDocument.Load(xmlFile);
            // queues each xml test request file
            objBlockingQueue.enQ(testRquestDoc);

            objTestExecutive.BeginTestHarness(objBlockingQueue, testRequests, THDriver);
        }
    }
}
