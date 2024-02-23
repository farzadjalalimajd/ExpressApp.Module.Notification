using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.SystemModule;
using ExpressApp.Module.Notification.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using ExpressApp.Module.Notification.Base;

namespace ExpressApp.Module.Notification.Controllers
{
    public partial class AlertWindowController : WindowController
    {
        private readonly INotificationDelivery notificationDeliveryManager;

        public AlertWindowController()
        {
            InitializeComponent();

            TargetWindowType = WindowType.Main;
        }

        [ActivatorUtilitiesConstructor]
        public AlertWindowController(INotificationDelivery notificationDeliveryManager) : this()
        {
            this.notificationDeliveryManager = notificationDeliveryManager;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target Window.

            notificationDeliveryManager.Added += NotificationDeliveryManager_NewNotification;

            Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));

                var c1 = CriteriaOperator.FromLambda<GNRL_Notification>(x => IsCurrentUserIdOperator.IsCurrentUserId(x.ToUser.Oid) && !x.IsDelivered);

                var objectSpace = Application?.CreateObjectSpace(typeof(GNRL_Notification));
                var notifications = objectSpace?.GetObjects<GNRL_Notification>(c1).OrderByDescending(x => x.AlarmTime);

                if (notifications is not null)
                {
                    foreach (var notification in notifications)
                    {
                        try
                        {
                            notification.SetMemberValue(nameof(GNRL_Notification.IsDelivered), true);
                            objectSpace.CommitChanges();
                        }
                        catch (Exception)
                        {
                            objectSpace.Rollback();
                        }

                        ShowAltert(notification);
                    }
                }
            });
        }
        protected override void OnDeactivated()
        {
            notificationDeliveryManager.Added -= NotificationDeliveryManager_NewNotification;

            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void NotificationDeliveryManager_NewNotification(object sender, NotificationAddedEventArgs e)
        {
            if (e.ToUserId.Equals(Application.Security.UserId))
            {
                var objectSpace = Application.CreateObjectSpace(typeof(GNRL_Notification));
                var notification = objectSpace.GetObjectByKey<GNRL_Notification>(e.Oid);
                try
                {
                    notification.SetMemberValue(nameof(GNRL_Notification.IsDelivered), true);
                    objectSpace.CommitChanges();
                }
                catch (Exception)
                {
                    objectSpace.Rollback();
                }

                ShowAltert(notification);
            }
        }

        private void ShowAltert(GNRL_Notification notification)
        {
            if (Application is BlazorApplication blazorApplication)
            {
                var alertsHandlerService = blazorApplication.ServiceProvider.GetService<IAlertsHandlerService>();
                alertsHandlerService.ShowAlert(notification.Message, DevExpress.ExpressApp.Blazor.Components.AlertLevel.Information, false);
            }
        }
    }
}
