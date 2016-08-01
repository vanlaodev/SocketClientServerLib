namespace DemoCommon
{
    public interface IDemoSessionCommon
    {
        bool SendAndForget(string text);

        string SendAndWaitReply(string text, int timeout);

        bool Reply(string targetId, string text);
    }
}