global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;

global using System;

global using Task = System.Threading.Tasks.Task;

using Microsoft.VisualStudio.Shell.Interop;

using System.Runtime.InteropServices;
using System.Threading;

namespace Vurdalakov.LanguageLocker64
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.LanguageLocker64String)]
    public sealed class LanguageLocker64Package : ToolkitPackage
    {
        private KeyboardLayoutLocker _keyboardLayoutLocker = new KeyboardLayoutLocker();

        public Boolean IsLanguageLocked { get; private set; } = true;

        public async Task ToggleLanguageLockAsync()
        {
            try
            {
                this.IsLanguageLocked = !this.IsLanguageLocked;
                this.Trace($"Toggle lock {this.IsLanguageLocked}");

                try
                {
                    Settings.Instance.LockLanguage = this.IsLanguageLocked;
                    await Settings.Instance.SaveAsync();
                }
                catch (Exception ex)
                {
                    this.Trace(ex, "Cannot save settings");
                }

                if (this.IsLanguageLocked && !this._keyboardLayoutLocker.IsLocked)
                {
                    this._keyboardLayoutLocker.Lock();
                }
                else if (!this.IsLanguageLocked && this._keyboardLayoutLocker.IsLocked)
                {
                    this._keyboardLayoutLocker.Unlock();
                }
            }
            catch (Exception ex)
            {
                this.Trace(ex, "Cannot toggle language lock");
            }
        }

        public String GetCurrentLayoutName()
        {
            try
            {
                var keyboardLayout = this._keyboardLayoutLocker.GetCurrentKeyboardLayout();
                var keyboardLayoutName = this._keyboardLayoutLocker.GetKeyboardLayoutName(keyboardLayout);
                this.Trace($"0x{keyboardLayout:X8} '{keyboardLayoutName}'");
                return keyboardLayoutName;
            }
            catch (Exception ex)
            {
                this.Trace(ex, "Cannot get current language");
                return "";
            }
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Trace("Init started");

            // register command

            var commands = await this.RegisterCommandsAsync();

            // switch to the UI thread

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // handle settings

            try
            {
                await Settings.Instance.LoadAsync();
                this.IsLanguageLocked = Settings.Instance.LockLanguage;
            }
            catch (Exception ex)
            {
                this.Trace(ex, "Cannot load settings");
            }

            // lock keyboard layout if required

            this._keyboardLayoutLocker.Create();

            if (this.IsLanguageLocked)
            {
                this.Trace("Lock");
                this._keyboardLayoutLocker.Lock();
            }

            this.Trace("Init done");
        }

        // IDisposable

        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);

            // dispose locker

            this._keyboardLayoutLocker.Dispose();
        }

        // tracing

        private void Trace(String message) => System.Diagnostics.Debug.WriteLine($"LanguageLocker: {message}");

        private void Trace(Exception ex, String message) => this.Trace($"{message} ({ex.GetType().Name}) '{ex.Message}'");
    }
}
