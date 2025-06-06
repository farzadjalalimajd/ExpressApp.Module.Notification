﻿using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ExpressApp.Module.Notification.BusinessObjects;

namespace ExpressApp.Module.Notification.Controllers.NotificationConfig
{
    public partial class EvaluateMessageViewController : ObjectViewController<DetailView, GNRL_NotificationConfig>
    {
        public EvaluateMessageViewController()
        {
            InitializeComponent();

            var evaluateMessageAction = new SimpleAction(this, "GNRL_NotificationConfig.EvaluateMessage", PredefinedCategory.RecordEdit)
            {
                TargetObjectType = typeof(GNRL_NotificationConfig),
                TargetViewType = ViewType.DetailView,
            };
            evaluateMessageAction.Execute += EvaluateMessageAction_Execute;
        }

        private void EvaluateMessageAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string message;

            try
            {
                message = Convert.ToString(ObjectSpace.Evaluate(ViewCurrentObject.TargetType, CriteriaOperator.TryParse(ViewCurrentObject.Message), CriteriaOperator.TryParse(string.Empty)));

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "N/A";
                }

                Application.ShowViewStrategy.ShowMessage(message, InformationType.Info, displayInterval: 10000);
            }
            catch (Exception ex)
            {
                message = ex.Message;

                Application.ShowViewStrategy.ShowMessage(message, InformationType.Warning, displayInterval: 10000);
            }
        }
    }
}
