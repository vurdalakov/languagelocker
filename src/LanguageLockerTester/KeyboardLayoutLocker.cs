// This 
// https://github.com/vurdalakov/languagelocker
// 
namespace Vurdalakov
{
    using System;
    using System.Diagnostics;
    using Vurdalakov.TextServicesFramework;

    public class KeyboardLayoutLocker : IDisposable
    {
        private UInt16 _language = 0;
        private UInt32 _keyboardLayout = 0;

        private InputProcessorProfiles _inputProcessorProfiles = new InputProcessorProfiles();
        private ThreadMgr _threadMgr = new ThreadMgr();

        public Boolean IsLocked { get; private set; } = false;

        public Boolean Lock()
        {
            if (this.IsLocked)
            {
                Debug.WriteLine("Already locked");
                return true;
            }

            if (!this._inputProcessorProfiles.Create() || !this._threadMgr.Create())
            {
                return false;
            }

            if (this._inputProcessorProfiles.GetActivateKeyboardLayout(out var keyboardLayout))
            {
                this._language = keyboardLayout.langid;
                Debug.WriteLine($"Current language is 0x{this._language:X4}");

                this._keyboardLayout = (UInt32)keyboardLayout.hkl;
                Debug.WriteLine($"Current keyboard layout 0x{this._keyboardLayout:X8}");
            }

            this._threadMgr.KeyboardLayoutProfileActivated += this.OnKeyboardLayoutProfileActivated;

            this.IsLocked = true;

            return true;
        }

        public void Unlock()
        {
            if (!this.IsLocked)
            {
                Debug.WriteLine("Already unlocked");
                return;
            }

            this._inputProcessorProfiles.Release();

            this._threadMgr.KeyboardLayoutProfileActivated -= this.OnKeyboardLayoutProfileActivated;
            this._threadMgr.Release();

            this.IsLocked = false;
        }

        private void OnKeyboardLayoutProfileActivated(Object sender, KeyboardLayoutProfileActivationEventArgs e)
        {
            if (this._keyboardLayout != e.KeyboardLayout)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Debug.WriteLine("Restoring keyboard layout");
                    this._inputProcessorProfiles.SetCurrentLanguage(this._language);
                    this._inputProcessorProfiles.ActivateKeyboardLayout(this._language, this._keyboardLayout, NativeApi.TF_IPPMF_FORPROCESS);
                    Debug.WriteLine("Keyboard layout restored");
                });
            }
        }

        #region IDisposable

        private bool _disposed = false;

        protected virtual void Dispose(Boolean disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // dispose managed objects
                }

                // free unmanaged objects
                this.Unlock();

                this._disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
