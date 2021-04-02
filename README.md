# Hasher

A cross platform tool for analyzing the contents of a file or a directory tree. It's a great tool for finding duplicate files in the file system. A duplicate is defined as two or more files having the same hash. The analysis results will include each file's size, hash value, file name, containing directory name, attributes, created time, modified time, last accessed time.

## Building the project
Currently the only supported way to build the project is in Visual Studio. Visual Studio Mac version 8.9.4 is the reference build tool and environment. It will likely build just fine in Visual Studio for Windows but hasn't been tested.

### Prepare build environment
1. Download and install Visual Studio
1. Make sure Visual Studio has installed Mono and .NET Framework version 4.8.
1. Clone this github repository with submodules `git clone --recurse-submodules https://github.com/slessard/hasher.git`
1. Load the solution file, Hasher.sln, in Visual Studio
1. Click the build button or do the same action via the menu

## Running Hasher
Hasher is currently targetted to .NET Framework 4.8. As such it requires the Mono runtime to run on macOS and Linux. You can run it from the command line as such:
```
mono Hasher.exe
```
This will output Hasher's help text where you can see the command line flags and arguments that are available.

```text
Hasher [-all | -dupes] [-md5 | -sha256] path [outputfile]

    -all         Show the hash values for all files.
    -dupes       Show the hash values for duplicate files only. (Default)
    -md5         Calculate the hash value using the MD5 algorithm
    -sha256      Calculate the hash value using the SHA256 algorithm. (Default)
    path         The path to either a file or a directory to hash.
    outputfile   The path to a file that will contain the hash results.
                 If not specified the output is sent to the console window.
```

### Command line arguments
**`-all`** : Display information for all files. This flag cannot be combined with the `-dupes` flag.

**`-dupes`** : The default mode. Only display information for files that have the same hash. This flag cannot be combined with the `-all` flag.

**`-md5`** : When calculating a file's hash use the MD5 algorithm. This flag cannot be combined with the `-sha256` flag.

**`-sha256`** : The default algorithm. When calculating a file's hash use the SHA256 algorithm. This flag cannot be combined with the `-md5` flag.

**`path`** : Required. The directory or file to be analyzed. If the path is a directory that directory is recursed to analyze every file within that tree.

**`outputfile`** : Optional. The file to which the output will be written. If no value is specified the output is written to standard out.

