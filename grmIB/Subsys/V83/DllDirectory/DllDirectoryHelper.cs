using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace grmIB.Subsys.V83.DllDirectory
{
    public class DllDirectoryHelper : IDisposable
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetDllDirectory(string lpPathName);

        private string path = "";
        private bool enabled = false;

        public void Enable()
        {
            if (!enabled && path != "")
            {
                lock (this)
                {
                    SetDllDirectory(path);
                    enabled = true;
                }
            }
        }

        public void Disable()
        {
            if (enabled)
            {
                lock (this)
                {
                    SetDllDirectory(null);
                    enabled = false;
                }
            }
        }

        public void SetDllPath(string str)
        {
            if (System.IO.Directory.Exists(str))
            {
                path = str;
            }
            else
            {
                throw new Exception("Каталог не существует");
            }
        }

        public DllDirectoryHelper() { }

        public DllDirectoryHelper(string str)
        {
            SetDllPath(str);
        }

        public void Dispose()
        {
            Disable();
            path = null;
        }
    }
}
