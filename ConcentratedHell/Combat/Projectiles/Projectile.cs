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
        public static Dictionary<Type, Projectile> prefabrication;

        public static void Initialize()
        {
            collection = new List<Projectile>();

            Main.UpdateEvent += StaticUpdate;
            Main.DrawEvent += StaticDraw;
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
            List<Projectile> customizable = new List<Projectile>(collection);
            foreach(Projectile x in customizable)
            {
                x.Update();
            }
        }

        public static void StaticDraw()
        {
            foreach(Projectile x in collection)
            {
                x.Draw();
            }
        }

        #region Base class
        Type type;
        Vector2 position;
        Vector2 increment;
        Texture2D sprite;
        Vector2 spriteOrigin;
        float direction;
        float speed;

        float damage;

        public Projectile(Type _type, float _speed, float _damage, Vector2 _position, float _direction)
        {
            type = _type;
            sprite = spriteTable[type];
            spriteOrigin = sprite.Bounds.Center.ToVector2();

            speed = _speed;
            damage = _damage;

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
            Move();
            ValidatePosition();
            Point pointPos = position.ToPoint();
            foreach(Enemy x in Enemy.collection)
            {
                if(x.rect.Contains(position.ToPoint()))
                {
                    x.AffectHealth(damage);
                    Destroy();
                    break;
                }
            }
        }

        public void ValidatePosition()
        {
            if(!Map.IsValidPosition(position.ToPoint()))
            {
                Destroy();
            }
        }

        public void Move()
        {
            position += increment;
        }

        public virtual void Destroy()
        {
            collection.Remove(this);
        }

        public virtual void Draw()
        {
            Main.spriteBatch.Draw(sprite, position, null, Color.White, direction, spriteOrigin, 1f, SpriteEffects.None, 0f);
        }
        #endregion

        public enum Type
        {
            Generic
        }
    }
}