using System;
using System.Security.Cryptography;
using DemoCommon;
using SocketClientServerLib;

namespace DemoServer
{
    public class DemoServerClient : SslServerSessionBase, IDemoServerClient
    {
        private readonly SendAndWaitReplyCoordinator _sendAndWaitReplyCoordinator;

        public DemoServerClient(IIncomingDataProcessor incomingDataProcessor, IOutgoingDataProcessor outgoingDataProcessor, int receiveBufferSize) : base(incomingDataProcessor, outgoingDataProcessor, receiveBufferSize)
        {
            _sendAndWaitReplyCoordinator = new SendAndWaitReplyCoordinator(this);

            StateChanged += OnStateChanged;
        }

        private void OnStateChanged(ISessionBase sessionBase, SessionState sessionState)
        {
            if (sessionState == SessionState.Disconnecting)
            {
                _sendAndWaitReplyCoordinator.CancelAll();
            }
        }

        public bool SendAndForget(string text)
        {
            return SendData(new DemoPacket()
            {
                Text = text
            });
        }

        public string SendAndWaitReply(string text, int timeout)
        {
            var reply = _sendAndWaitReplyCoordinator.SendAndWaitReply(new DemoPacket()
            {
                Text = text
            }, timeout);
            return ((DemoPacket)reply).Text;
        }

        public bool Reply(string targetId, string text)
        {
            return SendData(new DemoPacket()
            {
                ReplyId = targetId,
                Text = text
            });
        }
    }
}