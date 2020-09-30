using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Helper
{
    sealed class TypeLibrary
    {
        readonly Type[] _types;
        readonly Dictionary<string, IGrouping<string, Type>> _byName;
        public readonly FunctionCache<string, Type> ByNamePart;
        public readonly FunctionCache<string, Type[]> ByNamePartMulti;
        public readonly FunctionCache<Type, string> CompleteName;
        public readonly FunctionCache<Type, string> PrettyName;

        public TypeLibrary(IEnumerable<Type> types)
        {
            _types = types.ToArray();
            ByNamePart = new FunctionCache<string, Type>(GetTypeByNamePart);
            ByNamePartMulti = new FunctionCache<string, Type[]>(GetTypesByNamePart);
            _byName = _types.GroupBy(t => t.Name, t => t).ToDictionary(t => t.Key, t => t);
            PrettyName = new FunctionCache<Type, string>(type => ObtainTypeName(type, true));
            CompleteName = new FunctionCache<Type, string>(type => ObtainTypeName(type, false));
        }

        Type GetTypeByNamePart(string arg) { return _types.Single(t => t.CompleteName().EndsWith(arg)); }
        Type[] GetTypesByNamePart(string arg) { return _types.Where(t => t.CompleteName().EndsWith(arg)).ToArray(); }
        public Type[] Types => _types;

        Type[] ConflictingTypes(Type type)
        {
            IGrouping<string, Type> result;
            if(!_byName.TryGetValue(type.Name, out result))
                return new Type[0];
            return result.Where(definedType => definedType.Namespace != type.Namespace).ToArray();
        }

        string ObtainTypeName(Type type, bool shortenNamespace)
        {
            if (type == typeof(int))
                return "int";
            if (type == typeof(string))
                return "string";

            var namePart = ObtainNamePart(type, shortenNamespace);
            var namespacePart = ObtainNameSpacePart(type, shortenNamespace);

            return namespacePart + namePart;
        }

        string ObtainNameSpacePart(Type type, bool shortenNamespace)
        {
            if(type.IsNested && !type.IsGenericParameter)
                return ObtainTypeName(type.DeclaringType, shortenNamespace) + ".";

            if (type.Namespace == null)
                return "";

            if (!shortenNamespace)
                return type.Namespace + ".";

            var conflictingTypes = ConflictingTypes(type);

            var namespaceParts = type.Namespace.Split('.').Reverse().ToArray();
            var namespacePart = "";
            for (var i = 0; i < namespaceParts.Length && conflictingTypes.Length > 0; i++)
            {
                namespacePart = namespaceParts[i] + "." + namespacePart;
                conflictingTypes = conflictingTypes.Where(conflictingType => ("." + conflictingType.Namespace + ".").EndsWith("." + namespacePart)).ToArray();
            }

            return namespacePart;
        }

        string ObtainNamePart(Type type, bool shortenNamespace)
        {
            var result = type.Name;
            
            if (result.Contains("`"))
                result = result.Remove(result.IndexOf('`'));

            if (type.IsGenericType)
                return result + ObtainNameForGeneric(type.GetGenericArguments(), shortenNamespace);

            return result;
        }

        string ObtainNameForGeneric(Type[] types, bool shortenNamespace)
        {
            var result = "";
            var delim = "<";
            foreach(var t in types)
            {
                result += delim;
                delim = ",";
                result += ObtainTypeName(t, shortenNamespace);
            }
            return result + ">";
        }
    }
}