/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

/* Copyright (C) 2012 Ruslan A. Abdrashitov

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE. */

using System.Collections.Generic;

namespace HTMLEngine.Core
{
    internal class DeviceChunkDrawCompiled : DeviceChunk
    {
        public HtCompiler compiled;
        private bool offsetApplied;

        public void Parse(IEnumerator<HtmlChunk> source, int width, string id = null, HtFont font = null, HtColor color = default(HtColor), TextAlign align = TextAlign.Left, VertAlign valign = VertAlign.Bottom)
        {
            compiled.Compile(source, width, id, font, color, align, valign);
            offsetApplied = false;
        }

        internal override void OnAcquire()
        {
            offsetApplied = false;
            compiled = HtEngine.GetCompiler();
            base.OnAcquire();
        }

        internal override void OnRelease()
        {
            compiled.Dispose();
            compiled = null;
            base.OnRelease();
        }

        public override void Draw(float deltaTime, string linkText, object userData)
        {
            if (!offsetApplied)
            {
                compiled.Offset(Rect.X, Rect.Y);
                offsetApplied = true;
            }
            compiled.Draw(deltaTime, userData);
        }

        public override void MeasureSize()
        {
            Rect.Width = compiled.CompiledWidth;
            Rect.Height = compiled.CompiledHeight;
        }
    }
}