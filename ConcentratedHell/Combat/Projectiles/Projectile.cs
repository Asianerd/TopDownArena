﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ConcentratedHell.Combat.Projectiles
{
    class Projectile
    {
        public static List<Projectile> collection;
        public static Dictionary<Type, Texture2D> spriteTable;

        public static void Initialize()
        {
            collection = new List<Projectile>();

            Main.UpdateEvent += StaticUpdate;
            Main.MidgroundDrawEvent += StaticDraw;
        }

        public static void LoadContent()
        {
            string projectilePath = "Weapons/Projectiles";
            spriteTable = new Dictionary<Type, Texture2D>();
            foreach(Type x in Enum.GetValues(typeof(Type)).Cast<Type>())
            {
                spriteTable.Add(x, Main.Instance.Content.Load<Texture2D>($"{projectilePath}/{x.ToString().ToLower()}"));
            }
        }

        public static void StaticUpdate()
        {
            foreach(Projectile x in collection)
            {
                x.Update();
            }
            collection.RemoveAll(n => !n.alive);
        }

        public static void StaticDraw()
        {
            foreach(Projectile x in collection)
            {
                x.Draw();
            }
        }

        #region Base class
        public Type type;
        public Vector2 position;
        public Vector2 increment;

        public Texture2D sprite;
        public Vector2 spriteOrigin;
        public float renderedScale = 3f;
        public Color color = Color.White;

        public GameValue age;
        public bool alive = true;

        public float direction;
        public float rotation = 0f;
        public float speed;
        public float weight;

        public float damage;

        public Projectile(Type _type, float _speed, float _damage, Vector2 _position, float _direction, float _weight = -100f)
        {
            type = _type;
            sprite = spriteTable[type];
            spriteOrigin = sprite.Bounds.Center.ToVector2();

            speed = _speed;
            weight = _weight == -100f? speed : _weight;
            damage = _damage;
            age = new GameValue(0, 240, 1, 0);

            position = _position;
            direction = _direction;

            increment = new Vector2(
                MathF.Cos(direction) * speed,
                MathF.Sin(direction) * speed
                );

            collection.Add(this);
        }

        public virtual void Update()
        {
            age.Regenerate(Universe.speedMultiplier);
            if(age.Percent() >= 1f)
            {
                Destroy();
            }

            Move();
            ValidatePosition();
            foreach(Entity.Entity x in Entity.Entity.collection)
            {
                if(x.rect.Contains(position))
                {
                    x.AffectHealth(damage, direction, weight);
                    x.Knockback(Cursor.Instance.playerToCursor, weight / 20f);
                    Destroy();
                    break;
                }
            }
        }

        public void ValidatePosition()
        {
            if(!Map.IsValidPosition(position.ToPoint()))
            {
                Destroy(mapCollide:true);
            }
        }

        public void Move()
        {
            position += increment * Universe.speedMultiplier;
        }

        public virtual void Destroy(bool mapCollide = false)
        {
            alive = false;
            var x = new Particles.WallCollidingParticle(position);
        }

        public virtual void Draw()
        {
            Main.spriteBatch.Draw(sprite, position, null, color, direction + rotation, spriteOrigin, renderedScale, SpriteEffects.None, 0f);
        }
        #endregion

        public enum Type
        {
            Generic,
            Plasma_orb,
            Pellet,
            Sniper_round,
            Small_round,
            Medium_round,
        }
    }
}
