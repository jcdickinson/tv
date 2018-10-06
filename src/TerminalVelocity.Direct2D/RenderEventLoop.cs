using System;
using System.Threading;
using TerminalVelocity.Collections.Concurrent;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Direct2D
{
    public sealed class RenderEventLoop : EventLoop
    {
        public override int Priority => int.MaxValue / 2;

        private readonly AutoResetEvent _eventReceived;
        private readonly Thread _renderThread;
        private readonly SingleConcurrentQueue<ResizeEventData> _resize;
        private readonly SingleConcurrentQueue<RenderEventData> _render;
        private readonly SingleConcurrentQueue<CreatedEventData> _create;

        private readonly DirectX.DirectX _directX;

        public RenderEventLoop(DirectX.DirectX directX)
        {
            _directX = directX ?? throw new ArgumentNullException(nameof(directX));

            _eventReceived = new AutoResetEvent(false);
            _renderThread = new Thread(EventLoop)
            {
                Name = "Render Event Loop",
                IsBackground = true
            };

            _resize = new SingleConcurrentQueue<ResizeEventData>();
            _render = new SingleConcurrentQueue<RenderEventData>();
            _create = new SingleConcurrentQueue<CreatedEventData>();
        }

        protected override void OnEventPublished<T>(T e)
        {
            if (e is ResizeEventData resize)
                _resize.Enqueue(resize);
            else if (e is RenderEventData render)
                _render.Enqueue(render);
            else if (e is CreatedEventData create)
                _create.Enqueue(create);
            _eventReceived.Set();
        }

        public override void Execute() => _renderThread.Start();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_eventReceived)
                {
                    _eventReceived.Set();
                    _renderThread.Join();
                }
            }
        }

        private void EventLoop()
        {
            SynchronizationContext.SetSynchronizationContext(SynchronizationContext);

            while (IsRunning)
            {
                _eventReceived.WaitOne();

                if (_create.TryDequeue(out CreatedEventData create))
                {
                    _directX.Initialize(create.Hwnd, create.Size);
                }

                if (!_directX.IsInitialized) continue;

                ExecuteEvents();

                if (_resize.TryDequeue(out ResizeEventData resize))
                {
                    _directX.Resize(resize.Size);
                }

                if (_render.TryDequeue(out RenderEventData paint))
                {
                    _directX.BeginDraw();
                    _directX.Clear(new SharpDX.Color4(1, 0, 0, 0.5f));

                    _directX.EndDraw();
                    _directX.Present();
                }
            }
        }
    }
}
