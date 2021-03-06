﻿/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Threading;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Eventing;
using TerminalVelocity.Renderer.Events;

namespace TerminalVelocity.Direct2D.DirectX
{
    public sealed class DirectXRenderEventLoop : Renderer.RenderEventLoop
    {
        public override int Priority => int.MaxValue / 2;

        private readonly AutoResetEvent _eventReceived;
        private readonly Thread _renderThread;
        private readonly Surface _directX;

        private EventLimiter<RenderEventData> _render;
        private EventLimiter<ResizeEventData> _resize;

        public DirectXRenderEventLoop(Surface directX)
        {
            _directX = directX ?? throw new ArgumentNullException(nameof(directX));

            _eventReceived = new AutoResetEvent(false);
            _renderThread = new Thread(EventLoop)
            {
                Name = "Render Event Loop",
                IsBackground = true
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _directX.PrepareDispose();
                using (_eventReceived)
                {
                    _eventReceived.Set();
                    _renderThread.Join();
                }
            }
        }

        protected override bool OnEventPublishing<T>(ulong eventId, ref T e)
        {
            _render.EventPublished<T>(eventId);
            _resize.EventPublished<T>(eventId);
            return base.OnEventPublishing(eventId, ref e);
        }

        protected override void OnEventPublished<T>(ulong eventId, in T e)
        {
            _eventReceived.Set();
        }

        public override void Execute() => _renderThread.Start();

        private void EventLoop()
        {
            CreateSynchronizationContext();

            while (IsRunning)
            {
                _eventReceived.WaitOne();

                _render.FreezeLatest();
                _resize.FreezeLatest();

                if (!_directX.IsDisposing)
                    ExecuteEvents();
            }
        }

        protected override bool OnEventExecuting<T>(ulong eventId, ref T e)
        {
            if (e is InitializeEventData create)
            {
                _directX.Initialize(this, create.Hwnd, create.Size);
                _render.FreezeLatest();
                _resize.FreezeLatest();

                return base.OnEventExecuting(eventId, ref e);
            }
            else if (_directX.IsInitialized && !_directX.IsDisposing)
            {
                if (_resize.ShouldExecuteEvent(eventId, e, out ResizeEventData resize))
                {
                    _directX.Resize(resize.Size);
                    _render.FreezeLatest();
                    return base.OnEventExecuting(eventId, ref e);
                }
                else if (_render.ShouldExecuteEvent(eventId, e, out RenderEventData render))
                {
                    _directX.BeginDraw();
                    return base.OnEventExecuting(eventId, ref e);
                }
                return false;
            }
            else
            {
                return base.OnEventExecuting(eventId, ref e);
            }
        }

        protected override void OnEventExecuted<T>(ulong eventId, in T e, EventStatus eventStatus)
        {
            base.OnEventExecuted(eventId, e, eventStatus);
            if (_directX.IsInitialized && !_directX.IsDisposing &&
                _render.ShouldExecuteEvent(eventId, e, out RenderEventData render))
                _directX.EndDraw();
        }
    }
}
