using System;
using System.Reflection;

namespace HWClassLibrary.DataBase
{
    internal class TableColumn
    {
        private readonly FieldInfo _fieldInfo;
        public string Name { get { return _fieldInfo.Name; } }
        public Type Type { get { return _fieldInfo.FieldType; } }

        public TableColumn(FieldInfo fieldInfo) { _fieldInfo = fieldInfo; }

        public string CreateTableCommand { get { return Name + " " + DBType; } }

        private string DBType
        {
            get
            {
                if (Type == typeof (string)) return "TEXT";
                throw new NotImplementedException();
            }
        }

        public string ValueAsLiteral(object o)
        {
            var value = _fieldInfo.GetValue(o);
            var result = value.ToString();
            if (DBType == "TEXT")
                result = "'" + result.Replace("'", "''") + "'";
            return result;
        }

        public void Value(object o, object value) { _fieldInfo.SetValue(o,value); }
    }
}