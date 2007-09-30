using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ShellLib
{
    internal abstract class Item : IDisposable
    {
        private readonly FolderItem _parent;
        private readonly PIDL _pidl;

        protected internal Item(FolderItem parent, PIDL pidl)
        {
            _pidl = pidl;
            _parent = parent;
        }

        public FolderItem Parent { get { return _parent; } }
        internal PIDL Pidl { get { return _pidl; } }

        public abstract void Dispose();

        public virtual string Name { get { return Parent.NameOfChild(Pidl); } }
        public virtual string Names { get { return Parent.NamesOfChild(Pidl); } }
        public virtual string FullName { get { return Parent.FullNameOfChild(Pidl); } }

        public ShellApi.SFGAO Attributes { get { return Parent.GetAttributes(Pidl); } }

        public override string ToString()
        {
            return Name;
        }

        public static Item CreateItem(string path)
        {
            return FolderItem.Desktop.CreateItem(ShellApi.SHParseDisplayName(path, IntPtr.Zero, 0));
        }
    }

    internal sealed class DesktopItem : FolderItem
    {
        internal DesktopItem()
            : base(null, PIDL.Desktop)
        {
        }

        protected override PIDL CreatePIDLAbs()
        {
            return ShellApi.SHGetDesktopFolder();
        }

        public override string Name { get { return "Desktop"; } }
    }

    internal sealed class ChildFolderItem : FolderItem
    {
        internal ChildFolderItem(FolderItem parent, PIDL pidl)
            : base(parent, pidl)
        {
        }

        protected override PIDL CreatePIDLAbs()
        {
            return Parent.BindToObject(Pidl);
        }
    }


    internal abstract class FolderItem : Item
    {
        private ShellFolder _data;

        public FolderItem(FolderItem parent, PIDL pidl) : base(parent, pidl)
        {
        }

        private ShellFolder COM
        {
            get
            {
                if (_data == null)
                    _data = CreateCOM();
                return _data;
            }
        }

        internal PIDL BindToObject(PIDL pidl)
        {
            return COM.BindToObject(pidl, IntPtr.Zero, ShellGUIDs.IID_IShellFolder);
        }

        private ShellFolder CreateCOM()
        {
            return ShellFolder.Create(CreatePIDLAbs());
        }

        protected abstract PIDL CreatePIDLAbs();

        public static FolderItem Windows { get { return Desktop.CreateChildFolderItem(ShellApi.CSIDL.CSIDL_WINDOWS); } }
        public static FolderItem Network { get { return Desktop.CreateChildFolderItem(ShellApi.CSIDL.CSIDL_NETWORK); } }
        public static DesktopItem Desktop { get { return new DesktopItem(); } }

        #region IDisposable Members

        public override void Dispose()
        {
            if (_data != null)
                Marshal.ReleaseComObject(_data);
        }

        #endregion

        public Item[] Content
        {
            get
            {
                IntPtr enumObject =
                    COM.EnumObjects(IntPtr.Zero, ShellApi.SHCONTF.SHCONTF_FOLDERS | ShellApi.SHCONTF.SHCONTF_NONFOLDERS | ShellApi.SHCONTF.SHCONTF_SHAREABLE | ShellApi.SHCONTF.SHCONTF_STORAGE);
                EnumIDList enumIDList = EnumIDList.Create(enumObject);
                List<Item> result = new List<Item>();
                while (true)
                {
                    PIDL pidl = enumIDList.Next();
                    if (pidl == null)
                        return result.ToArray();
                    string name4Debug = NameOfChild(pidl);
                    result.Add(CreateItem(pidl));
                }
            }
        }

        public Item CreateItem(PIDL pidl)
        {
            if (pidl == null)
                return null;
            if (COM.IsFolder(pidl))
                return CreateChildFolderItem(pidl);
            else
                return CreateFileItem(pidl);
        }

        private FileItem CreateFileItem(PIDL pidl)
        {
            return new FileItem(this, pidl);
        }

        private ChildFolderItem CreateChildFolderItem(PIDL pidl)
        {
            return new ChildFolderItem(this, pidl);
        }

        private ChildFolderItem CreateChildFolderItem(ShellApi.CSIDL csidl)
        {
            return CreateChildFolderItem(ShellApi.SHGetFolderLocation(IntPtr.Zero, csidl, IntPtr.Zero));
        }

        internal string NameOfChild(PIDL pidl)
        {
            return GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_INFOLDER);
        }

        internal string NamesOfChild(PIDL pidl)
        {
            return "\""+
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_NORMAL).Replace("\"", "\"\"")
                + "\"\t\"" +
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_FORADDRESSBAR).Replace("\"", "\"\"")
                + "\"\t\"" +
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_FOREDITING).Replace("\"", "\"\"")
                + "\"\t\"" +
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_FORPARSING).Replace("\"", "\"\"")
                + "\"\t\"" +
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_INFOLDER).Replace("\"", "\"\"")
                + "\"\t\"" +
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_INFOLDER | ShellApi.SHGNO.SHGDN_FORADDRESSBAR).Replace("\"", "\"\"")
                + "\"\t\"" +
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_INFOLDER | ShellApi.SHGNO.SHGDN_FOREDITING).Replace("\"", "\"\"")
                + "\"\t\"" +
                GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_INFOLDER | ShellApi.SHGNO.SHGDN_FORPARSING).Replace("\"", "\"\"")
                + "\""
                ;
        }

        private string GetDisplayNameOf(PIDL pidl, ShellApi.SHGNO flags)
        {
            return ConvertToString(pidl, COM.GetDisplayNameOf(pidl, flags));
        }

        public string FullNameOfChild(PIDL pidl)
        {
            return GetDisplayNameOf(pidl, ShellApi.SHGNO.SHGDN_FORPARSING);
        }

        private static string ConvertToString(PIDL pidl, ShellApi.STRRET name)
        {
            return ShellApi.StrRetToBuf(ref name, pidl, 256);
        }

        public ShellApi.SFGAO GetAttributes(PIDL pidl)
        {
            return COM.GetAttributesOf(ShellApi.SFGAO.SFGAO_ALL, pidl);
        }
    }

    internal sealed class FileItem : Item
    {
        public FileItem(FolderItem parent, PIDL pidl) : base(parent, pidl)
        {
        }

        public override void Dispose()
        {
        }
    }
}