// Adalid Claure
// CS325 - Fall 2010
// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoboDodge
{
    public class Terrain : DrawableGameComponent
    {
        public const int CeilingHeight = 3;
        public const float MinOffset = 0.5f;

        Game _game;
        LevelConfig _level;
        ICamerable _camera;
        BasicEffect _basicEffect;
        Random _random = new Random();

        VertexBuffer _floorVertexBuffer;
        VertexDeclaration _floorTextureDeclaration;

      
        Texture2D _terrainTexture;

        BoundingBox _terrainBox;
        public BoundingBox BoundingBox
        {
            get { return _terrainBox; }
        }

        BoundingBox[] _buildingBoxes;
        public BoundingBox[] BuildingBoxes
        {
            get { return _buildingBoxes; }
        }

        // used for path finding
        public List<Vector3> OpenNodes { get; private set; }
        public List<Vector3> ClosedNodes { get; private set; }

        public Terrain(Game game, ICamerable camera, LevelConfig level)
            : base(game)
        {
            _game = game;
            _level = level;
            _camera = camera;

            _basicEffect = new BasicEffect(_game.GraphicsDevice, null);

            Vector3[] boundaryPoints = new Vector3[2];
            boundaryPoints[0] = new Vector3(0, 0, 0);

            // TODO: get rid of that negative (here and many other places :[ )
            boundaryPoints[1] = new Vector3(_level.Width, CeilingHeight, -_level.Length);
            _terrainBox = BoundingBox.CreateFromPoints(boundaryPoints);

            ClosedNodes = new List<Vector3>();
            OpenNodes = new List<Vector3>();

            LoadFloorPlan();
            SetUpBoundingBoxes();
           
        }

        private void LoadFloorPlan()
        {
            _terrainTexture = _game.Content.Load<Texture2D>(_level.Textures["terrainfloor"]);
            SetUpVerticies();
        }

        private void SetUpBoundingBoxes()
        {
            int cityWidth = _level.FloorPlan.GetLength(0);
            int cityLength = _level.FloorPlan.GetLength(1);

            List<BoundingBox> boxes = new List<BoundingBox>();
            float? iMaxHeight = null;

            for (int x = 0; x < cityWidth; x++)
            {
                for (int z = 0; z < cityLength; z++)
                {
                    if (IsBuildingTile(_level.FloorPlan[x,z]))
                    {
                        // TODO: should raise a Configuration or LevelConfiguration exception
                        float iFlatHeight = float.Parse(_level.FloorPlan[x, z].ToString());

                        if (iMaxHeight == null || iFlatHeight > iMaxHeight)
                        {
                            iMaxHeight = iFlatHeight;
                        }

                        Vector3[] points = new Vector3[2];
                        points[0] = new Vector3(x, 0, -z);
                        points[1] = new Vector3(x - 1, iFlatHeight, -z - 1);
                        boxes.Add(BoundingBox.CreateFromPoints(points));

                        for (int i = 0; i < iFlatHeight; i++)
                        {
                            ClosedNodes.Add(new Vector3(x, i, -z));
                        }
                    }
                }
            }

            // add a box for the floor
            Vector3[] floor = new Vector3[] 
            { 
                new Vector3(1000, 0, 1000),
                new Vector3(-cityWidth, 0, -cityLength)
            };
            boxes.Add(BoundingBox.CreateFromPoints(floor));

            _buildingBoxes = boxes.ToArray();

            // set up our perimeter path finding nodes
            // TODO: hack alert - this HAS to be called after the building boxes are built, this should be somewhere else
            SetUpPerimeterNodes(new Vector3((float)cityWidth, (float)iMaxHeight, (float)-cityLength));
            SetUpOpenNodes(new Vector3((float)cityWidth, (float)iMaxHeight, (float)-cityLength));
        }

        private void SetUpOpenNodes(Vector3 MaxPoint)
        {
            for (int x = 0; x < (int)MaxPoint.X; x++)
            {
                for (int z = 0; z < (int)(-MaxPoint.Z); z++)
                {
                    for (int y = 0; y < (int)MaxPoint.Y; y++)
                    {
                        Vector3 v = new Vector3(x -0.0f, y, -z - 0.0f);
                        if (!ClosedNodes.Contains(v))
                        {
                            OpenNodes.Add(v);
                        }
                    }
                }
            }

            // do an intersection test for the npcs sphere to clean out the open nodes
            BoundingSphere tempSphere = new BoundingSphere(Vector3.Zero, GameConstants.PlayerSphereRadius);
            List<Vector3> tempList = new List<Vector3>();
            foreach (Vector3 tv in OpenNodes)
            {
                tempSphere.Center = tv;
                var q = BuildingBoxes.Where(b => b.Intersects(tempSphere));

                if (q.Count() > 0)
                {
                    tempList.Add(tv);
                }
            }

            foreach (Vector3 tv2 in tempList)
            {
                OpenNodes.Remove(tv2);
            }
        }

        /// <summary>
        /// Sets up the nodes used for path finding
        /// called by SetUpBoundingBoxes()
        /// </summary>
        private void SetUpPerimeterNodes(Vector3 MaxPoint)
        {
            // TODO: draw the floor and ceiling using the MaxPoint info, hack for now

            for (int x = 0; x < (int)MaxPoint.X; x++)
            {
                // nasty negative
                for (int y = 0; y < (int)-MaxPoint.Z; y++)
                {
                    // add closed off node list of the floor
                    ClosedNodes.Add(new Vector3(x,0, -y)); // TODO: get rid of negative! :(

                    // ceiling
                    ClosedNodes.Add(new Vector3(x, MaxPoint.Y, -y)); // TODO: get rid of negative! :(
                }
            }

            for (int z = 1; z < Math.Ceiling(MaxPoint.Y); z++)
            {
                for (int x = 0; x < (int)MaxPoint.X; x++)
                {
                    // Y[0] row in level.xml
                    ClosedNodes.Add(new Vector3(x, (float)z, 0.0f));

                    // Y[MAX] row in level.xml
                    ClosedNodes.Add(new Vector3(x, (float)z, MaxPoint.Z));
                }

                // negative hack, the MaxPoint.Z is negated in the calling function UGH!
                for (int y = 0; y < (int)-MaxPoint.Z; y++)
                {
                    // X[0] row in level.xml
                    ClosedNodes.Add(new Vector3(0.0f, (float)z, -y));

                    // X[MAX] row in level.xml
                    ClosedNodes.Add(new Vector3(MaxPoint.X, (float)z, -y));
                }
            }
        }

        private bool IsFloorTile(char c)
        {
            if (c == '.' || char.IsLetter(c))
            {
                return true;
            }

            return false;
        }

        private bool IsBuildingTile(char c)
        {
            if (char.IsDigit(c))
            {
                return true;
            }

            return false;
        }

        private void SetUpVerticies()
        {
            int cityWidth = _level.FloorPlan.GetLength(0);
            int cityLength = _level.FloorPlan.GetLength(1);

            List<VertexPositionNormalTexture> verticesList = new List<VertexPositionNormalTexture>();
            for (int x = 0; x < cityWidth; x++)
            {
                for (int z = 0; z < cityLength; z++)
                {
                    char current = _level.FloorPlan[x, z];

                    if (char.IsWhiteSpace(current))
                        continue;

                    float iFlatHeight = 0;

                    if (IsBuildingTile(current))
                    {
                        iFlatHeight = float.Parse(current.ToString());

                        float lesser = 0.5f;
                        float greater = 0.75f;
 
                        // top of box/wall tile
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z), new Vector3(0, 1, 0), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z - 1), new Vector3(0, 1, 0), new Vector2(lesser, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z), new Vector3(0, 1, 0), new Vector2(greater, 1)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z - 1), new Vector3(0, 1, 0), new Vector2(lesser, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z - 1), new Vector3(0, 1, 0), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z), new Vector3(0, 1, 0), new Vector2(greater, 1)));
                    }
                    else
                    {
                        float lesser = 0;
                        float greater = 0.25f;

                        // floor tile
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(0, 1, 0), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(0, 1, 0), new Vector2(lesser, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z), new Vector3(0, 1, 0), new Vector2(greater, 1)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(0, 1, 0), new Vector2(lesser, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z - 1), new Vector3(0, 1, 0), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z), new Vector3(0, 1, 0), new Vector2(greater, 1)));
                    }

                    if (iFlatHeight > 0)
                    {
                        float lesser = 0.25f;
                        float greater = 0.5f;

                        if (iFlatHeight > 2)
                        {
                            lesser = 0.75f;
                            greater = 1;
                        }

                        //front wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z - 1), new Vector3(0, 0, -1), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(0, 0, -1), new Vector2(greater, 1)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z - 1), new Vector3(0, 0, -1), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z - 1), new Vector3(0, 0, -1), new Vector2(lesser, 0)));

                        //back wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z), new Vector3(0, 0, 1), new Vector2(greater, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(0, 0, 1), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z), new Vector3(0, 0, 1), new Vector2(lesser, 0)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z), new Vector3(0, 0, 1), new Vector2(lesser, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z), new Vector3(0, 0, 1), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z), new Vector3(0, 0, 1), new Vector2(greater, 1)));

                        //left wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2(greater, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(-1, 0, 0), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z - 1), new Vector3(-1, 0, 0), new Vector2(lesser, 0)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z - 1), new Vector3(-1, 0, 0), new Vector2(lesser, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, iFlatHeight, -z), new Vector3(-1, 0, 0), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2(greater, 1)));

                        //right wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z), new Vector3(1, 0, 0), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z - 1), new Vector3(1, 0, 0), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z - 1), new Vector3(1, 0, 0), new Vector2(greater, 1)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z - 1), new Vector3(1, 0, 0), new Vector2(greater, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, 0, -z), new Vector3(1, 0, 0), new Vector2(lesser, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x - 1, iFlatHeight, -z), new Vector3(1, 0, 0), new Vector2(lesser, 0)));
                    }
                }
            }

            _floorVertexBuffer = new VertexBuffer(_game.GraphicsDevice, verticesList.Count * VertexPositionNormalTexture.SizeInBytes, BufferUsage.WriteOnly);

            _floorVertexBuffer.SetData<VertexPositionNormalTexture>(verticesList.ToArray());
            _floorTextureDeclaration = new VertexDeclaration(_game.GraphicsDevice, VertexPositionNormalTexture.VertexElements);
        }

        override public void Draw(GameTime gameTime)
        {
            // draw the floor tiles
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = _camera.ViewMatrix;
            _basicEffect.Projection = _camera.ProjectionMatrix;
            _basicEffect.Texture = _terrainTexture;
            _basicEffect.TextureEnabled = true;

            _basicEffect.EnableDefaultLighting();
            Vector3 lightDirection = new Vector3(1, -1, 1);
            lightDirection.Normalize();
            _basicEffect.DirectionalLight0.Direction = lightDirection;
            _basicEffect.DirectionalLight0.Enabled = true;
            _basicEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);

            //_basicEffect.DirectionalLight1.Enabled = false;
            _basicEffect.DirectionalLight1.Enabled = true;
            _basicEffect.DirectionalLight2.Enabled = false;
            _basicEffect.SpecularColor = new Vector3(0, 0, 0);

            _basicEffect.Begin();
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                _game.GraphicsDevice.VertexDeclaration = _floorTextureDeclaration;
                _game.GraphicsDevice.Vertices[0].SetSource(_floorVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                _game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _floorVertexBuffer.SizeInBytes / VertexPositionNormalTexture.SizeInBytes / 3);

                pass.End();
            }
            _basicEffect.End();
        }

        Vector3 RandPointInCube(int xMax, int yMax, int zMax)
        {
            int x = _random.Next(xMax) + 1;
            int y = _random.Next(yMax) + 1;
            int z = _random.Next(zMax) + 1;

            return new Vector3((float)x, (float)y, (float)z);
        }
    }
}
