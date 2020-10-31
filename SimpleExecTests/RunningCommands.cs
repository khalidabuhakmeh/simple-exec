using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleExecTests
{
    using System;
    using System.Runtime.InteropServices;
    using SimpleExec;
    using SimpleExecTests.Infra;
    using Xbehave;
    using Xunit;

    public class RunningCommands
    {
        private static readonly string command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "hello-world.cmd" : "ls";

        [Scenario]
        public void RunningASucceedingCommand(Exception exception)
        {
            "When I run a succeeding command"
                .x(() => exception = Record.Exception(() => Command.Run(command)));

            "Then no exception is thrown"
                .x(() => Assert.Null(exception));
        }

        [Scenario]
        public void RunningASucceedingCommandWithArgs(Exception exception)
        {
            "When I run a succeeding command"
                .x(() => exception = Record.Exception(
                    () => Command.Run("dotnet", $"exec {Tester.Path} hello world")));

            "Then no exception is thrown"
                .x(() => Assert.Null(exception));
        }

        [Scenario]
        public void RunningAFailingCommand(Exception exception)
        {
            "When I run a failing command"
                .x(() => exception = Record.Exception(
                    () => Command.Run("dotnet", $"exec {Tester.Path} error hello world")));

            "Then a command exception is thrown"
                .x(() => Assert.IsType<NonZeroExitCodeException>(exception));

            "And the exception contains the exit code"
                .x(() => Assert.Equal(1, ((NonZeroExitCodeException)exception).ExitCode));
        }

        [Scenario]
        public void RunningASucceedingCommandAsync(Exception exception)
        {
            "When I run a succeeding command async"
                .x(async () => exception = await Record.ExceptionAsync(() => Command.RunAsync(command)));

            "Then no exception is thrown"
                .x(() => Assert.Null(exception));
        }

        [Scenario]
        public void RunningAFailingCommandAsync(Exception exception)
        {
            "When I run a failing command async"
                .x(async () => exception = await Record.ExceptionAsync(
                    () => Command.RunAsync("dotnet", $"exec {Tester.Path} error hello world")));

            "Then a command exception is thrown"
                .x(() => Assert.IsType<NonZeroExitCodeException>(exception));

            "And the exception contains the exit code"
                .x(() => Assert.Equal(1, ((NonZeroExitCodeException)exception).ExitCode));
        }

        [Scenario]
        public void RunningAndCancellingACommand(Exception exception)
        {
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            "When I run a long-running command async"
                .x(async () => exception = await Record.ExceptionAsync(() =>
                    Command.RunAsync("ping", "localhost", cancellationToken: source.Token)));

            "Then a Operation Canceled Exception is Thrown"
                .x(() => Assert.IsType<TaskCanceledException>(exception));
        }

    }
}
