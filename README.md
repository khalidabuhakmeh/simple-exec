<img src="assets/simple-exec.png" width="100" />

# SimpleExec

_[![NuGet version](https://img.shields.io/nuget/v/SimpleExec.svg?style=flat)](https://www.nuget.org/packages/SimpleExec)_
_[![Build status](https://github.com/adamralph/simple-exec/workflows/.github/workflows/ci.yml/badge.svg)](https://github.com/adamralph/simple-exec/actions)_
_[![Spell check](https://github.com/adamralph/simple-exec/workflows/.github/workflows/spell-check.yml/badge.svg)](https://github.com/adamralph/simple-exec/actions?query=workflow%3A.github%2Fworkflows%2Fspell-check.yml)_

SimpleExec is a [.NET library](https://www.nuget.org/packages/SimpleExec) that runs external commands. It wraps [`System.Diagnostics.Process`](https://apisof.net/catalog/System.Diagnostics.Process) to make things easier.

SimpleExec intentionally does not invoke the system shell.

The command is echoed to standard error (stderr) for visibility.

Platform support: [.NET Standard 1.3 and upwards](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

## Quick start

```C#
using static SimpleExec.Command;
```

```C#
Run("foo.exe", "arg1 arg2");
```

## API

### Run

```C#
Run("foo.exe");
Run("foo.exe", "arg1 arg2", "my-directory", noEcho: true);

await RunAsync("foo.exe");
await RunAsync("foo.exe", "arg1 arg2", "my-directory", noEcho: true);
```

### Read

```C#
var output1 = Read("foo.exe");
var output2 = Read("foo.exe", "arg1 arg2", "my-directory", noEcho: true);

var output3 = await ReadAsync("foo.exe");
var output4 = await ReadAsync("foo.exe", "arg1 arg2", "my-directory", noEcho: true);
```

### Exceptions

If the command has a non-zero exit code, a `NonZeroExitCodeException` is thrown with an `int` `ExitCode` property and a message in the form of:

```C#
$"The process exited with code {ExitCode}."
```

### Windows

Sometimes, for whatever wonderful reasons, it's necessary to run a different command on Windows. For example, when running [Yarn](https://yarnpkg.com), some combination of mysterious factors may require that you explicitly run `cmd.exe` with Yarn as an argument, rather than running Yarn directly. The optional `windowsNames` and `windowsArgs` parameters may be used to specify a different command name and arguments for Windows:

```c#
Run("yarn", windowsName: "cmd", windowsArgs: "/c yarn");
```

---

<sub>[Run](https://thenounproject.com/term/target/975371) by [Gregor Cresnar](https://thenounproject.com/grega.cresnar/) from [the Noun Project](https://thenounproject.com/).</sub>
