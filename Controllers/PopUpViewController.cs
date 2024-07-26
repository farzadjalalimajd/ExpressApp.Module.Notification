using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;

namespace ExpressApp.Module.Notification.Controllers;

public partial class PopUpViewController : ViewController
{
    public PopUpViewController()
    {
        InitializeComponent();
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        if (new[] { TemplateContext.PopupWindowContextName, TemplateContext.LookupWindowContextName, "NotificationsPopupWindow" }.Contains(Frame.Context.Name))
        {
            PopUpViews.Add(View);
        }
    }

    protected override void OnDeactivated()
    {
        if (new[] { TemplateContext.PopupWindowContextName, TemplateContext.LookupWindowContextName, "NotificationsPopupWindow" }.Contains(Frame.Context.Name))
        {
            PopUpViews.Remove(View);
        }

        base.OnDeactivated();
    }

    private static List<object> PopUpViews
    {
        get
        {
            var valueManager = ValueManager.GetValueManager<List<object>>("PopUpViews");
            valueManager.Value ??= new List<object>();
            return valueManager.Value;
        }
    }

    public void CloseAll(ObjectView exclude)
    {
        while (PopUpViews.Any())
        {
            if (PopUpViews.First() is ObjectView detailView)
            {
                if (exclude is null || !detailView.Equals(exclude))
                {
                    detailView.Close();
                }

                PopUpViews.Remove(detailView);
            }
        }

        if (exclude is not null)
        {
            PopUpViews.Add(exclude);
        }
    }
}
