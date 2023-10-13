using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace grmIB.Subsys.V83.ActContext
{
    public partial class ContextHelper : IDisposable
    {
        private IntPtr hActCtx = IntPtr.Zero;
        private IntPtr lpCookie = IntPtr.Zero;
        private readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

        public bool IsContextCreated
        {
            get
            {
                bool rezult = true;
                if (hActCtx == IntPtr.Zero)
                {
                    rezult = false;
                }
                return rezult;
            }
        }

        public bool IsContextActivated
        {
            get
            {
                bool rezult = true;
                if (lpCookie == IntPtr.Zero)
                {
                    rezult = false;
                }
                return rezult;
            }
        }

        public void CreateContext(string manifestPath)
        {
            if (IsContextCreated)
            {
                throw new Exception("Ошибка создания контекста: контекст уже создан");
            }

            ACTCTX info = new ACTCTX
            {
                cbSize = Marshal.SizeOf(typeof(ACTCTX)),
                lpSource = manifestPath
            };

            lock (this)
            {
                hActCtx = CreateActCtx(ref info);
                if (hActCtx == INVALID_HANDLE_VALUE)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Ошибка создания контекста");
                }
            }
        }

        public void ActivateContext()
        {
            if (!IsContextCreated)
            {
                throw new Exception("Ошибка активации контекта: контекст не создан");
            }

            if (IsContextActivated)
            {
                return;
            }

            lock (this)
            {
                bool rezult = ActivateActCtx(hActCtx, out lpCookie);
                if (!rezult)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                }
            }
        }

        public void DeactivateContext()
        {
            if (!IsContextActivated)
            {
                return;
            }

            lock (this)
            {
                bool rezult = DeactivateActCtx(0, lpCookie);
                if (!rezult)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                }
                lpCookie = IntPtr.Zero;
            }
        }

        public void ReleaseContext()
        {
            if (!IsContextCreated)
            {
                return;
            }

            lock (this)
            {
                if (IsContextActivated)
                {
                    DeactivateContext();
                }
            }

            lock (this)
            {
                ReleaseActCtx(hActCtx);
                hActCtx = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            ReleaseContext();
        }
    }
}
