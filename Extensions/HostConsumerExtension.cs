using ExpressApp.Module.Notification.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExpressApp.Module.Notification.Extensions;

public static class HostConsumerExtension
{
    public static void RegisterNotification(this IHost host)
    {
        if (host.Services.GetRequiredService<INotificationScheduleAgent>() is INotificationScheduleAgent notificationScheduleWorker)
        {
            notificationScheduleWorker.Start();
        }

        if (host.Services.GetRequiredService<IEmailNotificationService>() is IEmailNotificationService emailNotificationService)
        {
            emailNotificationService.RegisterEvents();
        }
    }
}
