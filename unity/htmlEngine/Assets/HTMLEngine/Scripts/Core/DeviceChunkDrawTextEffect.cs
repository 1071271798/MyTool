/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

namespace HTMLEngine.Core
{
    internal class DeviceChunkDrawTextEffect : DeviceChunkDrawText
    {
        public DrawTextEffect Effect;
        public HtColor EffectColor;
        public int EffectAmount;

        public override void Draw(float deltaTime, string linkText, object userData)
        {
            bool isTextEmpty = this.Text.Length == 1 && this.Text[0] <= ' ';
            switch (this.Effect)
            {
                case DrawTextEffect.Shadow:
                    if (!isTextEmpty)
                    {
                        this.Font.Draw(null, this.Rect.Offset(this.EffectAmount, this.EffectAmount), this.EffectColor,
                                       this.Text, true, Effect, EffectColor, EffectAmount, null, userData);
                    }
                    break;
                case DrawTextEffect.Outline:
                    if (!isTextEmpty)
                    {
                        this.Font.Draw(null, this.Rect.Offset(this.EffectAmount, 0), this.EffectColor, this.Text, true, Effect, EffectColor, EffectAmount, null, userData);
                        this.Font.Draw(null, this.Rect.Offset(-this.EffectAmount, 0), this.EffectColor, this.Text, true, Effect, EffectColor, EffectAmount, null, userData);
                        this.Font.Draw(null, this.Rect.Offset(0, this.EffectAmount), this.EffectColor, this.Text, true, Effect, EffectColor, EffectAmount, null, userData);
                        this.Font.Draw(null, this.Rect.Offset(0, -this.EffectAmount), this.EffectColor, this.Text, true, Effect, EffectColor, EffectAmount, null, userData);
                    }
                    break;
            }

            HtDevice device = HtEngine.Device;
            if (0 != (this.Deco & DrawTextDeco.Underline)) {
								device.FillRect(new HtRect(Rect.X, Rect.Bottom - 2, DecoStop ? Rect.Width : this.TotalWidth, 1), this.Color, userData);
            }
            if (0 != (this.Deco & DrawTextDeco.Strike)) {
								device.FillRect(new HtRect(Rect.X, Rect.Bottom - Rect.Height / 2 - 1, DecoStop ? Rect.Width : this.TotalWidth, 1), this.Color, userData);
            }
            this.Font.Draw(this.Id, this.Rect, this.Color, this.Text, false, Effect, EffectColor, EffectAmount, linkText, userData);
        }
    }
}