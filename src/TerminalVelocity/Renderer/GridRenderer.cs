﻿/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Drawing;
using TerminalVelocity.Eventing;
using TerminalVelocity.Plugins;
using TerminalVelocity.Renderer.Events;

namespace TerminalVelocity.Renderer
{
    public class GridRenderer : IPlugin
    {
        private readonly ISurface _surface;
        private SizeF Size;
        private readonly RenderEvent _render;

        public GridRenderer(
            ISurface surface,

            RenderEvent onRenderEvent = null,
            ResizeEvent onResize = null
        )
        {
            _render = onRenderEvent;
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));
            onRenderEvent?.Subscribe(OnRender);
            onResize?.Subscribe(OnResize);
        }

        private EventStatus OnResize(in ResizeEventData e)
        {
            Size = e.Size;
            return EventStatus.Continue;
        }

        private EventStatus OnRender(in RenderEventData e)
        {
            var str = "Hello 😁 World - " + Environment.TickCount;
            var size = new SizeF(Size.Width, Size.Height);
            using (ISolidColorBrush brush = _surface.CreateSolidColorBrush(Color.White))
            using (ISolidColorBrush black = _surface.CreateSolidColorBrush(Color.Black))
            using (IFont font = _surface.CreateFont("Fira Code", 16))
            using (IText text = font.CreateText(brush, str.AsMemory(), size))
            {
                _surface.FillRectangle(black, new RectangleF(new PointF(0, 0), Size));
                text.Draw(new PointF(0, 0));
            }
            _render.Publish(new RenderEventData());
            return EventStatus.Halt;
        }

        public void Dispose()
        {

        }
    }
}
