using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace ReinforcementJump;

public class Game1 : Game
{
    //Graphics
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public Player PlayerInstance;
    public Entity SelectedInstance;
    public Game GameObject;
    public Vector2 WindowResolution; 
    SpriteFont font1;
    Line LineTest;
    public Texture2D PixelTexture;


    //Instance Info
    public readonly object SpriteListLock = new object();
    Texture2D texture;
    Texture2D PlayerTexture;
    public List<Sprite> SpriteList;
    public int Hits = 0;
    public float Zoom = 100;
    public int ID = 0; // ID for the entity, used to identify the entity in the list
    public int LineLength = 500;

    //Neural Network
    public NeuralNetwork PlayerNeuralNetwork = new(10, 5, 3);
    public List<Line> GeometryList = new List<Line>();
    public List<Line> RaycastList = new();
    public double[] RaycastDistances = new double[10];
    public List<DataPoint> CurrentData = new();

    bool KeyPressed = false;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        _graphics.PreferredBackBufferHeight = 800;
        _graphics.PreferredBackBufferWidth = 800;
       // _graphics.IsFullScreen = true;

        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        Content.RootDirectory = "Content";
        
        texture = Content.Load<Texture2D>("OIP");
        PixelTexture = new Texture2D(GraphicsDevice, 2, 2);
    // ImageData.AcquireData(Content, GraphicsDevice);
    // texture = Content.Load<Texture2D>("FaceDrawing");
    //  PlayerTexture = Content.Load<Texture2D>("Sunny");
    // Content.RootDirectory = "Content";
        font1 = Content.Load<SpriteFont>("Fonts/Montserrat");
        SpriteList = new List<Sprite>();
        Dictionary<string, float> Traits = new Dictionary<string, float>(); // Works like roblox dictionaries. <string> refers to the key type and <float> refers to the type of the values of the dictionary
        Traits.Add("Size", 1f);
        PlayerTexture = new Texture2D(GraphicsDevice, 100, 100);
        PlayerInstance = new Player(PlayerTexture, new Vector2(-50,-50), (3.5f * 1), Traits, 1, 1, "Player", this);
        GameObject = this;
        lock (SpriteListLock)
        {
            SpriteList.Add(PlayerInstance);
            // or Game1.SpriteList.Remove(entity);
        }
        SpriteList.Add(new Obstacle(texture, new Vector2(350, 1000), (0.4f), Traits, 1, 1, "Yes", this));
        int x = 2;
        for (int i = 1; i < 1; i++)
        {
            // NewTraits.Clear();
            // NewTraits.Add("Size", 0.1f);
            SpriteList.Add(new Obstacle(texture, new Vector2(350 * i, 1000), (0.4f), Traits, 1, 1, "Yes", this));
        }

        //test please delete
        double[] OutputsRecieved = new double[] { 1, 2, 3 };
        Random RandomNumber = new();
        double RandomValue = RandomNumber.NextDouble();
        double[] AccumulatedOutputs = new double[OutputsRecieved.Length];
        double OutputsSum = OutputsRecieved.Sum();
        for (int i = 0; i < OutputsRecieved.Length; i++)
        {
            OutputsRecieved[i] = OutputsRecieved[i] / OutputsSum;
            for (int a = 0; a < i + 1; a++)
            {
                AccumulatedOutputs[i] += OutputsRecieved[a];
            }
        }
        string man = "";
        for (int i = 0; i < OutputsRecieved.Length; i++)
        {
           man += OutputsRecieved[i] + "       ";

        }
        Debug.WriteLine(man);

