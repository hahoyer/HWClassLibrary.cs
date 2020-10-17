using System.Data.Common;
// ReSharper disable CheckNamespace

namespace hw.Helper
{
    public interface IReaderInitialize
    {
        void Initialize(DbDataReader reader);
    }
}