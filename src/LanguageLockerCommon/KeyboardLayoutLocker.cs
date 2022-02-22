// This 
// https://github.com/vurdalakov/languagelocker
// 
namespace Vurdalakov
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Vurdalakov.TextServicesFramework;

    public class KeyboardLayoutLocker : IDisposable
    {
        private UInt16 _language = 0;
        private UInt32 _keyboardLayout = 0;

        private InputProcessorProfiles _inputProcessorProfiles = new InputProcessorProfiles();
        private ThreadMgr _threadMgr = new ThreadMgr();

        public Boolean IsCreated { get; private set; } = false;
        public Boolean IsLocked { get; private set; } = false;

        public Boolean Create()
        {
            if (this.IsCreated)
            {
                Debug.WriteLine("Already created");
                return true;
            }

            this._threadMgr.KeyboardLayoutProfileActivated += this.OnKeyboardLayoutProfileActivated;

            if (!this._inputProcessorProfiles.Create() || !this._threadMgr.Create())
            {
                return false;
            }

            this.IsCreated = true;

            return true;
        }

        public void Destroy()
        {
            if (!this.IsCreated)
            {
                Debug.WriteLine("Not created");
                return;
            }

            this._inputProcessorProfiles.Release();

            this._threadMgr.KeyboardLayoutProfileActivated -= this.OnKeyboardLayoutProfileActivated;
            this._threadMgr.Release();

            this.IsCreated = false;
        }

        public UInt32 GetCurrentKeyboardLayout()
        {
            return this._inputProcessorProfiles.GetActivateKeyboardLayout(out var keyboardLayout) ? (UInt32)keyboardLayout.hkl: 0;
        }

        public UInt32[] GetKeyboardLayouts(UInt16 langid)
        {
            return this._inputProcessorProfiles.GetKeyboardLayouts(langid, out var keyboardLayouts) ? keyboardLayouts.Select(l => (UInt32)l.hkl).ToArray() : null;
        }

        public String GetKeyboardLayoutName(UInt32 keyboardLayout)
        {
            var language = keyboardLayout & 0xFFFF;
            var languageCulture = new CultureInfo((Int32)language);

            var keyboardLayoutName = languageCulture.ThreeLetterISOLanguageName;

            var keyboardLayouts = this.GetKeyboardLayouts(0);

            var multipleLayoutsPerLanguage = keyboardLayouts.Count(l => language == (l & 0xFFFF)) > 1;

            if (multipleLayoutsPerLanguage)
            {
                var layout = keyboardLayout >> 16;
                var layoutCulture = new CultureInfo((Int32)layout);

                var layoutName = 5 == layoutCulture.Name.Length ? layoutCulture.Name.Substring(3) : layoutCulture.TwoLetterISOLanguageName;

                keyboardLayoutName += $"-{layoutName}";
            }

            return keyboardLayoutName.ToUpper();
        }

        public Boolean Lock()
        {
            if (!this.IsCreated)
            {
                Debug.WriteLine("Not created");
                return false;
            }

            if (this.IsLocked)
            {
                Debug.WriteLine("Already locked");
                return true;
            }

            if (this._inputProcessorProfiles.GetActivateKeyboardLayout(out var keyboardLayout))
            {
                this._language = keyboardLayout.langid;
                Debug.WriteLine($"Current language is 0x{this._language:X4}");

                this._keyboardLayout = (UInt32)keyboardLayout.hkl;
                Debug.WriteLine($"Current keyboard layout 0x{this._keyboardLayout:X8}");
            }
            else
            {
                this._language = 0;
                this._keyboardLayout = 0;
            }

            this.IsLocked = true;
            Debug.WriteLine("Locked");

            return true;
        }

        public void Unlock()
        {
            if (!this.IsLocked)
            {
                Debug.WriteLine("Not locked");
                return;
            }

            this._language = 0;
            this._keyboardLayout = 0;

            this.IsLocked = false;
            Debug.WriteLine("Unlocked");
        }

        private void OnKeyboardLayoutProfileActivated(Object sender, KeyboardLayoutProfileActivationEventArgs e)
        {
            if (this.IsLocked && (this._keyboardLayout != 0) && (this._keyboardLayout != e.KeyboardLayout))
            {
                Task.Factory.StartNew(() =>
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
                this.Destroy();

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
