using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ExpressApp.Library.Controllers;
using ExpressApp.Module.Notification.BusinessObjects;

namespace ExpressApp.Module.Notification.Controllers.Notification
{
    public partial class OpenSourceViewController : ObjectViewController<DetailView, GNRL_Notification>
    {
        public const string OpenSourceActionId = $"{nameof(GNRL_Notification)}.OpenSource";
        public SimpleAction OpenSourceAction { get; private set; }

        public OpenSourceViewController()
        {
            InitializeComponent();

            OpenSourceAction = new SimpleAction(this, OpenSourceActionId, PredefinedCategory.PopupActions)
            {
                Caption = "Source",
                ImageName = "Detailed",
                TargetObjectsCriteria = $"[{nameof(GNRL_Notification.ObjectHandle)}] Is Not Null",
                TargetObjectsCriteriaMode = TargetObjectsCriteriaMode.TrueForAll,
            };
            OpenSourceAction.Execute += OpenNotificationSourceAction_Execute;
        }

        private void OpenNotificationSourceAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var obj = ObjectSpace.GetObjectByHandle(ViewCurrentObject.ObjectHandle);
            if (obj is not null)
            {
                var os = Application.CreateObjectSpace(obj.GetType());
                var view = Application.CreateDetailView(os, os.GetObject(obj));

                Frame.GetController<PopUpViewController>()?.CloseAll(View);

                Application.MainWindow.SetView(view);

                View.Close();
            }
            else
            {
                throw new UserFriendlyException("You do not have the necessary permissions to see this item.");
            }
        }
    }
}
