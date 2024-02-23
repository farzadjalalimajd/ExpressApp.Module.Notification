using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;

namespace ExpressApp.Module.Notification.Controllers.Notification
{
    public partial class SeenViewController : ObjectViewController<ListView, GNRL_Notification>
    {
        private readonly INotificationDelivery notificationDelivery;

        public SeenViewController()
        {
            InitializeComponent();

            var seenAction = new SimpleAction(this, "GNRL_Notification.Seen", PredefinedCategory.RecordEdit)
            {
                TargetObjectType = typeof(GNRL_Notification),
                TargetViewType = ViewType.ListView
            };
            seenAction.Execute += SeenAction_Execute;
        }

        [ActivatorUtilitiesConstructor]
        public SeenViewController(INotificationDelivery notificationDelivery) : this()
        {
            this.notificationDelivery = notificationDelivery;
        }

        private void SeenAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var viewSelectedObjects = ViewSelectedObjects.ToList();

            foreach (var item in ViewSelectedObjects)
            {
                item.SetMemberValue(nameof(GNRL_Notification.AlarmTime), null);
                item.SetMemberValue(nameof(GNRL_Notification.IsDelivered), true);
            }

            try
            {
                ObjectSpace.CommitChanges();

                foreach (var item in viewSelectedObjects)
                {
                    if (item.ToUser is not null)
                    {
                        notificationDelivery.NotifyDismiss(item.Oid, item.ToUser.Oid);
                    }
                }
            }
            catch (Exception)
            {
                ObjectSpace.Rollback();
                ObjectSpace.Refresh();

                throw;
            }
        }
    }
}
