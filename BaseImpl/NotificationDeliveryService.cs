using ExpressApp.Module.Notification.Base;

namespace ExpressApp.Module.Notification.BaseImpl;

public class NotificationDeliveryService : INotificationDelivery
{
    public event EventHandler<NotificationAddedEventArgs> Added;
    public event EventHandler<NotificationDismissedEventArgs> Dismissed;

    public void NotifyNew(Guid oid, string message, object toUserId, string objectHandle, AlertLevel level = AlertLevel.Information)
    {
        var eventArgs = new NotificationAddedEventArgs(oid, message, toUserId, level)
        {
            ObjectHandle = objectHandle
        };

        Added?.Invoke(this, eventArgs);
    }

    public void NotifyDismiss(Guid oid, object toUserId)
    {
        var eventArgs = new NotificationDismissedEventArgs(oid, toUserId);

        Dismissed?.Invoke(this, eventArgs);
    }
}
