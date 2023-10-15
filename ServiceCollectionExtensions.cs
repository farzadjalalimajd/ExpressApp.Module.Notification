using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BaseImpl;
using Microsoft.Extensions.DependencyInjection;

namespace ExpressApp.Module.Notification;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationModuleServices(this IServiceCollection services)
    {
        services.AddSingleton<INotificationDelivery, NotificationDeliveryService>();
        services.AddScoped<INotificationService, NotificationManagerService>();
        services.AddScoped<INotificationConfigHelper, NotificationConfigHelper>();

        return services;
    }
}
