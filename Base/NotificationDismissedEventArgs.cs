namespace ExpressApp.Module.Notification.Base;

public class NotificationDismissedEventArgs
{
    public NotificationDismissedEventArgs(Guid oid, object toUserId)
    {
        ToUserId = toUserId;
        Oid = oid;
    }

    public Guid Oid { get; set; }
    public object ToUserId { get; set; }
}
