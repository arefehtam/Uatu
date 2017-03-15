using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;
namespace LeggoWatcher
{
    public class Monitor : IDisposable
    {
        private List<Tuple<string, string>> filesInfo;
        private ReaderWriterLockSlim rwlock;
        private System.Timers.Timer processTimer;
        private string watchedPath;
        private FileSystemWatcher watcher;


        public Monitor(string watchedPath)
        {
            filesInfo = new List<Tuple<string, string>>();
            rwlock = new ReaderWriterLockSlim();
            this.watchedPath = watchedPath;
            StartFileSystemWatcher();

        }

        private void StartFileSystemWatcher()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = this.watchedPath;
            //watcher.Filter = @"\.txt |\.spl |\.rcv";
            watcher.InternalBufferSize = 16384;
            //watcher.NotifyFilter = NotifyFilters.CreationTime;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.IncludeSubdirectories = false;
            watcher.Filter = "*.*";
            watcher.Created += Watcher_FileCreated;
            watcher.Error += Watcher_Error;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            StartFileSystemWatcher();
        }
        private void Watcher_FileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                rwlock.EnterWriteLock();
                Tuple<string, string> fileInfo = new Tuple<string, string>(e.FullPath, e.Name);
                filesInfo.Add(fileInfo);
                if (processTimer == null)
                {
                    processTimer = new System.Timers.Timer(2000);
                    processTimer.Elapsed += ProcessQueue;
                    processTimer.Start();

                }
                else
                {
                    processTimer.Stop();
                    processTimer.Start();
                }

            }
            finally
            {
                rwlock.ExitWriteLock();
            }

        }
        private void ProcessQueue(object sender, ElapsedEventArgs args)
        {

            try
            {
                rwlock.EnterReadLock();
                foreach (Tuple<string, string> fileInfo in filesInfo)
                {
                    string file_src_path = fileInfo.Item1;
                    if (Regex.IsMatch(file_src_path, @"\.txt|\.spl|\.rcv", RegexOptions.IgnoreCase))
                    {
                        DateTime date_modified = File.GetLastWriteTime(file_src_path);
                        // yyyy-MM-dd HH:mm:ss +0330
                        string date_modified_str = date_modified.ToString("yyyy-MM-dd HH:mm:ss") + " +0330";

                        // Copy the file to folder
                        string file_dest_path = Path.GetDirectoryName(file_src_path) + @"\MArchive\" + fileInfo.Item2;

                        if (!Directory.Exists(Path.GetDirectoryName(file_dest_path)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(file_dest_path));

                        }

                        int count = 1;

                        string fileNameOnly = Path.GetFileNameWithoutExtension(file_dest_path);
                        string extension = Path.GetExtension(file_dest_path);
                        string path = Path.GetDirectoryName(file_dest_path);
                        string newFullPath = file_dest_path;

                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                            newFullPath = Path.Combine(path, tempFileName + extension);
                        }
                        //Copy file from src to dest
                        File.Copy(file_src_path, newFullPath);
                        File.Delete(file_src_path);

                        using (StreamWriter sw = File.AppendText(newFullPath))
                        {
                            sw.WriteLine(date_modified_str);
                        }
                    }
                }
                filesInfo.Clear();
            }
            finally
            {
                if (processTimer != null)
                {
                    processTimer.Stop();
                    processTimer.Dispose();
                    processTimer = null;
                }
                rwlock.ExitReadLock();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (rwlock != null)
                {
                    rwlock.Dispose();
                    rwlock = null;
                }
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                    watcher = null;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
