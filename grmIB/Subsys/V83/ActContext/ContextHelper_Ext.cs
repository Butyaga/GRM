using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace grmIB.Subsys.V83.ActContext
{
    public partial class ContextHelper : IDisposable
    {
        // Activation Context API Functions
        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "CreateActCtxW")]
        internal static extern IntPtr CreateActCtx(ref ACTCTX pActCtx);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ActivateActCtx(IntPtr hActCtx, out IntPtr lpCookie);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeactivateActCtx(int dwFlags, IntPtr lpCookie);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern void ReleaseActCtx(IntPtr hActCtx);
    }
}
