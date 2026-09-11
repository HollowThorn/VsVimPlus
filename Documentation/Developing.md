# Developing VsVimPlus

When developing please follow the
[coding guidelines](https://github.com/HollowThorn/VsVimPlus/blob/master/Documentation/CodingGuidelines.md)

## Requirements
VsVim can be developed using Visual Studio 2022. The required workloads
are:

- .NET Desktop Development
- F# Language
- Visual Studio Extension Development

## Multi Editor Support
VsVim supports multiple versions of Visual Studio: 2015 through 2019. While the
underlying editor components are remarkably compatible between the versions,
there are subtle behaviors differences that do show up. Further there are 
features, like async completion, which appear only in later versions of 
Visual Studio. 

The the unit tests are designed to test against multiple versions of Visual
Studio. This is done at build time with a series of `#if` directives to 
configure the unit tests to load a specific version of the VS editor
components.

By default VsVim will run unit tests against the version of Visual Studio that
is being used to edit the source code. This can be configured though by doing
the following:

- Setting `%VsVimTargetVersion%` to 14.0, 15.0 or 16.0
- Running `Build.cmd -testConfig <value>` with 14.0, 15.0 or 16.0

The version of Visual Studio being targetted for testing does **not** need to
be installed on the machine. 

## VimApp
The VimApp project in the solution is a light weight host of the VS WPF editor.
It starts up quickly and is good for rapidly testing out new features and bug
fixes.

The version of the WPF editor it loads is configured in exactly the same way 
as the unit tests.

## CI 
CI runs on GitHub Actions. The goals of the CI are:

1. To validate VsVimPlus's behavior on supported editor versions
1. To validate the consistency of the build: version numbers, VSIX content,
etc ...
1. To build and publish a Release VSIX as a GitHub Release whenever a version
tag (e.g. `v1.0.0`) is pushed

See the workflow definition and run history at
[GitHub Actions](https://github.com/HollowThorn/VsVimPlus/actions).
