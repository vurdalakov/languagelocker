namespace Vurdalakov.TextServicesFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class BaseInterfaceClass : IDisposable
    {
        public abstract Boolean Create();

        public virtual void Release()
        {
            this.UnadviseAllSinks();

            this.ReleaseInstance();
        }

        protected Object Instance { get; private set; }


        protected Boolean CreateInstance(String clsid)
        {
            return this.CreateInstance(new Guid(clsid));
        }

        protected Boolean CreateInstance(Guid clsid)
        {
            try
            {
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                {
                    throw new ThreadStateException("Should be an STA thread.");
                }

                var type = Type.GetTypeFromCLSID(clsid, true);
                this.Instance = Activator.CreateInstance(type);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cannot create instance of '{clsid}': {ex.Message}");
                return false;
            }
        }

        protected void ReleaseInstance()
        {
            if (this.Instance != null)
            {
                Marshal.ReleaseComObject(this.Instance);
                this.Instance = null;
            }
        }

        private List<UInt32> _sinkCookies = new List<UInt32>();

        protected Boolean AdviseSink(String clsid)
        {
            return this.AdviseSink(new Guid(clsid));
        }

        protected Boolean AdviseSink(Guid riid)
        {
            if (!this.IsCreated())
            {
                return false;
            }

            var source = this.Instance as NativeApi.ITfSource;

            var result = source.AdviseSink(riid, this, out var pdwCookie);

            var success = this.CheckResult(result, "AdviseSink") && (pdwCookie != NativeApi.TF_INVALID_COOKIE);

            if (success)
            {
                this._sinkCookies.Add(pdwCookie);
            }

            return success;
        }

        protected void UnadviseAllSinks()
        {
            if (!this.IsCreated())
            {
                return;
            }

            var source = this.Instance as NativeApi.ITfSource;

            foreach (var sinkCookie in this._sinkCookies)
            {
                source.UnadviseSink(sinkCookie);
            }

            this._sinkCookies.Clear();
        }

        protected Boolean IsCreated()
        {
            if (null == this.Instance)
            {
                Debug.WriteLine("Instance is not created");
                return false;
            }

            return true;
        }

        protected Boolean CheckResult(Int32 hResult, String methodName)
        {
            if (0 == hResult)
            {
                return true;
            }

            if (!methodName.EndsWith(")"))
            {
                methodName += "()";
            }

            Debug.WriteLine($"Method {methodName} failed with error 0x{hResult:X8} ({hResult})");
            return false;
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

                this.Release();

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
