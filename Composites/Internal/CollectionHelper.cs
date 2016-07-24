using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using JetBrains.Annotations;

namespace Autofac.Extras.Composites.Internal {
    internal static class CollectionHelper {
        private static readonly IReadOnlyCollection<Parameter> NoParameters = new Parameter[0];
        private static readonly ConcurrentDictionary<Type, Type> ElementTypeCache = new ConcurrentDictionary<Type, Type>();

        public static bool IsCollectionOf(Type type, IServiceWithType service) {
            return ElementTypeCache.GetOrAdd(type, GetElementTypeUncached) == service.ServiceType;
        }

        public static object ResolveCollection(IComponentContext context, IServiceWithType service, IComponentRegistration exceptRegistration) {
            var otherRegistrations = context.ComponentRegistry.RegistrationsFor((Service)service).Where(r => r != exceptRegistration);
            var otherInstances = otherRegistrations.Select(r => context.ResolveComponent(r, NoParameters)).ToArray();

            // not sure if it's worth compiling/caching the array builder
            // however CollectionRegistrationSource doesn't do this, so I'll skip for now
            var typed = Array.CreateInstance(service.ServiceType, otherInstances.Length);
            otherInstances.CopyTo(typed, 0);

            return typed;
        }

        [CanBeNull]
        private static Type GetElementTypeUncached(Type type) {
            if (type.IsArray)
                return type.GetElementType();

            if (!type.IsGenericType)
                return null;

            var definition = type.GetGenericTypeDefinition();
            if (definition != typeof(IEnumerable<>) && definition != typeof(ICollection<>) && definition != typeof(IList<>) && definition != typeof(IReadOnlyCollection<>) && definition != typeof(IReadOnlyList<>))
                return null;

            return type.GetGenericArguments()[0];
        }
    }
}
