using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Extras.Composites.Tests {
    public class AutofacRegistrationExtensionsTests {
        [Fact]
        public void RegisterComposite_CapturesPreviouslyRegisteredValues() {
            var builder = new ContainerBuilder();
            builder.RegisterType<C1>().As<I>();
            builder.RegisterType<C2>().As<I>();
            builder.RegisterComposite<Composite>().As<I>();

            var resolved = builder.Build().Resolve<I>();

            Assert.IsType<Composite>(resolved);
            Assert.Equal(
                new[] { typeof(C1), typeof(C2) }.OrderBy(t => t.Name),
                ((Composite)resolved).Inner.Select(i => i.GetType()).OrderBy(t => t.Name)
            );
        }

        [Fact]
        public void RegisterComposite_CapturesFollowingRegistrations_WithPreserveExistingDefaults() {
            var builder = new ContainerBuilder();
            builder.RegisterComposite<Composite>().As<I>();
            builder.RegisterType<C1>().As<I>().PreserveExistingDefaults();

            var resolved = builder.Build().Resolve<I>();

            Assert.IsType<Composite>(resolved);
            Assert.IsType<C1>(((Composite)resolved).Inner[0]);
        }

        [Fact]
        public void RegisterComposite_BlocksFollowingRegistrations_IfBlockImplicitOverridesAndNoPreserveExistingDefaults() {
            var builder = new ContainerBuilder();
            builder.RegisterComposite<Composite>().As<I>().BlockImplicitCompositeOverride();
            builder.RegisterType<C1>().As<I>();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void RegisterComposite_IsOverridenByFollowingRegistrations_IfNoBlockImplicitOverrides() {
            var builder = new ContainerBuilder();
            builder.RegisterComposite<Composite>().As<I>();
            builder.RegisterType<C1>().As<I>();

            var resolved = builder.Build().Resolve<I>();
            Assert.IsType<C1>(resolved);
        }

        [Fact]
        public void RegisterComposite_IsOverridenByFollowingRegistrations_IfBlockImplicitOverridesAndForceCompositeOverride() {
            var builder = new ContainerBuilder();
            builder.RegisterComposite<Composite>().As<I>().BlockImplicitCompositeOverride();
            builder.RegisterType<C1>().As<I>().ForceCompositeOverride();

            var resolved = builder.Build().Resolve<I>();
            Assert.IsType<C1>(resolved);
        }

        private interface I { }
        private class C1 : I { }
        private class C2 : I { }

        private class Composite : I {
            public IReadOnlyList<I> Inner { get; }

            public Composite(IReadOnlyList<I> inner) {
                Inner = inner;
            }
        }
    }
}
