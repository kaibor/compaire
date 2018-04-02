# Compaire
Small command line tool to compare the content of two directories by their checksums

## Screenshot

![Compaire main window](/screenshots/compaire_main.png)

## Release

[Download: Compaire v1.2](https://github.com/kaibor/compaire/releases/download/1.2/compaire.exe)

## Compatibility
Compaire was written in .NET Mono (C#) and supports the following operating systems:
* Microsoft Windows 7 / 8 / 10
* Mac OS X 10.7 and later
* Linux (Ubuntu, Debian, Raspbian, CentOS, Fedora, etc.)

**To run the application, you may need to install the latest version of the .NET Framework for your OS!**

## Usage

### General syntax

Open a command prompt or a terminal and type in the following term:

`compaire.exe [Path1] [Path2] [Algorithm] [Parameter]`

If you're a macOS or Linux user, you will need to add the mono prefix!

`mono compaire.exe [Path1] [Path2] [Algorithm] [Parameter]`

### Algorithms

Currently the following algorithms are integrated:

Algorithm |
--------- |
MD5 |
SHA1 |
SHA256 |
SHA384 |
SHA512 |

### Parameters

Parameters are alternative modifications of the program execution and output:

Parameter | Impact
--------- | ------
/l | Create a log file of the current session
/m | Generate a seperate file (.sums extension) which includes checksums of files in the master folder
/r | Recursive comparison (include subdirectories)
/s | Skip missing files automatically (don't show prompt for alternate path)

## FAQ

### Why did I wrote the tool ?

I am using encrypted NTFS volumes from VeraCrypt simultaneously on Windows and macOS and experienced corrupt files.

In order to prevent possible data loss due to errors with the file system, I wrote the tool to verify both records automatically by their checksums.

But there are also other purposes that may be useful to you :)

## Related links
[Download: .NET Framework 4.7.1 (Windows 7 SP1 and later)](https://www.microsoft.com/en-US/download/details.aspx?id=56116)

[Download: Mono Framework (Windows, macOS and Linux)](https://www.mono-project.com/download/stable/)
