// Adalid Claure
// CS325 - Fall 2010
// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using log4net;

namespace RoboDodge
{
    public class DodgeBall : ModelComponent, ICollidable
    {
        private int _iNumBounces = 0;

        static ILog log = LogManager.GetLogger(typeof(DodgeBall));

        private const string AssetName = "SphereHighPoly";

        private System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();

        public float Lifespan = -1; // -1 indefinitely
        public bool Destroyed = false;

        public Character Owner { get; private set; }

        public DodgeBall(RDGame game, ICamerable camera, BoundingBox[] terrainBoxes, Character owner)
            : base(LoadedModels.Instance[AssetName], game, camera, terrainBoxes)
        {
            Scale = 0.0775f; // grows 
            Texture = this.Game.Content.Load<Texture2D>("lightblue");
            Owner = owner;
        }

        public override void Update(GameTime gameTime)
        {
            float Elapsed = (float)_stopWatch.Elapsed.TotalSeconds;

            if (_iNumBounces > 4)
            {
                Destroyed = true;
            }

            if (Destroyed)
            {
                _game.Components.Remove(this);
                return;
            }

            if (Lifespan > 0 && !_stopWatch.IsRunning)
            {
                _stopWatch.Start();
            }

            if (Lifespan > 0 && Elapsed > Lifespan)
            {
                _game.Components.Remove(this);
                return;
            }

            Position += (-Velocity);

            base.Update(gameTime);
        }

        #region ICollidable Members

        /// <summary>
        /// Uses overlap testing to determine the last position before a collision
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private Vector3 GetLastNonCollisionPosition(BoundingBox b)
        {
            if (Position == null || Position == Vector3.Zero || Velocity == null || Velocity == Vector3.Zero)
            {
                return Vector3.Zero;
            }

            float fFactor = 1.0f;

            // TODO: this -Velocity hack is odd. See Update() where the position is updated
            //       by subatracting Velocity from Position
            Vector3 tp = Position - Vector3.Multiply(-Velocity, fFactor);
            BoundingSphere s = new BoundingSphere(tp, GameConstants.BallSphereRadius);

            int i = 0;

            do
            {
                ContainmentType t = b.Contains(s);

                if (!b.Intersects(s))
                {
                    fFactor *= fFactor < 0 ? 0.5f : -0.5f;
                }
                else
                {
                    fFactor *= fFactor > 0 ? 0.5f : -0.5f;
                }

                s.Center += Vector3.Multiply(Velocity, fFactor);

                i++;
            } while (i < 5);

            return s.Center;
        }

        private bool HandleTerrainCollision(BoundingSphere s)
        {
            var q = TerrainBoxes.Where(b => b.Intersects(s));

            if (q.Count() > 0)
            {
                BoundingBox box = q.First();

                if (box != null)
                {
                    Position = GetLastNonCollisionPosition(box);

                    Vector3 newVelocity = PhysicsHelper.ReflectVeloctiyOffBox(this, box);

                    if (newVelocity != Vector3.Zero)
                    {
                        Velocity = newVelocity;
                    }

                    if (_iNumBounces == 0)
                    {
                        SoundManager.Instance.Play(@"Audio\bounce");
                    }

                    _iNumBounces++;
                    return true;
                }
            }

            return false;
        }

        private bool HandlePlayerCollisions(BoundingSphere s)
        {
            BoundingSphere playerSphere = new BoundingSphere(_game.Camera.Position, GameConstants.PlayerSphereRadius);

            if (playerSphere.Intersects(s))
            {
                PlayerCharacter pc = (PlayerCharacter)Game.Components.Where(c => c is PlayerCharacter).FirstOrDefault();
                pc.Health--;

                SoundManager.Instance.Play(@"Audio\hit2");
                Destroyed = true;
                return true;
            }

            return false;
        }

        private bool HandleNPCCollisions(BoundingSphere s)
        {
            foreach (NPCOrb orb in _game.Components.Where(c => c is NPCOrb && this.Owner != c && ((NPCOrb)c).BoundingBox.Intersects(s)))
            {
                if (this.Owner is NPCOrb)
                {
                    continue;
                }

                Vector3 newVelocity = PhysicsHelper.ReflectVelocity(this,orb);
                
                if (newVelocity != Vector3.Zero)
                {
                    SoundManager.Instance.Play(@"Audio\shoot");

                    // let the npc respond to collision before modifying ball's (self) velocity
                    orb.RespondToCollision(this);

                    Velocity = newVelocity;
                    return true;
                }
            }

            return false;            
        }

        override public void HandleCollisions()
        {
            if (Velocity == Vector3.Zero)
            {
                return;
            }

            BoundingSphere s = new BoundingSphere(Position, GameConstants.BallSphereRadius);            

            // check collisions with terrain
            if (HandleTerrainCollision(s))
            {
                // no more collisions if we've hit terrain.
                return;
            }

            // check for collisions with player
            if (HandlePlayerCollisions(s))
            {
                return;
            }

            // check for collisions with npcs
            if (HandleNPCCollisions(s))
            {
                return;
            }
        }

        #endregion
    }
}
