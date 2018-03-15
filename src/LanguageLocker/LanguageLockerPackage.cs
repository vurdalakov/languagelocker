namespace LanguageLocker
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#1110", "#1112", "1.0", IconResourceID = 1400)] // Info on this package for Help/About
    [Guid(LanguageLockerPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class LanguageLockerPackage : Package
    {
        public const string PackageGuidString = "6b3f1e16-48d2-4bcb-bd1e-952d76e7aae1";

        public LanguageLockerPackage()
        {
        }

        private Vurdalakov.LanguageProfiles _languageProfiles = new Vurdalakov.LanguageProfiles();

        private Boolean _lockLanguage = true;

        private UInt16 _language = 0;

        private OleMenuCommand _lockLanguageMenuCommand;

        protected override void Initialize()
        {
            base.Initialize();

            // handle settings

            if (Properties.Settings.Default.UpgradeUserSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeUserSettings = false;
                Properties.Settings.Default.Save();
            }

            this._lockLanguage = Properties.Settings.Default.LockLanguage;

            // subscribe for language change events

            if (!this._languageProfiles.Subscribe())
            {
                Trace.WriteLine("Cannot subscribe");
                return;
            }

            this._language = this._languageProfiles.GetCurrentLanguage();

            if (0 == this._language)
            {
                Trace.WriteLine("Cannot get current language");
                return;
            }

            this._languageProfiles.LanguageChanged += this.OnLanguageProfilesLanguageChanged;

            // add menu command

            var commandService = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var commandSet = new Guid("edf0c6b1-3438-43e2-879a-b53094a60063");

                var commandID = new CommandID(commandSet, 0x0100);
                this._lockLanguageMenuCommand = new OleMenuCommand(this.OnLockLanguageMenuCommandClicked, commandID);
                this._lockLanguageMenuCommand.BeforeQueryStatus += this.OnLockLanguageMenuCommandBeforeQueryStatus;
                commandService.AddCommand(this._lockLanguageMenuCommand);
            }
        }

        private void OnLanguageProfilesLanguageChanged(Object sender, EventArgs e)
        {
            if (this._lockLanguage)
            {
                var newLanguage = this._languageProfiles.GetCurrentLanguage();

                if (newLanguage != this._language)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() => this._languageProfiles.SetCurrentLanguage(this._language));
                }
            }
        }

        private void OnLockLanguageMenuCommandBeforeQueryStatus(Object sender, EventArgs e)
        {
            this._lockLanguageMenuCommand.Text = this._lockLanguage ? "Unlock Keyboard Language" : "Lock Keyboard Language";
        }

        private void OnLockLanguageMenuCommandClicked(Object sender, EventArgs e)
        {
            this._lockLanguage = !this._lockLanguage;

            Properties.Settings.Default.LockLanguage = this._lockLanguage;
            Properties.Settings.Default.Save();

            if (this._lockLanguage)
            {
                this._language = this._languageProfiles?.GetCurrentLanguage() ?? 0;
            }
        }

        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);

            // unsubscribe from language change events

            this._languageProfiles.Unubscribe();
        }
    }
}
