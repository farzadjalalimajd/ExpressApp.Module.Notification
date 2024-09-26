using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Services.Localization;
using DevExpress.Persistent.Base;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace ExpressApp.Module.Notification.BaseImpl;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly INotificationDelivery notificationDeliveryService;
    private readonly ConcurrentBag<Guid> notificationKeyList;
    private Timer timer;
    private int workerCount = 0;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IOptions<SmtpClientOptions> options;
    private readonly ICaptionHelperProvider captionHelperProvider;

    public EmailNotificationService(INotificationDelivery notificationDeliveryService, IServiceScopeFactory serviceScopeFactory, IOptions<SmtpClientOptions> options, ICaptionHelperProvider captionHelperProvider)
    {
        this.notificationDeliveryService = notificationDeliveryService;
        this.serviceScopeFactory = serviceScopeFactory;
        this.options = options;
        this.captionHelperProvider = captionHelperProvider;
        notificationKeyList = new ConcurrentBag<Guid>();
    }

    public void RegisterEvents()
    {
        if (string.IsNullOrWhiteSpace(options.Value.Host) || string.IsNullOrWhiteSpace(options.Value.Domain) || string.IsNullOrWhiteSpace(options.Value.UserName) || string.IsNullOrWhiteSpace(options.Value.Password))
        {
            return;
        }

        notificationDeliveryService.Added += NotificationDeliveryService_Added;

        timer ??= new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        Task.Run(LoadFromDb);
    }

    private void LoadFromDb()
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var nonSecuredObjectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_Notification>();
            var c1 = new BinaryOperator(nameof(GNRL_Notification.IsEmailed), false);
            var c2 = new BinaryOperator(nameof(GNRL_Notification.IsDelivered), false);
            var c3 = new BinaryOperator(nameof(GNRL_Notification.AlarmTime), DateTime.Now, BinaryOperatorType.LessOrEqual);
            var c4 = new GroupOperator(GroupOperatorType.And, c1, c2, c3);
            var pendingNotifications = nonSecuredObjectSpace.GetObjects<GNRL_Notification>(c4);

            foreach (var pendingNotification in pendingNotifications)
            {
                if (!notificationKeyList.Any(x => x.Equals(pendingNotification.Oid)))
                {
                    notificationKeyList.Add(pendingNotification.Oid);
                }
            }
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError($"ExpressApp.Module.Notification.BaseImpl.EmailNotificationService.LoadFromDb: {ex.Message}");
        }
    }

    private void TimerCallback(object state)
    {
        if (notificationKeyList.Any())
        {
            while (workerCount < 5)
            {
                Interlocked.Increment(ref workerCount);

                Task.Run(Process);
            }
        }
    }

    private void Process()
    {
        try
        {
            while (notificationKeyList.TryTake(out Guid notificationKey))
            {
                using var scope = serviceScopeFactory.CreateScope();
                var nonSecuredObjectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
                var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<GNRL_Notification>();
                var notification = nonSecuredObjectSpace.GetObjectByKey<GNRL_Notification>(notificationKey);

                if ((notification.IsEmailed ?? true) || notification.IsDelivered || !notification.AlarmTime.HasValue)
                {
                    continue;
                }

                var fromAddress = new MailAddress(options.Value.UserName, "ExpressApp");
                //var toAddress = new MailAddress($"{notification.ToUser.UserName}@{options.Value.Domain}", notification.ToUser.UserName);
                var toAddress = new MailAddress($"farzadjalalimajd@gmail.com", notification.ToUser.UserName);

                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //var sourceClassFullName = (nonSecuredObjectSpace.GetObjectByHandle(notification.ObjectHandle) as PersistentBase).ClassInfo.FullName;
                //var subject = captionHelperProvider.GetCaptionHelper().GetClassCaption(sourceClassFullName);
                var subject = "ExpressApp";
                var body = notification.Message;

                var smtp = new SmtpClient
                {
                    Host = options.Value.Host,
                    Port = options.Value.Port,
                    EnableSsl = options.Value.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = options.Value.UseDefaultCredentials,
                    Credentials = new NetworkCredential(options.Value.UserName, options.Value.Password)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                };

                //smtp.Send(message);

                notification.SetMemberValue(nameof(GNRL_Notification.IsEmailed), true);
                nonSecuredObjectSpace.CommitChanges();
            }
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError($"ExpressApp.Module.Notification.BaseImpl.EmailNotificationService.Process: {ex.Message}");
        }
        finally
        {
            Interlocked.Decrement(ref workerCount);
        }
    }

    private void NotificationDeliveryService_Added(object sender, NotificationAddedEventArgs e)
    {
        if (!notificationKeyList.Any(x => x.Equals(e.Oid)))
        {
            notificationKeyList.Add(e.Oid);
        }
    }

    private string GetAlertHtml(string message)
    {
        return $@"<div style=""background:#FFCDB0;background-color:#FFCDB0;margin:0px auto;border-radius:4px;max-width:600px;direction: rtl;"">
                <table style=""background:#FFCDB0;background-color:#FFCDB0;width:100%;border-radius:4px;"">
                    <tbody>
                        <tr>
                            <td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;"">
                                <div class=""mj-column-per-100 mj-outlook-group-fix""
                                    style=""font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                                    <table style=""vertical-align:top;"" width=""100%"">
                                        <tbody>
                                            <tr>
                                                <td style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
                                                    <div
                                                        style=""font-family:Helvetica, Arial, sans-serif;font-size:18px;font-weight:bold;line-height:24px;text-align:left;color:#7A0B1F;"">
                                                        <p class=""date"" style=""margin: 0; margin-bottom: 5px; font-size: 16px;"">
                                                            ALERT</p>
                                                        <h2
                                                            style=""margin: 0; font-size: 24px; font-weight: bold; line-height: 24px;"">
                                                            Delivery Cancelled</h2>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
                                                    <div
                                                        style=""font-family:Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;line-height:24px;text-align:left;color:#7A0B1F;"">
                                                        <p style=""margin: 0;"">{message}</p>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align=""right"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
                                                    <table style=""border-collapse:separate;line-height:100%;"">
                                                        <tbody>
                                                            <tr>
                                                                <td style=""border:none;border-radius:30px;cursor:auto;background:#7A0B1F;""
                                                                    valign=""middle"">
                                                                    <a href=""https://google.com""
                                                                        style=""display: inline-block; background: #7A0B1F; color: #FFCDB0; font-family: Helvetica, Arial, sans-serif; font-size: 14px; font-weight: bold; line-height: 30px; margin: 0; text-decoration: none; text-transform: uppercase; padding: 10px 25px; border-radius: 30px;""
                                                                        target=""_blank""> more details </a>
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>";
    }
}