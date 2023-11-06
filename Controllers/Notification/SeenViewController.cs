using DevExpress.Data.Filtering;
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
                Caption = "Seen",
                ImageName = "Action_Grant_Set",
                SelectionDependencyType = SelectionDependencyType.Independent,
                TargetObjectType = typeof(GNRL_Notification),
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
            var c1 = CriteriaOperator.Parse("IsCurrentUserId([ToUser.Oid])");
            var c2 = CriteriaOperator.Parse("[IsSeen] = False");
            var c3 = new GroupOperator(c1, c2);

            var objs = ObjectSpace.GetObjects<GNRL_Notification>(c3);

            if (objs is not null)
            {
                foreach (var item in objs)
                {
                    item.SetMemberValue(nameof(GNRL_Notification.IsSeen), true);
                    item.SetMemberValue(nameof(GNRL_Notification.IsDeliverd), true);
                }
            }

            try
            {
                ObjectSpace.CommitChanges();

                if (objs is not null)
                {
                    foreach (var item in objs)
                    {
                        if (item.ToUser is not null)
                        {
                            notificationDelivery.NotifyDismiss(item.Oid, item.ToUser.Oid);
                        }
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
