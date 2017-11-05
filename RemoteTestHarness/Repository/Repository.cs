
/////////////////////////////////////////////////////////////////////////////
//  Repository.cs -Implements repository functionalities                   //
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
 *   Repository.cs - this class hosts a wcf service that provides functionality to
 *   upload files to repository
 *   download files from the repository
 *   query logs from the repository
 *  
 * 
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  Repository.cs,CommService.cs,
 *   - Compiler command: csc   Repository
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
using Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessMessages;
using Utilities;
namespace RepositoryTH
{
    public class Repository<T>
    {
        public Comm<T> objComm = new Comm<T>();
        byte[] block;
        HiResTimer hrt = null;
        public int repoPort = 8085;
        public int clientPort = 8082;
        public int THport = 8081;
        string savePath = "..\\..\\..\\RepositoryFiles";
        string ToSendPath = "..\\..\\..\\RepositoryFiles";
        int BlockSize = 1024;
        public Repository()
        {
            block = new byte[BlockSize];
            hrt = new HRTimer.HiResTimer();
        }
        //uploads files to repository
        public void uploadFile(FileTransferMessage msg)
        {
            int totalBytes = 0;
            hrt.Start();
            string filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            hrt.Stop();
            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes in {2} microsec.",
              filename, totalBytes, hrt.ElapsedMicroseconds
            );
        }
        //download file from repository
        public Stream downloadFile(string filename)
        {
            hrt.Start();
            string sfilename = Path.Combine(ToSendPath, filename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {
                outStream = new FileStream(sfilename, FileMode.Open);
            }
            else
            {
                Console.WriteLine("File {0} not found in Repository", filename);
                return outStream;
            }
            hrt.Stop();
            Console.Write("\n  Sent \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            return outStream;
        }
        //hosts the repository service
        public void initiateRepoServer()
        {
            //string sndrEndPoint1 = Comm.makeEndPoint("http://localhost", 8080);
            Receiver<T>.uploadFileHandler = uploadFile;
            Receiver<T>.downloadFileHandler = downloadFile;
            string rcvrEndPoint1 = Comm<T>.makeEndPoint("http://localhost", repoPort);
            objComm.rcvr.CreateRecvChannel(rcvrEndPoint1);
            Thread rcvThread1 = objComm.rcvr.start(rcvThreadProc);
            Console.Write("\n  rcvr thread id = {0}", rcvThread1.ManagedThreadId);
            Console.WriteLine();
        }
        //query logs from repository
        public Message getLogs(string query, string toAddress, string fromAddress)
        {
            int filesCount = 2;
            StringBuilder logBuilder = new StringBuilder();
            Message logResultmsg = new Message();
            logResultmsg.to = toAddress;
            logResultmsg.from = fromAddress;
            logResultmsg.type = "LogResult";
            logResultmsg.time = DateTime.Now;
            List<string> logs = new List<string>(System.IO.Directory.GetFiles(savePath, query + "*"));
            foreach (string log in logs)
            {
                if (log != null)
                {
                    if (System.IO.File.Exists(log))
                    {    //restricting number of files to 2 , as long strings cannot be passed overe the channel
                        if (filesCount > 0)
                        {
                            filesCount--;
                            logBuilder.Append("Logs for file ").Append(log.Substring(log.LastIndexOf("\\") + 1));
                            logBuilder.AppendLine(Environment.NewLine);
                            String[] logLines = System.IO.File.ReadAllLines(log);
                            foreach (string line in logLines)
                            {
                                logBuilder.Append(line);
                                //logBuilder.AppendLine(Environment.NewLine);
                            }
                            logBuilder.AppendLine(Environment.NewLine);

                        }
                    }
                    else
                    {
                        Console.WriteLine("Log {0} doesnt exist", log);
                    }
                }
                else
                {
                    Console.WriteLine("Log  still not completed");
                }
            }
            //append the string builder to the message body and send the message
            if(logBuilder.Length<1)
            {
                logBuilder.Append("Logs Not found for the given query");
            }
            logResultmsg.body = logBuilder.ToString();
            return logResultmsg;
        }
        //gets the logs and buids the result msg
        public Message ProcessLogs(Message msg)
        {
            Console.WriteLine("**************Requirement 9:Repository servicing log query sent from client: ");
            Console.WriteLine("Log Query");
            Console.WriteLine(msg.body.shift());
            Console.WriteLine("\n test processing thread {0} for message{1}", Thread.CurrentThread.ManagedThreadId, msg.author);
            LogRequest objLogRequest = msg.body.FromXml<LogRequest>();
            string author = objLogRequest.author;
            string testRequestName = objLogRequest.TestRequestName;
            string query = author + "_" + testRequestName + "_";
            Message resultMsg = getLogs(query, msg.from, msg.to);
            resultMsg.author = author;
            return resultMsg;
        }
        //sends the log results to requested client
        public void sendLogResult(Message msg)
        {
            Console.Write("\n  test result thread {0} for message{1}", Thread.CurrentThread.ManagedThreadId, msg.author);
            Console.WriteLine("msg {0}", msg.body.shift());
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
                if (msg.type == "LogRequest")
                {
                    Action<Message> LogResultProcess = (message) => sendLogResult(message);
                    Task objTask = Task<Message>.Factory.StartNew(() => ProcessLogs(msg))
                        .ContinueWith(antecedent => LogResultProcess(antecedent.Result));
                    listTasks.Add(objTask);

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
        /*static void Main(string[] args)
        {
            Repository<Repo> obj = new Repository<Repo>();
            Repository<Repo2> obj2 = new Repository<Repo2>();
            obj.initiateRepoServer();
            obj2.initiateRepoServer2();
            string senderEndPoint1 = Comm<Repo>.makeEndPoint("http://localhost", 8081);
            EndpointAddress baseAddress = new EndpointAddress(senderEndPoint1);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, senderEndPoint1);
            ICommunicator channel = factory.CreateChannel();
            Console.Write("\n  service proxy created for {0}", senderEndPoint1);
            channel.downLoadFile("");
            channel.downLoadFile("dshjgds");
        }*/
    }
    class Repo
    {
    }
    class RepositoryInitiator
    {
        static void Main(string[] args)
        {
            Console.Title = "Repository";
            Repository<Repo> obj = new Repository<Repo>();//8081
            obj.initiateRepoServer();

            /*Repository<Repo2> obj2 = new Repository<Repo2>();//8080
            obj2.initiateRepoServer2();
            string senderEndPoint1 = Comm<Repo>.makeEndPoint("http://localhost", 8081);
            EndpointAddress baseAddress = new EndpointAddress(senderEndPoint1);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, senderEndPoint1);
            ICommunicator channel = factory.CreateChannel();
            Console.Write("\n  service proxy created for {0}", senderEndPoint1);
            channel.downLoadFile("");
            channel.downLoadFile("dshjgds");*/
        }
    }
#if (TEST_REPOSITORY)
  class Program
  {
    static void Main(string[] args)
    {
       Console.Title = "Repository";
            Repository<Repo> obj = new Repository<Repo>();//8081
            obj.initiateRepoServer();
    }
  }
#endif
}