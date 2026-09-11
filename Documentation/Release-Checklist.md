# VsVimPlus Release Checklist

The checklist for authoring a release of VsVimPlus.

1. Ensure `VersionNumber` in [Constants.fs](https://github.com/HollowThorn/VsVimPlus/blob/master/Src/VimCore/Constants.fs)
and `Identity Version` in [source.extension.vsixmanifest](https://github.com/HollowThorn/VsVimPlus/blob/master/Src/VsVim2022/source.extension.vsixmanifest)
match and reflect the desired version.
1. Run `Build.cmd -test -testExtra -config Release` and verify everything passes.
    1. This creates `VsVim.vsix` at `Binaries\Deploy\Release\2022\VsVim.vsix`.
    1. Install the VSIX locally, restart Visual Studio and ensure everything is
    working as expected.
1. Merge the changes into `master` via a pull request and wait for CI to go green.
1. From `master`, tag the release commit and push the tag:
    ```
    git tag v<version>
    git push origin v<version>
    ```
1. Pushing the tag triggers CI to build a Release VSIX and publish it as a
[GitHub Release](https://github.com/HollowThorn/VsVimPlus/releases) with the VSIX
attached — no manual upload step needed.
