using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BaseImpl;
using Microsoft.Extensions.DependencyInjection;

namespace ExpressApp.Module.Notification.Extensions;

public static class RegisterServicesExtension
{
    public static IServiceCollection RegisterNotificationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<INotificationService, NotificationManagerService>();
        serviceCollection.AddScoped<INotificationConfigHelper, NotificationConfigHelper>();
        serviceCollection.AddSingleton<INotificationScheduleAgent, NotificationScheduleAgent>();

        serviceCollection.AddOptions<SmtpClientOptions>()
            .BindConfiguration("SmtpClientOptions");

        return serviceCollection;
    }
}
