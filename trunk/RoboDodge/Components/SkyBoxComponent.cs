using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoboDodge
{
    public class SkyBoxComponent : ModelComponent
    {
        public SkyBoxComponent(Model model, RDGame game, ICamerable camera)
            : base(model, game, camera,null)
        {
        }

        override public void Draw(GameTime gameTime)
        {
            GraphicsDevice device = _game.GraphicsDevice;

            TextureAddressMode u = device.SamplerStates[0].AddressU;
            TextureAddressMode v = device.SamplerStates[0].AddressV;
            device.RenderState.DepthBufferWriteEnable = false;

            device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            Matrix[] skyboxTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(skyboxTransforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    currentEffect.World = skyboxTransforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(Camera.Position);
                    currentEffect.View = Camera.ViewMatrix;
                    currentEffect.Projection = Camera.ProjectionMatrix;
                }
                mesh.Draw();
            }

            device.RenderState.DepthBufferWriteEnable = true;
            device.SamplerStates[0].AddressU = u;
            device.SamplerStates[0].AddressV = v;
        }
    }
}
