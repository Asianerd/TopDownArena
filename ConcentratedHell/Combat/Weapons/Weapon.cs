﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ConcentratedHell.Combat.Projectiles;
using ConcentratedHell.Entity;

namespace ConcentratedHell.Combat
{
    class Weapon
    {
        /* Handguns possibly?
         * 
         * Railgun
         * Burst rifle
         * Rocket launcher
         * Grenade launcher
         *      - Possible ice launcher?
         * #Amogus shotgun
         * Dildo launcher
         * #Sniper
         * #Anti-material rifle (BFG-9000 tendrils? / Piercing? / )
         *      - High damage & knockback
         *      - High cooldown
         *      - Special 
         * #Gatling gun
         *      - Low cooldown
         *      - Low damage & medium knockback
         * 
         * Kunai/ Shuriken
         *      - Inflict poison?
         * 
         * Grappling hook
         */

        #region Static
        public static Dictionary<Type, List<Texture2D>> spriteTable;
        public static Dictionary<Type, string> weaponNames;
        public static Dictionary<Type, string> weaponDescriptions;

        public static void Initialize()
        {
            string weaponSpritePath = "Weapons";
            spriteTable = new Dictionary<Type, List<Texture2D>>();
            foreach(Type x in Enum.GetValues(typeof(Type)).Cast<Type>())
            {
                List<Texture2D> sprites = new List<Texture2D>();
                switch(x)
                {
                    case Type.Gatling_Gun:
                        sprites = new List<Texture2D>()
                        {
                            Main.Instance.Content.Load<Texture2D>($"{weaponSpritePath}/{x.ToString().ToLower()}1"),
                            Main.Instance.Content.Load<Texture2D>($"{weaponSpritePath}/{x.ToString().ToLower()}2"),
                        };
                        break;
                    default:
                        sprites = new List<Texture2D>()
                        {
                            Main.Instance.Content.Load<Texture2D>($"{weaponSpritePath}/{x.ToString().ToLower()}")
                        };
                        break;
                }
                spriteTable.Add(x, sprites);
            }

            weaponNames = new Dictionary<Type, string>()
            {
                { Type.Shotgun, "Shotgun" },
                { Type.Plasma_Rifle, "Plasma rifle" },
                { Type.Auto_Shotgun, "Auto-shotgun" },
                { Type.Barrett, "Barrett" },
                { Type.Hell, "Hell" },
                { Type.Gatling_Gun, "Gatling gun" },
            };

            weaponDescriptions = new Dictionary<Type, string>()
            {
                { Type.Shotgun, "A vintage shotgun; Perfect for close range but not suited for large hordes" },
                { Type.Plasma_Rifle, "A sci-fi rifle that fires high-energy projectiles" },
                { Type.Auto_Shotgun, "Automatic shotgun that eats through ammo; Every round disposed rewards you with great satisfaction" },
                { Type.Barrett, "A regular Barrett sniper rifle" },
                { Type.Hell, "a dev weapon\n\nyoure not supposed to see this lmao" },
                { Type.Gatling_Gun, "A heavy gatling-gun that exterminates hordes with ease" },
                //{ Type.Gatling_Gun, "a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a " },
            };
        }
        #endregion

        public Texture2D sprite;
        public List<Texture2D> sprites;
        public int spriteIndex = 0;
        public Type type;
        public string name;
        public Projectile.Type projectileType;
        public Ammo.Type ammoType;
        public int ammoUsage;
        public GameValue cooldown;
        public float projectileSpawnDistance;
        public float knockback;
        public float speedMultiplier = 1f;

        public Weapon(Type _type, Projectile.Type _projectileType, float _projectileSpawnDistance, float _knockback, Ammo.Type _ammoType, GameValue _cooldown, float _speedMultiplier = 1f, int _ammoUsage = 1)
        {
            type = _type;
            name = weaponNames[type];
            projectileType = _projectileType;
            projectileSpawnDistance = _projectileSpawnDistance;
            knockback = _knockback;

            ammoType = _ammoType;
            ammoUsage = _ammoUsage;
            cooldown = _cooldown;
            cooldown.AffectValue(0f);

            speedMultiplier = _speedMultiplier;

            sprites = spriteTable[type];
            sprite = sprites[spriteIndex];
        }

        public virtual void Update()
        {
            cooldown.Regenerate();

            if (MouseInput.LMouse.isPressed && (cooldown.Percent() >= 1f))
            {
                if (Player.Instance.ammoInventory[ammoType] >= ammoUsage)
                {
                    ExecuteFire();
                }
            }
        }

        public virtual void ExecuteFire() // Run before Fire() method
        {
            Player.Instance.ammoInventory[ammoType] -= ammoUsage;
            Vector2 origin = new Vector2(
                (MathF.Cos(Cursor.Instance.playerToCursor) * projectileSpawnDistance) + Player.Instance.rect.Center.X,
                (MathF.Sin(Cursor.Instance.playerToCursor) * projectileSpawnDistance) + Player.Instance.rect.Center.Y
                );
            Fire(origin);
            cooldown.AffectValue(0f);

            Player.Instance.Knockback(
                MathF.Atan2(
                    Player.Instance.rect.Center.Y - origin.Y,
                    Player.Instance.rect.Center.X - origin.X
                    ),
                knockback * (Main.random.Next(97, 103) / 100f)
                );
        }

        public virtual void Fire(Vector2 origin) // Firing origin; The user's position
        {

        }

        public virtual void AltFire(Vector2 origin)
        {

        }

        public virtual void SpawnEmptyCartridge()
        {
            Vector2 cartridgeSpawn = (new Vector2(
                MathF.Cos(Cursor.Instance.playerToCursor),
                MathF.Sin(Cursor.Instance.playerToCursor)
                ) * (projectileSpawnDistance / 2f)) + Player.Instance.rect.Center.ToVector2();
            var z = new Particles.AmmoCartridgeParticle(ammoType, cartridgeSpawn, 1f);
        }

        public virtual void SpawnEmptyCartridge(Ammo.Type type)
        {
            Vector2 cartridgeSpawn = (new Vector2(
                MathF.Cos(Cursor.Instance.playerToCursor),
                MathF.Sin(Cursor.Instance.playerToCursor)
                ) * (projectileSpawnDistance / 2f)) + Player.Instance.rect.Center.ToVector2();
            var z = new Particles.AmmoCartridgeParticle(type, cartridgeSpawn, 1f);
        }

        public virtual void AdvanceSprite()
        {
            spriteIndex++;
            if(spriteIndex >= sprites.Count)
            {
                spriteIndex = 0;
            }

            sprite = sprites[spriteIndex];
        }

        public enum Type
        {
            Shotgun,
            Plasma_Rifle,
            Auto_Shotgun,
            Barrett,
            Hell,
            Gatling_Gun,
        }
    }
}
