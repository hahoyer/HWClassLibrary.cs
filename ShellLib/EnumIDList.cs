using System;
using System.Runtime.InteropServices;

namespace ShellLib
{
    internal class EnumIDList : IDisposable
    {
        private readonly ICOM _data;

        private EnumIDList(ICOM data)
        {
            _data = data;
        }

        static internal EnumIDList Create(IntPtr enumPtr)
        {
            return new EnumIDList((ICOM)Marshal.GetTypedObjectForIUnknown(enumPtr, typeof(ICOM)));
        }

        #region IDisposable Members

        public void Dispose()
        {
            Marshal.ReleaseComObject(_data);
        }

        #endregion

        internal PIDL Next()
        {
            IntPtr idList;
            int fetched;
            if (_data.Next(1, out idList, out fetched) != 0)
                return null;
            switch (fetched)
            {
                case 0:
                    return null;

                case 1:
                    return PIDL.Create(idList);
            }
            throw new UnexpectedNextException();
        }

        #region Nested type: UnexpectedNextException

        public class UnexpectedNextException : Exception
        {
        }

        #endregion

        [ComImport(), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown),
         Guid("000214F2-0000-0000-C000-000000000046")]
        interface ICOM
        {
            [PreserveSig()]
            Int32 Next(int celt, out IntPtr rgelt, out int pceltFetched);

            [PreserveSig()]
            Int32 Skip(int celt);

            [PreserveSig()]
            Int32 Reset();

            [PreserveSig()]
            Int32 Clone(out ICOM ppenum);
        }
    }
}