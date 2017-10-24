
/////////////////////////////////////////////////////////////////////////////
//  Client.cs - demonstrate threads communicating via Queue          //
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
 *  This is the entry point to the project This module  fetches the xml test requests from ../../../XMLTestRequests
 and puts them in the queue,Invokes TestExecutive and passes the queue object.
 It also creates an instance of logger class for the test requests, which logs the 
 execution details of the test harness to a file in ../../../Logs/TestHarnessLogs/ folder
 * 
 *  
 *   Public Interface
 *   ----------------
 *   Client objClient = new Client();
 *   objClient.readTestRequests();
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   Client.cs,TestExecutive.cs
 *   - Compiler command: csc Client.cs TestExecutive.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 4th October 2016
 *     - first release
 * 
 */
//

using Filemanager;
using SWTools;
using System;
using System.Xml.Linq;
using Utilities;
namespace TestHarness
{
    class Client
    {
        //xml test requests path
        private string m_testRequestFPath = "../../../XMLTestRequests";
        private FileManager m_objFileManager = null;
        private TestExecutive m_objTestExecutive = null;
        //path for the test harness logs
        Logger m_objLoggerTH = new Logger("../../../Logs/TestHarnessLogs/TestHarnessLog_" + DateTime.Now.ToString().Replace("/","-").Replace(":","-")+".txt");
       
        /*
         * fetches the xml test requests from ../../../XMLTestRequests
 and puts them in the queue,Invokes TestExecutive and passes the queue object.
 It also creates an instance of logger class for the test requests, which logs the 
 execution details of the test harness to a file in ../../../Logs/TestHarnessLogs/ folder
         * */
        public void readTestRequests()
        {
            try
            {
                m_objFileManager = new FileManager();
                //lists all the xml files in the path, all the test request files are stored
                string[] testRequests = m_objFileManager.listFiles(m_testRequestFPath, "xml");
                if(testRequests != null)
                {
                    if (testRequests.Length > 0)
                    {
                        BlockingQueue<XDocument> objBlockingQueue = new BlockingQueue<XDocument>();
                        XDocument testRquestDoc = null;
                        Console.WriteLine("Queuing XML Test Requests");
                        m_objLoggerTH.log("Queuing XML Test Requests");
                        //iterates over all test request files
                        for (int i= 0 ; i < testRequests.Length; i++ )
                        {
                            System.IO.FileStream xmlFile = new System.IO.FileStream(testRequests[i], System.IO.FileMode.Open);
                            testRquestDoc = new XDocument();
                            testRquestDoc = XDocument.Load(xmlFile);
                            // queues each xml test request file
                            objBlockingQueue.enQ(testRquestDoc);
                            string[] testRequestFName = testRequests[i].Split(new char[] { '\\' });
                            Console.WriteLine("Queued Test Request {0}", testRequestFName[1]);
                            m_objLoggerTH.log("Queued Test Request " + testRequestFName[1]);
                            xmlFile.Close();
                        }
                        m_objTestExecutive = new TestExecutive();
                        //invokes test executive , passes the reference to the queue
                    if (testRequests.Length > 0)
                        m_objTestExecutive.BeginTestHarness(objBlockingQueue, testRequests, m_objLoggerTH);
                    }
                    else
                    {
                        Console.WriteLine("No test Requests found");
                        m_objLoggerTH.log("No test Requests found");
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Caught exception in client, Check Log file {0}",m_objLoggerTH.fileName);
                m_objLoggerTH.log("Caught exception in client, Exception : " + ex);
            } 
        }
        static void Main(string[] args)
        {
            //Logger objLogger = new Logger("../../../Logs/Dummy.txt");
            //objLogger.log("testhsbjbsadsann");
            Client objClient = new Client();
            objClient.m_objLoggerTH.LogCompleted = false;
            objClient.readTestRequests();
            Console.ReadLine();
        }
    }
}
