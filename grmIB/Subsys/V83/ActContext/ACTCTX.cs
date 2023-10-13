using System;
using System.Runtime.InteropServices;

namespace grmIB.Subsys.V83.ActContext
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct ACTCTX
    {
        public int cbSize;
        public ACTCTX_FLAG dwFlags;
        public string lpSource;
        public ushort wProcessorArchitecture;
        public Int16 wLangId;
        public string lpAssemblyDirectory;
        public string lpResourceName;
        public string lpApplicationName;
        public IntPtr hModule;
    }
}