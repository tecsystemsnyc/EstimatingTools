using System;
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
    }
}
