using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using TerminalVelocity.Direct2D.Events;
using WinApi.DxUtils.Component;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    [Shared, Export]
    public sealed class Application : IDisposable
    {
        private sealed class OneLengthConcurrentQueue<T>
        {
            private sealed class Box
            {
                public readonly T Value;
                public Box(T value) => Value = value;
            }

            private volatile Box _box;
            public OneLengthConcurrentQueue() {}

            public void Enqueue(T value) => _box = new Box(value);
            public bool TryDequeue(out T value)
            {
                var box = Interlocked.Exchange(ref _box, null);
                if (box is null)
                {
                    value = default;
                    return false;
                }
                else
                {
                    value = box.Value;
                    return true;
                }
            }
        }

        private readonly Dx11Component _component;
        private readonly RenderWindow _renderWindow;
        private readonly DeviceContext _context;
        private readonly SharpDX.DXGI.SwapChain _swapChain;
        private readonly Lazy<SceneRoot> _sceneRoot;

        private readonly AutoResetEvent _renderReceived = new AutoResetEvent(false);
        private readonly OneLengthConcurrentQueue<SizeEvent> _size = new OneLengthConcurrentQueue<SizeEvent>();

        private volatile int _result;

        [ImportingConstructor]
        public Application(
            [Import] Dx11Component component,
            [Import] RenderWindow renderWindow,
            [Import] DeviceContext deviceContext,
            [Import] SharpDX.DXGI.SwapChain swapChain,
            [Import] Lazy<SceneRoot> sceneRoot)
        {
            _component = component;
            _renderWindow = renderWindow;
            _context = deviceContext;
            _swapChain = swapChain;
            _sceneRoot = sceneRoot;
        }

        public int Run()
        {
            var window = _renderWindow;
            window.Show();

            var renderThread = new Thread(RenderThread)
            {
                Name = "Render Thread",
                IsBackground = true
            };
            renderThread.Start();

            while ((_result = User32Methods.GetMessage(out var message, window.Handle, 0, 0)) > 0)
            {
                User32Methods.TranslateMessage(ref message);
                User32Methods.DispatchMessage(ref message);

                if (message.Value == (uint)WM.QUIT)
                {
                    _result = 0;
                    break;
                }
            }

            _result = 0;
            _renderReceived.Set();
            renderThread.Join();

            return _result;
        }

        private void RenderThread()
        {
            while (_result > 0)
            {
                if (_renderReceived.WaitOne(100) && _result >= 0)
                {
                    if (_size.TryDequeue(out var size))
                        _component.Resize(size.Size);
                        
                    _context.BeginDraw();
                    _context.Clear(new Color4(0, 0, 0, 0));

                    _sceneRoot.Value.OnRender();

                    _context.EndDraw();
                    if (_result > 0)
                        _swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
                }
            }
        }

        [Import(RenderEvent.ContractName)]
        public Event<RenderEvent> OnRender
        {
            set => value.Subscribe((ref RenderEvent e) => _renderReceived.Set());
        }

        [Import(SizeEvent.ContractName)]
        public Event<SizeEvent> OnSize
        {
            set => value.Subscribe((ref SizeEvent e) => _size.Enqueue(e));
        }

        [Import(LayoutEvent.ContractName)]
        public Event<LayoutEvent> OnLayout
        {
            set => value.Subscribe((ref LayoutEvent e) => _sceneRoot?.Value.OnLayout(ref e));
        }

        [Import(HitTestEvent.ContractName)]
        public Event<HitTestEvent> OnHitTest
        {
            set => value.Subscribe((ref HitTestEvent e) => _sceneRoot?.Value.OnHitTest(ref e));
        }

        public void Dispose()
        {
            using (_renderReceived)
            { }
        }
    }
}