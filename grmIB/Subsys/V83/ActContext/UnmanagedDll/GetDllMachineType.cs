using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace grmIB.Subsys.V83.ActContext.UnmanagedDll
{
    public static class NativeDllMachineType
    {
        public static MachineType GetDllMachineType(string dllPath)
        {
            // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // Offset to PE header is always at 0x3C.
            // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
            // followed by a 2-byte machine type field (see the document above for the enum).
            //
            long OffsetToOffsetPEHeader = 0x3c;
            int StartPEHeader = 0x00004550;

            using (var fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                fs.Seek(OffsetToOffsetPEHeader, SeekOrigin.Begin);
                int OffsetPEHeader = br.ReadInt32();

                fs.Seek(OffsetPEHeader, SeekOrigin.Begin);
                uint PEHeader = br.ReadUInt32();

                if (PEHeader != StartPEHeader) // "PE\0\0", little-endian
                {
                    throw new Exception("Can't find PE header");
                }

                return (MachineType)br.ReadUInt16();
            }
        }

        // Returns true if the dll is 64-bit, false if 32-bit
        public static bool DllIs64Bit(string dllPath)
        {
            switch (GetDllMachineType(dllPath))
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
                default:
                    return false;
            }
        }
    }
}
