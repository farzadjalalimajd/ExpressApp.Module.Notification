using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ExpressApp.Module.Notification.Base;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;

namespace ExpressApp.Module.Notification.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DeferredDeletion(false)]
[Persistent($"gnrl.NotificationConfig")]
public abstract class GNRL_NotificationConfig : BaseObject
{
    private Type targetType;

    public GNRL_NotificationConfig(Session session) : base(session)
    {
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();

        Enabled = true;
    }

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [Persistent("Name")]
    [DbType("varchar(200)")]
    public string Name
    {
        get { return GetPropertyValue<string>(); }
        set { SetPropertyValue(nameof(Name), value); }
    }

    [Persistent("Enabled")]
    [DbType("bit")]
    public bool Enabled
    {
        get { return GetPropertyValue<bool>(); }
        set { SetPropertyValue(nameof(Enabled), value); }
    }

    [Persistent("HasEmailNotification")]
    [DbType("bit")]
    public bool HasEmailNotification
    {
        get { return GetPropertyValue<bool>(); }
        set { SetPropertyValue(nameof(HasEmailNotification), value); }
    }

    [Size(SizeAttribute.Unlimited)]
    [Persistent("Message")]
    [DbType("varchar(max)")]
    public string Message
    {
        get { return GetPropertyValue<string>(); }
        set { SetPropertyValue(nameof(Message), value); }
    }

    [IgnoreDataMember]
    [TypeConverter(typeof(SecurityTargetTypeConverter))]
    [ImmediatePostData]
    [NonPersistent]
    public Type TargetType
    {
        get
        {
            if (targetType == null && !string.IsNullOrWhiteSpace(TargetTypeFullName))
            {
                if (TargetTypeFullName is null)
                {
                    targetType = null;
                }
                else
                {
                    ITypesInfo typesInfo = XafTypesInfo.Instance;
                    targetType = typesInfo.FindTypeInfo(TargetTypeFullName)?.Type;
                }
            }

            return targetType;
        }
        set
        {
            targetType = value;
            TargetTypeFullName = value?.FullName;

            OnChanged(nameof(TargetType));
        }
    }

    //[XafDisplayName("Members")]
    //[Size(SizeAttribute.Unlimited)]
    //public string ObjMembers
    //{
    //    get
    //    {
    //        if (TargetType != null)
    //        {
    //            ITypeInfo typeInfo = XafTypesInfo.Instance.FindTypeInfo(TargetType);
    //            return typeInfo.Members.Where(x => x.IsVisible || x.IsAttributeDefined<SecurityBrowsableAttribute>(recursive: true)).Select(x => x.Name).Aggregate((x, y) => $"{x}; {y}");
    //        }
    //        return string.Empty;
    //    }
    //}

    [CriteriaOptions(nameof(TargetType))]
    [EditorAlias("PopupCriteriaPropertyEditor")]
    [Size(SizeAttribute.Unlimited)]
    [Persistent("Criteria")]
    [DbType("varchar(max)")]
    public string Criteria
    {
        get { return GetPropertyValue<string>(); }
        set { SetPropertyValue(nameof(Criteria), value); }
    }

    [Association]
    [DevExpress.Xpo.Aggregated]
    public XPCollection<GNRL_NotificationRecipientConfig> Recipients => GetCollection<GNRL_NotificationRecipientConfig>();

    [Browsable(false)]
    [Persistent("TargetType")]
    [DbType("varchar(max)")]
    [ObjectValidatorIgnoreIssue(new Type[] { typeof(ObjectValidatorLargeNonDelayedMember) })]
    public string TargetTypeFullName
    {
        get { return GetPropertyValue<string>(); }
        set { SetPropertyValue(nameof(TargetTypeFullName), value); }
    }
}
