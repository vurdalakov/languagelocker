namespace Vurdalakov.LanguageLocker64
{
    [Command(PackageIds.LockLanguageCommand)]
    internal sealed class LockLanguageCommand : BaseCommand<LockLanguageCommand>
    {
        protected override void BeforeQueryStatus(EventArgs e)
        {
            var isLocked = this.GetPackage()?.IsLanguageLocked ?? false;
            var keyboardLayoutName = this.GetPackage()?.GetCurrentLayoutName();

            this.Command.Text = $"{(isLocked ? "Unlock Keyboard Layout" : "Lock Keyboard Layout")}{(String.IsNullOrEmpty(keyboardLayoutName) ? "" : $" ({keyboardLayoutName})")}";
            this.Command.Enabled = true;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) => await this.GetPackage()?.ToggleLanguageLockAsync();

        private LanguageLocker64Package GetPackage() => this.Package as LanguageLocker64Package;
    }
}
