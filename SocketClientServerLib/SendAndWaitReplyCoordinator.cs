using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SocketClientServerLib
{
    public class SendAndWaitReplyCoordinator
    {
        private readonly ISessionBase _session;
        private readonly Dictionary<string, object> _waitHandles = new Dictionary<string, object>();
        private readonly Dictionary<string, SNWRPacket> _replies = new Dictionary<string, SNWRPacket>();

        public SendAndWaitReplyCoordinator(ISessionBase session)
        {
            _session = session;

            _session.DataReceived += SessionOnDataReceived;
        }

        private void SessionOnDataReceived(ISessionBase sessionBase, Packet packet)
        {
            var replyPacket = packet as SNWRPacket;
            if (replyPacket != null && !string.IsNullOrEmpty(replyPacket.ReplyId))
            {
                lock (_waitHandles)
                {
                    if (_waitHandles.ContainsKey(replyPacket.ReplyId))
                    {
                        var waitHandle = _waitHandles[replyPacket.ReplyId];
                        lock (waitHandle)
                        {
                            lock (_replies)
                            {
                                _replies.Add(replyPacket.ReplyId, replyPacket);
                            }
                            Monitor.Pulse(waitHandle);
                        }
                    }
                }
            }
        }

        public virtual SNWRPacket SendAndWaitReply(SNWRPacket packet, int timeout)
        {
            var waitHandle = new object();
            lock (_waitHandles)
            {
                _waitHandles.Add(packet.Id, waitHandle);
            }
            if (_session.SendData(packet))
            {
                var replied = false;
                SNWRPacket replyPacket = null;
                lock (waitHandle)
                {
                    lock (_replies)
                    {
                        if (_replies.ContainsKey(packet.Id))
                        {
                            replyPacket = _replies[packet.Id];
                            replied = true;
                        }
                    }
                    if (!replied)
                    {
                        replied = Monitor.Wait(waitHandle, timeout);
                        if (replied)
                        {
                            lock (_replies)
                            {
                                if (_replies.ContainsKey(packet.Id))
                                {
                                    replyPacket = _replies[packet.Id];
                                }
                            }
                        }
                    }
                }
                if (!replied)
                {
                    throw new TimeoutException();
                }
                if (replyPacket == null)
                {
                    throw new OperationCanceledException();
                }
                return replyPacket;
            }
            throw new InvalidOperationException("Invalid state.");
        }

        public void CancelAll()
        {
            lock (_replies)
            {
                _replies.Clear();
            }
            IEnumerable<object> waitHandles;
            lock (_waitHandles)
            {
                waitHandles = _waitHandles.Values.ToList();
                _waitHandles.Clear();
            }
            foreach (var waitHandle in waitHandles)
            {
                lock (waitHandle)
                {
                    Monitor.Pulse(waitHandle);
                }
            }
        }
    }
}