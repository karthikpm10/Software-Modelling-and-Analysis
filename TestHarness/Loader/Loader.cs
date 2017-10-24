
/////////////////////////////////////////////////////////////////////////////
//  Loader.cs - demonstrate threads communicating via Queue          //
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
 *   Loader.cs - This class implements the ILoader interface and the startLoader function is the entry point to this project,
 it accepts a xml Test request string and the logger object for the test Harness and the test driver. It Parses the xml test request, 
 loads the required libraries and test driver and creates an instance of test driver and invokes test method of the test driver.
 It also logs the execution details of the test harness and test driver into respective logger objects.
 * 
 *  
 *   Public Interface
 *   ----------------
 *   Loader objLoader = new Loader();
 *   objLoader.startLoader(string xmlRequest, Logger objLoggerTH, Logger m_objLoggerTestdriver)
     objLoader.loadFiles(xmlRequestStruct objTestCase, Logger objLoggerTH, Logger m_objLoggerTestdriver)
 *   objLoader.run(ITest testDriver, string testDriverName, Logger objLoggerTH, Logger m_objLoggerTestdriver)
 *   objLoader.invokeLog(string msg, Logger objTestHarnessLog, Logger objTestRequestLog)
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  Loader.cs,LoaderInterface.cs,Logger.cs,TestInterface.cs
 *   - Compiler command: csc   Loader LoaderInterface Logger TestInterface 
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 4th October 2016
 *     - first release
 * 
 */
