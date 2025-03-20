namespace LookawayApp.Properties
{
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.3.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default => defaultInstance;

        [global::System.Configuration.UserScopedSetting]
        [global::System.Configuration.DefaultSettingValue("1200")]
        public int FocusTime
        {
            get => ((int)(this["FocusTime"]));
            set => this["FocusTime"] = value;
        }

        [global::System.Configuration.UserScopedSetting]
        [global::System.Configuration.DefaultSettingValue("20")]
        public int RestTime
        {
            get => ((int)(this["RestTime"]));
            set => this["RestTime"] = value;
        }
    }
}
