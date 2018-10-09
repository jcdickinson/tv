/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System.Collections.Generic;
using System.Linq;
using SimpleInjector;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Direct2D
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            using (var container = new Container())
            {
                Setup.SetupContainer(container);
                TerminalVelocity.Setup.SetupContainer(container);
                
                IOrderedEnumerable<EventLoop> loops = container.GetAllInstances<EventLoop>().OrderBy(x => x.Priority);
                Enumerate(container.GetAllInstances<Plugins.IPlugin>());

                foreach (EventLoop eventLoop in loops)
                {
                    eventLoop.Execute();
                }
            }

            return 0;
        }

        private static void Enumerate<T>(IEnumerable<T> enumerable)
        {
            foreach (T item in enumerable) { }
        }
    }
}
