/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

namespace TerminalVelocity.Eventing
{
    public delegate EventStatus EventSubscriber<T>(in T e)
        where T : struct;
}
