namespace Vurdalakov.TextServicesFramework
{
    using System;
    using System.Runtime.InteropServices;

    public class InputProcessorProfiles : BaseInterfaceClass, NativeApi.ITfLanguageProfileNotifySink
    {
        public NativeApi.ITfInputProcessorProfiles InputProcessorProfilesInterface => this.Instance as NativeApi.ITfInputProcessorProfiles;
        public NativeApi.ITfInputProcessorProfileMgr InputProcessorProfileMgrInterface => this.Instance as NativeApi.ITfInputProcessorProfileMgr;

        public event EventHandler<EventArgs> LanguageChanged;

        public override Boolean Create()
        {
            return this.CreateInstance(NativeApi.CLSID_TF_InputProcessorProfiles) &&
                this.AdviseSink(NativeApi.IID_ITfLanguageProfileNotifySink);
        }

        // ITfInputProcessorProfiles

        public UInt16 GetCurrentLanguage()
        {
            if (!this.IsCreated())
            {
                return 0;
            }

            var result = this.InputProcessorProfilesInterface.GetCurrentLanguage(out UInt16 currentLanguage);
            return this.CheckResult(result, "ITfInputProcessorProfiles.GetCurrentLanguage") ? currentLanguage : (UInt16)0U;
        }

        public Boolean SetCurrentLanguage(UInt16 newLanguage)
        {
            if (!this.IsCreated())
            {
                return false;
            }

            var result = this.InputProcessorProfilesInterface.ChangeCurrentLanguage(newLanguage);
            return this.CheckResult(result, "ITfInputProcessorProfiles.ChangeCurrentLanguage");
        }

        // ITfInputProcessorProfileMgr

        public Boolean ActivateKeyboardLayout(UInt16 language, UInt32 keyboardLayout, UInt32 flags)
        {
            if (!this.IsCreated())
            {
                return false;
            }

            var result = this.InputProcessorProfileMgrInterface.ActivateProfile(NativeApi.TF_PROFILETYPE_KEYBOARDLAYOUT, language, Guid.Empty, Guid.Empty, new IntPtr(keyboardLayout), flags);
            return this.CheckResult(result, "ITfInputProcessorProfileMgr.ActivateProfile");
        }

        public Boolean GetActivateKeyboardLayout(out NativeApi.TF_INPUTPROCESSORPROFILE keyboardLayout)
        {
            keyboardLayout = new NativeApi.TF_INPUTPROCESSORPROFILE();

            if (!this.IsCreated())
            {
                return false;
            }

            var result = this.InputProcessorProfileMgrInterface.GetActiveProfile(NativeApi.GUID_TFCAT_TIP_KEYBOARD, out keyboardLayout);
            return this.CheckResult(result, "ITfInputProcessorProfileMgr.GetActivateProfile");
        }

        public Boolean GetKeyboardLayouts(UInt16 langid, out NativeApi.TF_INPUTPROCESSORPROFILE[] keyboardLayouts)
        {
            keyboardLayouts = null;

            if (!this.IsCreated())
            {
                return false;
            }

            var result = this.InputProcessorProfileMgrInterface.EnumProfiles(langid, out var enumerator);
            if (!this.CheckResult(result, "ITfInputProcessorProfileMgr.EnumProfiles"))
            {
                return false;
            }

            result = enumerator.Reset();
            if (!this.CheckResult(result, "IEnumTfInputProcessorProfiles.Reset"))
            {
                return false;
            }

            var profiles = new NativeApi.TF_INPUTPROCESSORPROFILE[32];

            result = enumerator.Next((UInt32)profiles.Length, profiles, out var profilesRead);
            if (!this.CheckResult(result, "IEnumTfInputProcessorProfiles.Next"))
            {
                return false;
            }

            var keyboardLayoutsCount = 0;

            for (var i = 0; i < profilesRead; i++)
            {
                if (NativeApi.TF_PROFILETYPE_KEYBOARDLAYOUT == (profiles[i].dwProfileType & NativeApi.TF_PROFILETYPE_KEYBOARDLAYOUT))
                {
                    keyboardLayoutsCount++;
                }
            }

            keyboardLayouts = new NativeApi.TF_INPUTPROCESSORPROFILE[keyboardLayoutsCount];

            var keyboardLayoutsIndex = 0;

            for (var i = 0; i < profilesRead; i++)
            {
                if (NativeApi.TF_PROFILETYPE_KEYBOARDLAYOUT == (profiles[i].dwProfileType & NativeApi.TF_PROFILETYPE_KEYBOARDLAYOUT))
                {
                    keyboardLayouts[keyboardLayoutsIndex] = profiles[i];
                    keyboardLayoutsIndex++;
                }
            }

            return true;
        }

        // ITfLanguageProfileNotifySink

        public Int32 OnLanguageChange(UInt16 UInt16, out Boolean pfAccept)
        {
            pfAccept = true;
            return 0;
        }

        public Int32 OnLanguageChanged()
        {
            this.LanguageChanged?.BeginInvoke(this, new EventArgs(), null, null);
            return 0;
        }
    }
}