        Thread.Sleep(x * 100); // Sleep for 1 second
        Thread NewThread = new Thread(GameRun);
        NewThread.Start();


    }
    public void ResetGame()
    {
        PlayerInstance.Health = 100;
        List<Sprite> snapshot;
        lock (SpriteListLock)
        {
            snapshot = [.. SpriteList]; // no idea what this does but its apparently the same thing as new List<Sprite>(SpriteList);
        }
        foreach (Sprite sprite in snapshot)
        {
            if (sprite != PlayerInstance)
            {
                SpriteList.Remove(sprite);
            }
        }
        PlayerInstance.Position = new Vector2(-50, -50);
    }
    public void RunGame()
    {
        for (int i = 1; i < 2000; i++)
        {
            Random random = new();
            int X = random.Next(0, 4);
            Dictionary<string, float> Traits = new Dictionary<string, float>();
            Traits.Add("Size", 1f);
            SpriteList.Add(new Obstacle(texture, new Vector2(460 * i, X * 200 - 100), (0.4f), Traits, 1, 1, "Yes", this));
            Thread.Sleep(1000);
        }
    }
    protected void GameRun()
    {
        Thread NewThread = new Thread(() => Entity.Run(this));
        NewThread.Start();
        Dictionary<string, float> Traits = new Dictionary<string, float>();
        Traits.Add("Size", 1f);

        Dictionary<string, float> Traits2 = new Dictionary<string, float>();
        Traits2.Add("Size", 0.2f);

        Dictionary<string, float> Traits3 = new Dictionary<string, float>();
        Traits3.Add("Size", 0.2f);
        //   Texture2D DefaultFaceTexture = ImageData.Images["FaceDrawing"];
        //       Entity NewEntity2 = new Entity(DefaultFaceTexture, new Vector2(9000, 0 + 350), (0.5f), Traits2, 1, Game1.ID, "Mr. Plow");
        Vector2[] Quadrants =
            {   new Vector2(0, 0),
                new Vector2(900, 0),
                new Vector2(900, 900),
                new Vector2(0, 900)
            };



         LineLength = 500;
        int AngleGap = 15;
        int LineCount = 10;
        for (int i = 0; i < LineCount; i++)
        {
            int Angle = ((LineCount - 1) * AngleGap) / 2 - i * AngleGap;
            Vector2 TranslatedPos = new Vector2(
                (float)Math.Cos(Angle * Math.PI/180) * LineLength, (float)Math.Sin(Angle * Math.PI/180) * LineLength
                );
            LineTest = new Line(new Vector2(0, 0), TranslatedPos, PlayerInstance);
            GeometryList.Add(LineTest);
            RaycastList.Add(LineTest);
            List<Sprite> snapshot;

        }

        
     //   LineTest = new Line(new Vector2(0, 0), new Vector2(100, 100), PlayerInstance);
    //    GeometryList.Add(LineTest);
      //  var (PositionWithLowerX, PositionWithHigherX) = (LineTest.Position1.X < LineTest.Position2.X) ? (LineTest.Position1.X, LineTest.Position2.X) : (LineTest.Position2.X, LineTest.Position1.X);
        //  Debug.WriteLine(PositionWithLowerX + " " + PositionWithHigherX);
        for (int i = 1; i < 2; i++)
        {
            GeometryList.Add(new LineObstacle(new Vector2 (0,600), new Vector2(300000, 600)));
            GeometryList.Add(new LineObstacle(new Vector2(0, -200), new Vector2(300000, -200)));
            Thread.Sleep(200);
        }
        Thread GameRunning = new Thread(() => RunGame());
        GameRunning.Start();
    }
    protected List<Line> GetLinesToHitbox()
    {
        List<Sprite> snapshot;
        lock (SpriteListLock)
        {
            snapshot = [.. SpriteList]; // no idea what this does but its apparently the same thing as new List<Sprite>(SpriteList);
        }
        List<Line> ListOfLines = new();
        foreach (Sprite sprite in snapshot)
        {
            if (sprite == PlayerInstance)
            {
                continue;
            }
            GeometryRect Rect = sprite.Rect;
            Line[] Lines = Rect.GetLines();
            foreach (Line line in Lines)
            {
                ListOfLines.AddRange(Lines);
            }
        }
        foreach (Line LO in GeometryList)
        {
            if (LO is LineObstacle)
            {
                ListOfLines.Add(LO);
            }
        }
        return ListOfLines;
    }
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        int MouseX = Mouse.GetState().X;
        int MouseY = Mouse.GetState().Y;
        Vector2 MouseCoordinates = new Vector2(MouseX, MouseY);
        //Debug.WriteLine(MouseX + " " + MouseY);
        WindowResolution = new(this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);
        List<Sprite> snapshot;
        lock (SpriteListLock)
        {
            snapshot = new List<Sprite>(SpriteList);
        }
        foreach (Sprite sprite in snapshot)
        {
            sprite.Update(gameTime);
        }
        PlayerInstance.Update(gameTime);
        // TODO: Add your update logic here
        if (!KeyPressed && Keyboard.GetState().IsKeyDown(Keys.Q))
        {
            KeyPressed = true;
            /*
            List<Sprite> SpriteList = Sprite.GetSpritesInArea(PlayerInstance.CentrePos, 80, PlayerInstance.EnemyTeams);
            foreach(Sprite SpriteInstance in SpriteList)
            {
                Console.WriteLine(SpriteInstance);
                SpriteInstance.Health -= 10;
            }
            */
        }
        //   Zoom -= 0.1f;
        Dictionary<string, float> Traits = new Dictionary<string, float>(); // Works like roblox dictionaries. <string> refers to the key type and <float> refers to the type of the values of the dictionary
        if (Keyboard.GetState().IsKeyDown(Keys.Q))
        {
            KeyPressed = false;
        }

        lock (SpriteListLock)
        {
            snapshot = [.. SpriteList]; // no idea what this does but its apparently the same thing as new List<Sprite>(SpriteList);
        }
        List<Line> ListOfLines = GetLinesToHitbox();
        for (int i = 0; i < RaycastList.Count; i++)
        {
            Line RaycastLine = RaycastList[i];
            double ClosestRaycastDistance = LineLength;
            Vector2 ClosestRaycastPoint = RaycastLine.Position1 + RaycastLine.MaxP2Offset;
            foreach (Line line in ListOfLines)
            {
                var (Success, HitPoint) = Line.FindIntersection(RaycastLine, line);
                if (Success)
                {
                    //Position1 is the origin of the line
                   // Debug.WriteLine(HitPoint);
                    double Distance = Math.Sqrt(Math.Pow(HitPoint.X - RaycastLine.Position1.X, 2) + Math.Pow(HitPoint.Y - RaycastLine.Position1.Y, 2));
                    if (Distance <= ClosestRaycastDistance)
                    {
                        ClosestRaycastDistance = Distance;
                        ClosestRaycastPoint = HitPoint;
                    }
                }
            }
            RaycastLine.Position2 = ClosestRaycastPoint;
            RaycastDistances[i] = ClosestRaycastDistance/LineLength;
        }
        //SpriteHandler.DisposeSprites(SpriteList);
        try
        {
            List<Sprite> snapshotlist = new List<Sprite>(SpriteList);
            lock (SpriteListLock)
            {
                snapshotlist = new List<Sprite>(SpriteList);
            }
            foreach (Sprite Entity in snapshotlist)
            {
                if (Entity.Health <= 0)
                {
                    if (Entity != PlayerInstance)
                    {
                        SpriteList.Remove(Entity);
                    }
                }
            }
        }
        catch(Exception exception)
        {
          //  Debug.WriteLine(exception);
        }
        if (PlayerInstance.Position.X > 5000)
        {
            PlayerNeuralNetwork.AccumulatedRewards += 100;
            ResetGame();
            Thread GameRunning = new Thread(() => RunGame());
            GameRunning.Start();
        }

        base.Update(gameTime);
    }
    public static bool CharacterExists(SpriteFont Font, char character)
    {
        if (!Font.Characters.Contains(character) && character != '\r' && character != '\n') { return true; }
        else
        {
            return false;
        }
    }
    public static Vector2 GetCentrePos(Rectangle Rect)
    {
        // return new Vector2(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2);
        return new Vector2(Rect.X, Rect.Y);
    }
   public static List<Vector2> RotatePointsAroundAxis(List<Vector2> Points, Vector2 Axis, float Angle)
    {
        List<Vector2> NewPoints = new List<Vector2>();
        foreach (Vector2 Point in Points)
        {
            //  float X = (float)(Axis.X + (Point.X - Axis.X) * Math.Cos(Angle) - (Point.Y - Axis.Y) * Math.Sin(Angle));
            // float Y = (float)(Axis.Y + (Point.X - Axis.X) * Math.Sin(Angle) + (Point.Y - Axis.Y) * Math.Cos(Angle));
            double Distance = Math.Sqrt(Math.Pow(Point.X - Axis.X, 2) + Math.Pow(Point.Y - Axis.Y, 2)); // please finish this
            float X = (float)(Distance - (Point.Y - Axis.Y) * Math.Sin(Angle));
            float Y = (float)(Axis.Y + (Point.X - Axis.X) * Math.Sin(Angle) + (Point.Y - Axis.Y) * Math.Cos(Angle));
            NewPoints.Add(new Vector2(X, Y));
        }
        return NewPoints;
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Azure);

        // TODO: Add your drawing code here
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        List<Sprite> snapshot;
        List<Line> LineSnapshot;
        lock (SpriteListLock)
        {
            snapshot = new List<Sprite>(SpriteList);
            LineSnapshot = new List<Line>(GeometryList);
        }
        // snapshot.Add(PlayerInstance);
        Vector2 centrePos = PlayerInstance.CentrePos;
        Vector2 Centre = new Vector2(WindowResolution.X / 2, WindowResolution.Y / 2);
        int OffsetX = (int)-centrePos.X + (int)Centre.X;
        int OffsetY = (int)-centrePos.Y + (int)Centre.Y;
        void DrawLine(Line line)
        {
            double Iterations = 0;
            Color ColourToDraw = Color.Crimson;
            if (line.IsHighlighted)
            {
                ColourToDraw = Color.Blue;
            }
            if (!line.IsVertical == true)
            {
                if (Math.Abs(line.Gradient) < 1)
                {
                    //Get position with lower x
                    var (PositionWithLowerX, PositionWithHigherX) = (line.Position1.X < line.Position2.X) ? (line.Position1.X, line.Position2.X) : (line.Position2.X, line.Position1.X);
                    for (int StartX = (int)PositionWithLowerX; StartX < PositionWithHigherX; StartX++)
                    {

                        Texture2D Pixel = PixelTexture;
                        int XPos = StartX + OffsetX;
                        int YPos = (int)(line.Gradient * StartX + line.YIntercept + OffsetY);
                        Rectangle newRect = new(XPos, YPos, 3, 3);
                        double DistanceFromPlayer = Math.Sqrt(Math.Pow(XPos - PlayerInstance.Position.X, 2) + Math.Pow(YPos - PlayerInstance.Position.Y, 2));
                        Iterations++;
                        if (DistanceFromPlayer > 10000)
                        {
                            return;
                        }
                        _spriteBatch.Draw(texture, newRect, new Rectangle(0, 0, 3, 3), ColourToDraw);
                    }

                }
                else
                {
                    //Get position with lower y
                    var (PositionWithLowerY, PositionWithHigherY) = (line.Position1.Y < line.Position2.Y) ? (line.Position1.Y, line.Position2.Y) : (line.Position2.Y, line.Position1.Y);
                    for (int StartY = (int)PositionWithLowerY; StartY < PositionWithHigherY; StartY++)
                    {
                        Texture2D Pixel = PixelTexture;
                        //x = (y-b)/m
                        int XPos = (int)((StartY - line.YIntercept) / line.Gradient) + OffsetX;
                        int YPos = StartY + OffsetY;
                        Rectangle newRect = new(XPos, YPos, 3, 3);
                        _spriteBatch.Draw(texture, newRect, new(0, 0, 3, 3), Color.Crimson);
                    }
                    Iterations++;
                    if (Iterations > line.DistanceLimit)
                    {
                        return;
                    }
                }
            }
            else
            {
                double MaxY = Math.Max(line.Position1.Y, line.Position2.Y);
                double MinY = Math.Min(line.Position1.Y, line.Position2.Y);
                double Difference = MaxY - MinY;
                for (int i = 0; i < Difference; i++)
                {

                    Texture2D Pixel = PixelTexture;
                    int XPos = (int)line.Position1.X + OffsetX;
                    int YPos = (int)MaxY - i + OffsetY;
                    Rectangle newRect = new(XPos, YPos, 3, 3);
                    _spriteBatch.Draw(texture, newRect, new Rectangle(0, 0, 3, 3), Color.Crimson);
                    double DistanceFromPlayer = Math.Sqrt(Math.Pow(XPos - PlayerInstance.Position.X, 2) + Math.Pow(YPos - PlayerInstance.Position.Y, 2));
                    Iterations++;
                    if (Iterations > line.DistanceLimit || DistanceFromPlayer > 1000)
                    {
                        return;
                    }
                }
            }
           
        }
        foreach (Sprite sprite in snapshot)
        {
            if (true == true)
            {
                Color DrawColour = Color.Tomato;



                //Check if sprite is an entity Instance
                //Convert Sprite rectangle width to screen widthaaa
                int SpriteWidth = (int)(sprite.Rect.Width * Zoom / 100);
               // Debug.WriteLine(sprite.Rect.Width);
               // Debug.WriteLine(sprite.Position.X);
            //    Debug.WriteLine(centrePos.X);
              //  Debug.WriteLine((int)sprite.Rect.Position.X - sprite.Rect.Width / 2 * 1 - (int)centrePos.X + 0);
                DrawColour = Color.Green;

                Rectangle newRect = new Rectangle((int)sprite.Rect.Position.X - sprite.Rect.Width / 2 * 1 + OffsetX, (int)sprite.Rect.Position.Y - sprite.Rect.Height / 2 * 1 + OffsetY, sprite.Rect.Width, sprite.Rect.Height);
                //  _spriteBatch.Draw(sprite.Texture, newRect, new Rectangle(0, 0, sprite.Texture.Width, sprite.Texture.Height),  DrawColour, sprite.Rect.Rotation, new Vector2(sprite.Rect.Width/2, sprite.Rect.Height / 2), SpriteEffects.None, 0.0f);
                _spriteBatch.Draw(texture, newRect, new Rectangle(0, 0, sprite.Texture.Width, sprite.Texture.Height), DrawColour);
                // _spriteBatch.Draw(sprite.Texture, sprite.Rect, new Rectangle(0, 0, sprite.Texture.Width, sprite.Texture.Height), Color.Crimson, (float)sprite.Rotation, GetCentrePos(sprite.Rect), SpriteEffects.None, 0.0f);
                // _spriteBatch.Draw(sprite.Texture, sprite.Position, sprite.Rect, Color.White, 9.5f, sprite.Position, 1, SpriteEffects.None, 2);
                Line[] SpriteLines = sprite.Rect.GetLines();
                foreach (Line line in SpriteLines)
                {
                    DrawLine(line);
                }
            }
        }

        foreach (Line line in LineSnapshot)
        {
            DrawLine(line);
        }
        string MagnitudeString = "Magnitudes: ";
        double[] Inputs = RaycastDistances;
        for (int i = 1; i < Inputs.Length; i++)
        {
            MagnitudeString += (double)((int)(Inputs[i] * 10))/10;
            if (i < Inputs.Length - 1)
            {
                MagnitudeString += ", ";
            }
        }

        Vector2 TextPosition = font1.MeasureString("Health") / 2 + new Vector2(WindowResolution.X / 15, WindowResolution.Y / 15);
        Vector2 Pos = PlayerInstance.CentrePos;
        //  _spriteBatch.DrawString(font1, "Health: " + SelectedInstance.Health, TextPosition, Color.Crimson);
        Vector2 SelectedPos = PlayerInstance.Position;
        float Magnitude = (float)(Math.Sqrt(Math.Pow(Pos.X - SelectedPos.X, 2) + Math.Pow(Pos.Y - SelectedPos.Y, 2)));
        _spriteBatch.DrawString(font1, "Coords 1: " + Pos.X + ", " + Pos.Y, new Vector2(TextPosition.X, TextPosition.Y + 40), Color.Crimson);
        _spriteBatch.DrawString(font1, PlayerInstance.Health.ToString(), new Vector2(TextPosition.X, TextPosition.Y + 80), Color.Crimson);
      //  _spriteBatch.DrawString(font1, "Velocity: " + PlayerInstance.PreviousVelocity.X + ", " + PlayerInstance.PreviousVelocity.Y, new Vector2(TextPosition.X, TextPosition.Y + 100), Color.Crimson);

        string OutputsString = "Outputs: ";
        for (int i = 0; i < PlayerNeuralNetwork.PastOutput.Length; i++)
        {
            double output = PlayerNeuralNetwork.PastOutput[i];
            OutputsString += (double) ((int)(output * 1000)) / 1000;
            if (i < PlayerNeuralNetwork.PastOutput.Length - 1)
            {
                OutputsString += ", ";
            }
        }
        double AverageWeight = 0;
        int Dividend = 0;
        for (int i = 0; i < PlayerNeuralNetwork.Layers.Length; i++)
        {
            Layer layer = PlayerNeuralNetwork.Layers[i];
            for (int j = 0; i < layer.weights.GetLength(0); i++)
            {
                for (int k = 0; i < layer.weights.GetLength(1); i++)
                {
                    AverageWeight += layer.weights[j, k];
                   // Debug.WriteLine(layer.weights[j, k]);
                }
            }
        }
        _spriteBatch.DrawString(font1, MagnitudeString, new Vector2(TextPosition.X, TextPosition.Y + 100), Color.Crimson);
        _spriteBatch.DrawString(font1, OutputsString, new Vector2(TextPosition.X, TextPosition.Y + 120), Color.Crimson);

        _spriteBatch.DrawString(font1, (Dividend).ToString(), new Vector2(TextPosition.X, TextPosition.Y + 140), Color.Crimson);
        base.Draw(gameTime);
        _spriteBatch.End();
    }
    private void OnClientSizeChangedCallback(object sender, EventArgs e)
    {
        WindowResolution = new(this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);
    }
}
