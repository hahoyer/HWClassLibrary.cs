using System;
using System.Runtime.InteropServices;

namespace ShellLib
{
	[ComVisible(true)]
	[Guid("3766C955-DA6F-4fbc-AD36-311E342EF180")]
	public class FilterByExtension : IFolderFilter
	{
		// Allows a client to specify which individual items should be enumerated.
		// Note: The host calls this method for each item in the folder. Return S_OK, to have the item enumerated. 
		// Return S_FALSE to prevent the item from being enumerated.
		public Int32 ShouldShow(
			Object psf,				// A pointer to the folder's IShellFolder interface.
			IntPtr pidlFolder,		// The folder's PIDL.
			IntPtr pidlItem)		// The item's PIDL.
		{
		    return ShellFolder.ShouldShow(psf, pidlItem, ValidExtension) ? 0:1;
		}

		// Allows a client to specify which classes of objects in a Shell folder should be enumerated.
		public Int32 GetEnumFlags( 
			Object psf,				// A pointer to the folder's IShellFolder interface.
			IntPtr pidlFolder,		// The folder's PIDL.
			IntPtr phwnd,			// A pointer to the host's window handle.
			out UInt32 pgrfFlags)	// One or more SHCONTF values that specify which classes of objects to enumerate.
		{
			pgrfFlags = (uint)ShellApi.SHCONTF.SHCONTF_FOLDERS | (uint)ShellApi.SHCONTF.SHCONTF_NONFOLDERS;
			return 0;
		}

		public string[] ValidExtension;
	}

}