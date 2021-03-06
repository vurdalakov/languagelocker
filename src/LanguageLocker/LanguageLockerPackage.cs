﻿namespace Vurdalakov
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#1110", "#1112", "1.1", IconResourceID = 1400)] // Info on this package for Help/About
    [Guid(LanguageLockerPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class LanguageLockerPackage : AsyncPackage
    {
        public const String PackageGuidString = "6B3F1E16-48D2-4BCB-BD1E-952D76E7AAE1";

        public LanguageLockerPackage()
        {
            Debug.WriteLine("LLocker load");
        }

        private KeyboardLayoutLocker _keyboardLayoutLocker = new KeyboardLayoutLocker();

        private Boolean _lockLanguage = true;

        private OleMenuCommand _lockLanguageMenuCommand;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Debug.WriteLine("LLocker init started");

            await base.InitializeAsync(cancellationToken, progress);
            
            // switch to the UI thread

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // handle settings

            try
            {
                if (Properties.Settings.Default.UpgradeUserSettings)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeUserSettings = false;
                    Properties.Settings.Default.Save();
                }

                this._lockLanguage = Properties.Settings.Default.LockLanguage;
            }
            catch (Exception ex)
            {
                this.Trace("Cannot load settings", ex);
            }

            // lock keyboard layout if required

            this._keyboardLayoutLocker.Create();

            if (this._lockLanguage)
            {
                this._keyboardLayoutLocker.Lock();
            }

            // add menu command

            var commandService = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var commandSet = new Guid("EDF0C6B1-3438-43E2-879A-B53094A60063");

                var commandID = new CommandID(commandSet, 0x0100);
                this._lockLanguageMenuCommand = new OleMenuCommand(this.OnLockLanguageMenuCommandClicked, commandID);
                this._lockLanguageMenuCommand.BeforeQueryStatus += this.OnLockLanguageMenuCommandBeforeQueryStatus;
                this._lockLanguageMenuCommand.Enabled = this._keyboardLayoutLocker.IsCreated;
                commandService.AddCommand(this._lockLanguageMenuCommand);
            }

            Debug.WriteLine("LLocker init done");
        }

        private void OnLockLanguageMenuCommandBeforeQueryStatus(Object sender, EventArgs e)
        {
            try
            {
                this._lockLanguageMenuCommand.Text = this._keyboardLayoutLocker.IsLocked ? "Unlock Keyboard Layout" : "Lock Keyboard Layout";

                var keyboardLayout = this._keyboardLayoutLocker.GetCurrentKeyboardLayout();
                var keyboardLayoutName = this._keyboardLayoutLocker.GetKeyboardLayoutName(keyboardLayout);
                Debug.WriteLine($"0x{keyboardLayout:X8} '{keyboardLayoutName}'");

                this._lockLanguageMenuCommand.Text += $" ({keyboardLayoutName})";
            }
            catch (Exception ex)
            {
                this.Trace("Cannot set menu item text", ex);
            }
        }

        private void OnLockLanguageMenuCommandClicked(Object sender, EventArgs e)
        {
            try
            {
                this._lockLanguage = !this._lockLanguage;

                Properties.Settings.Default.LockLanguage = this._lockLanguage;
                Properties.Settings.Default.Save();

                if (this._lockLanguage && !this._keyboardLayoutLocker.IsLocked)
                {
                    this._keyboardLayoutLocker.Lock();
                }
                else if (!this._lockLanguage && this._keyboardLayoutLocker.IsLocked)
                {
                    this._keyboardLayoutLocker.Unlock();
                }
            }
            catch (Exception ex)
            {
                this.Trace("Cannot handle menu command", ex);
            }
        }

        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);

            // dispose locker

            this._keyboardLayoutLocker.Dispose();
        }

        private void Trace(String message, Exception ex)
        {
            Debug.WriteLine($"{message}: {ex.GetType().Name} - {ex.Message}");
        }
    }
}
