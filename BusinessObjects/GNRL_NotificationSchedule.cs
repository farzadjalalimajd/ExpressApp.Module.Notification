using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace ExpressApp.Module.Notification.BusinessObjects;

[ImageName("Actions_Settings")]
[XafDisplayName("Notification Schedule")]
[MapInheritance(MapInheritanceType.ParentTable)]
public class GNRL_NotificationSchedule : GNRL_NotificationConfig
{
    public GNRL_NotificationSchedule(Session session) : base(session)
    {
    }
}