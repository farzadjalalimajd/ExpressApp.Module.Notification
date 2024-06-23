using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BaseImpl;
using Microsoft.Extensions.DependencyInjection;

namespace ExpressApp.Module.Notification.Extensions;

public static class RegisterServicesExtension
{
    public static IServiceCollection RegisterNotificationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<INotificationDelivery, NotificationDeliveryService>();
        serviceCollection.AddScoped<INotificationService, NotificationManagerService>();
        serviceCollection.AddScoped<INotificationConfigHelper, NotificationConfigHelper>();
        serviceCollection.AddSingleton<INotificationScheduleAgent, NotificationScheduleAgent>();
        serviceCollection.AddSingleton<IEmailNotificationService, EmailNotificationService>();

        serviceCollection.AddOptions<SmtpClientOptions>()
            .BindConfiguration("SmtpClientOptions");

        return serviceCollection;
    }
}
