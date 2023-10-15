namespace ExpressApp.Module.Notification.Base;

public class NotificationAddedEventArgs
{
    public NotificationAddedEventArgs(Guid oid, string message, object fromUserId, object toUserId, AlertLevel level = AlertLevel.Information)
    {
        Message = message;
        ToUserId = toUserId;
        Level = level;
        FromUserId = fromUserId;
        Oid = oid;
    }

    public Guid Oid { get; set; }
    public string Message { get; set; }
    public string ObjectHandle { get; set; }
    public object FromUserId { get; set; }
    public object ToUserId { get; set; }
    public AlertLevel Level { get; set; }
}
