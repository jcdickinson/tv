using System;
using System.Reflection;
using SimpleInjector;
using TerminalVelocity.Eventing;

namespace TerminalVelocity
{
    public static class Setup
    {
        public static void SetupContainer(Container container)
        {
            container.ResolveUnregisteredType += ResolveEvent;

            container.RegisterEventLoop<InteractionEventLoop>();
            container.RegisterSingleton<Preferences.Behavior>();
            container.RegisterSingleton<Preferences.TerminalConfiguration>();
        }

        private static void ResolveEvent(object sender, UnregisteredTypeEventArgs e)
        {
            if (e.Handled ||
                e.UnregisteredServiceType.GetCustomAttributes<EventAttribute>() == null)
                return;

            Type current = e.UnregisteredServiceType;
            while (current != null)
            {
                if (current.IsGenericType)
                {
                    Type gen = current.GetGenericTypeDefinition();
                    if (gen == typeof(Event<,>))
                    {
                        current = gen;
                        break;
                    }
                }
                current = current.BaseType;
            }

            if (current != typeof(Event<,>)) return;

            var container = (Container)sender;
            Registration registration = SimpleInjector.Lifestyle.Singleton.CreateRegistration(
                e.UnregisteredServiceType, container);
            e.Register(registration);
        }

        public static void RegisterEventLoop<TEventLoop>(this Container container)
            where TEventLoop : EventLoop
        {
            Registration registration = Lifestyle.Singleton.CreateRegistration<TEventLoop>(container);
            container.Collection.Append(typeof(EventLoop), registration);
            container.AddRegistration<TEventLoop>(registration);
        }
    }
}
