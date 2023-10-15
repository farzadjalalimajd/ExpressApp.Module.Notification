using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExpressApp.Module.Notification.Base
{
    public interface INotificationService
    {
        public void Send(string message, object fromUserId, object toUserId, string objectHandle, AlertLevel level = AlertLevel.Information);

        public void Send(IEnumerable<Notification> notifications);
    }

    public record Notification(string Message, object FromUserId, object ToUserId, string ObjectHandle, AlertLevel Level = AlertLevel.Information);
}
