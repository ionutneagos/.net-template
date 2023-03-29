﻿using System.Reflection;

#nullable disable
namespace Domain.Common
{
    public static class EntityHelpers
    {
        public static IEnumerable<Type> GetAllDescendantsOf(this Assembly assembly, Type genericTypeDefinition)
        {
            static IEnumerable<Type> GetAllAscendants(Type t)
            {
                Type current = t;
                while (current.BaseType != typeof(object))
                {
                    yield return current?.BaseType;
                    current = current?.BaseType;
                }
            }

            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition));

            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("Specified type is not a valid generic type definition.",
                    nameof(genericTypeDefinition));

            return assembly.GetTypes()
                           .Where(t => GetAllAscendants(t).Any(d => d.IsGenericType && d.GetGenericTypeDefinition()
                                .Equals(genericTypeDefinition)));
        }
    }
}
