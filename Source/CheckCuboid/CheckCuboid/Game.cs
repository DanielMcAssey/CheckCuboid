using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace CheckCuboid
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        InputManager _obj_input;
        Camera _obj_camera;
        GraphicsDevice _obj_graphics;
        SpriteFont _obj_font;
        bool isInside = false;
        bool moveCube = false;

        //Cube
        Model mCubeModel = null;
        Vector3 mCubePosition = Vector3.Zero;
        Vector3 mCubeScale = Vector3.One;
        Matrix[] mCubeTransforms;
        Vector3 mCubeRotation = Vector3.Zero;
        Matrix mCubeRotationMatrix = Matrix.Identity;
        Matrix mCubeWorld = Matrix.Identity;

        //Point
        Model mPointModel = null;
        Vector3 mPointPosition = Vector3.Zero;
        Vector3 mPointScale = Vector3.One;
        Matrix[] mPointTransforms;
        Vector3 mPointRotation = Vector3.Zero;
        Matrix mPointRotationMatrix = Matrix.Identity;
        Matrix mPointWorld = Matrix.Identity;

        #region "Check Cuboid Code"
        public bool IsVectorInCube(Vector3 _pointPosition) 
        {
            Vector3 relativePos = _pointPosition - this.mCubePosition;
            Matrix reverseMat = Matrix.Invert(this.mCubeRotationMatrix);
            Vector3 rotatedPos = new Vector3((reverseMat.M11 * relativePos.X) + (reverseMat.M21 * relativePos.Y) + (reverseMat.M31 * relativePos.Z),
                (reverseMat.M12 * relativePos.X) + (reverseMat.M22 * relativePos.Y) + (reverseMat.M23 * relativePos.Z),
                (reverseMat.M13 * relativePos.X) + (reverseMat.M23 * relativePos.Y) + (reverseMat.M33 * relativePos.Z));

            if (Math.Abs(rotatedPos.X) >= this.mCubeScale.X / 2)
                return false;

            if (Math.Abs(rotatedPos.Y) >= this.mCubeScale.Y / 2)
                return false;

            if (Math.Abs(rotatedPos.Z) >= this.mCubeScale.Z / 2)
                return false;

            return true;
        }
        #endregion

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.Window.Title = "Check Cuboid - Daniel Mcassey (495652)";
            this.graphics.PreferredBackBufferHeight = 720;
            this.graphics.PreferredBackBufferWidth = 1280;
            this.graphics.PreferMultiSampling = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this._obj_graphics = this.graphics.GraphicsDevice;
            this._obj_input = new InputManager();
            this._obj_input.AddKeyboardInput("KEY_LEFT", Keys.Left, false);
            this._obj_input.AddKeyboardInput("KEY_RIGHT", Keys.Right, false);
            this._obj_input.AddKeyboardInput("KEY_UP", Keys.Up, false);
            this._obj_input.AddKeyboardInput("KEY_DOWN", Keys.Down, false);

            this._obj_input.AddKeyboardInput("KEY_FORWARD", Keys.OemPlus, false);
            this._obj_input.AddKeyboardInput("KEY_BACK", Keys.OemMinus, false);

            this._obj_input.AddKeyboardInput("CUBE_SCALE_UP", Keys.U, false);
            this._obj_input.AddKeyboardInput("CUBE_SCALE_DOWN", Keys.O, false);

            this._obj_input.AddKeyboardInput("CUBE_ROT_LEFT", Keys.J, false);
            this._obj_input.AddKeyboardInput("CUBE_ROT_RIGHT", Keys.L, false);
            this._obj_input.AddKeyboardInput("CUBE_ROT_UP", Keys.I, false);
            this._obj_input.AddKeyboardInput("CUBE_ROT_DOWN", Keys.K, false);

            this._obj_input.AddKeyboardInput("CUBE_RESET", Keys.F1, true);
            this._obj_input.AddKeyboardInput("CUBE_TOGL_MOVE", Keys.F2, true);

            this._obj_input.AddKeyboardInput("CAMERA_LEFT", Keys.A, false);
            this._obj_input.AddKeyboardInput("CAMERA_RIGHT", Keys.D, false);
            this._obj_input.AddKeyboardInput("CAMERA_UP", Keys.W, false);
            this._obj_input.AddKeyboardInput("CAMERA_DOWN", Keys.S, false);
            this._obj_input.AddKeyboardInput("CAMERA_RESET", Keys.Space, true);
            this._obj_input.AddKeyboardInput("CAMERA_ZOOM_IN", Keys.E, false);
            this._obj_input.AddKeyboardInput("CAMERA_ZOOM_OUT", Keys.Q, false);
            this._obj_input.AddKeyboardInput("CAMERA_RESET", Keys.Space, true);
            
            this._obj_camera = new Camera(ref this._obj_graphics, ref this._obj_input);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            this.mCubeModel = this.Content.Load<Model>("Models/cuboid");
            this.mCubeTransforms = new Matrix[this.mCubeModel.Bones.Count];
            this.mCubeModel.CopyAbsoluteBoneTransformsTo(this.mCubeTransforms);
            this.mCubeScale = new Vector3(1.5f, 1f, 1f);
            this.mCubeScale *= 100;

            this.mPointModel = this.Content.Load<Model>("Models/sphere");
            this.mPointTransforms = new Matrix[this.mPointModel.Bones.Count];
            this.mPointModel.CopyAbsoluteBoneTransformsTo(this.mPointTransforms);
            this.mPointScale *= 1;

            this._obj_font = this.Content.Load<SpriteFont>("Fonts/default");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float timeDiff = (float)gameTime.ElapsedGameTime.TotalSeconds;
            this._obj_input.startUpdate();

            #region "Controls"
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float pointSpeed = 1f;

            if (this.moveCube)
            {
                if (this._obj_input.IsPressed("KEY_UP", PlayerIndex.One))
                    this.mCubePosition += new Vector3(0f, pointSpeed, 0f);

                if (this._obj_input.IsPressed("KEY_DOWN", PlayerIndex.One))
                    this.mCubePosition += new Vector3(0f, -pointSpeed, 0f);

                if (this._obj_input.IsPressed("KEY_LEFT", PlayerIndex.One))
                    this.mCubePosition += new Vector3(-pointSpeed, 0f, 0f);

                if (this._obj_input.IsPressed("KEY_RIGHT", PlayerIndex.One))
                    this.mCubePosition += new Vector3(pointSpeed, 0f, 0f);

                if (this._obj_input.IsPressed("KEY_FORWARD", PlayerIndex.One))
                    this.mCubePosition += new Vector3(0f, 0f, -pointSpeed);

                if (this._obj_input.IsPressed("KEY_BACK", PlayerIndex.One))
                    this.mCubePosition += new Vector3(0f, 0f, pointSpeed);
            }
            else
            {
                if (this._obj_input.IsPressed("KEY_UP", PlayerIndex.One))
                    this.mPointPosition += new Vector3(0f, pointSpeed, 0f);

                if (this._obj_input.IsPressed("KEY_DOWN", PlayerIndex.One))
                    this.mPointPosition += new Vector3(0f, -pointSpeed, 0f);

                if (this._obj_input.IsPressed("KEY_LEFT", PlayerIndex.One))
                    this.mPointPosition += new Vector3(-pointSpeed, 0f, 0f);

                if (this._obj_input.IsPressed("KEY_RIGHT", PlayerIndex.One))
                    this.mPointPosition += new Vector3(pointSpeed, 0f, 0f);

                if (this._obj_input.IsPressed("KEY_FORWARD", PlayerIndex.One))
                    this.mPointPosition += new Vector3(0f, 0f, -pointSpeed);

                if (this._obj_input.IsPressed("KEY_BACK", PlayerIndex.One))
                    this.mPointPosition += new Vector3(0f, 0f, pointSpeed);
            }

            if (this._obj_input.IsPressed("CUBE_RESET", PlayerIndex.One))
            {
                this.mCubePosition = Vector3.Zero;
                this.mCubeScale = new Vector3(1.5f, 1f, 1f) * 100;
                this.mCubeRotation = Vector3.Zero;
            }

            if (this._obj_input.IsPressed("CUBE_TOGL_MOVE", PlayerIndex.One))
                this.moveCube = !this.moveCube;

            if (this._obj_input.IsPressed("CUBE_SCALE_UP", PlayerIndex.One))
                this.mCubeScale += new Vector3(1f, 1f, 1f);

            if (this._obj_input.IsPressed("CUBE_SCALE_DOWN", PlayerIndex.One))
                this.mCubeScale -= new Vector3(1f, 1f, 1f);

            if (this._obj_input.IsPressed("CUBE_ROT_LEFT", PlayerIndex.One))
                this.mCubeRotation += new Vector3(0f, -1f, 0f) * timeDiff;
            else if (this._obj_input.IsPressed("CUBE_ROT_RIGHT", PlayerIndex.One))
                this.mCubeRotation += new Vector3(0f, 1f, 0f) * timeDiff;

            if (this._obj_input.IsPressed("CUBE_ROT_UP", PlayerIndex.One))
                this.mCubeRotation += new Vector3(-1f, 0f, 0f) * timeDiff;
            else if (this._obj_input.IsPressed("CUBE_ROT_DOWN", PlayerIndex.One))
                this.mCubeRotation += new Vector3(1f, 0f, 0f) * timeDiff;

