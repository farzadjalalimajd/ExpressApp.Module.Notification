using DevExpress.Persistent.Base;

namespace ExpressApp.Module.Notification.Base
{
    public enum AlertLevel
    {
        [ImageName("State_Priority_High")]
        Critical,
        [ImageName("State_Priority_Normal")]
        Warning,
        [ImageName("State_Validation_Information")]
        Information,
        [ImageName("State_Priority_Low")]
        Success
    }
}
