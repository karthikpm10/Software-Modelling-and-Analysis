/////////////////////////////////////////////////////////////////////////////
//  CodeToTest1.cs - demonstrate threads communicating via Queue          //
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
 *   This module append the given substring into the mainstring 'iteration' number of times and returns the new string
 * 
 *  
 *   Public Interface
 *   ----------------
 *   CodeToTest1 cd = new CodeToTest1();
 *   cd.insertSubString( mainString, substring, iterations, logs)
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   CodeToTest1.cs
 *   - Compiler command: csc CodeToTest1.cs
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
namespace CodeToTest
{
    public class CodeToTest1
    {
        //append the given substring into the mainstring 'iteration' number of times and returns the new string
        public string insertSubString(string mainString,string substring,int iterations,StringBuilder logs)
        {
            logs.AppendLine(Environment.NewLine);
            logs.Append("Inserting sub string \"").Append(substring).Append("\" into main string \"").Append(mainString).Append("\" ").Append(iterations).Append(" times");
            for (int i=0;i<iterations;i++)
            {
                mainString +=  " "+substring; 
            }
            logs.AppendLine(Environment.NewLine);
            logs.Append("Modified string is :\"").Append(mainString).Append("\"");
            return mainString;
        }
        public static void main(string[] args)
        {
            CodeToTest1 cd = new CodeToTest1();
            StringBuilder logs = new StringBuilder();
            cd.insertSubString("mainString", "substring", 5, logs);
        }
    }
}
