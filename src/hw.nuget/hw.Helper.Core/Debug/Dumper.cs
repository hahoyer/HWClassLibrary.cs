using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Helper;

namespace hw.Debug
{
    public sealed class Dumper
    {
        static BindingFlags AnyBinding { get { return BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic; } }

        public readonly Configuration Configuration = new Configuration();
        readonly Dictionary<object, long> _activeObjects = new Dictionary<object, long>();
        long _nextObjectId = 0;

        internal string Dump(object x)
        {
            if(x == null)
                return "null";

            long key;
            if(_activeObjects.TryGetValue(x, out key))
            {
                if(key == -1)
                    _activeObjects[x] = _nextObjectId++;
                return "[==>{" + key + "#}";
            }

            _activeObjects.Add(x, -1);

            var result = Dump(x.GetType(), x);

            key = _activeObjects[x];
            if(key != -1)
                result += "{" + key + "#}";
            _activeObjects.Remove(x);

            return result;
        }

        internal static string DumpData(object x) { return DumpData(x.GetType(), x); }

        string Dump(Type t, object x)
        {
            var dea = DumpClassAttribute(t);
            if(dea != null)
                return dea.Dump(t, x);

            var handler = Configuration.GetHandler(t);
            if(handler != null)
                return handler.Dump(t, x);

            var result = BaseDump(t, x) + DumpData(t, x);
            if(result != "")
                result = result.Surround("(", ")");

            if(t == x.GetType() || result != "")
                result = t + result;

            return result;
        }

        static string DumpData(Type t, object x)
        {
            var dumpData = t.GetAttribute<DumpDataClassAttribute>(false);
            if(dumpData != null)
                return dumpData.Dump(t, x);
            var f = t.GetFields(AnyBinding);
            var fieldDump = "";
            if(f.Length > 0)
                fieldDump = DumpMembers(f, x);
            MemberInfo[] p = t.GetProperties(AnyBinding);
            var propertyDump = "";
            if(p.Length > 0)
                propertyDump = DumpMembers(p, x);
            if(fieldDump == "")
                return propertyDump;
            if(propertyDump == "")
                return fieldDump;
            return fieldDump + "\n" + propertyDump;
        }

        string BaseDump(Type t, object x)
        {
            var baseDump = "";
            if(t.BaseType != null && t.BaseType != typeof(object) && t.BaseType != typeof(ValueType))
                baseDump = Dump(t.BaseType, x);
            if(baseDump != "")
                baseDump = "\nBase:" + baseDump;
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

        static List<int> CheckMemberAttributes(MemberInfo[] f, object x)
        {
            var l = new List<int>();
            for(var i = 0; i < f.Length; i++)
            {
                var pi = f[i] as PropertyInfo;
                if(pi != null && pi.GetIndexParameters().Length > 0)
                    continue;
                if(!CheckDumpDataAttribute(f[i]))
                    continue;
                if(!CheckDumpExceptAttribute(f[i], x))
                    continue;
                l.Add(i);
            }
            return l;
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

        static string DumpMembers(MemberInfo[] f, object x) { return DumpSomeMembers(CheckMemberAttributes(f, x), f, x); }

        static string DumpSomeMembers(IList<int> l, MemberInfo[] f, object x)
        {
            var result = "";
            for(var i = 0; i < l.Count; i++)
            {
                if(i > 0)
                    result += "\n";
                if(l.Count > 10)
                    result += i + ":";
                result += f[l[i]].Name;
                result += "=";
                try
                {
                    result += Tracer.Dump(Value(f[l[i]], x));
                }
                catch(Exception)
                {
                    result += "<not implemented>";
                }
            }
            return result;
        }
        static bool CheckDumpExceptAttribute(MemberInfo f, object x)
        {
            foreach(var dea in Attribute.GetCustomAttributes(f, typeof(DumpAttributeBase)).Select(ax => ax as IDumpExceptAttribute).Where(ax => ax != null))
            {
                var v = Value(f, x);
                return !dea.IsException(v);
            }
            return true;
        }

        static object Value(MemberInfo info, object x)
        {
            var fi = info as FieldInfo;
            if(fi != null)
                return fi.GetValue(x);
            var pi = info as PropertyInfo;
            if(pi != null)
                return pi.GetValue(x, null);

            throw new NotImplementedException();
        }
    }
}