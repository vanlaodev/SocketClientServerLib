using System;
using DemoCommon;
using SocketClientServerLib;

namespace DemoClient
{
    public class DemoClient : SslClientSessionBase, IDemoClient
    {
        private readonly SendAndWaitReplyCoordinator _sendAndWaitReplyCoordinator;

        public DemoClient(IIncomingDataProcessor incomingDataProcessor, IOutgoingDataProcessor outgoingDataProcessor, int receiveBufferSize) : base(incomingDataProcessor, outgoingDataProcessor, receiveBufferSize)
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