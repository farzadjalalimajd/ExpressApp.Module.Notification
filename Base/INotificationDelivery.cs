namespace ExpressApp.Module.Notification.Base;

public interface INotificationDelivery
{
    public event EventHandler<NotificationAddedEventArgs> Added;
    public event EventHandler<NotificationDismissedEventArgs> Dismissed;

    public void NotifyNew(Guid oid, string message, object fromUserId, object toUserId, string objectHandle, AlertLevel level = AlertLevel.Information);
    public void NotifyDismiss(Guid oid, object toUserId);
}
