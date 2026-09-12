using System;

namespace Vim.VisualStudio.RecentFiles
{
    /// <summary>
    /// These values must match up with those defined in RecentFiles.vsct
    /// </summary>
    internal static class GuidList
    {
        internal const string PackageGuidString = "8ecfad0e-b593-4d73-9ef7-27efd2767cf8";
        internal const string CommandSetGuidString = "9bc31fa3-cbec-487c-9e54-5ab7258ecdce";

        internal static readonly Guid CommandSet = new Guid(CommandSetGuidString);
    }
}
