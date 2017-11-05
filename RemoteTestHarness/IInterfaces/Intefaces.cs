/////////////////////////////////////////////////////////////////////
// Interfaces.cs - interfaces for communication between system parts//
//                                                                 //
// Karthik Palepally Muniyappa                                     //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Interfaces.cs provides interfaces:
 * - ICallback      used by child AppDomain to send messages to TestHarness
 * - ILoadAndTest   used by TestHarness
 * - ITest          used by LoadAndTest
 *
 * Required files:
 * ---------------
 * - ITest.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 20th Nov 2016
 * - first release
 */
using LoggerTH;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarnessMessages;
namespace IInterfaces
{
    /////////////////////////////////////////////////////////////
    // used by TestHarness to communicate with child AppDomain
    public interface ILoadAndTest
    {
        TestResults test(TestRequest requestInfo,Logger objLogger,string author);
        void setCallback(ICallback cb);
        void loadPath(string path);
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to invoke test driver's test()
    public interface ITest
    {
        bool test();
        string getLog();
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to send messages to TestHarness
    public interface ICallback
    {
        void sendMessage(Message msg);
    }
}
