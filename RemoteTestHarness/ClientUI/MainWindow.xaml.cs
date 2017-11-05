/////////////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs - defines the fuctionality for User interface       //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Platform:     Windows 10,                                              //
//  Application:  RemoteTestHarness Project                                //
//  Author:       Karthik Palepally Muniyappa                              //
//                                                                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This modules defines the controls for the buttons, text boxes and list items,
 *   It hosts a client service , and sends test requests to TestHarness server , uploads files to the repository and queries
 *   repository for the logs
 * 
 *  
 *  
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   MainWindow.xaml.cs
 *   - Compiler command: csc MainWindow.xaml.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20th November 2016
 *     - first release
 * 
 */
using Cmmunication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;
using TestHarnessMessages;
using Utilities;
namespace ClientUI
{
    public partial class MainWindow : Window
    {
        public Comm<Client> objComm = new Comm<Client>();
        public ICommunicator Repochannel = null;
        delegate void NewMessage(Messages.Message msg);
        public int repoPort = 8085;
        public int clientPort = 8082;
        public int THport = 8081;
        public string repoPath = "..\\..\\..\\repofiles";
        public string testRequestPath = "..\\..\\..\\XMLTestRequests//TestRequest1.xml";
        public string url = "http://localhost";
        event NewMessage OnNewMessage;
        Thread rcvThread;
        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
            Console.Title = "CLient1";
            //hosting client service
            LibFiles.Text = Path.GetFullPath(repoPath);
            TestRequest.Text = Path.GetFullPath(testRequestPath);
            hostClient();
            authorName.Text = "Karthik";
            TestRequestQuery.Text = "TestRequest1";
            Title = "CLient 1";
            OnNewMessage += new NewMessage(OnNewMessageHandler);
            BrowseTestRequest.IsEnabled = false;
            submitTestRequest.IsEnabled = false;
            // calling test executive for demonstration
            testExecutive();


        }
        public void testExecutive()
        {
            Console.WriteLine("*******Requirement 13: Running Test execuitive to demonstrate requirements ");
            //upload files
            List<string> listFiles = new List<string>(System.IO.Directory.GetFiles(Path.GetFullPath(repoPath) + "\\", "*.dll"));
            if (listFiles.Count > 0)
            {
                if (Repochannel == null)
                {
                    createRepoChannel();
                }
                foreach (string file in listFiles)
                {
                    Console.WriteLine("**********Requirement 2 & 6: library file {0} sent to repository", file);
                    uploadFileToRepo(file, file.Substring(file.LastIndexOf("\\") + 1));
                }
                //send test request
                XDocument testRquestDoc = new XDocument();
                testRquestDoc = XDocument.Load(Path.GetFullPath(testRequestPath));
                TestRequest TRobj = testRquestDoc.ToString().FromXml<TestRequest>();
                Messages.Message msg = new Messages.Message();
                msg.body = testRquestDoc.ToString();
                msg.type = "TestRequest";
                msg.from = Comm<Client>.makeEndPoint(url, clientPort);
                msg.to = Comm<Client>.makeEndPoint(url, THport);
                msg.time = DateTime.Now;
                msg.author = TRobj.author;
                Console.WriteLine("**********Requirement 2 & 6: Test Request  sent to TestHarness");
                Console.WriteLine(msg.body);
                objComm.sndr.PostMessage(msg);
                //query for logs
                LogRequest objlogRequest = new LogRequest();
                objlogRequest.author = authorName.Text;
                objlogRequest.TestRequestName = TestRequestQuery.Text;
                Messages.Message logMessage = new Messages.Message();
                logMessage.author = authorName.Text;
                logMessage.body = objlogRequest.ToXml();
                logMessage.to = Comm<Client>.makeEndPoint(url, repoPort);
                logMessage.from = Comm<Client>.makeEndPoint(url, clientPort);
                logMessage.type = "LogRequest";
                logMessage.time = DateTime.Now;
                objComm.sndr.PostMessage(logMessage);
            }
        }
        void rcvThreadProc()
        {
            while (true)
            {
                // get message out of receive queue - will block if queue is empty
                Messages.Message rcvdMsg = new Messages.Message();
                rcvdMsg = objComm.rcvr.GetMessage();

                // call window functions on UI thread
                this.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  OnNewMessage,
                  rcvdMsg);
            }
        }
        //called on UI thread, updates the appropriate list boxes
        void OnNewMessageHandler(Messages.Message msg)
        {
            if (msg.type == "LogResult")
            {
                if (msg.body == "LogsNotfound")
                {
                    Logs.Items.Insert(0, "Log not found");
                }
                else
                {
                    Logs.Items.Insert(0, msg.body);
                }
            }
            else if (msg.type == "TestResult")
            {
                Results.Items.Insert(0, msg.body);
            }
        }
        public void hostClient()
        {
            string rcvrEndPoint = Comm<Client>.makeEndPoint(url, clientPort);
            try
            {
                objComm.rcvr.CreateRecvChannel(rcvrEndPoint);
                rcvThread = objComm.rcvr.start(rcvThreadProc);
                rcvThread.IsBackground = true;
                Console.Write("\n  rcvr thread id = {0}", rcvThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append(rcvrEndPoint.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
        }
        //opens file browser
        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = sender as System.Windows.Controls.Button;
            string buttonName = b.Name;
            if (buttonName == "BrowseDll")
            {
                var dialog = new FolderBrowserDialog();

                dialog.ShowDialog();
                if (dialog.SelectedPath.Length > 0)
                    LibFiles.Text = dialog.SelectedPath;
            }
            else
            {
                FileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                if (fileDialog.FileName.Length > 0)
                    TestRequest.Text = fileDialog.FileName;
            }
        }
        //sends dll's to repository
        private void UploadFilesButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> listFiles = new List<string>(System.IO.Directory.GetFiles(LibFiles.Text + "\\", "*.dll"));
            if (listFiles.Count > 0)
            {
                if (Repochannel == null)
                {
                    createRepoChannel();
                }
                foreach (string file in listFiles)
                {
                    Console.WriteLine("**********Requirement 2 & 6: library file {0} sent to repository", file);
                    uploadFileToRepo(file, file.Substring(file.LastIndexOf("\\") + 1));
                }
            }
            else
            {
                Results.Items.Insert(0, "No DLL's found in the selected DIrectory");
            }
            BrowseTestRequest.IsEnabled = true;
            submitTestRequest.IsEnabled = true;
        }
        //sends test request to Test Harness
        private void submitTestRequestButton_Click(object sender, RoutedEventArgs e)
        {
            XDocument testRquestDoc = new XDocument();
            testRquestDoc = XDocument.Load(TestRequest.Text);
            TestRequest TRobj = testRquestDoc.ToString().FromXml<TestRequest>();
            Messages.Message msg = new Messages.Message();
            msg.body = testRquestDoc.ToString();
            msg.type = "TestRequest";
            msg.from = Comm<Client>.makeEndPoint(url, clientPort);
            msg.to = Comm<Client>.makeEndPoint(url, THport);
            msg.time = DateTime.Now;
            msg.author = TRobj.author;
            Console.WriteLine("**********Requirement 2 & 6: Test Request  sent to TestHarness");
            Console.WriteLine(msg.body);
            objComm.sndr.PostMessage(msg);
        }
        //gets logs from the repository
        private void GetLogsButton_Click(object sender, RoutedEventArgs e)
        {
            LogRequest objlogRequest = new LogRequest();
            objlogRequest.author = authorName.Text;
            objlogRequest.TestRequestName = TestRequestQuery.Text;
            Messages.Message logMessage = new Messages.Message();
            logMessage.author = authorName.Text;
            logMessage.body = objlogRequest.ToXml();
            logMessage.to = Comm<Client>.makeEndPoint(url, repoPort);
            logMessage.from = Comm<Client>.makeEndPoint(url, clientPort);
            logMessage.type = "LogRequest";
            logMessage.time = DateTime.Now;
            objComm.sndr.PostMessage(logMessage);
            Console.WriteLine("");
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //  objComm.sndr.PostMessage("quit");
            objComm.sndr.Close();
            objComm.rcvr.Close();
        }
        // creates a repository channel for communication
        public void createRepoChannel()
        {
            string senderEndPoint1 = Comm<Client>.makeEndPoint(url, repoPort);
            EndpointAddress baseAddress = new EndpointAddress(senderEndPoint1);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, senderEndPoint1);
            Repochannel = factory.CreateChannel();
        }
        //uploads file to repository
        public void uploadFileToRepo(string filePath, string filename)
        {
            try
            {
                //hrt.Start();
                using (var inputStream = new FileStream(filePath, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = filename;
                    msg.transferStream = inputStream;
                    Repochannel.upLoadFile(msg);
                }
                // hrt.Stop();
                //Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            }
            catch (Exception e)
            {
                Console.Write("\n  can't find \"{0}\" exception {1}", filePath, e);
            }
        }
    }
    public class Client
    {
    }
}
