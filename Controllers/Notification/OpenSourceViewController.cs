using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using ExpressApp.Library.Controllers;
using ExpressApp.Module.Notification.BusinessObjects;

namespace ExpressApp.Module.Notification.Controllers.Notification
{
    public partial class OpenSourceViewController : ObjectViewController<DetailView, GNRL_Notification>
    {
        public OpenSourceViewController()
        {
            InitializeComponent();

            var openSourceAction = new SimpleAction(this, "GNRL_Notification.OpenSource", PredefinedCategory.RecordEdit)
            {
                TargetObjectType = typeof(GNRL_Notification),
                TargetViewType = ViewType.DetailView,
            };
            openSourceAction.Execute += OpenNotificationSourceAction_Execute;
        }

        private void OpenNotificationSourceAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var obj = ObjectSpace.GetObjectByHandle(ViewCurrentObject.ObjectHandle) ?? throw new UserFriendlyException(CaptionHelper.GetLocalizedText("Exceptions/UserVisibleExceptions", "RequestedObjectIsNotFound"));
            var os = Application.CreateObjectSpace(obj.GetType());
            var view = Application.CreateDetailView(os, os.GetObject(obj));

            Frame.GetController<PopUpViewController>()?.CloseAll(View);

            Application.MainWindow.SetView(view);

            if (new[] { TemplateContext.PopupWindow, TemplateContext.LookupWindow, TemplateContext.LookupWindow }.Contains(Frame.Context))
            {
                View.Close();
            }
        }
    }
}
