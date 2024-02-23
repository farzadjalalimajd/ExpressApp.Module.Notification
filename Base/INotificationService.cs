namespace ExpressApp.Module.Notification.Base
{
    public interface INotificationService
    {
        public void Send(string message, object toUserId, string objectHandle, bool HasEmailNotification = false);

        public void Send(IEnumerable<Notification> notifications);
    }

    public record Notification(string Message, object ToUserId, string ObjectHandle, bool HasEmailNotification = false);
}
