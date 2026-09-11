VsVim
===
This is a personal fork of [VsVim](https://github.com/VsVim/VsVim), the free Vim
emulator for Visual Studio, originally created by Jared Parsons and maintained by
its contributors. All of the underlying Vim emulation here — modes, motions,
operators, registers, macros, and the rest — is VsVim's work, not this fork's. All
code remains under the Apache 2.0 license (`License.txt`), unmodified from upstream.

**Why this fork exists:** VsVim itself doesn't need to change to stay a faithful,
reliable Vim emulator. This fork is a place to experiment with extra
editor-experience features layered on top of it — things that are more about
"nice to have while editing in Visual Studio" than "core Vim emulation" — without
asking the upstream project to take on that scope or maintenance burden. Features
added here may or may not be proposed back upstream.

### Fork features

- **Mode-colored status bar** — the bottom command bar now changes color based on
  the current Vim mode (Normal, Insert, Visual, Replace, Command...), similar to
  Neovim statusline plugins. It's on by default; colors can be customized (or the
  feature turned off entirely) under `Tools > Options > VsVim > Mode Colors`.

[![Build Status](https://github.com/VsVim/VsVim/actions/workflows/main.yml/badge.svg?branch=master)](https://github.com/VsVim/VsVim/actions/workflows/main.yml?branch=master)

## Developing
VsVim can be developed using Visual Studio 2022. The details of the 
development process can be found in
[Developing.md](https://github.com/VsVim/VsVim/blob/master/Documentation/Developing.md)

When developing please follow the
[coding guidelines](https://github.com/VsVim/VsVim/blob/master/Documentation/CodingGuidelines.md)

## License

All code in this project is covered under the Apache 2 license a copy of which 
is available in the same directory under the name License.txt.

## Latest Builds

The build representing the latest source code can be downloaded from the
[Open Vsix Gallery](http://vsixgallery.com/extension/VsVim.Microsoft.e214908b-0458-4ae2-a583-4310f29687c3/).  

For Chinese Version: [中文版本](README.ch.md)
