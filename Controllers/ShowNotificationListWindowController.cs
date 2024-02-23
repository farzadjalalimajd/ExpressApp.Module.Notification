using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExpressApp.Module.Notification.Controllers
{
    public partial class ShowNotificationListWindowController : WindowController
    {
        private int notificationCoutner;
#pragma warning disable IDE0052 // Remove unread private members
        private Timer timer;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly INotificationDelivery notificationDeliveryManager;

        public PopupWindowShowAction NotificationsAction { get; private set; }

        public int NotificationCount
        {
            get => notificationCoutner;
            set
            {
                notificationCoutner = value;

                if (NotificationsAction is not null)
                {
                    NotificationsAction.Caption = notificationCoutner == 0 ? string.Empty : notificationCoutner.ToString();
                }
            }
        }

        public ShowNotificationListWindowController()
        {
            InitializeComponent();

            TargetWindowType = WindowType.Main;

            NotificationsAction = new PopupWindowShowAction(this, "Notifications", PredefinedCategory.Notifications);
            NotificationsAction.CustomizePopupWindowParams += NotificationsAction_CustomizePopupWindowParams;
        }

        private void NotificationsAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            e.View = Application.CreateListView(typeof(GNRL_Notification), true);
            e.DialogController.CloseOnCurrentObjectProcessing = false;
        }

        [ActivatorUtilitiesConstructor]
        public ShowNotificationListWindowController(INotificationDelivery notificationDeliveryManager) : this()
        {
            this.notificationDeliveryManager = notificationDeliveryManager;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            notificationDeliveryManager.Added += NotificationDeliveryManager_NewNotification;
            notificationDeliveryManager.Dismissed += NotificationDeliveryManager_Dismissed;

            timer ??= new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        }

        private void Callback(object state)
        {
            var objectSpace = Application.CreateObjectSpace(typeof(GNRL_Notification));
            var criteria = CriteriaOperator.FromLambda<GNRL_Notification>(x => IsCurrentUserIdOperator.IsCurrentUserId(x.ToUser.Oid) && x.AlarmTime != null && x.AlarmTime <= DateTime.Now);
            NotificationCount = objectSpace.GetObjects<GNRL_Notification>(criteria).Count;
        }

        private void NotificationDeliveryManager_Dismissed(object sender, NotificationDismissedEventArgs e)
        {
            if (e.ToUserId.Equals(Application.Security.UserId))
            {
                NotificationCount--;
            }
        }

        protected override void OnDeactivated()
        {
            notificationDeliveryManager.Added -= NotificationDeliveryManager_NewNotification;
            notificationDeliveryManager.Dismissed -= NotificationDeliveryManager_Dismissed;

            timer.Dispose();

            base.OnDeactivated();
        }

        private void NotificationDeliveryManager_NewNotification(object sender, NotificationAddedEventArgs e)
        {
            if (e.ToUserId.Equals(Application.Security.UserId))
            {
                NotificationCount++;
            }
        }
    }
}
