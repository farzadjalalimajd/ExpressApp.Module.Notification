using DevExpress.ExpressApp.Core;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using System.ServiceModel.Channels;

namespace ExpressApp.Module.Notification.BaseImpl;

public class NotificationManagerService : INotificationService
{
    private readonly INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory;
    private readonly INotificationDelivery notificationDeliveryManager;

    public NotificationManagerService(INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory, INotificationDelivery notificationDeliveryManager)
    {
        this.nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
        this.notificationDeliveryManager = notificationDeliveryManager;
    }

    public void Send(string message, object fromUserId, object toUserId, string objectHandle, AlertLevel level = AlertLevel.Information)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (fromUserId is null)
        {
            return;
        }

        if (toUserId is null)
        {
            return;
        }

        var objectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_Notification>();
        var notification = objectSpace.CreateObject<GNRL_Notification>();
        notification.SetMemberValue(nameof(GNRL_Notification.Message), message);
        notification.SetMemberValue(nameof(GNRL_Notification.ObjectHandle), objectHandle);
        notification.SetMemberValue(nameof(GNRL_Notification.ToUser), objectSpace.GetObjectByKey<PermissionPolicyUser>(toUserId));
        notification.SetMemberValue(nameof(GNRL_Notification.FromUser), objectSpace.GetObjectByKey<PermissionPolicyUser>(fromUserId));
        notification.SetMemberValue(nameof(GNRL_Notification.DateCreated), DateTime.Now);
        notification.SetMemberValue(nameof(GNRL_Notification.Level), level);
        objectSpace.CommitChanges();

        Task.Run(() => {
            notificationDeliveryManager.NotifyNew(notification.Oid, notification.Message, notification.FromUser.Oid, notification.ToUser.Oid, notification.ObjectHandle);
        });
    }

    public void Send(IEnumerable<Notification.Base.Notification> notifications)
    {
        var objectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_Notification>();
        var newItems = new List<GNRL_Notification>();

        foreach (var item in notifications)
        {
            if (string.IsNullOrWhiteSpace(item.Message))
            {
                continue;
            }

            if (item.FromUserId is null)
            {
                continue;
            }

            if (item.ToUserId is null)
            {
                continue;
            }

            var notification = objectSpace.CreateObject<GNRL_Notification>();
            notification.SetMemberValue(nameof(GNRL_Notification.Message), item.Message);
            notification.SetMemberValue(nameof(GNRL_Notification.ObjectHandle), item.ObjectHandle);
            notification.SetMemberValue(nameof(GNRL_Notification.ToUser), objectSpace.GetObjectByKey<PermissionPolicyUser>(item.ToUserId));
            notification.SetMemberValue(nameof(GNRL_Notification.FromUser), objectSpace.GetObjectByKey<PermissionPolicyUser>(item.FromUserId));
            notification.SetMemberValue(nameof(GNRL_Notification.DateCreated), DateTime.Now);
            notification.SetMemberValue(nameof(GNRL_Notification.Level), item.Level);

            newItems.Add(notification);
        }

        objectSpace.CommitChanges();

        Task.Run(() => {
            foreach (var item in newItems)
            {
                notificationDeliveryManager.NotifyNew(item.Oid, item.Message, item.FromUser.Oid, item.ToUser.Oid, item.ObjectHandle);
            }
        });
    }
}
