using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace ExpressApp.Module.Notification.BusinessObjects;

[DefaultClassOptions]
//[NavigationItem("Settings")]
[NavigationItem(false)]
[MapInheritance(MapInheritanceType.ParentTable)]
public class GNRL_NotificationSchedule : GNRL_NotificationConfig
{
    public GNRL_NotificationSchedule(Session session) : base(session)
    {
    }
}