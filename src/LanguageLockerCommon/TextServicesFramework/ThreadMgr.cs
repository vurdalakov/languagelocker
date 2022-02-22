namespace Vurdalakov.TextServicesFramework
{
    using System;
    using System.Diagnostics;

    public sealed class ThreadMgr : BaseInterfaceClass, NativeApi.ITfInputProcessorProfileActivationSink
    {
        public event EventHandler<EventArgs> InputProcessorProfileActivated;
        public event EventHandler<EventArgs> InputProcessorProfileDeactivated;

        public event EventHandler<KeyboardLayoutProfileActivationEventArgs> KeyboardLayoutProfileActivated;
        public event EventHandler<KeyboardLayoutProfileActivationEventArgs> KeyboardLayoutProfileDeactivated;

        public override Boolean Create()
        {
            return this.CreateInstance(NativeApi.CLSID_TF_ThreadMgr) &&
                this.AdviseSink(NativeApi.IID_ITfInputProcessorProfileActivationSink);
        }

        public Int32 OnActivated(UInt32 dwProfileType, UInt16 langid, ref Guid clsid, ref Guid catid, ref Guid guidProfile, IntPtr hkl, UInt32 dwFlags)
        {
            Debug.WriteLine($"OnActivated({dwProfileType}, 0x{hkl.ToInt64():X8}, 0x{dwFlags:X8})");

            var isActivated = NativeApi.TF_IPSINK_FLAG_ACTIVE == (dwFlags & NativeApi.TF_IPSINK_FLAG_ACTIVE);

            if (NativeApi.TF_PROFILETYPE_INPUTPROCESSOR == dwProfileType)
            {
                var eventArgs = new EventArgs();

                if (isActivated)
                {
                    this.InputProcessorProfileActivated?.BeginInvoke(this, eventArgs, null, null);
                }
                else
                {
                    this.InputProcessorProfileDeactivated?.BeginInvoke(this, eventArgs, null, null);
                }
            }
            else if (NativeApi.TF_PROFILETYPE_KEYBOARDLAYOUT == dwProfileType)
            {
                var eventArgs = new KeyboardLayoutProfileActivationEventArgs((UInt32)hkl);

                if (isActivated)
                {
                    this.KeyboardLayoutProfileActivated?.BeginInvoke(this, eventArgs, null, null);
                }
                else
                {
                    this.KeyboardLayoutProfileDeactivated?.BeginInvoke(this, eventArgs, null, null);
                }
            }

            return 0;
        }
    }

    public class KeyboardLayoutProfileActivationEventArgs : EventArgs
    {
        public UInt32 KeyboardLayout { get; }

        public KeyboardLayoutProfileActivationEventArgs(UInt32 keyboardLayout)
        {
            this.KeyboardLayout = keyboardLayout;
        }
    }
}
