using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Core;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace ExpressApp.Module.Notification.BaseImpl;

public class NotificationScheduleAgent : INotificationScheduleAgent
{
    private ConcurrentBag<Guid> concurrentBag;
    private Timer timer;
    private int workerCount = 0;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public NotificationScheduleAgent(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public void Start()
    {
        var delay = TimeSpan.FromMinutes(1);
        var interval = TimeSpan.FromHours(1);
        var now = DateTime.Now.Add(delay);
        var next = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).Add(interval);

        timer ??= new Timer(TimerCallback, null, next.Subtract(now).Add(delay), interval);
    }

    private void TimerCallback(object state)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var nonSecuredObjectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_NotificationSchedule>();
            var notificationSchedules = nonSecuredObjectSpace.GetObjects<GNRL_NotificationSchedule>(new BinaryOperator(nameof(GNRL_NotificationSchedule.Enabled), true));
            concurrentBag = new ConcurrentBag<Guid>(notificationSchedules?.Where(x => !(concurrentBag?.Contains(x.Oid) ?? false)).Select(x => x.Oid));

            while (workerCount < 5)
            {
                Interlocked.Increment(ref workerCount);

                Task.Run(Process);
            }
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError($"ExpressApp.Module.Notification.BaseImpl.NotificationScheduleAgent.TimerCallback: {ex.Message}");
        }
    }

    private void Process()
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var nonSecuredObjectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var notificationConfigHelper = scope.ServiceProvider.GetService<INotificationConfigHelper>();
            var notificationService = scope.ServiceProvider.GetService<INotificationService>();

            while (concurrentBag.TryTake(out Guid notificationScheduleKey))
            {
                var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_NotificationSchedule>();
                var notificationSchedule = nonSecuredObjectSpace.GetObjectByKey<GNRL_NotificationSchedule>(notificationScheduleKey);

                if (string.IsNullOrWhiteSpace(notificationSchedule.Message.Replace("'", ""))) continue;

                var targetObjects = notificationConfigHelper.GetTargetObjects(notificationSchedule);

                if (targetObjects.Count == 0) continue;

                var notifications = new List<Base.Notification>();

                foreach (var targetObject in targetObjects)
                {
                    var message = notificationSchedule.Message;

                    if (targetObject is XPBaseObject baseObject)
                    {
                        try
                        {
                            message = Convert.ToString(baseObject.Evaluate(message));
                        }
                        catch (Exception ex)
                        {
                            Tracing.Tracer.LogError($"Exception: {ex.Message}.");

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
                            var notification = new Base.Notification(message, toUserId, objHandle, notificationSchedule.HasEmailNotification);
                            notifications.Add(notification);
                        }
                    }
                }

#if RELEASE
                Tracing.Tracer.LogText($"ExpressApp.Module.Notification.BaseImpl.NotificationScheduleAgent.TimerCallback: notifications sending {notifications.Count}.");
                notificationService.Send(notifications);
                Tracing.Tracer.LogText($"ExpressApp.Module.Notification.BaseImpl.NotificationScheduleAgent.TimerCallback: notifications sent.");
#endif
            }
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError($"ExpressApp.Module.Notification.BaseImpl.NotificationScheduleAgent.TimerCallback: {ex.Message}, thread: [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
        }
        finally
        {
            Interlocked.Decrement(ref workerCount);
        }
    }
}
