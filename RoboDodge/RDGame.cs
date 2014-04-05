// Adalid Claure
// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaInput = Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using log4net;

namespace RoboDodge
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class RDGame : Microsoft.Xna.Framework.Game
    {
        static ILog log = LogManager.GetLogger(typeof(RDGame));

        bool _bSpacePressed = false;
        
        LevelManager _levelManager; 
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont _font;
        TransitionScreen _transition;
        //CoordCross _cross;
        BoundingSphereRenderer _boundingSphereRenderer;
        Random _random;

        public FPSCamera Camera{ get; private set; }
        public PlayerCharacter PlayerCharacter { get; private set; }
        public Terrain Terrain { get; private set; }

        public RDGame()
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Debug("RoboBall started");
            GameConfig.LoadConfig("Resources/config.xml");

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _random = new Random();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = GameConfig.Instance.Height;
            graphics.PreferredBackBufferWidth = GameConfig.Instance.Width;

            if (GameConfig.Instance.FullScreen)
            {
                graphics.ToggleFullScreen();
            }

            graphics.ApplyChanges();

            LoadedModels.Instance.Content = Content;

            Camera = new FPSCamera(graphics.GraphicsDevice.Viewport,
                        new Vector3(0, 0.5f, 0), -0.75f, 0);

            _boundingSphereRenderer = new BoundingSphereRenderer(this);

            _transition = new TransitionScreen(this);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            try
            {
                // Create a new SpriteBatch, which can be used to draw textures.
                spriteBatch = new SpriteBatch(GraphicsDevice);
                
                _levelManager = new LevelManager(this,"Resources/levels.xml");

                if (GameConfig.Instance.StartLevel > 0)
                {
                    _levelManager.SetCurrentLevel(GameConfig.Instance.StartLevel-1);
                }            

                _font = Content.Load<SpriteFont>("GameFont");

                LoadedModels.Instance.Add("skybox");
                LoadedModels.Instance.Add("orb_model");
                LoadedModels.Instance.Add("SphereHighPoly");
                LoadedModels.Instance.Add("pants");

                SoundManager.Instance.Mute = GameConfig.Instance.Mute;
                SoundManager.Instance.Add(@"Audio\boing",Content);
                SoundManager.Instance.Add(@"Audio\bg1", Content);
                SoundManager.Instance.Add(@"Audio\tick", Content);
                SoundManager.Instance.Add(@"Audio\bounce", Content);
                SoundManager.Instance.Add(@"Audio\hit2", Content);
                SoundManager.Instance.Add(@"Audio\shoot", Content);
                SoundManager.Instance.Add(@"Audio\cheer", Content);
                SoundManager.Instance.Add(@"Audio\aww", Content);

                _boundingSphereRenderer.OnCreateDevice();
                //_cross = new CoordCross(GraphicsDevice);

                ResetLevel();

                if (!GameConfig.Instance.BGMute)
                {
                    SoundManager.Instance.Play(@"Audio\bg1");
                }
            }
            catch (Exception ex)
            {
                log.Fatal("Could load complete LoadContent()", ex);
                MessageBox.Show("Exception: " + ex.Message);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Clears the Components collection, resets the camera/player health and location
        /// and resets all NPC health and location
        /// </summary>
        private void ResetLevel()
        {
            Components.Clear();

            Terrain = new Terrain(this, Camera, _levelManager.CurrentLevel);

            // starting position of the camera
            Camera.Position = new Vector3(_levelManager.CurrentLevel.StartVector.X,
                                        Terrain.MinOffset,
                                        -(_levelManager.CurrentLevel.StartVector.Y));
            
            // set up our camera boundaries
            Camera.BoundingBox = Terrain.BoundingBox;
            Camera.BlockBoxes = Terrain.BuildingBoxes;

            // enemy
            PlayerCharacter = new PlayerCharacter(LoadedModels.Instance["orb_model"], this, Camera, Terrain);
            Components.Add(PlayerCharacter);

            // skybox component
            Components.Add(new SkyBoxComponent(LoadedModels.Instance["skybox"], this, Camera));

            // place the enemies on the level according to the config
            int i = 0;
            foreach (OrbInitInfo npcInfo in _levelManager.CurrentLevel.NPCs)
            {

                object[] args = new object[] { this, Camera, Terrain };
                ModelComponent m = (ModelComponent)System.Activator.CreateInstance(npcInfo.Type, args);
                m.Position = npcInfo.Path.StartPoint;

                if (_levelManager.CurrentLevel.Paths.Count() > 0 && m is Character && !(m is PlayerCharacter))
                {
                    if (i >= _levelManager.CurrentLevel.Paths.Count())
                    {
                        i = 0;
                    }

                    Character c = m as Character;
                    c.CurrentPath = new NPCPath(_levelManager.CurrentLevel.Paths[i]);
                    c.DefaultPath = c.CurrentPath;
                    i++;
                }

                Components.Add(m);
            }

            // RANDOM GEMS - use the Terrain object to place the configured amount of random gems on the map
            int iNodeCount = Terrain.OpenNodes.Count();
            int iGemCount = _levelManager.CurrentLevel.GemCount;
            for (int x = 0; x < iGemCount; x++)
            {
                int nodeIdx = _random.Next(0, iNodeCount);

                if (Terrain.BoundingBox.Contains(Terrain.OpenNodes[nodeIdx]) != ContainmentType.Disjoint)
                {
                    Gem g = new Gem(this, Camera, Terrain.BuildingBoxes)
                    {
                        Position = Terrain.OpenNodes[nodeIdx]
                    };

                    Components.Add(g);
                }
                else
                {
#if (DEBUG)
                    throw new Exception(string.Format("OpenNode ({0}) not in Level BoundingBox: {1}", Terrain.OpenNodes[nodeIdx], Terrain.BoundingBox));
#endif
                }
            }

            // place the descretely placed gems on the map
            foreach (GemInitInfo gi in _levelManager.CurrentLevel.Gems)
            {
                Gem g = new Gem(this, Camera, Terrain.BuildingBoxes)
                {
                    Position = gi.Position
                };

                Components.Add(g);
            }

            //DebugOpenNodes();
            //DebugClosedNodes();
            //DebugPath();
        }

        #region Debug Functions
        private void DebugClosedNodes()
        {
            // DEBUG CODE: SHOWS CLOSED NODES
            Model mo = Content.Load<Model>("SphereHighPoly");
            foreach (Vector3 v in Terrain.ClosedNodes)
            {
                ModelComponent mc = new ModelComponent(mo, this, Camera, Terrain.BuildingBoxes)
                {
                    Position = v,
                    Scale = 0.0775f,
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.PiOver2),
                };

                Components.Add(mc);
            }
        }

        private void DebugOpenNodes()
        {
            Model mo = Content.Load<Model>("SphereHighPoly");
            foreach (Vector3 v in Terrain.OpenNodes)
            {
                ModelComponent mc = new ModelComponent(mo, this, Camera, Terrain.BuildingBoxes)
                {
                    Position = v,
                    Scale = 0.0775f,
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.PiOver2),
                };

                Components.Add(mc);
            }
        }

        private void DebugPath()
        {
            PathFinder f = new PathFinder(this.Terrain);

            Vector3 start = new Vector3(0.9160264f,0.2f,-4.41638f);
            Vector3 finish = new Vector3(24.8919f, 0.9593245f, -14.998441f);

            NPCPath path = f.GetPath(start, finish);

            log.DebugFormat("DebugPath: Count {0}, Start: {1} End {2}", path.PathNodes.Count, path.StartPoint, path.EndPoint);

            foreach (Vector3 v in path.PathNodes)
            {
                ModelComponent mc = new ModelComponent(LoadedModels.Instance.Models["pants"], this, Camera, Terrain.BuildingBoxes)
                {
                    Position = v,
                    Scale = 0.00325f,
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.PiOver2),
                };

                Components.Add(mc);
            }
        }
        #endregion

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                this.Exit();
            }

            // we'll only update the transition screen
            if (_transition.IsActive)
            {
                _transition.Update(gameTime);
                return;
            }

            #region Input Handling
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                _bSpacePressed = true;
            }
            else
            {
                if (_bSpacePressed)
                {
                    _bSpacePressed = false;
                    PlayerCharacter.Fire();
                }
            }
            #endregion 

            #region Collision Handling
            foreach (IGameComponent g in Components)
            {
                if (g is ICollidable)
                {
                    ((ICollidable)g).HandleCollisions();
                }
            }
            #endregion

            #region Level & Transition Handling
            // see if the player has collected all the gems
            if (Components.Where(x => x is Gem).Count() == 0)
            {
                // transition to the next screen if we need to
                _transition.NextScreen();
                SoundManager.Instance.Play(@"Audio\cheer");
                if (_levelManager.CurrentLevelIndex < _levelManager.Levels.Count-1)
                {
                    _levelManager.SetCurrentLevel(_levelManager.CurrentLevelIndex + 1);
                }
                else
                {
                    // we've completed all the levels, start over
                    _levelManager.SetCurrentLevel(0);
                }

                ResetLevel();
                return;
            }
            else if (PlayerCharacter.Health <= 0)
            {
                SoundManager.Instance.Play(@"Audio\aww");
                _transition.ResetLevel();
                ResetLevel();
                PlayerCharacter.ResetHealth();
                return;
            }
            #endregion

            #region Camera Movement
            // TODO: Move this into the camera's class
            Vector3 oldPost = Camera.Position;
            Camera.Update(mouseState, keyState, GamePad.GetState(PlayerIndex.One));

            if (!GameConfig.Instance.FreeCamera)
            {
                // fix the cam's position if we've gone below the surface
                if (Camera.Position.Y < (Camera.BoundingBox.Min.Y + Terrain.MinOffset))
                {
                    Vector3 newPos = Camera.Position;
                    newPos.Y = Camera.BoundingBox.Min.Y + Terrain.MinOffset;
                    Camera.Position = newPos;
                }
            }
            #endregion

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            if (!_transition.IsActive)
            {
                Terrain.Draw(gameTime);
                base.Draw(gameTime);

                //_cross.Draw(Camera.ViewMatrix, Camera.ProjectionMatrix);
 
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

                int iGemCount = Components.Where(x => x is Gem).Count();
                string output = string.Format("Level: {0}    Remaining Gems: {1}    Health: {2}", _levelManager.CurrentLevelIndex + 1, iGemCount, PlayerCharacter.Health);

                Vector2 textSize = _font.MeasureString(output);
                float xPos = (graphics.GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2);
                spriteBatch.DrawString(_font, output, new Vector2(xPos, 1), Color.Yellow);

                if (GameConfig.Instance.ShowDebugInfo)
                {
                    string debug = string.Format("Player Position: {0}", PlayerCharacter.GetPosition());
                    spriteBatch.DrawString(_font, debug, new Vector2(1, 25), Color.White);
                }

                spriteBatch.End();
            }
            else
            {
                // we're at a transition screen, draw that instead
                _transition.Draw(gameTime);
            }
        }
    }
}
