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
using HTMLEngine.Core;

namespace HTMLEngine
{
    public class HtCompiler : PoolableObject
    {
        internal override void OnAcquire() { this.d = OP<DeviceChunkCollection>.Acquire(); }

        internal override void OnRelease()
        {
            this.d.Dispose();
            this.d = null;
        }

        private readonly Reader reader = new Reader();

        private DeviceChunkCollection d;

        public int CompiledWidth { get; private set; }
        public int CompiledHeight { get; private set; }

        public string GetLink(int x, int y)
        {
            if (this.d != null)
            {
                foreach (var data in this.d.Links)
                {
                    if (data.Key.Contains(x, y)) return data.Value;
                }
            }
            return null;
        }

        public void Compile(string source, int width)
        {
            this.reader.SetSource(source);
            {
                using (HtmlChunkCollection h = OP<HtmlChunkCollection>.Acquire())
                {
                    h.Read(this.reader);
                    this.Compile(h.GetEnumerator(), width);
                }
            }
        }

        internal void Compile(IEnumerator<HtmlChunk> source, int width, string id = null, HtFont font = null, HtColor color = default(HtColor), TextAlign align = TextAlign.Left, VertAlign valign = VertAlign.Bottom)
        {
            this.d.Clear();
            this.CompiledWidth = width;
            this.d.Parse(source, width, id, font, color, align, valign);
            this.MergeSameTextChunks();
            this.UpdateHeight();
        }

        private void UpdateHeight()
        {
            if (this.d.Lines.Count > 0)
            {
                DeviceChunkLine line = this.d.Lines[this.d.Lines.Count - 1];
                this.CompiledHeight = line.Y + line.Height;
            }
            else
            {
                this.CompiledHeight = 0;
            }
        }

        private void MergeSameTextChunks() {
          if (this.d != null) {
            for (int lineIndex = 0; lineIndex < this.d.Lines.Count; lineIndex++) {
              DeviceChunkLine line = this.d.Lines[lineIndex];
              DeviceChunk currChunk = null;
              for (int chunkIndex = 0; chunkIndex < line.Chunks.Count; ) {
                DeviceChunk chunk = line.Chunks[chunkIndex];
                if (currChunk == null) {
                  currChunk = chunk;
                  chunkIndex++;
                } else {
                  string linkText1;
                  this.d.Links.TryGetValue(currChunk, out linkText1);
                  string linkText2;
                  this.d.Links.TryGetValue(chunk, out linkText2);
                  if (string.Equals(linkText1, linkText2)) {
                    var textEffectChunk1 = currChunk as DeviceChunkDrawTextEffect;
                    var textEffectChunk2 = chunk as DeviceChunkDrawTextEffect;
                    if (textEffectChunk1 != null && textEffectChunk2 != null) {
                      // try to merge text effect chunk.
                      if (textEffectChunk1.Font.Equals(textEffectChunk2.Font) &&
													textEffectChunk1.Deco == textEffectChunk2.Deco &&
                          textEffectChunk1.Color.R == textEffectChunk2.Color.R &&
                          textEffectChunk1.Color.G == textEffectChunk2.Color.G &&
                          textEffectChunk1.Color.B == textEffectChunk2.Color.B &&
                          textEffectChunk1.Color.A == textEffectChunk2.Color.A &&
                          (textEffectChunk1.DecoStop == true || (textEffectChunk1.DecoStop == false && textEffectChunk2.DecoStop == false)) &&
                          textEffectChunk1.Effect == textEffectChunk2.Effect &&
                          textEffectChunk1.EffectColor.R == textEffectChunk2.EffectColor.R &&
                          textEffectChunk1.EffectColor.G == textEffectChunk2.EffectColor.G &&
                          textEffectChunk1.EffectColor.B == textEffectChunk2.EffectColor.B &&
                          textEffectChunk1.EffectColor.A == textEffectChunk2.EffectColor.A &&
                          textEffectChunk1.EffectAmount == textEffectChunk2.EffectAmount)
                      {
                        if (textEffectChunk2.PrevIsWord) {
                          textEffectChunk1.Text = textEffectChunk1 + " " + textEffectChunk2.Text;
                          textEffectChunk1.Rect.Width += textEffectChunk1.Font.WhiteSize + textEffectChunk2.Rect.Width;
                        } else {
                          textEffectChunk1.Text = textEffectChunk1 + textEffectChunk2.Text;
                          textEffectChunk1.Rect.Width += textEffectChunk2.Rect.Width;
                        }
                        line.Chunks.RemoveAt(chunkIndex);
                        textEffectChunk2.Dispose();
                        textEffectChunk2 = null;
                        continue;
                      }
                    } else if (textEffectChunk1 == null && textEffectChunk2 == null) {
                      var textChunk1 = currChunk as DeviceChunkDrawText;
                      var textChunk2 = chunk as DeviceChunkDrawText;
                      if (textChunk1 != null && textChunk2 != null) {
                        // try to merge text chunk.
                        if (textChunk1.Font.Equals(textChunk2.Font) && 
														textChunk1.Deco == textChunk2.Deco &&
                            textChunk1.Color.R == textChunk2.Color.R &&
                            textChunk1.Color.G == textChunk2.Color.G &&
                            textChunk1.Color.B == textChunk2.Color.B &&
                            textChunk1.Color.A == textChunk2.Color.A &&
                            (textChunk1.DecoStop == true || (textChunk1.DecoStop == false && textChunk2.DecoStop == false)))
                        {
                          if (textChunk2.PrevIsWord) {
                            textChunk1.Text = textChunk1 + " " + textChunk2.Text;
                            textChunk1.Rect.Width += textChunk1.Font.WhiteSize + textChunk2.Rect.Width;
                          } else {
                            textChunk1.Text = textChunk1 + textChunk2.Text;
                            textChunk1.Rect.Width += textChunk2.Rect.Width;
                          }
                          line.Chunks.RemoveAt(chunkIndex);
                          textChunk2.Dispose();
                          textChunk2 = null;
                          continue;
                        }
                      }
                    }
                  }
                  currChunk = chunk;
                  chunkIndex++;
                }
              }
            }
          }
        }

        public void Draw(float deltaTime, object userData = null)
        {
            if (this.d != null)
            {
                for (int lineIndex = 0; lineIndex < this.d.Lines.Count; lineIndex++)
                {
                    DeviceChunkLine line = this.d.Lines[lineIndex];
                    for (int chunkIndex = 0; chunkIndex < line.Chunks.Count; chunkIndex++)
                    {
                        DeviceChunk chunk = line.Chunks[chunkIndex];
                        string linkText;
                        if (this.d.Links.TryGetValue(chunk, out linkText)) {
														chunk.Draw(deltaTime, linkText, userData);
                        } else {
														chunk.Draw(deltaTime, null, userData);
                        }
                    }
                }
            }
        }

        public void Offset(int dx, int dy)
        {
            if (this.d != null)
            {
                for (int lineIndex = 0; lineIndex < this.d.Lines.Count; lineIndex++)
                {
                    DeviceChunkLine line = this.d.Lines[lineIndex];
                    for (int chunkIndex = 0; chunkIndex < line.Chunks.Count; chunkIndex++)
                    {
                        DeviceChunk chunk = line.Chunks[chunkIndex];
                        chunk.Rect.X += dx;
                        chunk.Rect.Y += dy;
                    }
                }
            }
        }

    }
}