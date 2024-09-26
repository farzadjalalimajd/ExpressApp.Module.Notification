using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using System.Collections;

namespace ExpressApp.Module.Notification.BaseImpl;

public class NotificationConfigHelper : INotificationConfigHelper
{
    private readonly INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory;

    public NotificationConfigHelper(INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory)
    {
        this.nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
    }

    public virtual IList GetTargetObjects(GNRL_NotificationConfig notificationConfig, params object[] criteriaParameters)
    {
        var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(notificationConfig.TargetType);
        var criteriaOperator = CriteriaOperator.TryParse(notificationConfig.Criteria, criteriaParameters);
        var result = nonSecuredObjectSpace.GetObjects(notificationConfig.TargetType, criteriaOperator);
        return result;
    }

    public virtual IEnumerable<PermissionPolicyUser> GetRecipients(GNRL_NotificationConfig config, params object[] criteriaParameters)
    {
        var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_NotificationConfig>();
        var notificationConfig = nonSecuredObjectSpace.GetObject(config);

        var result = new List<PermissionPolicyUser>();

        if (notificationConfig.TargetType is not null)
        {
            var targetObjects = GetTargetObjects(notificationConfig, criteriaParameters);

            try
            {
                if (targetObjects is not null)
                {
                    foreach (var targetObject in targetObjects)
                    {
                        var targetObjectKeyValue = nonSecuredObjectSpace.GetKeyValue(targetObject);

                        foreach (var recipient in notificationConfig.Recipients)
                        {
                            var parameters = new List<object>();

                            if (!string.IsNullOrWhiteSpace(recipient.Criteria))
                            {
                                for (int index = 0; index < recipient.Criteria.ToCharArray().Count(c => c == '?'); index++)
                                {
                                    parameters.Add(targetObjectKeyValue);
                                }
                            }

                            var users = GetRecipients(recipient, [.. parameters]);

                            foreach (var user in users)
                            {
                                if (!result.Any(x => x.UserName == user.UserName))
                                {
                                    result.Add(user);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DevExpress.Persistent.Base.Tracing.Tracer.LogError(ex.Message);
            }
        }

        return result.Distinct();
    }

    public virtual IEnumerable<PermissionPolicyUser> GetRecipients(GNRL_NotificationRecipientConfig notificationRecipientConfig, params object[] criteriaParameters)
    {
        var result = new List<PermissionPolicyUser>();

        try
        {
            if (notificationRecipientConfig.TargetType is not null)
            {
                var usernameList = new List<string>();
                var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(notificationRecipientConfig.TargetType);
                var objects = nonSecuredObjectSpace.GetObjects(notificationRecipientConfig.TargetType, CriteriaOperator.TryParse(notificationRecipientConfig.Criteria, criteriaParameters));

                if (objects is IEnumerable)
                {
                    if (string.IsNullOrWhiteSpace(notificationRecipientConfig.Members))
                    {
                        if (notificationRecipientConfig.TargetType.IsAssignableTo(typeof(ISecurityUser)))
                        {
                            foreach (ISecurityUser obj in objects)
                            {
                                if (obj.IsActive)
                                {
                                    usernameList.Add(obj.UserName);
                                }
                            }
                        }
                        else
                        {
                            DevExpress.Persistent.Base.Tracing.Tracer.LogError("Target type must implement 'ISecurityUser' interface.");
                        }
                    }
                    else
                    {
                        foreach (var obj in objects)
                        {
                            var _members = notificationRecipientConfig.Members.Split(';');

                            foreach (var member in _members)
                            {
                                var memberType = notificationRecipientConfig.TargetType.GetProperty(member.Trim())?.PropertyType;

                                if (memberType is null)
                                {
                                    continue;
                                }

                                if (memberType.IsAssignableTo(typeof(ISecurityUser)))
                                {
                                    var securityUser = notificationRecipientConfig.TargetType.GetProperty(member.Trim()).GetValue(obj) as ISecurityUser;

                                    if (securityUser.IsActive)
                                    {
                                        usernameList.Add(securityUser.UserName);
                                    }
                                }
                                else if (memberType.IsAssignableTo(typeof(IEnumerable<ISecurityUser>)))
                                {
                                    var elementType = memberType.GetElementType();

                                    var securityUsers = notificationRecipientConfig.TargetType.GetProperty(member.Trim()).GetValue(obj) as IEnumerable<ISecurityUser>;

                                    foreach (var securityUser in securityUsers)
                                    {
                                        if (securityUser.IsActive)
                                        {
                                            usernameList.Add(securityUser.UserName);
                                        }
                                    }
                                }
                                else
                                {
                                    DevExpress.Persistent.Base.Tracing.Tracer.LogError("Target type must implement 'ISecurityUser' interface.");
                                }
                            }
                        }
                    }
                }

                if (usernameList.Count != 0)
                {
                    result = nonSecuredObjectSpace.GetObjectsQuery<PermissionPolicyUser>().Where(x => usernameList.Contains(x.UserName)).AsEnumerable().ToList();
                }
            }
        }
        catch (Exception ex)
        {
            DevExpress.Persistent.Base.Tracing.Tracer.LogError(ex.Message);
        }

        return result;
    }

    protected virtual IEnumerable<PermissionPolicyUser> GetRecipient(Type targetType, string criteria, string members, object objectKeyValue)
    {
        var usernameList = new List<string>();

        var parameters = new List<object>();

        if (!string.IsNullOrWhiteSpace(criteria))
        {
            var count = criteria.ToCharArray().Count(c => c == '?');
            for (int index = 0; index < count; index++)
            {
                parameters.Add(objectKeyValue);
            }
        }

        var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(targetType);
        var objects = nonSecuredObjectSpace.GetObjects(targetType, CriteriaOperator.TryParse(criteria, [.. parameters]));

        try
        {
            if (string.IsNullOrWhiteSpace(members))
            {
                if (targetType.IsAssignableTo(typeof(ISecurityUser)))
                {
                    foreach (ISecurityUser obj in objects)
                    {
                        if (obj.IsActive)
                        {
                            usernameList.Add(obj.UserName);
                        }
                    }
                }
                else
                {
                    DevExpress.Persistent.Base.Tracing.Tracer.LogError("Target type must implement 'ISecurityUser' interface.");
                }
            }
            else
            {
                foreach (var obj in objects)
                {
                    var _members = members.Split(';');

                    foreach (var member in _members)
                    {
                        var memberType = targetType.GetProperty(member.Trim())?.PropertyType;

                        if (memberType is null)
                        {
                            continue;
                        }

                        if (memberType.IsAssignableTo(typeof(ISecurityUser)))
                        {
                            var securityUser = targetType.GetProperty(member.Trim()).GetValue(obj) as ISecurityUser;

                            if (securityUser.IsActive)
                            {
                                usernameList.Add(securityUser.UserName);
                            }
                        }
                        else if (memberType.IsAssignableTo(typeof(IEnumerable<ISecurityUser>)))
                        {
                            var elementType = memberType.GetElementType();

                            var securityUsers = targetType.GetProperty(member.Trim()).GetValue(obj) as IEnumerable<ISecurityUser>;

                            foreach (var securityUser in securityUsers)
                            {
                                if (securityUser.IsActive)
                                {
                                    usernameList.Add(securityUser.UserName);
                                }
                            }
                        }
                        else
                        {
                            DevExpress.Persistent.Base.Tracing.Tracer.LogError("Target type must implement 'ISecurityUser' interface.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DevExpress.Persistent.Base.Tracing.Tracer.LogError(ex.Message);
        }

        var result = nonSecuredObjectSpace.GetObjectsQuery<PermissionPolicyUser>().Where(x => usernameList.Contains(x.UserName)).AsEnumerable();

        return result;
    }
}
