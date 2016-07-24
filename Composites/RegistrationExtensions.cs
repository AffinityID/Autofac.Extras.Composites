using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extras.Composites.Internal;
using JetBrains.Annotations;

namespace Autofac.Extras.Composites {
    [PublicAPI]
    public static class RegistrationExtensions {
        private static readonly string BlockImplicitCompositeOverrideMetadataKey = "Autofac.Extras.Composites:Override.NoImplicit:" + Guid.NewGuid().ToString("D");
        private static readonly string ForceCompositeOverrideMetadataKey = "Autofac.Extras.Composites:Override.Force:" + Guid.NewGuid().ToString("D");

        [PublicAPI, NotNull]
        public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterComposite<TImplementer>([NotNull] this ContainerBuilder builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var registrationBuilder = RegistrationBuilder.ForType<TImplementer>();
            builder.RegisterCallback(registry => AddComposite(registry, registrationBuilder));

            return registrationBuilder;
        }

        [PublicAPI, NotNull]
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterComposite([NotNull] this ContainerBuilder builder, [NotNull] Type implementationType) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            var registrationBuilder = RegistrationBuilder.ForType(implementationType);
            builder.RegisterCallback(registry => AddComposite(registry, registrationBuilder));

            return registrationBuilder;
        }

        public static IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> BlockImplicitCompositeOverride<TLimit>([NotNull] this IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationBuilder) {
            return registrationBuilder.WithMetadata(BlockImplicitCompositeOverrideMetadataKey, true);
        }

        public static IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForceCompositeOverride<TLimit>([NotNull] this IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationBuilder) {
            return registrationBuilder.WithMetadata(ForceCompositeOverrideMetadataKey, true);
        }

        private static void AddComposite<TLimit>([NotNull] IComponentRegistry registry, [NotNull] IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationBuilder) {
            registrationBuilder.OnPreparing(e => {
                foreach (var service in e.Component.Services.OfType<IServiceWithType>()) {
                    e.Parameters = e.Parameters.Concat(new[] {
                        new ResolvedParameter(
                            (p, _) => CollectionHelper.IsCollectionOf(p.ParameterType, service),
                            (_, c) => CollectionHelper.ResolveCollection(c, service, e.Component)
                        )
                    });
                }
            });

            var registration = registrationBuilder.CreateRegistration();
            registry.Register(registration);

            object blockImplicitOverride;
            if (!registration.Metadata.TryGetValue(BlockImplicitCompositeOverrideMetadataKey, out blockImplicitOverride) || !(bool)blockImplicitOverride)
                return;

            registry.Registered += (sender, e) => EnsureNoImplicitOverride(registration, e);
        }

        private static void EnsureNoImplicitOverride(IComponentRegistration registration, ComponentRegisteredEventArgs e) {
            object forceOverride;
            if (e.ComponentRegistration.Metadata.TryGetValue(ForceCompositeOverrideMetadataKey, out forceOverride) && (bool) forceOverride)
                return;

            foreach (var service in registration.Services) {
                IComponentRegistration @default;
                if (e.ComponentRegistry.TryGetRegistration(service, out @default) && @default != registration) {
                    throw new InvalidOperationException(
                        $"Registration {e.ComponentRegistration.Activator.LimitType} has overriden the composite registration {registration.Activator.LimitType} which was created with {nameof(BlockImplicitCompositeOverride)}. " +
                        $"Please add {nameof(Autofac.RegistrationExtensions.PreserveExistingDefaults)}() call to preserve the composite, or add {nameof(ForceCompositeOverride)}() to force the override."
                    );
                }
            }
        }
    }
}