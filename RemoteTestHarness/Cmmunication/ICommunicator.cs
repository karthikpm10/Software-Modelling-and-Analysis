/////////////////////////////////////////////////////////////////////
// ICommunicator.cs - RemoteTestHarness Communicator Service Contract//
// ver 2.0                                                         //
// Karthik Palepally Muniyappa                                     //
/////////////////////////////////////////////////////////////////////
/*
 * Maintenance History:
 * ====================
 * ver 1.0 :  November 20
 * - first release
 */
using Messages;
using System.IO;
using System.ServiceModel;
namespace Cmmunication
{
    [ServiceContract]
    public interface ICommunicator
    {
        [OperationContract(IsOneWay = true)]
        void PostMessage(Message msg);
        [OperationContract(IsOneWay = true)]
        void upLoadFile(FileTransferMessage msg);
        [OperationContract]
        Stream downLoadFile(string filename);
        // used only locally so not exposed as service method
        Message GetMessage();
    }
    //cotract for uploading files to repository
    [MessageContract]
    public class FileTransferMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }
        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
}
