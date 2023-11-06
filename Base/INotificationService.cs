namespace ExpressApp.Module.Notification.Base
{
    public interface INotificationService
    {
        public void Send(string message, object fromUserId, object toUserId, string objectHandle, AlertLevel level = AlertLevel.Information);

        public void Send(IEnumerable<Notification> notifications);
    }

    public record Notification(string Message, object FromUserId, object ToUserId, string ObjectHandle, AlertLevel Level = AlertLevel.Information);
}