//
using LoaderInterface;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TestInterface;
using Utilities;
namespace THLoader
{
    public class Loader : MarshalByRefObject, Iloader
    {
        private string strDriverLibpath = "../../../TestDriverLibraries/";
        private string strTestRequestLogPath = "../../../Logs/TestRequestLogs/";
        /*
         * It accepts a xml Test request string and the logger object for the test Harness and the test driver. It Parses the xml test request, 
           loads the required libraries and test driver and creates an instance of test driver and invokes test method of the test driver.
            It also logs the execution details of the test harness and test driver into respective logger objects.
         * 
         * */
        public void startLoader(string xmlRequest, Logger objLoggerTH, Logger m_objLoggerTestdriver)
        {
            try
            {
                objLoggerTH.log("Executing loader in the domain: " + AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("Executing loader in the Domain {0}", AppDomain.CurrentDomain.FriendlyName);
                XDocument objxmlRequest = new XDocument();
                //converts xml test request string to an Xdocument object
                objxmlRequest = XDocument.Parse(xmlRequest);
                XMLParser objXMLParser = new XMLParser();
                //parses the xml test request 
                List<xmlRequestStruct> objListTestCases = objXMLParser.parse(objxmlRequest, objLoggerTH);

                if ((objListTestCases != null) && (objListTestCases.Count > 0))
                {
                    m_objLoggerTestdriver.fileName = strTestRequestLogPath + objListTestCases[0].author + "_" + System.Guid.NewGuid().ToString()+ "_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt";
                    Console.WriteLine("Test Driver Log File name is {0} ",m_objLoggerTestdriver.fileName);
                    Console.WriteLine("Started writing test driver logs into the file {0}", m_objLoggerTestdriver.fileName);
                    m_objLoggerTestdriver.log("Author :" + objListTestCases[0].author);
                    // iterates over each test case
                    for (int i = 0; i < objListTestCases.Count; i++)
                    {
                        try
                        {
                            m_objLoggerTestdriver.log("Test Name : " + objListTestCases[i].testName);
                            m_objLoggerTestdriver.log("Test Time : " + objListTestCases[i].timeStamp.ToString());
                            // loads the libraries and test driver required for a test case
                            ITest testDriver = loadFiles(objListTestCases[i], objLoggerTH, m_objLoggerTestdriver);
                            if (testDriver != null)
                            {
                                // runs the test driver
                                run(testDriver, objListTestCases[i].testDriver, objLoggerTH, m_objLoggerTestdriver);
                                //gets the test driver logs
                                string testDriverLogs = testDriver.getLog();
                                testDriverLogs = testDriverLogs != null ? testDriverLogs : "Test Driver didnt Return any logs";
                                m_objLoggerTestdriver.log("*****************************************Logs from test driver execution*****************************************");
                                m_objLoggerTestdriver.log(testDriverLogs);
                                m_objLoggerTestdriver.log("*****************************************END OF Logs from test driver execution*****************************************");
                            }
                            else
                            {
                                Console.WriteLine("Test Case {0} couldnt be run as instance of test driver couldnt be created", objListTestCases[i].testName);
                                invokeLog("Test Case " + objListTestCases[i].testName + " couldnt be run as instance of test driver couldnt be created", objLoggerTH, m_objLoggerTestdriver);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Caught Exception while executing test: {0} ", objListTestCases[i].testName);
                            invokeLog("Caught Exception while executing test:  " + objListTestCases[i].testName + " Exception : " + ex, objLoggerTH, m_objLoggerTestdriver);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Please Provide a Valid Test Request file");
                    objLoggerTH.log("Please Provide a Valid Test Request file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught Exception in Loader");
                objLoggerTH.log("Caught Exception in Loader " + ex);
            }
            m_objLoggerTestdriver.LogCompleted = true;
        }
        //loads the files required for a test driver and creates an instance of the test driver and returns it
        public ITest loadFiles(xmlRequestStruct objTestCase, Logger objLoggerTH, Logger m_objLoggerTestdriver)
        {
            ITest testDriver = null;
            try
            {
                Assembly assem = null;
                Type[] types = null;
                List<string> testCode = objTestCase.testCode;
                //iterates over each libraries required for the test case
                foreach (string lib in testCode)
                {
                    try
                    {
                        // loads the library
                        assem = Assembly.LoadFrom(strDriverLibpath + lib);
                        invokeLog("Library : " + lib + "Loaded", objLoggerTH, m_objLoggerTestdriver);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("file {0} of test case {1} couldnt be loaded, caught exception in Loder,Check file {2} for details ", strDriverLibpath + lib, objTestCase.testName, objLoggerTH.fileName);
                        invokeLog("Library : " + lib + "  of test case " + objTestCase.testName + " Couldn't be loaded, Exception e: " + ex.ToString(), objLoggerTH, m_objLoggerTestdriver);
                        return null;
                    }
                }
                try
                {
                    // loads the test driver
                    assem = Assembly.LoadFrom(strDriverLibpath + objTestCase.testDriver);
                    invokeLog("Test Driver : " + objTestCase.testDriver + "Loaded", objLoggerTH, m_objLoggerTestdriver);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("file {0} of test case {1} couldnt be loaded, caught exception in Loader, Check file {2} for details ", strDriverLibpath + objTestCase.testDriver, objTestCase.testName, objLoggerTH.fileName);
                    invokeLog("Test Driver : " + objTestCase.testDriver + " of test case " + objTestCase.testName + " Couldn't be loaded, Exception e: " + ex.ToString(), objLoggerTH, m_objLoggerTestdriver);
                    return null;
                }
                types = assem.GetExportedTypes();
                bool testDriverCreated = false;
                foreach (Type t in types)
                {
                    // does this type derive from ITest ?
                    if (t.IsClass && typeof(ITest).IsAssignableFrom(t))
                    {
                        // create instance of test driver
                        testDriver = (ITest)Activator.CreateInstance(t);
                        testDriverCreated = true;
                        Console.WriteLine("Instance of Test Driver  {0} , which derives from ITest  Created", objTestCase.testDriver);
                        invokeLog("Instance of Test Driver " + objTestCase.testDriver + " which derives from ITest Created", objLoggerTH, m_objLoggerTestdriver);
                    }
                }
                if (!testDriverCreated)
                {
                    Console.WriteLine("Instance of Test Driver {0} Not Created", objTestCase.testDriver);
                    invokeLog("Instance of Test Driver " + objTestCase.testDriver + " Not Created", objLoggerTH, m_objLoggerTestdriver);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in Loader");
                objLoggerTH.log("Exception in Loader, Exception :" + ex);
                testDriver = null;
            }
            return testDriver;
        }
        // runs the test drive and logs the result 
        void run(ITest testDriver, string testDriverName, Logger objLoggerTH, Logger m_objLoggerTestdriver)
        {
            try
            {
                Console.Write(" Running test driver : {0} in Domain {1} ", testDriverName, AppDomain.CurrentDomain.FriendlyName);
                invokeLog("Running test driver " + testDriverName, objLoggerTH, m_objLoggerTestdriver);
                if (testDriver.test() == true)
                {
                    Console.WriteLine("\n  test passed");
                    invokeLog("test driver " + testDriverName + " Passed", objLoggerTH, m_objLoggerTestdriver);
                }
                else
                {
                    Console.WriteLine("\n  test failed");
                    invokeLog("test driver " + testDriverName + " Failed", objLoggerTH, m_objLoggerTestdriver);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("Caught Exception in Test Driver {0}", testDriverName);
                invokeLog("test driver " + testDriverName + " Failed", objLoggerTH, m_objLoggerTestdriver);
                invokeLog("Caught Exception in Test Driver " + testDriverName + " Exception:" + ex, objLoggerTH, m_objLoggerTestdriver);
            }
        }

        // logs the given message into test harness log file and test driver log file
        public void invokeLog(string msg, Logger objTestHarnessLog, Logger objTestRequestLog)
        {
            objTestHarnessLog.log(msg);
            objTestRequestLog.log(msg);
        }
        static void Main(string[] args)
        {
            System.IO.FileStream xmlFile = new System.IO.FileStream("../../../XMLTestRequests/TestRequest", System.IO.FileMode.Open);
            XDocument testRquestDoc = new XDocument();
            testRquestDoc = XDocument.Load(xmlFile);
            Loader objLoader = new Loader();
            Logger objLoggerTestDriver = new Logger();
            Logger objLoggerTH = new Logger("../../../ Logs / TestHarnessLogs / TestHarnessLog_" + DateTime.Now.ToString().Replace(" / ", " - ").Replace(":", " - ") + ".txt");
            objLoader.startLoader(testRquestDoc.ToString(), objLoggerTH, objLoggerTestDriver);

        }
    }
}
