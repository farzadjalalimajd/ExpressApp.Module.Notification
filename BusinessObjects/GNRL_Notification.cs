using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.General;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using System.ComponentModel;

namespace ExpressApp.Module.Notification.BusinessObjects;

[DeferredDeletion(false)]
[OptimisticLocking(false)]
[Persistent($"gnrl.Notification")]
public class GNRL_Notification : BaseObject, ISupportNotifications
{
    public GNRL_Notification(Session session) : base(session)
    {
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();

        IsDelivered = false;
        IsPostponed = false;
    }

    [VisibleInListView(true)]
    [Size(SizeAttribute.Unlimited)]
    [Persistent("Message")]
    [DbType("varchar(max)")]
    public string Message
    {
        get { return GetPropertyValue<string>(); }
        private set { SetPropertyValue(nameof(Message), value); }
    }

    [HideInUI(HideInUI.ListViewColumn | HideInUI.ListViewCustomizationForm | HideInUI.DetailViewEditor | HideInUI.DetailViewCustomizationForm)]
    [Persistent("DateCreated")]
    [DbType("datetime2(0)")]
    public DateTime DateCreated
    {
        get { return GetPropertyValue<DateTime>(); }
        private set { SetPropertyValue(nameof(DateCreated), value); }
    }

    [HideInUI(HideInUI.ListViewColumn | HideInUI.ListViewCustomizationForm | HideInUI.DetailViewEditor | HideInUI.DetailViewCustomizationForm)]
    [NoForeignKey]
    [Persistent("ToApplicationUser")]
    public PermissionPolicyUser ToUser
    {
        get { return GetPropertyValue<PermissionPolicyUser>(); }
        private set { SetPropertyValue(nameof(ToUser), value); }
    }

    [HideInUI(HideInUI.ListViewColumn | HideInUI.ListViewCustomizationForm | HideInUI.DetailViewEditor | HideInUI.DetailViewCustomizationForm)]
    [Persistent("IsDelivered")]
    [DbType("bit")]
    public bool IsDelivered
    {
        get { return GetPropertyValue<bool>(); }
        private set { SetPropertyValue(nameof(IsDelivered), value); }
    }

    [HideInUI(HideInUI.ListViewColumn | HideInUI.ListViewCustomizationForm | HideInUI.DetailViewEditor | HideInUI.DetailViewCustomizationForm)]
    [Persistent("ObjectHandle")]
    [DbType("varchar(max)")]
    public string ObjectHandle
    {
        get { return GetPropertyValue<string>(); }
        private set { SetPropertyValue(nameof(ObjectHandle), value); }
    }

    /// <summary>
    /// Three state property, it is null if this notification doesn't need email notification
    /// </summary>
    [HideInUI(HideInUI.ListViewColumn | HideInUI.ListViewCustomizationForm | HideInUI.DetailViewEditor | HideInUI.DetailViewCustomizationForm)]
    [Persistent("IsEmailed")]
    [DbType("bit")]
    public bool? IsEmailed
    {
        get { return GetPropertyValue<bool?>(); }
        private set { SetPropertyValue(nameof(IsEmailed), value); }
    }

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [Persistent("AlarmTime")]
    [DbType("datetime2(0)")]
    public DateTime? AlarmTime
    {
        get { return GetPropertyValue<DateTime?>(); }
        private set { SetPropertyValue(nameof(AlarmTime), value); }
    }

    [HideInUI(HideInUI.ListViewColumn | HideInUI.ListViewCustomizationForm | HideInUI.DetailViewEditor | HideInUI.DetailViewCustomizationForm)]
    [Persistent("IsPostponed")]
    [DbType("bit")]
    public bool IsPostponed
    {
        get { return GetPropertyValue<bool>(); }
        private set { SetPropertyValue(nameof(IsPostponed), value); }
    }

    #region ISupportNotifications
    DateTime? ISupportNotifications.AlarmTime { get => AlarmTime; set => AlarmTime = value; }

    object ISupportNotifications.UniqueId => Oid;

    string ISupportNotifications.NotificationMessage => Message;

    bool ISupportNotifications.IsPostponed { get => IsPostponed; set => IsPostponed = value; }
    #endregion
}
