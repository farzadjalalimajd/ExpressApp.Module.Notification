using DevExpress.Data.Utils;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ExpressApp.Module.Notification.Base;
using System.ComponentModel;
using System.Runtime.Serialization;
namespace ExpressApp.Module.Notification.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DeferredDeletion(false)]
[Persistent($"gnrl.NotificationRecipientConfig")]
public class GNRL_NotificationRecipientConfig : BaseObject, ICheckedListBoxItemsProvider
{
    private Type targetType;

    public GNRL_NotificationRecipientConfig(Session session) : base(session)
    {
        Changed += LIMS_NotificationRecipientConfig_Changed;
    }

    private void LIMS_NotificationRecipientConfig_Changed(object sender, ObjectChangeEventArgs e)
    {
        if (e.PropertyName == nameof(TargetTypeFullName))
        {
            Members = null;
        }
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

            OnChanged();

            OnItemsChanged();
        }
    }

    [ImmediatePostData]
    [Size(SizeAttribute.Unlimited)]
    [ObjectValidatorIgnoreIssue(new Type[] { typeof(ObjectValidatorLargeNonDelayedMember) })]
    [EditorAlias("CheckedListBoxEditor")]
    [Persistent("Members")]
    [DbType("varchar(max)")]
    public string Members
    {
        get { return GetPropertyValue<string>(); }
        set { SetPropertyValue(nameof(Members), value); }
    }

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

    [VisibleInDetailView(false)]
    [CollectionOperationSet(AllowAdd = false, AllowRemove = false)]
    public IList<PermissionPolicyUser> RecipientUsers
    {
        get
        {
            var service = Session.ServiceProvider.GetService<INotificationConfigHelper>();
            return service.GetRecipients(this).ToList();
        }
    }

    [Association]
    [Persistent("NotificationConfig")]
    public GNRL_NotificationConfig NotificationConfig
    {
        get { return GetPropertyValue<GNRL_NotificationConfig>(); }
        set { SetPropertyValue(nameof(NotificationConfig), value); }
    }

    [Browsable(false)]
    [Persistent("TargetType")]
    [DbType("varchar(max)")]
    [ObjectValidatorIgnoreIssue(new Type[] { typeof(ObjectValidatorLargeNonDelayedMember) })]
    public string TargetTypeFullName
    {
        get { return GetPropertyValue<string>(); }
        set { SetPropertyValue(nameof(TargetTypeFullName), value); }
    }

    Dictionary<object, string> ICheckedListBoxItemsProvider.GetCheckedListBoxItems(string targetMemberName)
    {
        var dictionary = new Dictionary<object, string>();

        if (targetMemberName == "Members" && TargetType != null)
        {
            ITypeInfo typeInfo = XafTypesInfo.Instance.FindTypeInfo(TargetType);

            foreach (IMemberInfo member in typeInfo.Members)
            {
                if (member.IsVisible || member.IsAttributeDefined<SecurityBrowsableAttribute>(recursive: true))
                {
                    string memberCaption = CaptionHelper.GetMemberCaption(member);

                    if (dictionary.ContainsKey(member.Name))
                    {
                        throw new LightDictionary<string, string>.DuplicatedKeyException(member.Name, dictionary[member.Name], memberCaption);
                    }

                    dictionary.Add(member.Name, memberCaption);
                }
            }
        }

        return dictionary;
    }

    public event EventHandler ItemsChanged;

    protected virtual void OnItemsChanged()
    {
        if (this.ItemsChanged != null)
        {
            this.ItemsChanged(this, EventArgs.Empty);
        }
    }
}