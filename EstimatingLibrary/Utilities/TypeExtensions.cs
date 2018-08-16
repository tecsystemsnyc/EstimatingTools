using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities
{
    public static class TypeExtensions
    {
        public static bool IsImplementationOf(this Type baseType, Type interfaceType)
        {
            return baseType.GetInterfaces().Any(x =>
                      x == interfaceType ||
                      (x.IsGenericType &&
                      x.GetGenericTypeDefinition() == interfaceType));
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static Type GetItemType(this IEnumerable enumerable)
        {
            if (enumerable == null) return null;
            var args = enumerable.GetType().GetInterface("IEnumerable`1");
            if (args == null)
            {
                return null;
            }

            if (args.GenericTypeArguments.Length > 0)
            {
                return args.GenericTypeArguments[0];
            }
            else
            {
                return null;
            }
        }
    }
}
