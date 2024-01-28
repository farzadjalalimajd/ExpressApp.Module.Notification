using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Xpo;
using ExpressApp.Library.Controllers;
using ExpressApp.Module.Notification.BusinessObjects;

namespace ExpressApp.Module.Notification;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
public sealed class NotificationModule : ModuleBase
{
    public NotificationModule()
    {
        // 
        // NotificationModule
        // 
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.CloneObject.CloneObjectModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));

        AdditionalControllerTypes.Add(typeof(PopUpViewController));
    }
    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
    {
        ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
        return new ModuleUpdater[] { updater };
    }
    public override void Setup(XafApplication application)
    {
        base.Setup(application);
        // Manage various aspects of the application UI and behavior at the module level.

        var auditService = DevExpress.Persistent.AuditTrail.AuditTrailService.GetService(Application.ServiceProvider);
        auditService.CustomizeAuditTrailSettings += AuditService_CustomizeAuditTrailSettings;
    }
    public override void CustomizeTypesInfo(ITypesInfo typesInfo)
    {
        base.CustomizeTypesInfo(typesInfo);
        CalculatedPersistentAliasHelper.CustomizeTypesInfo(typesInfo);
    }

    private void AuditService_CustomizeAuditTrailSettings(object sender, DevExpress.Persistent.AuditTrail.CustomizeAuditTrailSettingsEventArgs e)
    {
        e.AuditTrailSettings.RemoveType(typeof(GNRL_Notification));
        e.AuditTrailSettings.RemoveType(typeof(GNRL_NotificationConfig));
        e.AuditTrailSettings.RemoveType(typeof(GNRL_NotificationRecipientConfig));
        e.AuditTrailSettings.RemoveType(typeof(GNRL_NotificationSchedule));
    }
}
