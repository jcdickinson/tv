/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimpleInjector;
using TerminalVelocity.Eventing;
using TerminalVelocity.Plugins;

namespace TerminalVelocity
{
    [ExcludeFromCodeCoverage]
    public static class Setup
    {
        private static readonly Type ObjectType;
        private static readonly Type EventType;
        private static readonly Type EventLoopType;
        private static readonly Type ContainerType;
        private static readonly MethodInfo GetInstanceMethod;

        static Setup()
        {
            ObjectType = typeof(object);
            EventType = typeof(Event<,>);
            EventLoopType = typeof(EventLoop);
            ContainerType = typeof(Container);

            GetInstanceMethod = (
                from method in typeof(Container).GetMethods()
                where method.IsGenericMethodDefinition && method.Name == "GetInstance"
                let parameters = method.GetParameters()
                let args = method.GetGenericArguments()
                where parameters.Length == 0 && args.Length == 1
                select method
            ).First();
        }

        public static void SetupContainer(Container container)
        {
            container.ResolveUnregisteredType += ResolveEvent;

            container.RegisterEventLoop<InteractionEventLoop>();
            container.RegisterEventLoop<Terminal.TerminalEventLoop>();
            container.RegisterSingleton<Preferences.Behavior>();
            container.RegisterSingleton<Preferences.TerminalConfiguration>();
            container.RegisterPlugin<Renderer.GridRenderer>();
            container.RegisterPlugin<Terminal.Terminal>();
        }

        private static void ResolveEvent(object sender, UnregisteredTypeEventArgs e)
        {
            if (e.Handled ||
                e.UnregisteredServiceType.GetCustomAttributes<EventAttribute>() == null ||
                !(sender is Container container))
                return;

            var foundEvent = false;
            for (Type current = e.UnregisteredServiceType; current != null; current = current.BaseType)
            {
                if (current.IsGenericType)
                {
                    Type gen = current.GetGenericTypeDefinition();
                    if (gen == EventType)
                    {
                        foundEvent = true;
                        break;
                    }
                }
            }

            if (!foundEvent) return;

            (ConstructorInfo constructor, Type eventLoopType) =
                (from candidate in e.UnregisteredServiceType.GetConstructors()
                let parameters = candidate.GetParameters()
                where parameters.Length == 1
                let parameter = parameters[0]
                where !parameter.IsIn && !parameter.IsOut && !parameter.IsRetval &&
                    EventLoopType.IsAssignableFrom(parameter.ParameterType)
                select (candidate, parameter.ParameterType)).FirstOrDefault();

            if (constructor == null) return;

            Expression containerExpression = Expression.Constant(container);
            Expression eventLoopExpression = Expression.Call(
                containerExpression,
                GetInstanceMethod.MakeGenericMethod(eventLoopType)
            );
            Expression eventExpression = Expression.New(constructor, eventLoopExpression);
            Expression convertExpression = Expression.Convert(eventExpression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(convertExpression);
            Func<object> instanceCreator = lambda.Compile();

            Registration registration = Lifestyle.Singleton.CreateRegistration(
                e.UnregisteredServiceType, instanceCreator, container);
            e.Register(registration);
        }

        public static void RegisterPlugin<TPlugin>(this Container container)
            where TPlugin: class, IPlugin
        {
            Registration registration = Lifestyle.Singleton.CreateRegistration<TPlugin>(container);
            container.Collection.Append(typeof(IPlugin), registration);
            container.AddRegistration<TPlugin>(registration);
        }

        public static void RegisterAlternateInterface<TService, TConcrete>(this Container container)
            where TService : class
            where TConcrete: TService
        {
            Registration registration = container.GetRegistration(typeof(TConcrete), true).Registration;
            container.AddRegistration<TService>(registration);
        }

        public static void RegisterEventLoop<TEventLoop>(this Container container)
            where TEventLoop : EventLoop
        {
            Registration registration = Lifestyle.Singleton.CreateRegistration<TEventLoop>(container);
            container.Collection.Append(typeof(EventLoop), registration);
            container.AddRegistration<TEventLoop>(registration);
        }

        public static void RegisterEventLoop<TEventLoop, TConcrete>(this Container container)
            where TEventLoop : EventLoop
            where TConcrete : TEventLoop
        {
            Registration registration = Lifestyle.Singleton.CreateRegistration<TConcrete>(container);
            container.Collection.Append(typeof(EventLoop), registration);
            container.AddRegistration<TEventLoop>(registration);
            container.AddRegistration<TConcrete>(registration);
        }
    }
}
