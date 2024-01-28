using ExpressApp.Library.Abstracts;
using ExpressApp.Module.Notification.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExpressApp.Module.Notification.BaseImpl;

public class ConfigServices : IHostConsumer, IServiceCollectionConsumer
{
    public void Consume(IHost host)
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

    public void Consume(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<INotificationDelivery, NotificationDeliveryService>();
        serviceCollection.AddScoped<INotificationService, NotificationManagerService>();
        serviceCollection.AddScoped<INotificationConfigHelper, NotificationConfigHelper>();
        serviceCollection.AddSingleton<INotificationScheduleAgent, NotificationScheduleAgent>();
        serviceCollection.AddSingleton<IEmailNotificationService, EmailNotificationService>();

        serviceCollection.AddOptions<SmtpClientOptions>()
            .BindConfiguration("SmtpClientOptions");
    }
}