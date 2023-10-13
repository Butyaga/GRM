using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace grmIB.Subsys.V83
{
    class V83Isolated : IDisposable
    {
        private static string dllFileName = "comcntr.dll";
        private static string manifestFileName = "comcntr.dll.manifest";
        private static string PrepareLocation = "PrepereDll";
        private string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private string sourcePath;
        private ActContext.ContextHelper context = new ActContext.ContextHelper();
        private DllDirectory.DllDirectoryHelper dllDirectory = new DllDirectory.DllDirectoryHelper();
        private bool prepared = false;
        private bool disposed = false;

        public V83Isolated() { }

        public V83Isolated(string path, bool autoPrepare = true)
        {
            SetSourceDirectory(path);
            if (autoPrepare)
            {
                Prepare();
            }
        }

        public void SetSourceDirectory(string path)
        {
            if (disposed)
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                throw new Exception($"Не существует каталог: {path}");
            }

            string dllFullPath = Path.Combine(path, dllFileName);
            if (!File.Exists(dllFullPath))
            {
                throw new Exception($"Не существует файла: {dllFullPath}");
            }

            sourcePath = path;
        }

        public void Prepare()
        {
            if (prepared || disposed)
            {
                return;
            }

            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new Exception($"Не задан исходный каталог библиотеки: {dllFileName}");
            }

            string prepareDirectoryPath = Path.Combine(appDirectory, PrepareLocation);

            string sourceDllFullPath = Path.Combine(sourcePath, dllFileName);
            string dllVersion = GetDllVersion(sourceDllFullPath);
            prepareDirectoryPath = Path.Combine(prepareDirectoryPath, dllVersion);

            string dllType = "x32";
            if (DllIs_x64(sourceDllFullPath))
            {
                dllType = "x64";
            }
            prepareDirectoryPath = Path.Combine(prepareDirectoryPath, dllType);
            CreateDirectory(prepareDirectoryPath);

            CopyFile(Path.Combine(appDirectory, manifestFileName), prepareDirectoryPath);
            CopyFile(sourceDllFullPath, prepareDirectoryPath);

            dllDirectory.SetDllPath(sourcePath);
            context.CreateContext(Path.Combine(prepareDirectoryPath, manifestFileName));
        }

        public void Activate()
        {
            context?.ActivateContext();
            dllDirectory?.Enable();
        }

        public void Deactivate()
        {
            dllDirectory?.Disable();
            context?.DeactivateContext();
        }

        private string GetDllVersion(string fileFullPath)
        {
            string version = "0.0.0.0";
            if (File.Exists(fileFullPath))
            {
                var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(fileFullPath);
                version = info.FileVersion;
            }
            return version;
        }

        private bool DllIs_x64(string fileFullPuth)
        {
            return ActContext.UnmanagedDll.NativeDllMachineType.DllIs64Bit(fileFullPuth);
        }

        private void CopyFile(string filePath, string toDir)
        {
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                throw new Exception($"Копируемый файл не существует: {file.FullName}");
            }

            string newPath = Path.Combine(toDir, file.Name);
            if (File.Exists(newPath))
            {
                return;
            }

            try
            {
                file.CopyTo(Path.Combine(toDir, file.Name));
            }
            catch (Exception)
            {
                throw new Exception($"Ошибка копирования файла: {file.FullName} в каталог: {toDir}");
            }
        }

        private void CreateDirectory(string pathDirectory)
        {
            DirectoryInfo dir = new DirectoryInfo(pathDirectory);
            if (!dir.Exists)
            {
                try
                {
                    dir.Create();
                }
                catch (Exception)
                {
                    throw new Exception($"Ошибка создания каталога: {dir.FullName}");
                }
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            dllDirectory.Dispose();
            dllDirectory = null;

            context.Dispose();
            context = null;

            appDirectory = null;
            sourcePath = null;

            disposed = true;
        }
    }
}
