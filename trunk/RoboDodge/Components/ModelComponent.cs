using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using log4net;

namespace RoboDodge
{
    public class ModelComponent : DrawableGameComponent, ICollidable
    {
        static ILog log = LogManager.GetLogger(typeof(ModelComponent));

        protected RDGame _game;

        public ICamerable Camera;
        public float Scale = 1.0f;
        public Vector3 Position = Vector3.Zero;
        public Vector3 Velocity = Vector3.Zero;
        public float Speed = 0.0f;
        public Quaternion Rotation = Quaternion.Identity;
        public Texture2D Texture = null;
        public BoundingBox BoundingBox;

        // TODO: what is this used for exactly?
        public Matrix[] Transforms;

        private Model _model = null;
        public Model Model
        {
            get { return _model; }
            set
            {
                _model = value;

                if (value != null)
                {
                    Transforms = new Matrix[_model.Bones.Count()];
                }
            }
        }

        // ICollidable member
        BoundingBox[] _terrainBoxes;
        public BoundingBox[] TerrainBoxes
        {
            get
            {
                return _terrainBoxes;
            }
        }

        public virtual Vector3 GetPosition()
        {
            return Position;
        }

        public ModelComponent(Model model, RDGame game, ICamerable camera, BoundingBox[] terrainBoxes)
            : base(game)
        {
            _game = game;

            Model = model;
            
            Camera = camera;
            BoundingBox = GetBoundingBox();

            // TODO: this is a bit of a hack, consider redesign of how icolidable's know about the terrain
            _terrainBoxes = terrainBoxes;            
        }

        protected BoundingBox GetBoundingBox()
        {
            // taken from: http://pasteall.org/1800/csharp
            Matrix worldMatrix = Matrix.CreateScale(Scale, Scale, Scale)
                        * Matrix.CreateFromQuaternion(Rotation)
                        * Matrix.CreateTranslation(Position);

            Vector3 Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix transform = Transforms[mesh.ParentBone.Index] * worldMatrix;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int stride = part.VertexStride;
                    int numberv = part.NumVertices;
                    byte[] vertexData = new byte[stride * numberv];

                    mesh.VertexBuffer.GetData(vertexData);

                    for (int ndx = 0; ndx < vertexData.Length; ndx += stride)
                    {
                        float floatvaluex = BitConverter.ToSingle(vertexData, ndx);
                        float floatvaluey = BitConverter.ToSingle(vertexData, ndx + 4);
                        float floatvaluez = BitConverter.ToSingle(vertexData, ndx + 8);
                        Vector3 vectCurrentVertex = new Vector3(floatvaluex, floatvaluey, floatvaluez);
                        Vector3 vectWorldVertex = Vector3.Transform(vectCurrentVertex, transform);

                        if (vectWorldVertex.X < Min.X) Min.X = vectWorldVertex.X;
                        if (vectWorldVertex.X > Max.X) Max.X = vectWorldVertex.X;
                        if (vectWorldVertex.Y < Min.Y) Min.Y = vectWorldVertex.Y;
                        if (vectWorldVertex.Y > Max.Y) Max.Y = vectWorldVertex.Y;
                        if (vectWorldVertex.Z < Min.Z) Min.Z = vectWorldVertex.Z;
                        if (vectWorldVertex.Z > Max.Z) Max.Z = vectWorldVertex.Z;
                    }
                }
            }

            return new BoundingBox(Min, Max);
        }

        override public void Draw(GameTime gameTime)
        {
            Matrix worldMatrix = Matrix.CreateScale(Scale, Scale, Scale)
                                    * Matrix.CreateFromQuaternion(Rotation)
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

        override public void Update(GameTime gameTime)
        {
            // update the bounding box
            //Vector3 center = BoundingBox.Max - BoundingBox.Min;

            //Vector3 offset = this.Position - center;
            //BoundingBox.Max += offset;
            //BoundingBox.Min += offset;

            BoundingBox = GetBoundingBox();
            base.Update(gameTime);
        }

        // ICollidable member
        virtual public void HandleCollisions()
        {
            //throw new NotImplementedException();
        }
    }
}
