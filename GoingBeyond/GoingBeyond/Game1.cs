using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoingBeyond
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        Model myModel;
        float aspectRatio;
        SoundEffect soundEngine;
        SoundEffectInstance soundEngineInstance;
        SoundEffect soundHyperspaceActivation;
        protected override void LoadContent()
        {
            //CreateanewSpriteBatch,whichcanbeusedtodrawtextures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            myModel = Content.Load<Model>("Models\\p1_wedge");
            soundEngine = Content.Load<SoundEffect>("Audio\\Waves\\engine_2");
            soundEngineInstance = soundEngine.CreateInstance();
            soundHyperspaceActivation =
            Content.Load<SoundEffect>("Audio\\Waves\\hyperspace_activate");
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
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
        Vector3 modelVelocity=Vector3.Zero;
        protected override void Update(GameTime gameTime)
        {
            //Allowsthegametoexit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            //Getsomeinput.
            UpdateInput();
            //Addvelocitytothecurrentposition.
            modelPosition += modelVelocity;
            //Bleedoffvelocityovertime.
            modelVelocity *= 0.95f;
            base.Update(gameTime);
        }

        protected void UpdateInput()
        {
            //Getthegamepadstate.
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);
            KeyboardState currentKeyState = Keyboard.GetState();
            //Rotatethemodelusingtheleftthumbstick,andscaleitdown
            if (currentKeyState.IsKeyDown(Keys.A))
                modelRotation += 0.10f;
            else if (currentKeyState.IsKeyDown(Keys.D))
                modelRotation -= 0.10f;
            else
                modelRotation -= currentState.ThumbSticks.Left.X * 0.10f;
                //Createsomevelocityiftherighttriggerisdown.
                Vector3 modelVelocityAdd = Vector3.Zero;
                //Findoutwhatdirectionweshouldbethrusting,
                //usingrotation.
                modelVelocityAdd.X = -(float)Math.Sin(modelRotation);
                modelVelocityAdd.Z = -(float)Math.Cos(modelRotation);
            //Nowscaleourdirectionbyhowhardthetriggerisdown.
            if (currentKeyState.IsKeyDown(Keys.W))
                modelVelocityAdd *= 1;
            else
                modelVelocityAdd *= currentState.Triggers.Right;
                //Finally,addthisvectortoourvelocity.
                modelVelocity += modelVelocityAdd;
                GamePad.SetVibration(PlayerIndex.One,
                currentState.Triggers.Right,
                currentState.Triggers.Right);

            if (currentState.Triggers.Right > 0 || currentKeyState.IsKeyDown(Keys.D) || currentKeyState.IsKeyDown(Keys.A))
            {
                if (soundEngineInstance.State == SoundState.Stopped)
                {
                    soundEngineInstance.Volume = 0.75f;
                    soundEngineInstance.IsLooped = true;
                    soundEngineInstance.Play();
                }
                else
                    soundEngineInstance.Resume();
            }
            else if (currentState.Triggers.Right == 0 || currentKeyState.IsKeyUp(Keys.D) || currentKeyState.IsKeyUp(Keys.A))
            {
                if (soundEngineInstance.State == SoundState.Playing)
                    soundEngineInstance.Pause();
            }

            if (currentState.Buttons.A == ButtonState.Pressed || currentKeyState.IsKeyDown(Keys.Enter))
            {
                modelPosition = Vector3.Zero;
                modelVelocity = Vector3.Zero;
                modelRotation = 0.0f;
                soundHyperspaceActivation.Play();
            }

            //Incaseyougetlost,pressAtowarpbacktothecenter.
            if (currentState.Buttons.A == ButtonState.Pressed || currentKeyState.IsKeyDown(Keys.Enter))
            {
                    modelPosition = Vector3.Zero;
                    modelVelocity = Vector3.Zero;
                    modelRotation = 0.0f;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        Vector3 modelPosition=Vector3.Zero;
        float modelRotation=0.0f;
        Vector3 cameraPosition=new Vector3(0.0f,50.0f,5000.0f);
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //Copyanyparenttransforms.
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);
            //Drawthemodel.Amodelcanhavemultiplemeshes,soloop.
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                //Thisiswherethemeshorientationisset,aswell
                //asourcameraandprojection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] *
                    Matrix.CreateRotationY(modelRotation)
                    * Matrix.CreateTranslation(modelPosition);
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                    Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(45.0f), aspectRatio,
                    1.0f, 10000.0f);
                }
                //Drawthemesh,usingtheeffectssetabove.
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
