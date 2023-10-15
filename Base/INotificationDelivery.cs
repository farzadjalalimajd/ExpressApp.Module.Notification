using DevExpress.Data.Helpers;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base.General;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.XtraPrinting;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace ExpressApp.Module.Notification.Base;

public interface INotificationDelivery
{
    public event EventHandler<NotificationAddedEventArgs> Added;
    public event EventHandler<NotificationDismissedEventArgs> Dismissed;

    public void NotifyNew(Guid oid, string message, object fromUserId, object toUserId, string objectHandle, AlertLevel level = AlertLevel.Information);
    public void NotifyDismiss(Guid oid, object toUserId);
}
