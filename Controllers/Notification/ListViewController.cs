using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using ExpressApp.Module.Notification.Base;
using ExpressApp.Module.Notification.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;

namespace ExpressApp.Module.Notification.Controllers.Notification
{
    public partial class ListViewController : ObjectViewController<ListView, GNRL_Notification>
    {
        public ListViewController()
        {
            InitializeComponent();
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            var criteria = CriteriaOperator.FromLambda<GNRL_Notification>(x => IsCurrentUserIdOperator.IsCurrentUserId(x.ToUser.Oid) && !x.IsDelivered);
            var objectSpace = Application.CreateObjectSpace(typeof(GNRL_Notification));
            var notifications = objectSpace.GetObjects<GNRL_Notification>(criteria);
            foreach (var notification in notifications)
            {
                notification.SetMemberValue(nameof(GNRL_Notification.IsDelivered), true);
            }

            try
            {
                objectSpace.CommitChanges();
            }
            catch (Exception)
            {
                objectSpace.Rollback();
                objectSpace.Refresh();
            }

            View.AllowNew.SetItemValue(string.Empty, false);
            View.AllowEdit.SetItemValue(string.Empty, false);
            View.AllowDelete.SetItemValue(string.Empty, false);

            Frame.GetController<ListViewProcessCurrentObjectController>().CustomizeShowViewParameters += NotificationViewController_CustomizeShowViewParameters;
        }

        void NotificationViewController_CustomizeShowViewParameters(object sender, CustomizeShowViewParametersEventArgs e)
        {
            var nestedObjectSpace = ObjectSpace.CreateNestedObjectSpace();

            e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
            e.ShowViewParameters.Context = TemplateContext.PopupWindow;
            e.ShowViewParameters.Controllers.Add(Application.CreateController<DialogController>());
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(nestedObjectSpace, nestedObjectSpace.GetObject(ViewCurrentObject));
        }

        protected override void OnDeactivated()
        {
            Frame.GetController<ListViewProcessCurrentObjectController>().CustomizeShowViewParameters -= NotificationViewController_CustomizeShowViewParameters;

            base.OnDeactivated();
        }
    }
}
