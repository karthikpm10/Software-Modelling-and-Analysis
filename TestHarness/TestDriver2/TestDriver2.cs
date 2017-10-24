/////////////////////////////////////////////////////////////////////////////
//  TestDriver2.cs - demonstrate threads communicating via Queue          //
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
 *   This module implements Itest Interface, this module is designed to perform Construction testing on CodeToTest1 and CodeToTest2 
 * 
 *  
 *   Public Interface
 *   ----------------
 *   
 *   
 *   
 *   TestDriver1 objTestDriver2 = new TestDriver2();
 *   objTestDriver1.test()
 *   objTestDriver1.getLog()
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   TestDriver1.cs, ITest.cs, CodeToTest1.cs,CodeToTest3.cs 
 *   - Compiler command: csc TestDriver1.cs ITest.cs CodeToTest1.cs CodeToTest3.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 4th October 2016
 *     - first release
 * 
 */
//
using CodeToTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestInterface;
namespace TestDriver
{
    public class TestDriver2 : ITest
    {
        StringBuilder m_logs = new StringBuilder();
        // tests some functionality of CodeToTest1 and CodeToTest3
        public bool test()
        {
            CodeToTest1 code1 = new CodeToTest1();
            string newStr = code1.insertSubString("SMA", "Interesting", 5,m_logs);
            CodeToTest3 code2 = new CodeToTest3();
            int numOfInstances = code2.countSubstring(newStr, "Interesting", m_logs);
            if (numOfInstances == 5)
                return true;
            else
                return false;
        }
        //returns the logs returned by CodeToTest1 and CodeToTest3 
        public string getLog()
        {
            return m_logs.ToString();
        }
        public static void main(string[] args)
        {
            TestDriver2 objTestDriver1 = new TestDriver2();
            objTestDriver1.test();
            objTestDriver1.getLog();

        }
    }
}
