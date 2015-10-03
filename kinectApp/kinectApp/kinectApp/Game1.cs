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
using Microsoft.Kinect;

using kinectApp.Entities;

namespace kinectApp
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D kinectRGBVideo;
        Texture2D jointMarker;
        Texture2D overlay;
        SpriteFont font;
        int screenHeight;
        int screenWidth;

        KinectSensor sensor;
        MultiSourceFrameReader _multiReader;
        string connectedStatus;
        byte[] _colorImageBuffer;
        bool _isDrawing;

        readonly EntityManager entityManager;
        Body[] _bodies;
        Joint[] _joints;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;

            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.PreferredBackBufferWidth = screenWidth;
            Content.RootDirectory = "Content";

            entityManager = new EntityManager();
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
            sensor = KinectSensor.GetDefault();
            sensor.IsAvailableChanged += KinectSensors_StatusChanged;

            sensor.Open();

            _multiReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);

            // Hook-up the frames arrived event
            _multiReader.MultiSourceFrameArrived += OnMultipleFramesArrivedHandler;

            for (int i = 0; i < 250; i++)
                entityManager.AddEntity(Entities.Germs.GermFactory.CreateSmallGerm());

            base.Initialize();
        }

        private void OnMultipleFramesArrivedHandler(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            // Retrieve multisource frame reference
            MultiSourceFrameReference multiRef = e.FrameReference;

            MultiSourceFrame multiFrame = multiRef.AcquireFrame();
            if (multiFrame == null) return;

            // Retrieve data stream frame references
            ColorFrameReference colorRef = multiFrame.ColorFrameReference;
            BodyFrameReference bodyRef = multiFrame.BodyFrameReference;

            using (ColorFrame colorFrame = colorRef.AcquireFrame())
            {
                using (BodyFrame bodyFrame = bodyRef.AcquireFrame())
                {
                    if (colorFrame == null || bodyFrame == null || _isDrawing) return;

                    _isDrawing = true;
                    int width = colorFrame.FrameDescription.Width;
                    int height = colorFrame.FrameDescription.Height;

                    if ((_colorImageBuffer == null) || (_colorImageBuffer.Length != width * height * /*colorImageFrame.FrameDescription.BytesPerPixel*/ 4))
                    {
                        _colorImageBuffer = new byte[width * height * /*colorImageFrame.FrameDescription.BytesPerPixel*/ 4];
                    }

                    colorFrame.CopyConvertedFrameDataToArray(_colorImageBuffer, ColorImageFormat.Rgba);

                    Color[] color = new Color[height * width];
                    kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, width, height);

                    // Go through each pixel and set the bytes correctly
                    // Remember, each pixel got a Red, Green and Blue
                    int index = 0;
                    for (int y = 0; y < width; y++)
                    {
                        for (int x = 0; x < height; x++)
                        {
                            Color c = new Color(_colorImageBuffer[index + 0], _colorImageBuffer[index + 1], _colorImageBuffer[index + 2], _colorImageBuffer[index + 3]);
                            color[y * height + x] = c;
                            index += 4;
                        }
                    }

                    // Set pixeldata from the ColorImageFrame to a Texture2D
                    kinectRGBVideo.SetData(color);

                    if (bodyFrame != null)
                    {
                        _bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];

                        bodyFrame.GetAndRefreshBodyData(_bodies);

                        foreach (var body in _bodies)
                        {
                            if (body != null)
                            {
                                if (body.IsTracked)
                                {
                                    // Find the joints
                                    Joint handRight = body.Joints[JointType.HandRight];
                                    Joint thumbRight = body.Joints[JointType.ThumbRight];

                                    Joint handLeft = body.Joints[JointType.HandLeft];
                                    Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                    if (_joints == null || _joints.Length != 4)
                                    {
                                        _joints = new Joint[4];
                                    }

                                    _joints[0] = handRight;
                                    _joints[1] = thumbRight;
                                    _joints[2] = handLeft;
                                    _joints[3] = thumbLeft;
                                }
                            }
                        }
                    }

                    this.BeginDraw();
                }


            }

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            kinectRGBVideo = new Texture2D(GraphicsDevice, 1920, 1080);

            jointMarker = new Texture2D(GraphicsDevice, 50, 50);
            Color[] data = new Color[50 * 50];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Green;
            jointMarker.SetData(data);

            overlay = Content.Load<Texture2D>("overlay");
            font = Content.Load<SpriteFont>("SpriteFont1");

            // TODO: use this.Content to load your game content here#
            entityManager.Load(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            sensor.Close();
            entityManager.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here

            entityManager.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 1900, 1000), Color.White);
            //spriteBatch.Draw(overlay, new Rectangle(0, 0, 640, 480), Color.White);

            if (_joints != null)
            {
                int i = 0;
                foreach (Joint joint in _joints)
                {
                    spriteBatch.Draw(jointMarker, new Rectangle(100 + i, 150, 10, 10), Color.Green);
                    i += 20;
#if DEBUG
                    Console.WriteLine("Joint at " + joint.Position.X + ", " + joint.Position.Y);
#endif
                }
            }

            spriteBatch.DrawString(font, connectedStatus, new Vector2(20, 80), Color.White);
            entityManager.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            // TODO: Add your drawing code here


            base.Draw(gameTime);
        }

        protected override void EndDraw()
        {
            _isDrawing = false;
            base.EndDraw();
        }

        private void KinectSensor_BodySourceFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (_isDrawing) return;
            _isDrawing = true;
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];
                            }
                        }
                    }
                }
            }
            this.BeginDraw();

        }

        private void kinectSensor_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            if (_isDrawing) return;
            _isDrawing = true;
            using (ColorFrame colorImageFrame = e.FrameReference.AcquireFrame())
            {
                if (colorImageFrame != null)
                {
                    int width = colorImageFrame.FrameDescription.Width;
                    int height = colorImageFrame.FrameDescription.Height;

                    if ((_colorImageBuffer == null) || (_colorImageBuffer.Length != width * height * /*colorImageFrame.FrameDescription.BytesPerPixel*/ 4))
                    {
                        _colorImageBuffer = new byte[width * height * /*colorImageFrame.FrameDescription.BytesPerPixel*/ 4];
                    }

                    colorImageFrame.CopyConvertedFrameDataToArray(_colorImageBuffer, ColorImageFormat.Rgba);

                    Color[] color = new Color[height * width];
                    kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, width, height);

                    // Go through each pixel and set the bytes correctly
                    // Remember, each pixel got a Red, Green and Blue
                    int index = 0;
                    for (int y = 0; y < width; y++)
                    {
                        for (int x = 0; x < height; x++)
                        {
                            Color c = new Color(_colorImageBuffer[index + 0], _colorImageBuffer[index + 1], _colorImageBuffer[index + 2], _colorImageBuffer[index + 3]);
                            color[y * height + x] = c;
                            index += 4;
                        }
                    }

                    // Set pixeldata from the ColorImageFrame to a Texture2D
                    kinectRGBVideo.SetData(color);
                    this.BeginDraw();
                }
            }
        }

        private void KinectSensors_StatusChanged(object sender, IsAvailableChangedEventArgs e)
        {
            connectedStatus = e.IsAvailable ? "Sensor is available." : "**Sensor is not available**";
        }
    }
}
