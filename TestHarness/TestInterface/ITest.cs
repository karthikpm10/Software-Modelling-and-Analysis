/////////////////////////////////////////////////////////////////////////////
//  ITest.cs - demonstrate threads communicating via Queue          //
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
 *   This module defines an  interface for the Test Driver 
 * 
 *  
 *   Public Interface
 *   ----------------
 *   
 *  bool test();
 *  string getLog();
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   ITest.cs
 *   - Compiler command: csc ITest.cs
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
namespace TestInterface
{
    public interface ITest
    {
        bool test();
        string getLog();
    }
}
