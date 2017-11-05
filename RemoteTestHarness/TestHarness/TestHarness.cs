
/////////////////////////////////////////////////////////////////////////////
//  TestHarness.cs - implengts testing of est requests                     //
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
 *   This module  takes  test requests , for each test request, it creates a temporary directory,loads the dll from the repository 
 *   into this directory, it creates a child app domain , 
 injects the loader into it ,passes the test request, and also creates a new instance of logger class for each test request 
 and passes this to the loader, this newly created logger object logs test request related execution details
 * 
 *  
 *  
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   TestHarness.cs,Loader.cs,LoaderInterface.cs,Logger.cs,Interfaces.cs
 *   - Compiler command: csc  TestExecutive
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20th November 2016
 *     - first release
 * 
 */
//
using Cmmunication;
using HRTimer;
using IInterfaces;
using LoggerTH;
using Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Policy;
using System.Threading;
using TestHarnessMessages;
using Utilities;
namespace TestHarness
{
    public class Callback : MarshalByRefObject, ICallback
    {
        public void sendMessage(Message message)
        {
            Console.Write("\n  received msg from childDomain: \"" + message.body + "\"");
        }
    }
    class TestHarness
    {
        int BlockSize = 1024;
        byte[] block;
        HiResTimer hrt = null;
        private ICallback cb_;
        public TestHarness()
        {
            cb_ = new Callback();
            block = new byte[BlockSize];
            hrt = new HiResTimer();
        }
        // uploads logs and test results to repo
        public void uploadFileToRepo(string filePath, string filename, ICommunicator repoChannel)
        {
            try
            {
                hrt.Start();
                using (var inputStream = new FileStream(filePath, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = filename;
                    msg.transferStream = inputStream;
                    repoChannel.upLoadFile(msg);
                }
                hrt.Stop();
                Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            }
            catch (Exception e)
            {
                Console.Write("\n  can't find \"{0}\" exception {1}", filePath, e);
            }
        }
        //writes the test result into temp directory created
        public void writeResultsToFile(string fpath, Message msg, ICommunicator repoChannel)
        {
            Console.WriteLine("***********Requirement 8 :Result file name is " + fpath.Substring(fpath.LastIndexOf("\\") +1 ));
            if (fpath != null)
            {
                using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(fpath, true))
                {
                    file.WriteLine(DateTime.Now + " : " + msg.body);
                }
            }
            else
            {
                Console.WriteLine("File name is null");
            }
            Console.WriteLine("**********Requirement7 : sending Results to repository");
            //uploads the results to repository
            uploadFileToRepo(fpath, fpath.Substring(fpath.LastIndexOf("\\") + 1), repoChannel);
        }
        //processes the test request
        public Message processTestRequest(Message msg, ICommunicator repoChannel)
        {
            Console.WriteLine(" thread {0} Datetime for msgs{1} {2}", Thread.CurrentThread.ManagedThreadId, msg.author, DateTime.Now);
            AppDomain ad = null;
            Message resultMsg = new Message();
            resultMsg.type = "TestResult";
            resultMsg.to = msg.from;
            resultMsg.from = msg.to;
            resultMsg.author = msg.author;
            resultMsg.time = DateTime.Now;
            ILoadAndTest ldandtst = null;
            TestRequest tr = msg.body.FromXml<TestRequest>();
            Logger objLogger = new Logger();
            TestResults objTestResults = new TestResults();
            if (tr != null)
            {
                string tempDirectory = ProcessAndload(tr, msg.author, repoChannel);
                if (tempDirectory == null)
                {
                    Console.WriteLine("**********Requirement 3: one or more libraries not found in the repository ********");
                    resultMsg.body = "ERROR : Dll's not found in Repository";
                    return resultMsg;
                }
                ad = createChildAppDomain();
                ldandtst = installLoader(ad, tempDirectory);
                if (ldandtst != null)
                {
                    objTestResults = ldandtst.test(tr, objLogger, msg.author);
                    resultMsg.body = objTestResults != null ? objTestResults.ToXml() : "no result";
                }
                Console.WriteLine("**********Requirement7 : sending logs to repository");
                //uploading log to repo
                uploadFileToRepo(objLogger.fileName, objLogger.fileName.Substring(objLogger.fileName.LastIndexOf("\\") + 1), repoChannel);
                // write results to file and upload to repo
                string resultFileName = tempDirectory + "\\" + msg.author + "_" + tr.TestRequestName + "_" + "TestResult" + "_" + System.Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt";
                writeResultsToFile(resultFileName, resultMsg, repoChannel);
                Console.WriteLine("**********Requirement7 : Unloading Child App domain");
                AppDomain.Unload(ad);
                try
                {
                    System.IO.Directory.Delete(tempDirectory, true);
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": removed directory " + tempDirectory);
                }
                catch (Exception ex)
                {
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not remove directory " + tempDirectory + " exception " + ex.Message);
                }
            }
            return resultMsg;
        }
        //list the library files required for a test request
        public List<string> getRequestFiles(TestRequest req)
        {
            List<string> requestFiles = new List<string>();
            List<TestElement> testElements = req.tests;
            foreach (var testElement in testElements)
            {
                requestFiles.Add(testElement.testDriver);
                foreach (var code in testElement.testCodes)
                {
                    requestFiles.Add(code);
                }
            }
            return requestFiles;
        }
        //creates a local directory,and loads the libraries from repository
        public string ProcessAndload(TestRequest req, string author, ICommunicator repoChannel)
        {
            string loaclRequestdir = makeKey(author);
            string filePath = System.IO.Path.GetFullPath(loaclRequestdir);
            Console.Write("\n  creating local test directory \"" + loaclRequestdir + "\"");
            System.IO.Directory.CreateDirectory(loaclRequestdir);
            List<string> testfiles = getRequestFiles(req);
            Console.WriteLine("**************Requirement6:Downloading libraries from repository");
            foreach (string file in testfiles)
            {
                try
                {
                    hrt.Start();
                    int totalBytes = 0;
                    Stream strm = repoChannel.downLoadFile(file);
                    if (strm != null)
                    {
                        string rfilename = Path.Combine(filePath, file);
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);
                        using (var outputStream = new FileStream(rfilename, FileMode.Create))
                        {
                            while (true)
                            {
                                int bytesRead = strm.Read(block, 0, BlockSize);
                                totalBytes += bytesRead;
                                if (bytesRead > 0)
                                    outputStream.Write(block, 0, bytesRead);
                                else
                                    break;
                            }
                        }
                        hrt.Stop();
                        Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", file, totalBytes, hrt.ElapsedMicroseconds);
                    }
                    else
                    {
                        filePath = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}", ex.Message);
                    filePath = null;
                }
            }
            return filePath;
        }
        string makeKey(string author)
        {
            DateTime now = DateTime.Now;
            string nowDateStr = now.Date.ToString("d");
            string[] dateParts = nowDateStr.Split('/');
            string key = "";
            foreach (string part in dateParts)
                key += part.Trim() + '_';
            string nowTimeStr = now.TimeOfDay.ToString();
            string[] timeParts = nowTimeStr.Split(':');
            for (int i = 0; i < timeParts.Count() - 1; ++i)
                key += timeParts[i].Trim() + '_';
            key += timeParts[timeParts.Count() - 1];
            key = author + "_" + key;
            return key;
        }
        public AppDomain createChildAppDomain()
        {
            try
            {
                Console.Write("\n  creating child AppDomain - Req #4");
                AppDomainSetup domaininfo = new AppDomainSetup();
                domaininfo.ApplicationBase
                  = "file:///" + System.Environment.CurrentDirectory;  // defines search path for LoadAndTest library
                //Create evidence for the new AppDomain from evidence of current
                Evidence adevidence = AppDomain.CurrentDomain.Evidence;
                // Create Child AppDomain
                AppDomain ad
                  = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);
                Console.Write("\n  created AppDomain \"" + ad.FriendlyName + "\"");
                return ad;
            }
            catch (Exception except)
            {
                Console.Write("\n  " + except.Message + "\n\n");
            }
            return null;
        }
        ILoadAndTest installLoader(AppDomain ad, string tempDir)
        {
            ad.Load("Loader");
            //showAssemblies(ad);
            //Console.WriteLine();
            // create proxy for LoadAndTest object in child AppDomain
            ObjectHandle oh
              = ad.CreateInstance("Loader", "LoaderTH.Loader");
            object ob = oh.Unwrap();    // unwrap creates proxy to ChildDomain
                                        // Console.Write("\n  {0}", ob);
                                        // set reference to LoadAndTest object in child
            ILoadAndTest landt = (ILoadAndTest)ob;
            // create Callback object in parent domain and pass reference
            // to LoadAndTest object in child
            landt.setCallback(cb_);
            landt.loadPath(tempDir);  // send file path to LoadAndTest
            return landt;
        }
    }
#if (TEST_TESTHARNESS)
  class Program
  {
    static void Main(string[] args)
    {
      TestHarness obj = new TestHarness()
    ICommunicator repoChannel;
    Message msg=new Message();
    obj.processTestRequest(msg,repoChannel)
    }
  }
#endif
}
