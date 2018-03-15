namespace Vurdalakov
{
    using System;
    using System.Runtime.InteropServices;

    public class LanguageProfiles : ITfLanguageProfileNotifySink
    {
        private static readonly Guid CLSID_TF_InputProcessorProfiles = new Guid("33C53A50-F456-4884-B049-85FD643ECFED");
        private static readonly Guid IID_ITfLanguageProfileNotifySink = new Guid("43C9FE15-F494-4C17-9DE2-B8A4AC350AA8");

        private ITfInputProcessorProfiles _inputProcessorProfiles;
        private ITfSource _source;
        private UInt32 _sourceCookie;

        public delegate void LanguageChangedEventHandler(Object sender, EventArgs e);
        public event LanguageChangedEventHandler LanguageChanged;

        public UInt16 GetCurrentLanguage()
        {
            if (null == this._inputProcessorProfiles)
            {
                return 0;
            }

            return 0 == this._inputProcessorProfiles.GetCurrentLanguage(out UInt16 currentLanguage) ? currentLanguage : (UInt16)0;
        }

        public Boolean SetCurrentLanguage(UInt16 newLanguage)
        {
            if (null == this._inputProcessorProfiles)
            {
                return false;
            }

            return 0 == this._inputProcessorProfiles.ChangeCurrentLanguage(newLanguage);
        }

        public Boolean Subscribe()
        {
            var type = Type.GetTypeFromCLSID(CLSID_TF_InputProcessorProfiles);
            _inputProcessorProfiles = Activator.CreateInstance(type) as ITfInputProcessorProfiles;

            _source = _inputProcessorProfiles as ITfSource;
            var hr = _source.AdviseSink(IID_ITfLanguageProfileNotifySink, this, out _sourceCookie);

            return (0 == hr) && (_sourceCookie > 0);
        }

        public void Unubscribe()
        {
            if (_source != null)
            {
                if (_sourceCookie != 0)
                {
                    _source.UnadviseSink(_sourceCookie);
                    _sourceCookie = 0;
                }

                Marshal.ReleaseComObject(_source);
                _source = null;
            }
        }

        public Int32 OnLanguageChange(UInt16 UInt16, out Boolean pfAccept)
        {
            pfAccept = true;
            return 0;
        }

        public Int32 OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, new EventArgs());
            return 0;
        }
    }

    // https://github.com/NyaRuRu/TSF-TypeLib

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("43C9FE15-F494-4C17-9DE2-B8A4AC350AA8")]
    public interface ITfLanguageProfileNotifySink
    {
        [PreserveSig]
        Int32 OnLanguageChange([In] UInt16 UInt16, [Out, MarshalAs(UnmanagedType.Bool)] out Boolean pfAccept);
        [PreserveSig]
        Int32 OnLanguageChanged();
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("4EA48A35-60AE-446F-8FD6-E6A8D82459F7")]
    internal interface ITfSource
    {
        [PreserveSig]
        Int32 AdviseSink([In] ref Guid riid, [In, MarshalAs(UnmanagedType.IUnknown)] Object punk, [Out] out UInt32 pdwCookie);
        [PreserveSig]
        Int32 UnadviseSink([In] UInt32 dwCookie);
    }

    [ComImport, Guid("33C53A50-F456-4884-B049-85FD643ECFED")]
    public class TF_InputProcessorProfiles
    {
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1F02B6C5-7842-4EE6-8A0B-9A24183A95CA"), CoClass(typeof(TF_InputProcessorProfiles))]
    public interface ITfInputProcessorProfiles
    {
        [PreserveSig]
        Int32 Register([In] ref Guid rclsid);
        [PreserveSig]
        Int32 Unregister([In] ref Guid rclsid);
        [PreserveSig]
        Int32 AddLanguageProfile([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfile, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.U2)] char[] pchDesc, [In] uint cchDesc, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6, ArraySubType = UnmanagedType.U2)] char[] pchIconFile, [In] uint cchFile, [In] uint uIconIndex);
        [PreserveSig]
        Int32 RemoveLanguageProfile([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfile);
        [PreserveSig]
        Int32 EnumInputProcessorInfo([Out, MarshalAs(UnmanagedType.Interface)] out Object ppEnum);
        [PreserveSig]
        Int32 GetDefaultLanguageProfile([In] UInt16 UInt16, [In] ref Guid catid, [Out] out Guid pclsid, [Out] out Guid pguidProfile);
        [PreserveSig]
        Int32 SetDefaultLanguageProfile([In] UInt16 UInt16, [In] ref Guid rclsid, [In] ref Guid guidProfiles);
        [PreserveSig]
        Int32 ActivateLanguageProfile([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfiles);
        [PreserveSig]
        Int32 GetActiveLanguageProfile([In] ref Guid rclsid, [Out] out UInt16 pUInt16, [Out] out Guid pguidProfile);
        [PreserveSig]
        Int32 GetLanguageProfileDescription([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfile, [Out, MarshalAs(UnmanagedType.BStr)] out string pbstrProfile);
        [PreserveSig]
        Int32 GetCurrentLanguage([Out] out UInt16 pUInt16);
        [PreserveSig]
        Int32 ChangeCurrentLanguage([In] UInt16 UInt16);
        [PreserveSig]
        Int32 GetLanguageList([Out] IntPtr ppUInt16, [Out] out uint pulCount);
        [PreserveSig]
        Int32 EnumLanguageProfiles([In] UInt16 UInt16, [Out, MarshalAs(UnmanagedType.Interface)] out Object ppEnum);
        [PreserveSig]
        Int32 EnableLanguageProfile([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfile, [In, MarshalAs(UnmanagedType.Bool)] bool fEnable);
        [PreserveSig]
        Int32 IsEnabledLanguageProfile([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfile, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfEnable);
        [PreserveSig]
        Int32 EnableLanguageProfileByDefault([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfile, [Out, MarshalAs(UnmanagedType.Bool)] out bool fEnable);
        [PreserveSig]
        Int32 SubstituteKeyboardLayout([In] ref Guid rclsid, [In] UInt16 UInt16, [In] ref Guid guidProfile, [In] IntPtr HKL);
    }
}
