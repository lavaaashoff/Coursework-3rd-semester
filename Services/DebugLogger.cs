using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace CouseWork3Semester.Services
{
    public static class DebugLogger
    {
        private static readonly object _lock = new object();
        private static readonly string _dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
        private static readonly string _path = Path.Combine(_dir, "debug.log");

        public static void Write(string message)
        {
            try
            {
                lock (_lock)
                {
                    if (!Directory.Exists(_dir))
                        Directory.CreateDirectory(_dir);

                    using var sw = new StreamWriter(_path, append: true, Encoding.UTF8);
                    var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var pid = Process.GetCurrentProcess().Id;
                    var tid = Thread.CurrentThread.ManagedThreadId;
                    sw.WriteLine($"[{ts}] [PID:{pid}] [TID:{tid}] {message}");
                }
            }
            catch
            {
                // Игнорируем ошибки логирования, чтобы не ломать приложение
            }
        }

        public static void Write(string context, Exception ex)
        {
            Write($"{context} EXCEPTION: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}