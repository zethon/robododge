using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoboDodge
{
    public class PlayerCharacter : Character
    {
        private DodgeBall playerBall;

        public PlayerCharacter(Model model, RDGame game, ICamerable camera, Terrain terrain)
            : base(model, game, camera, terrain)
        {
            Scale = 0.0f;

            playerBall = new DodgeBall(game, camera, terrain.BuildingBoxes, this)
            {
                Position = camera.Position,
                Lifespan = -1,
            };

            _game.Components.Add(playerBall);

            Health = GameConfig.Instance.PlayerHealth;
        }

        public override Vector3 GetPosition()
        {
            return Camera.Position;
        }


        override public void Fire()
        {
            SoundManager.Instance.Play(@"Audio\tick");
            Matrix cameraRotation = Matrix.CreateFromQuaternion(Camera.Rotation);

            //Vector3 cameraForward = new Vector3(0, -0.225f, -0.3f);
            Vector3 cameraForward = new Vector3(0, 0, -1);

            Vector3 nonNormalized;
            Vector3 cameraRotatedForward;
            
            nonNormalized = cameraRotatedForward = Vector3.Transform(cameraForward, cameraRotation);
            cameraRotatedForward.Normalize();

            playerBall.Position = Camera.Position + nonNormalized;
            playerBall.Velocity = cameraRotatedForward * -0.375f;
            playerBall.Lifespan =  5;

            playerBall = new DodgeBall(_game, Camera, TerrainBoxes, this)
            {
                Position = Camera.Position,
                Lifespan = -1,
            };

            _game.Components.Add(playerBall);
            
        }

        public void ResetHealth()
        {
            Health = GameConfig.Instance.PlayerHealth;
        }

        public override void Update(GameTime gameTime)
        {
            if (playerBall == null)
                return;

            Matrix cameraRotation = Matrix.CreateFromQuaternion(Camera.Rotation);

            Vector3 cameraForward = new Vector3(0, -0.225f, -0.3f);

            Vector3 cameraRotatedForward = Vector3.Transform(cameraForward, cameraRotation);

            playerBall.Position = Camera.Position + cameraRotatedForward;

            base.Draw(gameTime);
        }

    }
}
