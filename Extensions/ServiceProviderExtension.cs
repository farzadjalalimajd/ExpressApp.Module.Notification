using ExpressApp.Module.Notification.Base;
using Microsoft.Extensions.DependencyInjection;

namespace ExpressApp.Module.Notification.Extensions;

public static class ServiceProviderExtension
{
    public static void InitializeNotificationScheduler(this IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetRequiredService<INotificationScheduleAgent>() is INotificationScheduleAgent notificationScheduleWorker)
        {
            notificationScheduleWorker.Start();
        }
    }
}