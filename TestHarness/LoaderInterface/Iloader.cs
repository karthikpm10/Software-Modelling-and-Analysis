/////////////////////////////////////////////////////////////////////////////
//  Iloader.cs - demonstrate threads communicating via Queue          //
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
 *   This module defines an  interface for the loader 
 * 
 *  
 *   Public Interface
 *   ----------------
 *   
 *   startLoader(string xmlRequest,Logger m_objLoggerTH,Logger m_objloggerTestDriver);
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   Iloader.cs
 *   - Compiler command: csc Iloader.cs
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
namespace LoaderInterface
{
    public interface Iloader
    {
        void startLoader(string xmlRequest,Logger m_objLoggerTH,Logger m_objloggerTestDriver);
    }
}
