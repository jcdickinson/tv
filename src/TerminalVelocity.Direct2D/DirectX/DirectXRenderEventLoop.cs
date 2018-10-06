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

        private long _latestRender;
        private long _latestResize;

        private long _currentRender;
        private long _currentResize;

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

        protected override void OnEventPublished<T>(ulong eventId, in T e)
        {
            if (e is RenderEventData) Interlocked.Exchange(ref _latestRender, (long)eventId);
            if (e is ResizeEventData) Interlocked.Exchange(ref _latestResize, (long)eventId);

            _eventReceived.Set();
        }

        public override void Execute() => _renderThread.Start();

        private void EventLoop()
        {
            CreateSynchronizationContext();

            while (IsRunning)
            {
                _eventReceived.WaitOne();

                _currentRender = Interlocked.Read(ref _latestRender);
                _currentResize = Interlocked.Read(ref _latestResize);

                if (!_directX.IsDisposing)
                    ExecuteEvents();
            }
        }

        protected override void OnEventExecuting<T>(ulong eventId, ref T e)
        {
            if (e is InitializeEventData create)
            {
                _directX.Initialize(this, create.Hwnd, create.Size);
                _currentRender = Interlocked.Read(ref _latestRender);
                _currentResize = Interlocked.Read(ref _latestResize);

                base.OnEventExecuting(eventId, ref e);
            }
            else if (_directX.IsInitialized && !_directX.IsDisposing)
            {
                if (e is ResizeEventData resize && eventId == (ulong)_currentResize)
                {
                    _directX.Resize(resize.Size);
                    _currentRender = Interlocked.Read(ref _latestRender);
                }
                else if (e is RenderEventData render && eventId == (ulong)_currentRender)
                {
                    _directX.BeginDraw();
                }
                base.OnEventExecuting(eventId, ref e);
            }
            else
            {
                base.OnEventExecuting(eventId, ref e);
            }
        }

        protected override void OnEventExecuted<T>(ulong eventId, in T e, EventStatus eventStatus)
        {
            base.OnEventExecuted(eventId, e, eventStatus);
            if (_directX.IsInitialized && !_directX.IsDisposing &&
                e is RenderEventData render && eventId == (ulong)_currentRender)
                _directX.EndDraw();
        }
    }
}
