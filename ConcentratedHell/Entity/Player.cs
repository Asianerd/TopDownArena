﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ConcentratedHell.Combat;
using ConcentratedHell.Entity;

namespace ConcentratedHell
{
    class Player:Entity.Entity
    {
        #region Static
        public static Player Instance = null;
        public static List<Texture2D> playerEyeSprites;
        public delegate void PlayerEvents();
        public static PlayerEvents OnWeaponChange;

        public static void LoadContent(Texture2D _sprite)
        {
            Instance.sprite = _sprite;

            string _playerEyeAssetsPath = "Player/Blink";
            playerEyeSprites = new List<Texture2D>();
            for (int i = 1; i <= 5; i++)
            {
                playerEyeSprites.Add(Main.Instance.Content.Load<Texture2D>($"{_playerEyeAssetsPath}/blink{i}"));
            }
        }
        #endregion

        public static Vector2 size;
        public bool hasMovementInput = false;

        public Dictionary<Ammo.Type, int> ammoInventory;
        public Dictionary<Weapon.Type, Weapon> arsenal;
        public Weapon equippedWeapon = null;

        public GameValue eyeBlink;
        public GameValue regenerateCooldown;

        public Player():base(new Rectangle(0, 0, 64, 64), Type.Player, new GameValue(0, 1000, 0.5f), 8f)
        {
            Instance = this;

            size = new Vector2(64, 64);

            direction = 0f;
            speed = 5f;

            ammoInventory = new Dictionary<Ammo.Type, int>()
            {
                { Ammo.Type.Small, 5000 },
                { Ammo.Type.Medium, 40 },
                { Ammo.Type.Large, 14 },
                { Ammo.Type.Shell, 3000 },
                { Ammo.Type.Rocket, 6 },
                { Ammo.Type.Plasma, 80 },
            };

            arsenal = new Dictionary<Weapon.Type, Weapon>();

            eyeBlink = new GameValue(0, 120, 1);
            regenerateCooldown = new GameValue(0, 120, 1);
        }

        public override void Update()
        {
            if (Map.IsValidPosition(new Rectangle(position.ToPoint(), rect.Size)))
            {
                rect.Location = position.ToPoint();
            }

            speed = 8f * (Main.keyboardState.IsKeyDown(Keys.LeftControl) ? 0.5f : 1f) * Universe.speedMultiplier * (equippedWeapon == null ? 1f : equippedWeapon.speedMultiplier);

            if (!UI.UI.selectionActive)
            {
                if (equippedWeapon != null)
                {
                    equippedWeapon.Update();
                }
            }

            Move();
            if (Input.inputs[Keys.LeftShift].active)
            {
                Skill.ExecuteSkill(Skill.Type.Dash);
            }

            regenerateCooldown.Regenerate(Universe.speedMultiplier);
            if(regenerateCooldown.Percent() >= 1f)
            {
                health.Regenerate(Universe.speedMultiplier);
            }

            eyeBlink.Regenerate(0.2f * Universe.speedMultiplier);
            if (eyeBlink.Percent() >= 1f)
            {
                eyeBlink.AffectValue(0f);
            }
        }

        public void Move()
        {
            Vector2 targetVelocity = Vector2.Zero;
            foreach (Keys key in Input.playerInputKeys)
            {
                if (Main.keyboardState.IsKeyDown(key))
                {
                    targetVelocity += Input.directionalVectors[Input.keyDirections[key]];
                }
            }
            hasMovementInput = targetVelocity != Vector2.Zero;
            if (hasMovementInput)
            {
                CalculateDirection(Vector2.Zero, targetVelocity);
                targetVelocity.Normalize();
            }
            else
            {
                return;
            }

            Vector2 targetPosition = position + (targetVelocity * speed);
            Rectangle targetRectangle = new Rectangle(targetPosition.ToPoint(), rect.Size);

            if(Map.IsValidPosition(targetRectangle))
            {
                position = targetPosition;
                return;
            }

            Vector2 xVel = new Vector2(targetVelocity.X, 0);
            Rectangle xRect = new Rectangle((position + (xVel * speed)).ToPoint(), rect.Size);
            if (Map.IsValidPosition(xRect))
            {
                position.X = targetPosition.X;
            }

            Vector2 yVel = new Vector2(0, targetVelocity.Y);
            Rectangle yRect = new Rectangle((position + (yVel * speed)).ToPoint(), rect.Size);
            if (Map.IsValidPosition(yRect))
            {
                position.Y = targetPosition.Y;
            }
        }

        void CalculateDirection(Vector2 start, Vector2 end)
        {
            direction = MathF.Atan2(end.Y - start.Y, end.X - start.X);
        }

        public override void Draw()
        {
            Main.spriteBatch.Draw(sprite, rect, Map.IsValidPosition(rect)?Color.White:Color.Red);
            Vector2 eyePosition = new Vector2(
                (MathF.Cos(Cursor.Instance.playerToCursor) * 5f) + rect.Center.X,
                (MathF.Sin(Cursor.Instance.playerToCursor) * 5f) + rect.Center.Y - 5f
                );
            // sprites = 1, 2, 3, 4, 5(o)
            // blink = 1, 2, 3, 4, 5, 6, 7, 8, 9..120
            Texture2D _eyeSprite = playerEyeSprites[(eyeBlink.I >= playerEyeSprites.Count ? (playerEyeSprites.Count - 1) : (int)eyeBlink.I)];
            Main.spriteBatch.Draw(
                _eyeSprite,
                eyePosition,
                null,
                Color.White,
                0f,
                _eyeSprite.Bounds.Center.ToVector2(),
                5f,
                SpriteEffects.None,
                0f
                );
            if(equippedWeapon != null)
            {
                float distance = 45f + (5f * (float)equippedWeapon.cooldown.Percent());
                Vector2 renderedPosition = new Vector2(
                    (MathF.Cos(Cursor.Instance.playerToCursor) * distance) + rect.Center.X,
                    (MathF.Sin(Cursor.Instance.playerToCursor) * distance) + rect.Center.Y
                    );
                Main.spriteBatch.Draw(
                    equippedWeapon.sprite,
                    renderedPosition,
                    null,
                    Color.White,
                    Cursor.Instance.playerToCursor,
                    equippedWeapon.sprite.Bounds.Center.ToVector2(),
                    5f,
                    Math.Abs(Cursor.Instance.playerToCursor) >= (Math.PI / 2f) ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    0f);
            }
        }

        #region Weapon-related
        public void AddWeapon(Weapon weapon)
        {
            if (!arsenal.Keys.Contains(weapon.type))
            {
                arsenal.Add(weapon.type, weapon);
                equippedWeapon = arsenal[weapon.type];

                UI.SelectionWheel.SelectionWheel.Instance.UpdateSections();
            }
        }

        public void EquipWeapon(Weapon.Type type)
        {
            equippedWeapon = arsenal[type];
            if(OnWeaponChange != null)
            {
                OnWeaponChange();
            }
        }
        #endregion

        #region Ammo
        public void AffectAmmo(Ammo.Type type, int amount)
        {
            ammoInventory[type] += amount;
            UI.PickupText.AppendItem(type, amount);
        }
        #endregion

        #region Stats
        public override void OnDeath(float _direction = -10, float power = 3, float spread = 0.1F)
        {
            
        }

        public override void Anger()
        {
            
        }

        public override void DrawHealthBar()
        {
            
        }

        public override void AffectHealth(double damage, float _direction, float _speed)
        {
            base.AffectHealth(damage, _direction, _speed);

            regenerateCooldown.AffectValue(0f);
        }
        #endregion
    }
}
