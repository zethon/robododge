using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace RoboDodge
{
    public class Gem : ModelComponent
    {
        public const string AssetName = @"pants";
        public const float ScaleMax = 0.00525f;
        public const float ScaleMin = 0.00125f;

        private float _angle = 0.0f;

        public bool Captured = false;

        public Gem(RDGame game, ICamerable camera, BoundingBox[] terrain)
            : base(LoadedModels.Instance[AssetName], game, camera, terrain)
        {
            Scale = 0.00325f;
            Rotation = Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.PiOver2);
        }

        public override void Update(GameTime gameTime)
        {
            if (Captured)
            {
                _game.Components.Remove(this);

                // TODO: play sound!
                SoundManager.Instance.Play(@"Audio\boing");

                return;
            }

            _angle += 0.075f;
        }

        public override void Draw(GameTime gameTime)
        {
            Vector3 rotAxis = new Vector3(3 * _angle, _angle, 2 * _angle);
            rotAxis.Normalize();

            Matrix worldMatrix = Matrix.CreateScale(Scale)
                                    * Matrix.CreateFromAxisAngle(rotAxis, _angle)
                                    * Matrix.CreateTranslation(Position);

            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(Transforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (Texture != null)
                    {
                        effect.EnableDefaultLighting();
                        effect.Texture = Texture;
                        effect.TextureEnabled = true;
                    }

                    effect.World = Transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = Camera.ViewMatrix;
                    effect.Projection = Camera.ProjectionMatrix;
                }

                mesh.Draw();
            }
        }

        public override void HandleCollisions()
        {
            // NOTE: this is probably not a good way to do this
            if (_game is RDGame)
            {
                RDGame rg = _game as RDGame;

                BoundingSphere playerSphere = new BoundingSphere(rg.Camera.Position, 0.2f);
                BoundingBox gemBox = GetBoundingBox();

                if (gemBox.Contains(playerSphere) != ContainmentType.Disjoint)
                {
                    Captured = true;
                }
            }
        }
    }
}
