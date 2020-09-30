using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using System.Reflection;

namespace hw.DebugFormatter
{
    public sealed class Dumper
    {
        static BindingFlags AnyBinding
        {
            get { return BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic; }
        }

        public readonly Configuration Configuration = new Configuration();
        readonly Dictionary<object, long> _activeObjects = new Dictionary<object, long>();
        long _nextObjectId;

        internal string Dump(object target)
        {
            if(target == null)
                return "null";

            long key;
            if(_activeObjects.TryGetValue(target, out key))
            {
                if(key == -1)
                {
                    key = _nextObjectId++;
                    _activeObjects[target] = key;
                }
                return "[see{" + key + "#}]";
            }

            _activeObjects.Add(target, -1);

            var result = Dump(target.GetType(), target);

            key = _activeObjects[target];
            if(key != -1)
                result += "{" + key + "#}";
            _activeObjects.Remove(target);

            return result;
        }

        internal string DumpData(object target) { return DumpData(target.GetType(), target); }

        string Dump(Type t, object target)
        {
            var dea = DumpClassAttribute(t);
            if(dea != null)
                return dea.Dump(t, target);

            var handler = Configuration.GetDump(t);
            if(handler != null)
                return handler(t, target);

            var result = ",\n".SaveConcat(BaseDump(t, target), DumpData(t, target));
            if(result != "")
                result = result.Surround("{", "}");

            if(t == target.GetType() || result != "")
                result = t + result;

            return result;
        }

        string DumpData(Type type, object data)
        {
            var dumpData = type.GetAttribute<DumpDataClassAttribute>(false);
            if(dumpData != null)
                return dumpData.Dump(type, data);

            var memberCheck = Configuration.GetMemberCheck(type);
            var results = type
                .GetFields(AnyBinding)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(AnyBinding))
                .Where(memberInfo => IsRelevant(memberInfo, type, data))
                .Where(memberInfo => memberCheck(memberInfo, data))
                .Select(memberInfo => Format(memberInfo, data))
                .ToArray();
            return FormatMemberDump(results);
        }

        static string FormatMemberDump(string[] results)
        {
            var result = results;
            if(result.Length > 10)
                result = result
                    .Select((s, i) => i + ":" + s)
                    .ToArray();
            return result.Stringify(",\n");
        }

        string BaseDump(Type t, object target)
        {
            var baseDump = "";
            if(t.BaseType != null && t.BaseType != typeof(object) && t.BaseType != typeof(ValueType))
                baseDump = Dump(t.BaseType, target);
            if(baseDump != "")
                baseDump = "Base:" + baseDump;
            return baseDump;
        }

        static DumpClassAttribute DumpClassAttribute(Type t)
        {
            var result = t.GetRecentAttribute<DumpClassAttribute>();
            if(result != null)
                return result;
            return DumpClassAttributeInterfaces(t);
        }

        static DumpClassAttribute DumpClassAttributeInterfaces(Type t)
        {
            return t
                .SelectHierachical(i => i.GetInterfaces())
                .SelectMany(i => i.GetAttributes<DumpClassAttribute>(false))
                .SingleOrDefault();
        }

        static bool IsRelevant(MemberInfo memberInfo, Type type, object target)
        {
            if(memberInfo.DeclaringType != type)
                return false;
            var propertyInfo = memberInfo as PropertyInfo;
            if(propertyInfo != null && propertyInfo.GetIndexParameters().Length > 0)
                return false;
            return CheckDumpDataAttribute(memberInfo) && CheckDumpExceptAttribute(memberInfo, target);
        }

        static bool CheckDumpDataAttribute(MemberInfo m)
        {
            var dda = m.GetAttribute<DumpEnabledAttribute>(true);
            if(dda != null)
                return dda.IsEnabled;

            return !IsPrivateOrDump(m);
        }

        static bool IsPrivateOrDump(MemberInfo m)
        {
            if(m.Name.Contains("Dump") || m.Name.Contains("dump"))
                return true;

            var fieldInfo = m as FieldInfo;
            if(fieldInfo != null)
                return fieldInfo.IsPrivate;

            if(((PropertyInfo) m).CanRead)
                return ((PropertyInfo) m).GetGetMethod(true).IsPrivate;
            return true;
        }

        static string Format(MemberInfo memberInfo, object target)
        {
            try
            {
                return memberInfo.Name + "=" + Tracer.Dump(target.InvokeValue(memberInfo));
            }
            catch(Exception)
            {
                return "<not implemented>";
            }
        }

        static bool CheckDumpExceptAttribute(MemberInfo f, object target)
        {
            foreach(
                var dea in
                    Attribute.GetCustomAttributes(f, typeof(DumpAttributeBase))
                        .Select(ax => ax as IDumpExceptAttribute)
                        .Where(ax => ax != null))
            {
                var v = target.InvokeValue(f);
                return !dea.IsException(v);
            }
            return true;
        }
    }
}