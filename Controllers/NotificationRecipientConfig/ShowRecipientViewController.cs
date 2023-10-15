using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ExpressApp.Module.Notification.BusinessObjects;

namespace ExpressApp.Module.Notification.Controllers.NotificationRecipientConfig
{
    public partial class ShowRecipientViewController : ObjectViewController<DetailView, GNRL_NotificationRecipientConfig>
    {
        public ShowRecipientViewController()
        {
            InitializeComponent();

            var showRecipientAction = new PopupWindowShowAction(this, "GNRL_NotificationRecipientConfig.ShowRecipient", PredefinedCategory.RecordEdit)
            {
                Caption = "Recipients",
                ImageName = "BO_User",
                TargetObjectType = typeof(GNRL_NotificationRecipientConfig),
                TargetObjectsCriteria = "[TargetType] Is Not Null",
                TargetObjectsCriteriaMode = TargetObjectsCriteriaMode.TrueForAll,
                TargetViewType = ViewType.DetailView,
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject,
            };
            showRecipientAction.CustomizePopupWindowParams += ShowRecipientAction_CustomizePopupWindowParams;
        }

        private void ShowRecipientAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = ObjectSpace.CreateNestedObjectSpace();
            var obj = os.GetObject(ViewCurrentObject);
            os.ReloadCollection(obj.RecipientUsers);
            e.View = Application.CreateDetailView(os, "GNRL_NotificationRecipientConfig_RecipientUsers_DetailView", false, obj);
        }
    }
}
