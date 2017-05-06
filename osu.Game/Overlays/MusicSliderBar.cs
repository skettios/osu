using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class MusicSliderBar : SliderBar<double>
    {
        public Container Fill { get; }

        public Color4 FillColour
        {
            get { return Fill.Colour; }
            set { Fill.Colour = value; }
        }

        public bool IsSeeking { get; private set; }

        public Action<float> OnSeek;

        private BindableDouble seekPosition;
        public BindableDouble SeekPosition
        {
            get { return seekPosition; }
            set
            {
                seekPosition = value;
                Current.BindTo(value);
            }
        }

        private bool enabled = true;
        public bool IsEnabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (!enabled)
                    Fill.Width = 0;
            }
        }

        public MusicSliderBar()
        {
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                Fill = new Container
                {
                    Name = "FillContainer",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            };
        }

        protected override void UpdateValue(float value)
        {
            if (IsSeeking || !IsEnabled)
                return;

            updatePosition(value, false);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            seek(state);
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            seek(state);
            return true;
        }

        protected override bool OnDragStart(InputState state) => IsSeeking = true;

        protected override bool OnDragEnd(InputState state) => IsSeeking = false;

        private void seek(InputState state)
        {
            float seekLocation = state.Mouse.Position.X / DrawWidth;

            if (!IsEnabled)
                return;

            OnSeek?.Invoke(seekLocation);
            updatePosition(seekLocation);
        }

        private void updatePosition(float position, bool easing = true)
        {
            position = MathHelper.Clamp(position, 0, 1);
            Fill.TransformTo(() => Fill.Width, position, easing ? 200 : 0, EasingTypes.OutQuint, new TransformSeek());
        }

        private class TransformSeek : TransformFloat
        {
            public override void Apply(Drawable d)
            {
                base.Apply(d);
                d.Width = CurrentValue;
            }
        }
    }
}
