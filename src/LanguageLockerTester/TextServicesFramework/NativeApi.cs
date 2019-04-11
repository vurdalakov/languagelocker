namespace Vurdalakov.TextServicesFramework
{
    using System;
    using System.Runtime.InteropServices;

    public static class NativeApi
    {
        // -------------------------------------------------------------------------------------------------------------------------------------------

        // ITfInputProcessorProfileActivationSink

        // https://docs.microsoft.com/en-us/windows/desktop/api/msctf/nn-msctf-itfinputprocessorprofileactivationsink

        public const UInt32 TF_PROFILETYPE_INPUTPROCESSOR = 0x0001;
        public const UInt32 TF_PROFILETYPE_KEYBOARDLAYOUT = 0x0002;

        public const UInt32 TF_IPSINK_FLAG_ACTIVE = 0x0001;

        public const String IID_ITfInputProcessorProfileActivationSink = "71C6E74E-0F28-11D8-A82A-00065B84435C";

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID_ITfInputProcessorProfileActivationSink)]
        public interface ITfInputProcessorProfileActivationSink
        {
            [PreserveSig]
            Int32 OnActivated(UInt32 dwProfileType, UInt16 langid, ref Guid clsid, ref Guid catid, ref Guid guidProfile, IntPtr hkl, UInt32 dwFlags);
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly Guid GUID_TFCAT_TIP_KEYBOARD = new Guid("34745C63-B2F0-4784-8B67-5E12C8701A31");

        // ITfInputProcessorProfileMgr

        // https://docs.microsoft.com/en-us/windows/desktop/api/msctf/nn-msctf-itfinputprocessorprofilemgr

        public const UInt32 TF_IPPMF_FORPROCESS = 0x10000000;
        public const UInt32 TF_IPPMF_FORSESSION = 0x20000000;
        public const UInt32 TF_IPPMF_FORSYSTEMALL = 0x40000000;
        public const UInt32 TF_IPPMF_ENABLEPROFILE = 0x00000001;
        public const UInt32 TF_IPPMF_DISABLEPROFILE = 0x00000002;
        public const UInt32 TF_IPPMF_DONTCARECURRENTINPUTLANGUAGE = 0x00000004;

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct TF_INPUTPROCESSORPROFILE
        {
            public UInt32 dwProfileType;
            public UInt16 langid;
            public Guid clsId;
            public Guid guidProfile;
            public Guid catid;
            public IntPtr hklSubstitute;
            public UInt32 dwCaps;
            public IntPtr hkl;
            public UInt32 dwFlags;
        }

        public const String IID_ITfInputProcessorProfileMgr = "71C6E74C-0F28-11D8-A82A-00065B84435C";

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID_ITfInputProcessorProfileMgr)]
        public interface ITfInputProcessorProfileMgr
        {
            Int32 ActivateProfile(UInt32 dwProfileType, ushort langid, ref Guid clsid, ref Guid guidProfile, IntPtr hkl, UInt32 dwFlags);
            Int32 DeactivateProfile(UInt32 dwProfileType, ushort langid, ref Guid clsid, ref Guid guidProfile, IntPtr hkl, UInt32 dwFlags);
            Int32 GetProfile(UInt32 dwProfileType, ushort langid, ref Guid clsid, ref Guid guidProfile, IntPtr hkl, out TF_INPUTPROCESSORPROFILE     pProfile);
            Int32 EnumProfiles(short langid, out IntPtr ppEnum);
            Int32 ReleaseInputProcessor(ref Guid rclsid, UInt32 dwFlags);
            Int32 RegisterProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile, String desc, UInt32 cchDesc, String iconFile, UInt32 cchFile, UInt32 uIconIndex, IntPtr hklsubstitute, UInt32 dwPreferredLayout, Boolean bEnabledByDefault, UInt32 dwFlags);
            Int32 UnregisterProfile(Guid rclsid, ushort langid, Guid guidProfile, UInt32 dwFlags);
            Int32 GetActiveProfile(ref Guid catid, out TF_INPUTPROCESSORPROFILE pProfile);
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------

        // ITfLanguageProfileNotifySink

        // https://docs.microsoft.com/en-us/windows/desktop/api/msctf/nn-msctf-itflanguageprofilenotifysink

        public const String IID_ITfLanguageProfileNotifySink = "43C9FE15-F494-4C17-9DE2-B8A4AC350AA8";

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID_ITfLanguageProfileNotifySink)]
        public interface ITfLanguageProfileNotifySink
        {
            [PreserveSig]
            Int32 OnLanguageChange(UInt16 langid, out Boolean pfAccept);
            [PreserveSig]
            Int32 OnLanguageChanged();
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------

        // ITfSource

        // https://docs.microsoft.com/en-us/windows/desktop/api/msctf/nn-msctf-itfsource

        public const String CLSID_TF_InputProcessorProfiles = "33C53A50-F456-4884-B049-85FD643ECFED";

        [ComImport, Guid(CLSID_TF_InputProcessorProfiles)]
        public class TF_InputProcessorProfiles
        {
        }

        public const String IID_ITfInputProcessorProfiles = "1F02B6C5-7842-4EE6-8A0B-9A24183A95CA";

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID_ITfInputProcessorProfiles), CoClass(typeof(TF_InputProcessorProfiles))]
        public interface ITfInputProcessorProfiles
        {
            Int32 Register(ref Guid rclsid);
            Int32 Unregister(ref Guid rclsid);
            Int32 AddLanguageProfile(ref Guid rclsid, UInt16 langid, ref Guid guidProfile, String pchDesc, UInt32 cchDesc, String pchIconFile, UInt32 cchFile, UInt32 uIconIndex);
            Int32 RemoveLanguageProfile(ref Guid rclsid, UInt16 langid, ref Guid guidProfile);
            Int32 EnumInputProcessorInfo(out IntPtr ppEnum);
            Int32 GetDefaultLanguageProfile(UInt16 langid, ref Guid catid, out Guid pclsid, out Guid pguidProfile);
            Int32 SetDefaultLanguageProfile(UInt16 langid, ref Guid rclsid, ref Guid guidProfiles);
            Int32 ActivateLanguageProfile(ref Guid rclsid, UInt16 langid, ref Guid guidProfiles);
            Int32 GetActiveLanguageProfile(ref Guid rclsid, out UInt16 plangid, out Guid pguidProfile);
            Int32 GetLanguageProfileDescription(ref Guid rclsid, UInt16 langid, ref Guid guidProfile, out String pbstrProfile);
            Int32 GetCurrentLanguage(out UInt16 plangid);
            Int32 ChangeCurrentLanguage(UInt16 langid);
            Int32 GetLanguageList(IntPtr pplangid, out UInt32 pulCount);
            Int32 EnumLanguageProfiles(UInt16 langid, out IntPtr ppEnum);
            Int32 EnableLanguageProfile(ref Guid rclsid, UInt16 langid, ref Guid guidProfile, Boolean fEnable);
            Int32 IsEnabledLanguageProfile(ref Guid rclsid, UInt16 langid, ref Guid guidProfile, out Boolean pfEnable);
            Int32 EnableLanguageProfileByDefault(ref Guid rclsid, UInt16 langid, ref Guid guidProfile, Boolean fEnable);
            Int32 SubstituteKeyboardLayout(ref Guid rclsid, UInt16 langid, ref Guid guidProfile, IntPtr hKL);
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------

        // ITfSource

        // https://docs.microsoft.com/en-us/windows/desktop/api/msctf/nn-msctf-itfsource

        public const UInt32 TF_INVALID_COOKIE = unchecked((UInt32)(-1));

        public const String IID_ITfSource = "4EA48A35-60AE-446F-8FD6-E6A8D82459F7";

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID_ITfSource)]
        public interface ITfSource
        {
            Int32 AdviseSink(ref Guid riid, [In, MarshalAs(UnmanagedType.IUnknown)] Object punk, out UInt32 pdwCookie);
            Int32 UnadviseSink(UInt32 dwCookie);
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------

        // ITfThreadMgr

        // https://docs.microsoft.com/en-us/windows/desktop/api/msctf/nn-msctf-itfthreadmgr

        public const String CLSID_TF_ThreadMgr = "529A9E6B-6587-4F23-AB9E-9C7D683E3C50";

        [ComImport, Guid(CLSID_TF_ThreadMgr)]
        public class TF_ThreadMgr
        {
        }

        public const String IID_ITfThreadMgr = "AA80E801-2021-11D2-93E0-0060B067B86E";

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID_ITfThreadMgr), CoClass(typeof(TF_ThreadMgr))]
        public interface ITfThreadMgr
        {
            Int32 Activate(out UInt32 ptid);
            Int32 Deactivate();
            Int32 CreateDocumentMgr(out IntPtr ppdim);
            Int32 EnumDocumentMgrs(out IntPtr ppEnum);
            Int32 GetFocus(out IntPtr ppdimFocus);
            Int32 SetFocus(IntPtr pdimFocus);
            Int32 AssociateFocus(IntPtr hwnd, IntPtr pdimNew, out IntPtr ppdimPrev);
            Int32 IsThreadFocus([MarshalAs(UnmanagedType.Bool)] out Boolean pfThreadFocus);
            Int32 GetFunctionProvider(ref Guid clsid, out IntPtr ppFuncProv);
            Int32 EnumFunctionProviders(out IntPtr ppEnum);
            Int32 GetGlobalCompartment(out IntPtr ppCompMgr);
        }
    }
}
