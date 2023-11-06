using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Core;
using DevExpress.Xpo;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using System.Collections.Concurrent;

namespace ExpressApp.Module.Notification.BaseImpl;

public class NotificationAgent
{
    private readonly INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory;
    private ConcurrentBag<Guid> concurrentBag;
    private readonly INotificationConfigHelper notificationConfigHelper;
    private readonly INotificationService notificationService;

    public NotificationAgent(INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory, INotificationConfigHelper notificationConfigHelper, INotificationService notificationService)
    {
        this.nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
        this.notificationConfigHelper = notificationConfigHelper;
        this.notificationService = notificationService;
    }

    public void Start()
    {
        Task.Run(() =>
        {
            Thread.Sleep(TimeSpan.FromMinutes(1));

            var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_NotificationSchedule>();
            var notificationSchedules = nonSecuredObjectSpace.GetObjects<GNRL_NotificationSchedule>(new BinaryOperator(nameof(GNRL_NotificationSchedule.Enabled), true));
            concurrentBag = new ConcurrentBag<Guid>(notificationSchedules?.Select(x => x.Oid));

            for (int index = 0; index < 5; index++)
            {
                Task.Run(Process);
            }

            void Process()
            {
                while (concurrentBag.TryTake(out Guid notificationScheduleKey))
                {
                    var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_NotificationSchedule>();
                    var notificationSchedule = nonSecuredObjectSpace.GetObjectByKey<GNRL_NotificationSchedule>(notificationScheduleKey);
                    var message = notificationSchedule.Message;

                    if (string.IsNullOrWhiteSpace(message.Replace("'", ""))) continue;

                    var targetObjects = notificationConfigHelper.GetTargetObjects(notificationSchedule);

                    if (targetObjects.Count == 0) continue;

                    var notifications = new List<Base.Notification>();

                    foreach (var targetObject in targetObjects)
                    {
                        if (targetObject is XPBaseObject baseObject)
                        {
                            try
                            {
                                message = Convert.ToString(baseObject.Evaluate(message));
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(message.Replace("'", ""))) continue;

                        var objKeyValue = nonSecuredObjectSpace.GetKeyValue(targetObject);
                        var objHandle = nonSecuredObjectSpace.GetObjectHandle(targetObject);

                        foreach (var recipient in notificationSchedule.Recipients)
                        {
                            var parameters = new List<object>();
                            if (!string.IsNullOrWhiteSpace(recipient.Criteria))
                            {
                                for (int index = 0; index < recipient.Criteria.ToCharArray().Count(c => c == '?'); index++)
                                {
                                    parameters.Add(objKeyValue);
                                }
                            }

                            var permissionPolicyUsers = notificationConfigHelper.GetRecipients(recipient, parameters.ToArray());

                            if (!permissionPolicyUsers.Any()) continue;

                            foreach (var permissionPolicyUser in permissionPolicyUsers)
                            {
                                var toUserId = permissionPolicyUser.Oid;
                                var notification = new Base.Notification(message, null, toUserId, objHandle);
                                notifications.Add(notification);
                            }
                        }
                    }

                    notificationService.Send(notifications);
                }
            }
        });
    }
}