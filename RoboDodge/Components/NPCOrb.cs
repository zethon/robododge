// Adalid Claure
// CS325 - Fall 2010
// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using log4net;

namespace RoboDodge
{
    /// <summary>
    /// NPC objects in the game that attack the player
    /// </summary>
    public class NPCOrb : Character
    {
        static ILog log = LogManager.GetLogger(typeof(NPCOrb));
        
        Stopwatch _lastFireTimer;

        public const string AssetName = @"orb_model";
        public bool IsFiring { get; private set; }

        public NPCOrb(RDGame game, ICamerable camera, Terrain terrain)
            : base(LoadedModels.Instance[AssetName], game, camera, terrain) 
        {
            Health = 5;

            Scale = 0.01925f;

            Rotation = Quaternion.CreateFromAxisAngle(Vector3.Left, MathHelper.Pi)
                        * Quaternion.CreateFromAxisAngle(Vector3.Forward, -MathHelper.Pi);
            
            // this allows a "default" Y value
            Position = new Vector3(0, 0.15f, 0);

            Velocity = new Vector3(0, 0, 0);
            Speed = 0.07f;

            _lastFireTimer = new Stopwatch();
            _lastFireTimer.Start();

            CurrentState = new IdleState(this);
        }

        override public void Update(GameTime gameTime)
        {
            if (Health < 0)
            {
                Destroyed = true;
            }

            // test to see if we've been desotryed
            if (Destroyed)
            {
                _game.Components.Remove(this);
                return;
            }

            CurrentState.Update();

            // handle the bot firing a weapon
            if (IsFiring)
            {
                IsFiring = false;
                DoFire();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Actually creates the ball that gets fired, called by Update()
        /// </summary>
        private void DoFire()
        {
            if (!CanFire())
            {
                return;
            }

            lock (this)
            {
                SoundManager.Instance.Play(@"Audio\tick");

                DodgeBall db = new DodgeBall(_game, Camera, TerrainBoxes, this)
                {
                    Lifespan = 3,
                    Scale = 0.0775f
                };

                db.Position = this.Position;

                // TODO: this assumes the target is the player -- bad assumption
                PlayerCharacter pc = Game.Components.Where(c => c is PlayerCharacter).FirstOrDefault() as PlayerCharacter;
                Vector3 diff = Vector3.Subtract(this.GetPosition(), pc.GetPosition());
                db.Velocity = Vector3.Normalize(diff) * 0.375f;

                // add the new ball to the game components
                _game.Components.Add(db);

                // start the timer to prevent rapid fire
                _lastFireTimer.Start();
            }
        }

        private bool CanFire()
        {
            if (_lastFireTimer.ElapsedMilliseconds < 1500)
            {
                return false;
            }

            _lastFireTimer.Reset();
            return true;
        }
        

        override public void Fire()
        {
            lock (this)
            {
                IsFiring = true;
            }
        }

        public void RespondToCollision(ModelComponent obj)
        {
            DodgeBall ball = obj as DodgeBall;

            if (ball != null)
            {
                CurrentState.DoBallCollision(ball);
            }
        }

        #region ICollidable Members

        private bool HandleTerrainCollisions()
        {
            if (Position == Vector3.Zero || (BoundingBox.Max == BoundingBox.Min))
            {
                return false;
            }
            
            var q = TerrainBoxes.Where(x => x.Intersects(BoundingBox));

            if (q.Count() > 0)
            {
                BoundingBox box = q.First();

                if (box.IsFloorBox())
                {
                    Velocity = Vector3.Reflect(Velocity, Vector3.Up);
                }
                else
                {
                    Velocity = -Velocity;
                }

                Matrix mx = Matrix.CreateWorld(Position, Velocity, Vector3.Up);
                Rotation = Quaternion.CreateFromRotationMatrix(mx);

                return true;
            }

            return false;
        }

        public override void HandleCollisions()
        {
            // don't bother testing for collisions if we're not moving
            if (Velocity == Vector3.Zero)
            {
                return;
            }

            if (HandleTerrainCollisions())
            {
                return;
            }
        }

        #endregion
    }
}
