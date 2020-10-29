using System.Threading;

namespace SimpleExec
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal static class ProcessExtensions
    {
        public static void Run(this Process process, bool noEcho, string echoPrefix)
        {
            process.EchoAndStart(noEcho, echoPrefix);
            process.WaitForExit();
        }

        public static Task RunAsync(this Process process, bool noEcho, string echoPrefix, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<object>();
            var cancelled = false;

            process.Exited += (s, e) =>
            {
                if (cancelled)
                {
                    tcs.TrySetCanceled(cancellationToken);
                }
                else
                {
                    tcs.SetResult(default);
                }
            };

            process.EnableRaisingEvents = true;
            process.EchoAndStart(noEcho, echoPrefix);

            cancellationToken.Register(() => {
                process.Kill();
                cancelled = true;
            });

            return tcs.Task;
        }

        private static void EchoAndStart(this Process process, bool noEcho, string echoPrefix)
        {
            if (!noEcho)
            {
                var message = $"{(string.IsNullOrEmpty(process.StartInfo.WorkingDirectory) ? "" : $"{echoPrefix}: Working directory: {process.StartInfo.WorkingDirectory}{Environment.NewLine}")}{echoPrefix}: {process.StartInfo.FileName} {process.StartInfo.Arguments}";
                Console.Error.WriteLine(message);
            }

            process.Start();
        }

        public static void Throw(this Process process) =>
            throw new NonZeroExitCodeException(process.ExitCode);
    }
}
