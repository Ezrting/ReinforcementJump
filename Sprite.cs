using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReinforcementJump;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Security.Cryptography;

public class Sprite
{
    public Texture2D Texture;
    public Vector2 _Position;
    public Vector2 Position
    {
        get
        {
            return _Position;
        }
        set
        {
           if (_Position != value)
           {
               _Position = value;
           }
           if (GameRunning == null) return;
           foreach (Line line in GameRunning.GeometryList)
           {
                if (line.AttachedPlayer == this)
                {
                    Vector2 CentrePos = this.Position;
                    Vector2 Difference = line.Position2 - line.Position1;
                    line.Position1 = CentrePos;
                    line.Position2 = CentrePos + Difference;
                }
           }
        }
    }
    public static float SCALE = 1f;
    public float Speed;
    public float Size = 1f;
    private readonly float BaseSpeed;
    public float TurnSpeed = 0.1f;
    public double Rotation = 0;
    public List<GeometryRect> HitboxList = new List<GeometryRect>();
    public Vector2 Difference = new Vector2(0, 0);
    public int Team;
    public float Health = 100;
    public Dictionary<string, List<Texture2D>> Animations;
    public Game1 GameRunning;
    public GeometryRect Rect
    {
        get
        {
            float Zoom = GameRunning.Zoom;
            Vector2 Pos = GameRunning.PlayerInstance.CentrePos;
            Vector2 Centre = Pos;
            int BaseX = (int)Position.X;// - (int)Pos.X + (int)Centre.X;
            int BaseY = (int)Position.Y;// - (int)Pos.Y + (int)Centre.Y;

            Vector2 DifferenceToCentre = new Vector2(BaseX, BaseY) - Centre;
            Vector2 RectPos = new Vector2((int)(Centre.X + (DifferenceToCentre.X * Zoom / 100)), (int)(Centre.Y + (DifferenceToCentre.Y * Zoom / 100)));
            return new GeometryRect(Position, Texture.Width * (int)(SCALE * Size * 100 * Zoom / 100) / 100, Texture.Height * (int)(SCALE * Size * 100 * Zoom / 100) / 100, (float)0);
            //  return new Rectangle(Centre.X + (DifferenceToCentre.X * Zoom / 100)), (int)(Centre.Y + (DifferenceToCentre.Y * Zoom / 100)), Texture.Width * (int)(SCALE * 100 * Zoom / 100) / 100, Texture.Height * (int)(SCALE * 100 * Zoom / 100) / 100);
        }
    }

    public Sprite(Texture2D texture, Vector2 position, float speed, Dictionary<string, float> Traits, int team, Game1 gameRunning)
    {
        Texture = texture;
        Position = position;
        Speed = speed;
        BaseSpeed = speed;
        Team = team;
        GameRunning = gameRunning;
        float SizeValue = 1;
        if (Traits.TryGetValue("Size", out SizeValue)) // out keyword lets you pass variables by reference rather than type(used here to change a variable's value instead of the number itself which the variable holds, cause thats impossible)
        {
            Size = SizeValue;
        }

        GameRunning = gameRunning;
    }
    public virtual void Update(GameTime gameTime)
    {
        Random random = new Random();
        int R = random.Next(1, 50);
        Vector2 PlayerPos = GameRunning.PlayerInstance.CentrePos;
        double Distance = Math.Sqrt(Math.Pow(PlayerPos.X - Position.X, 2) + Math.Pow(PlayerPos.Y - Position.Y, 2));
        Vector2 DesiredDifference = PlayerPos - Position;
        DesiredDifference.Normalize();
        Vector2 TurnDifference = DesiredDifference - Difference;
        Difference = DesiredDifference - new Vector2(TurnDifference.X / (1f + (TurnSpeed / 5)), TurnDifference.Y / (1f + (TurnSpeed / 5)));
        Difference.Normalize();
        //  Rotation = Math.Atan((Difference.Y) / (Difference.X)) * 180 / Math.PI;
        Vector2 NewPosition = Position;
        NewPosition.Y += (float)Difference.Y * Speed;
        NewPosition.X += (float)Difference.X * Speed;
        Position = NewPosition;
        if (PlayerPos.X - NewPosition.X > 1000)
        {
            List<Sprite> snapshot;
            lock (GameRunning.SpriteListLock)
            {
                snapshot = new List<Sprite>(GameRunning.SpriteList);
            }
            Health = 0;
        }
        /*
        if (Game1.PlayerInstance.Health > 0 && Rect.Intersects(Game1.PlayerInstance.Rect, Game1.PlayerInstance.Rect) && Health > 0)
        {
            Health = 0;
            Game1.PlayerInstance.Health -= 8;
            Game1.Hits += 1;
        }
        */

        // Position.Y += (float)Math.Sin(DesiredAngle) * Speed;
        // Position.X += (float)Math.Cos(DesiredAngle) * Speed;


        //Debug.WriteLine(DesiredAngle);
        //  Debug.WriteLine((float)Math.Sin(DesiredAngle) * Speed);
        // Debug.WriteLine((float)Math.Cos(DesiredAngle) * Speed);
        // Speed = (float)Distance / 1000 * BaseSpeed;
        // Debug.WriteLine((PlayerPos.Y - Position.Y) + " " + Distance);
        /*
         switch (R)
         {
             case 1:
                 break;
             case 2:
                 break;
             case 3:
                 break;
             case 40:
                 Position.Y -= (float)Math.Sin(Angle) * 50;
                 Position.X -= (float)Math.Cos(Angle) * 50;
               //  Debug.WriteLine("BAM!");
                 break;
             default:
                 break;
         }
         switch(R)
         {
             case 1:
                 Position.X += Speed;
                 break;
             case 2:
                 Position.X -= Speed;
                 break;
             case 3:
                 Position.Y += Speed;
                 break;
             case 4:
                 Position.Y -= Speed;
                 break;
         }
         */
    }
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Rectangle Rect = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        spriteBatch.Draw(Texture, Position, Rect, Color.White, 9.5f, new Vector2(300, 300), 1, SpriteEffects.None, 1);
    }
    public static List<Sprite> GetSpritesInArea(Game1 GameRunning, Vector2 Position, float Radius, List<int> TeamsToCheck)
    {

        List<Sprite> snapshot;
        lock (GameRunning.SpriteListLock)
        {
            snapshot = new List<Sprite>(GameRunning.SpriteList);
        }
        List<Sprite> SpriteList = new List<Sprite>();
        foreach (Sprite SpriteInstance in snapshot)
        {
            bool Added = false;
            float magnitude = (float)Math.Sqrt(Math.Pow(SpriteInstance.Position.X - Position.X, 2) + Math.Pow(SpriteInstance.Position.Y - Position.Y, 2));
            if (magnitude < Radius)
            {
                Type type = SpriteInstance.GetType();
                Added = true;
                for (int i = 0; i < TeamsToCheck.Count; i++)
                {
                    if (TeamsToCheck[i] < 0 || TeamsToCheck[i] > 4)
                    {
                        throw new ArgumentOutOfRangeException("Team value must be between 0 and 4.");
                    }
                    //   Debug.WriteLine(TeamsToCheck[i].ToString() + " and " + SpriteInstance.Team.ToString());
                    if (SpriteInstance.Team == TeamsToCheck[i] && SpriteInstance.Health > 0)
                    {
                        SpriteList.Add(SpriteInstance);
                    }
                }
            }
            if (Added == false && Radius > 5000)
            {
                //  Debug.WriteLine("Sprite not added: " + SpriteInstance.GetType().Name + " at position " + SpriteInstance.Position + " with team " + SpriteInstance.Team + " and health " + SpriteInstance.Health);
            }
        }
        return SpriteList;
    }
