using System;

namespace ShellLib
{
    public class PIDL
    {
        private readonly IntPtr _value;

        private PIDL(IntPtr data)
        {
            _value = data;
        }

        internal IntPtr Value { get { return _value; } }

        public static PIDL Create(ShellApi.CSIDL csidl)
        {
            return ShellApi.SHGetFolderLocation(IntPtr.Zero, csidl, IntPtr.Zero);
        }

        public static PIDL Create(IntPtr data)
        {
            if(data == IntPtr.Zero)
                return null;
            return new PIDL(data);
        }

        public static PIDL Desktop { get { return ShellApi.SHGetDesktopFolder(); } }

        public static IntPtr[] Convert(PIDL[] apidl)
        {
            IntPtr[] result = new IntPtr[apidl.Length];
            for (int i = 0; i < apidl.Length; i++)
                result[i] = apidl[i].Value;
            return result;
        }
    }
}