﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ConcentratedHell.Particles
{
    class BloodFloorSplatter:Particle
    {
        public BloodFloorSplatter(Vector2 position, float scale) : base(
            Main.Instance.Content.Load<Texture2D>("Particles/BloodFloorSplatter/1"),
            position,
            new GameValue(0, 1200, 1, 0),
            Depth.Background
            )
        {
            renderedScale = scale / 4f;
            rotation = (Main.random.Next(0, 100) / 100f) * MathF.PI * 2f;
        }

        public override void Update()
        {
            base.Update();

            color = new Color(0.8f, 0.8f, 0.8f, 0.8f) * (1f - (float)age.Percent());
        }
    }
}
