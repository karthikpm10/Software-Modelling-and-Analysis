/////////////////////////////////////////////////////////////////////////////
//  FileManager.cs - demonstrate threads communicating via Queue          //
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
 *   This module performs operations related to file system
 * 
 *  
 *   Public Interface
 *   ----------------
 *   FileManager fm = new FileManager();
 *   fm.listFiles(string fPath, string fileExtension )
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   FileManager.cs
 *   - Compiler command: csc FileManager.cs
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
namespace Filemanager
{
    public class FileManager
    {
        // lists the files with the given extension in the given path
        public string[] listFiles(string fPath, string fileExtension )
        {
            try
            {
                string[] filesList = System.IO.Directory.GetFiles(fPath, "*." + fileExtension);
                return filesList;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Caught exception in File manager, Exception: ",ex);
                return null;
            }
        }
         static void Main(string[] args)
        {
            FileManager fm = new FileManager();
            fm.listFiles("../../../XMLTestRequests/", "xml");
        }
    }
}
