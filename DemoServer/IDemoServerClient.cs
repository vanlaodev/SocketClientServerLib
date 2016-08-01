using DemoCommon;
using SocketClientServerLib;

namespace DemoServer
{
    public interface IDemoServerClient : ISslServerSessionBase, IDemoSessionCommon
    {

    }
}