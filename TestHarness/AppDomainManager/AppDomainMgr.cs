/////////////////////////////////////////////////////////////////////////////
//  AppDomainMgr.cs - demonstrate threads communicating via Queue          //
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
 *   This module Builds a child app domain and returns a reference to this child app domain created 
 * 
 *  
 *   Public Interface
 *   ----------------
 *   AppDomainMgr ad = new AppDomainMgr();
 *   ad.createAppDomain( childDomainName, applicationBase)
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   AppDomainMgr.cs
 *   - Compiler command: csc AppDomainMgr.cs
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
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
namespace AppDomainManager
{
    public class AppDomainMgr
    {
        // create a child app domain and return it
        public AppDomain createAppDomain(string childDomainName,string applicationBase)
        {
            //creating child domain info
            AppDomainSetup childDomaininfo = new AppDomainSetup();
            if(applicationBase!=null)
            {
                childDomaininfo.ApplicationBase = applicationBase;
            }
            //creating evidence for child domain
            Evidence childevidence = AppDomain.CurrentDomain.Evidence;
            //creating child domain
            AppDomain childAppDomain = AppDomain.CreateDomain(childDomainName, childevidence, childDomaininfo);
            return childAppDomain;
        }
        public static void main(string[] args)
        {
            AppDomainMgr ad = new AppDomainMgr();
            ad.createAppDomain("childDomainName", "applicationBase");
        }
    }
}
