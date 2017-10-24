
/////////////////////////////////////////////////////////////////////////////
//  XMLParser.cs - demonstrate threads communicating via Queue          //
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
 *   XMLParser.cs - This class implements functibalities related to xml parsing, given an xml file , kit parse it anad return an object of xmlRequestStruct
 *   xmlRexmlRequestStruct is a container for all test request related data
 * 
 *  
 *   Public Interface
 *   ----------------
 *   XMLParser objXMLParser = new XMLParser();
 *   objXMLParser.parse(XDocument xml,Logger objLoggerTH,Logger m_objLoggerTestdriver)
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  XMLParser.cs
 *   - Compiler command: csc   XMLParser
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 4th October 2016
 *     - first release
 * 
 */
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;
namespace Parser
{
    public class xmlRequestStruct
    {
        public string testName { get; set; }
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public String testDriver { get; set; }
        public List<string> testCode { get; set; }
        public void show()
        {
            Console.Write("\n  {0,-12} : {1}", "test name", testName);
            Console.Write("\n  {0,12} : {1}", "author", author);
            Console.Write("\n  {0,12} : {1}", "time stamp", timeStamp);
            Console.Write("\n  {0,12} : {1}", "test driver", testDriver);
            foreach (string library in testCode)
            {
                Console.Write("\n  {0,12} : {1}", "library", library);
            }
        }
    }
    class XMLParser
    {
        // parses the xml and stores the required fields of the xml into an object of instance xmlRequestStruct
        public List<xmlRequestStruct> parse(XDocument xml,Logger objLoggerTH)
        {
            List<xmlRequestStruct> testList = null;
            try
            {
                if (xml == null)
                    return testList;
                string author = xml.Descendants("author").Any() ? xml.Descendants("author").First().Value : "NO Name";
                XElement[] xtests = xml.Descendants("test").Any() ? xml.Descendants("test").ToArray() : null;
                if (xtests != null)
                {
                    testList = new List<xmlRequestStruct>();
                    int numTests = xtests.Count();
                    xmlRequestStruct test = null;
                    for (int i = 0; i < numTests; ++i)
                    {
                        test = new xmlRequestStruct();
                        test.testCode = new List<string>();
                        test.author = author;
                        test.timeStamp = DateTime.Now;
                        test.testName = xtests[i].Attribute("name") != null ? xtests[i].Attribute("name").Value : "No Name";
                        test.testDriver = xtests[i].Element("testDriver").Value;
                        IEnumerable<XElement> xtestCode = xtests[i].Elements("library");
                        //iterates over all library nodes in the xml
                        foreach (var xlibrary in xtestCode)
                        {
                            test.testCode.Add(xlibrary.Value);
                        }
                        testList.Add(test);
                    }
                }
            }
            catch (Exception ex)
            {
                objLoggerTH.log("Caught exception in XMLParser, Exception :"+ex);
                Console.WriteLine("Caught exception in XMLParser, Check the file " + objLoggerTH.fileName);
                testList = null;
            }
            return testList;
        }
        public static void main(String []args)
        {
            System.IO.FileStream xmlFile = new System.IO.FileStream("../../../XMLTestRequests/TestRequest.xml", System.IO.FileMode.Open);
            XDocument testRquestDoc = new XDocument();
            testRquestDoc = XDocument.Load(xmlFile);
            XMLParser objXMLParser = new XMLParser();
            
            Logger objLoggerTH = new Logger("../../../Logs/TestHarnessLogs/TestHarnessLog_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt");
            objXMLParser.parse(testRquestDoc,  objLoggerTH);

        }
    }
}
