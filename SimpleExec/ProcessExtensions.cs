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

        public static async Task RunAsync(this Process process, bool noEcho, string echoPrefix, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<object>();

            using (var started = new SemaphoreSlim(0, 1))
            using (var ranToCompletionOrCanceled = new SemaphoreSlim(1, 1))
            using (var cancellation = cancellationToken.Register(() =>
            {
                started.Wait();

                if (tcs.Task.Status == TaskStatus.RanToCompletion)
                {
                    return;
                }

                ranToCompletionOrCanceled.Wait();

                try
                {
                    if (tcs.Task.Status == TaskStatus.RanToCompletion)
                    {
                        return;
                    }

                    // best effort only, since exceptions may be thrown for all kinds of reasons
                    // and the _same exception_ may be thrown for all kinds of reasons
                    // System.Diagnostics.Process is "fine"
                    try
                    {
                        process.Kill();
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                    }

                    // SetCanceled(cancellationToken) would make more sense here
                    // but it only exists from .NET 5 onwards
                    // see https://github.com/dotnet/runtime/issues/30862
                    // however, we know TrySetCanceled will succeed
                    // because we have full control of the underlying task
                    tcs.TrySetCanceled(cancellationToken);
                }
                finally
                {
                    ranToCompletionOrCanceled.Release();
                }
            }))
            {
                process.Exited += (s, e) =>
                {
                    cancellation.Dispose();

                    if (tcs.Task.Status == TaskStatus.Canceled)
                    {
                        return;
                    }

                    ranToCompletionOrCanceled.Wait();

                    try
                    {
                        if (tcs.Task.Status == TaskStatus.Canceled)
                        {
                            return;
                        }

                        tcs.SetResult(default);
                    }
                    finally
                    {
                        ranToCompletionOrCanceled.Release();
                    }
                };

                process.EnableRaisingEvents = true;
                process.EchoAndStart(noEcho, echoPrefix);
                started.Release();

                await tcs.Task.ConfigureAwait(false);
            }
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
