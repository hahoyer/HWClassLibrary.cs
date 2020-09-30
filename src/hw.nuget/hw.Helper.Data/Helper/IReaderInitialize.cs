using System.Data.Common;

namespace hw.Helper
{
    public interface IReaderInitialize
    {
        void Initialize(DbDataReader reader);
    }
}