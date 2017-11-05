
/////////////////////////////////////////////////////////////////////////////
//  Loader.cs - loads tests and runs them                                  //
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
 *   Loader.cs - This class implements the ILoadAndTest interface and the test function is the entry point to this project,
 it accepts a  Test request  and the logger object for the test Harness and the channel object to communicate with repository. It Parses the  test request, 
 loads the required libraries and test driver and creates an instance of test driver and invokes test method of the test driver.
 It also logs the execution details of the test harness and test driver into respective logger objects.
 * 
 *  
 * 
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  Loader.cs,ILoadAndTest.cs,Logger.cs,ITest.cs
 *   - Compiler command: csc   Loader ILoadAndTest Logger ITest 
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20th November 2016
 *     - first release
 * 
 */
//s
using IInterfaces;
using System;
using System.Collections.Generic;
using LoggerTH;
using TestHarnessMessages;
using System.Reflection;
using System.Xml.Linq;
using HRTimer;

namespace LoaderTH
{
    public class Loader : MarshalByRefObject, ILoadAndTest
    {
        HiResTimer hrt = new HiResTimer();
        private string loadPath_ = "";
        private ICallback cb_ = null;
        public void loadPath(string path)
        {
            loadPath_ = path;
            Console.Write("\n  loadpath = {0}", loadPath_);
        }
        public void setCallback(ICallback cb)
        {
            cb_ = cb;
        }
        //build result run the test and send the result
        public TestResults test(TestRequest testrequest, Logger objLogger, string author)
        {
            TestResults objTestResults = new TestResults();
            objTestResults.author = author;
            objTestResults.timeStamp = DateTime.Now;
            try
            {
                Console.WriteLine("Executing loader in the Domain {0}", AppDomain.CurrentDomain.FriendlyName);
                if (testrequest != null)
                {
                    string logFileName = author + "_" + testrequest.TestRequestName + "_" + System.Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt";
                    objLogger.fileName = loadPath_ + "\\" + logFileName;
                    Console.WriteLine("*******Requirement 8 :Test Driver Log File name is {0} ", logFileName);
                    objLogger.log("Author :" + author);
                    // iterates over each test case
                    foreach (TestElement testelement in testrequest.tests)
                    {
                        TestResult objTestResult = new TestResult();
                        objTestResult.testName = testelement.testName;
                        objTestResult.log = logFileName;
                        try
                        {
                            objLogger.log("Test Name : " + testelement.testName);
                            objLogger.log("Test Time : " + DateTime.Now.ToString());
                            // loads the libraries and test driver required for a test case
                            ITest testDriver = loadFiles(testelement, objLogger);
                            objTestResult.passed = runTest(testDriver, testelement, objLogger);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Caught Exception while executing test: {0} ", testelement.testName);
                            objLogger.log("Caught Exception while executing test:  " + testelement.testName + " Exception : " + ex);
                        }
                        objTestResults.results.Add(objTestResult);
                    }
                }
                else
                {
                    Console.WriteLine("Please Provide a Valid Test Request file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught Exception in Loader", ex);
            }
            objLogger.LogCompleted = true;
            return objTestResults;
        }
        //runs the test
        public bool runTest(ITest testDriver, TestElement testelement, Logger objLogger)
        {
            bool status = false;
            if (testDriver != null)
            {
                // runs the test driver, get result from run and append to callback message
                status = run(testDriver, testelement.testDriver, objLogger);
                //gets the test driver logs
                string testDriverLogs = testDriver.getLog();
                testDriverLogs = testDriverLogs != null ? testDriverLogs : "Test Driver didnt Return any logs";
                objLogger.log("*****************************************Logs from test driver execution*****************************************");
                objLogger.log(testDriverLogs);
                objLogger.log("*****************************************END OF Logs from test driver execution*****************************************");
            }
            else
            {
                Console.WriteLine("Test Case {0} couldnt be run as instance of test driver couldnt be created", testelement.testName);
                objLogger.log("Test Case " + testelement.testName + " couldnt be run as instance of test driver couldnt be created");
            }
            return status;
        }
        //loads the libraries into the domain
        public ITest loadFiles(TestElement objTestElement, Logger objLogger)
        {
            ITest testDriver = null;
            try
            {
                Assembly assem = null;
                //Type[] types = null;
                List<string> testCode = objTestElement.testCodes;
                //iterates over each libraries required for the test case
                foreach (string lib in testCode)
                {
                    try
                    {
                        // loads the library
                        assem = Assembly.LoadFrom(loadPath_ + "/" + lib);
                        objLogger.log("Library : " + lib + "Loaded");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("file {0} of test case {1} couldnt be loaded, caught exception in Loder,Check file {2} for details ", loadPath_ + "/" + lib, objTestElement.testName, objLogger.fileName);
                        objLogger.log("Library : " + lib + "  of test case " + objTestElement.testName + " Couldn't be loaded, Exception e: " + ex.ToString());
                        return null;
                    }
                }
                //loads the test driver
                testDriver = loadDriver(objTestElement, objLogger);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in Loader");
                objLogger.log("Exception in Loader, Exception :" + ex);
                testDriver = null;
            }
            return testDriver;
        }
        //loads the test driver
        public ITest loadDriver(TestElement objTestElement, Logger objLogger)
        {
            Assembly assem = null;
            ITest testDriver = null;
            try
            {
                // loads the test driver
                 assem = Assembly.LoadFrom(loadPath_ + "/" + objTestElement.testDriver);
                objLogger.log("Test Driver : " + objTestElement.testDriver + "Loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("file {0} of test case {1} couldnt be loaded, caught exception in Loader, Check file {2} for details ", loadPath_ + "/" + objTestElement.testDriver, objTestElement.testName, objLogger.fileName);
                objLogger.log("Test Driver : " + objTestElement.testDriver + " of test case " + objTestElement.testName + " Couldn't be loaded, Exception e: " + ex.ToString());
                return null;
            }
            Type[] types = assem.GetExportedTypes();
            bool testDriverCreated = false;
            foreach (Type t in types)
            {
                // does this type derive from ITest ?
                if (t.IsClass && typeof(ITest).IsAssignableFrom(t))
                {
                    // create instance of test driver
                    testDriver = (ITest)Activator.CreateInstance(t);
                    testDriverCreated = true;
                    Console.WriteLine("Instance of Test Driver  {0} , which derives from ITest  Created", objTestElement.testDriver);
                    objLogger.log("Instance of Test Driver " + objTestElement.testDriver + " which derives from ITest Created");
                }
            }
            if (!testDriverCreated)
            {
                Console.WriteLine("Instance of Test Driver {0} Not Created", objTestElement.testDriver);
                objLogger.log("Instance of Test Driver " + objTestElement.testDriver + " Not Created");
            }
            return testDriver;
        }

        //retrun bool
        public bool run(ITest testDriver, string testDriverName, Logger objLogger)
        {
            bool status = false;
            try
            {
                Console.Write(" Running test driver : {0} in Domain {1} ", testDriverName, AppDomain.CurrentDomain.FriendlyName);
                objLogger.log("Running test driver " + testDriverName);
                hrt.Start();
                if (testDriver.test() == true)
                {
                    status = true;
                    Console.WriteLine("\n  test passed");
                    hrt.Stop();
                    Console.WriteLine("******Requirement 12: Time Taken for executings test");
                    Console.WriteLine("Time for executing test {0}",hrt.ElapsedMicroseconds);
                    objLogger.log("test driver " + testDriverName + " Passed");
                }
                else
                {
                    Console.WriteLine("\n  test failed");
                    objLogger.log("test driver " + testDriverName + " Failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("Caught Exception in Test Driver {0}", testDriverName, ex);
                objLogger.log("test driver " + testDriverName + " Failed");
                objLogger.log("Caught Exception in Test Driver " + testDriverName + " Exception:" + ex);
            }
            return status;
        }

        static void Main(string[] args)
        {
            System.IO.FileStream xmlFile = new System.IO.FileStream("../../../XMLTestRequests/TestRequest", System.IO.FileMode.Open);
            XDocument testRquestDoc = new XDocument();
            testRquestDoc = XDocument.Load(xmlFile);
            Loader objLoader = new Loader();
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

            Logger objLoggerTH = new Logger("../../../ Logs / TestHarnessLogs / TestHarnessLog_" + DateTime.Now.ToString().Replace(" / ", " - ").Replace(":", " - ") + ".txt");
            objLoader.test(tr, objLoggerTH, "Karthik");

        }
    }
#if (TEST_LOADER)
  class Program
  {
    static void Main(string[] args)
    {
        System.IO.FileStream xmlFile = new System.IO.FileStream("../../../XMLTestRequests/TestRequest", System.IO.FileMode.Open);
            XDocument testRquestDoc = new XDocument();
            testRquestDoc = XDocument.Load(xmlFile);
            Loader objLoader = new Loader();
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

            Logger objLoggerTH = new Logger("../../../ Logs / TestHarnessLogs / TestHarnessLog_" + DateTime.Now.ToString().Replace(" / ", " - ").Replace(":", " - ") + ".txt");
            objLoader.test(tr, objLoggerTH, "Karthik");
    }
  }
#endif
}