#nullable enable
    public static (Sprite[], Line[]) GetCollidingObjects(Game1 GameRunning, Sprite SpriteToCheck)
    {
        List<Sprite> snapshot;
        lock (GameRunning.SpriteListLock)
        {
            snapshot = new List<Sprite>(GameRunning.SpriteList);
        }
        List<Sprite> CollidingObjects = new();
        List<Line> IntersectingLineObstacles = new();
        GeometryRect RectToCheck = SpriteToCheck.Rect;
        Line[] RectLines = RectToCheck.GetLines();
        for (int i = 0; i < RectLines.Length; i++)
        {
            Line RaycastLine = RectLines[i];
            Vector2 ClosestRaycastPoint = RaycastLine.Position1 + RaycastLine.MaxP2Offset;
            foreach (Line line in GameRunning.GeometryList)
            {
                if (line is LineObstacle)
                {
                    if (Line.FindIntersection(RaycastLine, line).Item1)
                    {
                        GameRunning.PlayerInstance.Health -= 0.1f;
                        IntersectingLineObstacles.Add(line);
                    }
                }
            }
            foreach (Sprite sprite in snapshot)
            {
                if (sprite == SpriteToCheck)
                {
                    continue;
                }
                if (CollidingObjects.Contains(sprite))
                {
                    continue;
                }
                if (sprite.Rect.Rotation == 0 && SpriteToCheck.Rect.Rotation == 0)
                {
                    List<Vector2> Vertices1 = sprite.Rect.GetVertices();
                    Vector2 Top1 = Vertices1[0];
                    Vector2 Bottom1 = Vertices1[3];
                    List<Vector2> Vertices2 = RectToCheck.GetVertices();
                    Vector2 Top2 = Vertices2[0];
                    Vector2 Bottom2 = Vertices2[3];
                    if (Top1.X !< Bottom2.X && Bottom1.X !> Top2.X &&
                        Top1.Y !< Bottom2.Y && Bottom1.Y !> Top2.Y)
                    {
                        CollidingObjects.Add(sprite);
                    }
                }
                else
                {
                    if (SpriteToCheck.Rect.Intersects(SpriteToCheck.Rect, sprite.Rect))
                    {
                        CollidingObjects.Add(sprite);
                    }
                }
            }
        }
        return ([.. CollidingObjects],[.. IntersectingLineObstacles]);
    }
    public static Sprite? GetClosestSprite(Game1 GameRunning, Vector2 Position, float Radius, List<int> Teams)
    {
        List<Sprite> Contenders = GetSpritesInArea(GameRunning, Position, Radius, Teams);
        //  Debug.WriteLine("Contenders: " + Contenders.Count);
        float ClosestMagnitude = 100000;
        //  Debug.WriteLine("rUNNING");
        Sprite? ClosestContender = null;
        for (int i = 0; (i < Contenders.Count); i++)
        {
            Vector2 ContenderPosition = Contenders[i].Position;
            float Magnitude = (float)(Math.Sqrt(Math.Pow(Position.X - ContenderPosition.X, 2) + Math.Pow(Position.Y - ContenderPosition.Y, 2)));
            if (Magnitude < ClosestMagnitude)
            {
                ClosestMagnitude = Magnitude;
                ClosestContender = Contenders[i];
            }
        }
        return ClosestContender;
    }
    public static void PlayAnimation(Sprite sprite, string AnimationName)
    {

    }
}