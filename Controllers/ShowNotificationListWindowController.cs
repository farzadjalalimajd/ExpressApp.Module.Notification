using DevExpress.CodeParser.Diagnostics;
using DevExpress.Data.Filtering;
using DevExpress.Data.Linq.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.MiddleTier;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ExpressApp.Module.Notification.Controllers
{
    public partial class ShowNotificationListWindowController : WindowController
    {
        private int notificationCoutner;
#pragma warning disable IDE0052 // Remove unread private members
        private Timer timer;
#pragma warning restore IDE0052 // Remove unread private members
        public const string NotificationsActionId = "Notifications";
        private readonly INotificationDelivery notificationDeliveryManager;
        private readonly ILogger<ShowNotificationListWindowController> logger;

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
                    //NotificationsAction.ImageName = notificationCoutner == 0 ? "notifications" : "notifications_active";
                }
            }
        }

        public ShowNotificationListWindowController()
        {
            InitializeComponent();

            TargetWindowType = WindowType.Main;

            NotificationsAction = new PopupWindowShowAction(this, NotificationsActionId, PredefinedCategory.Notifications)
            {
                Caption = string.Empty,
                //ImageName = "BO_Notifications",
                ImageName = "notifications",
            };
            NotificationsAction.CustomizePopupWindowParams += NotificationsAction_CustomizePopupWindowParams;
        }

        private void NotificationsAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {

            Tracing.Tracer.LogWarning("FarzadJM");

            var listViewId = "GNRL_Notification_PopUp_ListView";
            var objectSpace = Application.CreateObjectSpace(typeof(GNRL_Notification));
            var collectionSource = Application.CreateCollectionSource(objectSpace, typeof(GNRL_Notification), listViewId);
            e.View = Application.CreateListView(listViewId, collectionSource, true);
            e.DialogController.CloseOnCurrentObjectProcessing = false;
        }

        [ActivatorUtilitiesConstructor]
        public ShowNotificationListWindowController(INotificationDelivery notificationDeliveryManager, ILogger<ShowNotificationListWindowController> logger) : this()
        {
            this.notificationDeliveryManager = notificationDeliveryManager;
            this.logger = logger;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            notificationDeliveryManager.Added += NotificationDeliveryManager_NewNotification;
            notificationDeliveryManager.Dismissed += NotificationDeliveryManager_Dismissed;

            timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        private void Callback(object state)
        {
            var objectSpace = Application.CreateObjectSpace(typeof(GNRL_Notification));
            NotificationCount = objectSpace.GetObjects<GNRL_Notification>(CriteriaOperator.Parse($"IsCurrentUserId([{nameof(GNRL_Notification.ToUser)}.{nameof(PermissionPolicyUser.Oid)}]) And [{nameof(GNRL_Notification.IsSeen)}] = False")).Count;
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