#endregion

            this.mCubeRotationMatrix = (Matrix.CreateRotationY(this.mCubeRotation.Y) * Matrix.CreateRotationZ(this.mCubeRotation.Z) * Matrix.CreateRotationX(this.mCubeRotation.X));
            this.mCubeWorld = (Matrix.CreateScale(this.mCubeScale) * this.mCubeRotationMatrix * Matrix.CreateTranslation(this.mCubePosition));

            this.mPointRotationMatrix = (Matrix.CreateRotationY(this.mPointRotation.Y) * Matrix.CreateRotationZ(this.mPointRotation.Z) * Matrix.CreateRotationX(this.mPointRotation.X));
            this.mPointWorld = (Matrix.CreateScale(this.mPointScale) * this.mPointRotationMatrix * Matrix.CreateTranslation(this.mPointPosition));
            
            this._obj_camera.HandleInput(PlayerIndex.One);
            this._obj_camera.Update(timeDiff, Matrix.Identity);

            this.isInside = this.IsVectorInCube(this.mPointPosition);

            this._obj_input.endUpdate();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string statusInsideCube = "Inside Cube: ";
            string statusPointPos = "Point Position [x,y,z]: " + this.mPointPosition.ToString();
            string statusCubePos = "Cube Position [x,y,z]: " + this.mCubePosition.ToString();

            if (this.isInside)
                statusInsideCube += "YES";
            else
                statusInsideCube += "NO";

            spriteBatch.Begin();
            spriteBatch.DrawString(this._obj_font, statusInsideCube, new Vector2(20, 10), Color.Black, 0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(this._obj_font, statusPointPos, new Vector2(20, 30), Color.Black, 0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(this._obj_font, statusCubePos, new Vector2(20, 50), Color.Black, 0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(this._obj_font, "Made by: Daniel McAssey\nCMPDMCAS - 495652", new Vector2(20, 100), Color.Black, 0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.End();

            this._obj_graphics.BlendState = BlendState.Additive;
            foreach (ModelMesh localMesh in this.mPointModel.Meshes)
            {
                Matrix localWorld = this.mPointTransforms[localMesh.ParentBone.Index] * this.mPointWorld;
                Matrix localWorldInverseTranspose = Matrix.Transpose(Matrix.Invert(localWorld));

                foreach (BasicEffect localEffect in localMesh.Effects)
                {
                    localEffect.EnableDefaultLighting();
                    localEffect.World = localWorld;
                    localEffect.View = this._obj_camera.View;
                    localEffect.Projection = this._obj_camera.Projection;
                    localEffect.DiffuseColor = Vector3.Normalize(Color.Red.ToVector3());
                    localEffect.AmbientLightColor = Vector3.Normalize(Color.White.ToVector3());
                    localEffect.SpecularPower = 0f;

                    if (this.isInside)
                        localEffect.SpecularColor = Vector3.Normalize(Color.Red.ToVector3());
                    else
                        localEffect.SpecularColor = Vector3.Normalize(Color.White.ToVector3());
                    
                    localEffect.FogColor = Vector3.Normalize(Color.White.ToVector3());
                    localEffect.Alpha = 1f;
                }

                localMesh.Draw();
            }

            foreach (ModelMesh localMesh in this.mCubeModel.Meshes)
            {
                Matrix localWorld = this.mCubeTransforms[localMesh.ParentBone.Index] * this.mCubeWorld;
                Matrix localWorldInverseTranspose = Matrix.Transpose(Matrix.Invert(localWorld));

                foreach (BasicEffect localEffect in localMesh.Effects)
                {
                    localEffect.EnableDefaultLighting();
                    localEffect.World = localWorld;
                    localEffect.View = this._obj_camera.View;
                    localEffect.Projection = this._obj_camera.Projection;
                    localEffect.DiffuseColor = Vector3.Normalize(Color.DarkGray.ToVector3());
                    localEffect.Alpha = 0.55f;
                }

                localMesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
