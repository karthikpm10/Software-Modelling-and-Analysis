/////////////////////////////////////////////////////////////////////////////
//  CodeToTest2.cs - demonstrate threads communicating via Queue          //
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
 *   This module counts the number of instances of substring in the mainstring  and returns the count
 * 
 *  
 *   Public Interface
 *   ----------------
 *   CodeToTest2 cd = new CodeToTest2();
 *   cd.countSubstring(string mainString,string subString,StringBuilder logs)
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   CodeToTest2.cs
 *   - Compiler command: csc CodeToTest2.cs
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace CodeToTest
{
    public class CodeToTest2
    {
        //counts the number of instances of substring in the mainstring  and returns the count
        public int countSubstring(string mainString,string subString,StringBuilder logs)
        {
            int count = Regex.Matches(mainString, subString).Count;
            logs.AppendLine(Environment.NewLine);
            logs.Append("Count of \"").Append(subString).Append("\"").Append(" in \"").Append(mainString).Append("\"").Append(" is ").Append(count.ToString());
            return count;
        }
        public static void main(string[] args)
        {
            CodeToTest2 cd = new CodeToTest2();
            StringBuilder logs = new StringBuilder();
            cd.countSubstring(" mainString", "subString", logs);
        }
    }
}
