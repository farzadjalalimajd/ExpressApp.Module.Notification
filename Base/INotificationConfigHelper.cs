using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using ExpressApp.Module.Notification.BusinessObjects;
using System.Collections;

namespace ExpressApp.Module.Notification.Base;

public interface INotificationConfigHelper
{
    IList GetTargetObjects(GNRL_NotificationConfig notificationConfig, params object[] criteriaParameters);
    IEnumerable<PermissionPolicyUser> GetRecipients(GNRL_NotificationConfig notificationConfig, params object[] criteriaParameters);
    IEnumerable<PermissionPolicyUser> GetRecipients(GNRL_NotificationRecipientConfig notificationRecipientConfig, params object[] criteriaParameters);
}