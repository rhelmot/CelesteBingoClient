using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        private List<string> Toasts = new List<string>();
        private List<float> ToastTimers = new List<float>();

        private void UpdateToasts() {
            for (int i = 0; i < this.ToastTimers.Count; i++) {
                this.ToastTimers[i] += Engine.RawDeltaTime;
                if (this.ToastTimers[i] > 4f) {
                    this.ToastTimers.RemoveAt(i);
                    this.Toasts.RemoveAt(i);
                    i--;
                }
            }
        }

        private void RenderToasts() {
            Draw.SpriteBatch.Begin();
            float globalScale = Engine.ViewHeight / 1080f;
            var currentBase = (float)Engine.ViewHeight;
            for (int i = this.ToastTimers.Count - 1; i >= 0; i--) {
                var scale = 0.7f * globalScale;
                var timer = this.ToastTimers[i];
                var text = this.Toasts[i];
                var alpha = timer < 0.25f ? timer * 4f : timer > 3.5f ? (4.0f - timer) * 2 : 1f;
                var rise = timer < 0.25f ? timer * 4f : 1f;

                var textSize = ActiveFont.Measure(text) * scale;
                ActiveFont.DrawOutline(text, new Vector2(Engine.ViewWidth - 20, currentBase), new Vector2(1f, rise), Vector2.One * scale, Color.White * alpha, 1f, Color.Black * alpha);

                currentBase -= textSize.Y * 1.1f * rise;
            }
            Draw.SpriteBatch.End();
        }
        
        public static void Toast(string text) {
            Instance.Toasts.Add(text);
            Instance.ToastTimers.Add(0f);
        }

    }
}
