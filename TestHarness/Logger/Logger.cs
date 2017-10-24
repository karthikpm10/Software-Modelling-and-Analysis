/////////////////////////////////////////////////////////////////////////////
//  Logger.cs - demonstrate threads communicating via Queue          //
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
 *   This module logs the the given message to the file initalized with object, it facillitates logging functionality 
 * 
 *  
 *   Public Interface
 *   ----------------
 *   
 *   public Logger(String fname)
 *   public Logger()
 *   Logger objLogger = new Logger();
 *   objLogger.log(string message )
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   Logger.cs
 *   - Compiler command: csc Logger.cs
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
namespace Utilities
{
    public class Logger:MarshalByRefObject
    {
        public string fileName { get; set; }
        public bool LogCompleted { get; set; }
        //constructor which accepts a file name
        public Logger(String fname)
        {
            fileName = fname;
        }
        //constructor which doesn't accept anything
        public Logger()
        {
        }
        // writes the message to the file name initialized with the calling object
        public void log(String message)
        {
            if(fileName !=null)
            {
                using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(fileName, true))
                {
                    file.WriteLine(DateTime.Now + " : " + message);
                }
            }
            else
            {
                Console.WriteLine("File name is null");
            }
        }
        public static void main(String []args)
        {
            Logger objLogger = new Logger("../../../Logs/Dummy.txt");
            objLogger.log("test");
        }
    }
}
