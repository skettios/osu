﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Framework.Graphics.Primitives;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        private const int bottom_bar_height = 5;

        protected override bool HideOnEscape => false;

        private static readonly Vector2 handle_size = new Vector2(14, 25);

        private const float transition_duration = 200;

        private readonly MusicSliderBar bar;
        private readonly SongProgressGraph graph;

        public Action<double> OnSeek;

        public IClock AudioClock;

        private double lastHitTime => ((objects.Last() as IHasEndTime)?.EndTime ?? objects.Last().StartTime) + 1;

        private double firstHitTime => objects.First().StartTime;

        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                graph.Objects = objects = value;
            }
        }

        private readonly BindableDouble seekPosition = new BindableDouble { MinValue = 0, MaxValue = 1 };

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.FillColour = bar.FillColour = colours.BlueLighter;
        }

        public SongProgress()
        {
            const float graph_height = SquareGraph.Column.WIDTH * 6;

            Height = bottom_bar_height + graph_height + handle_size.Y;
            Y = bottom_bar_height;

            Add(graph = new SongProgressGraph
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Height = graph_height,
                Margin = new MarginPadding { Bottom = bottom_bar_height },
            });
            Add(bar = new SongProgressBar(bottom_bar_height, graph_height, handle_size)
            {
                Alpha = 1,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                OnSeek = delegate(float position)
                {
                    OnSeek?.Invoke(firstHitTime + position * (lastHitTime - firstHitTime));
                },
                SeekPosition = seekPosition
            });
        }

        protected override void LoadComplete()
        {
            State = Visibility.Visible;
        }

        private bool allowSeeking;

        public bool AllowSeeking
        {
            get
            {
                return allowSeeking;
            }

            set
            {
                if (allowSeeking == value) return;

                allowSeeking = value;
                updateBarVisibility();
            }
        }

        private void updateBarVisibility()
        {
            bar.FadeTo(allowSeeking ? 1 : 0, transition_duration, EasingTypes.In);
            MoveTo(new Vector2(0, allowSeeking ? 0 : bottom_bar_height), transition_duration, EasingTypes.In);
        }

        protected override void PopIn()
        {
            updateBarVisibility();
            FadeIn(500, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            FadeOut(100);
        }

        protected override void Update()
        {
            base.Update();

            if (objects == null)
                return;

            double progress = ((AudioClock?.CurrentTime ?? Time.Current) - firstHitTime) / lastHitTime;

            seekPosition.Value = progress;
            graph.Progress = (int)(graph.ColumnCount * progress);
        }
    }
}
