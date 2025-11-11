using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ReinforcementJump
{
    public class Obstacle : Sprite
    {
        const double sixtieth = 1 / 60.0;
        public Vector2 CentrePos;
        public bool DashOnCooldown = false;
        List<Vector2> Forces = new List<Vector2>();
        int MoveForceIndex = 0;
        bool IsMoving = false;
        Obstacle CurrentAttackTarget;
        Obstacle CurrentMoveTarget;
        List<int> EnemyTeams = new List<int> { 1, 2, 3, 4 }; // Example teams, adjust as needed
        float ERange = 60;
        int ID;
        float ECooldown = 0;
        float Weight = 1f;
        string TextureString;
        int Ammo = 10;
        int MaxAmmo = 10;
        public string ObstacleType; // Default Obstacle name, can be changed later
        public Shape[] Hitboxes;
        // Remove the current team from the enemy teams list
        public Obstacle(Texture2D Texture, Vector2 Position, float Speed, Dictionary<string, float> Traits, int Team, int iD, string obstacleType, Game1 gameRunning ) : base(Texture, Position, Speed, Traits, Team, gameRunning)
        {
            Forces.Add(new Vector2(0, 0));
            Health = 100;
            ID = iD;
            ObstacleType = obstacleType;
            Circle circle = new Circle(Position, 50 * Size);
            Hitboxes =  new Shape[] { circle };

            Dictionary<string, IObstacleAction> DictionaryOfCommonActions = new()
            {
                ["CheckIfAttack"] = new CheckIfAttackAction(GameRunning)
                // Add other actions here as needed
            };
        }
        public override void Update(GameTime gameTime)
        {
            double ElapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
            if (this.Forces.Count == 0)
            {
               this.Forces.Add(Vector2.Zero);
            }
            Random random = new Random();
            int R = random.Next(1, 50);
            if (CurrentMoveTarget != null && CurrentMoveTarget.Health > 0)
            {
                Vector2 MovePos = CurrentMoveTarget.Position;
                double Distance = Math.Sqrt(Math.Pow(MovePos.X - Position.X, 2) + Math.Pow(MovePos.Y - Position.Y, 2));
                // Debug.WriteLine(Position.X);
                Vector2 DesiredDifference = MovePos - Position;
                // DesiredDifference.Normalize();
                //  Debug.WriteLine(DesiredDifference);
                Vector2 TurnDifference = DesiredDifference - Difference;
                Difference = DesiredDifference - new Vector2(TurnDifference.X / (1f + (TurnSpeed / 5)), TurnDifference.Y / (1f + (TurnSpeed / 5)));
                if (Difference != Vector2.Zero)
                {
                    Difference.Normalize();
                }
                Rotation = Math.Atan((Difference.Y) / (Difference.X)) * 180 / Math.PI;

                //   Position.Y += (float)Difference.Y * Speed;
                //  Position.X += (float)Difference.X * Speed;
                if (IsMoving == true)
                {
                    Forces[MoveForceIndex] = new Vector2((float)Difference.X * Speed, (float)Difference.Y * Speed);
                }
                else
                {

                    //   Debug.WriteLine(MoveForceIndex.ToString() + " " + Forces.Count.ToString());
                    try
                    {
                        Forces[MoveForceIndex] = new Vector2(0, 0);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                    }
                }
            }
            else
            {
                try
                {
                    this.Forces[MoveForceIndex] = new Vector2(0, 0);
                }
                catch(Exception exception){
                    Debug.WriteLine(exception.Message);
                }
            }
            for (int i = 0; i < Forces.Count; i++)
            {
                Vector2 NewPosition = Position;
                NewPosition.X += Forces[i].X * (float)(ElapsedTime / (float)sixtieth) / Weight;
                NewPosition.Y += Forces[i].Y * (float)(ElapsedTime / (float)sixtieth) / Weight;
                Position = NewPosition;
            }
            if (GameRunning.PlayerInstance.Health > 0 && Rect.Intersects(GameRunning.PlayerInstance.Rect, GameRunning.PlayerInstance.Rect) && Health > 0)
            {
               // Health = 0;
             //   Game1.PlayerInstance.Health -= 8;
             //   Game1.Hits += 1;
            }

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
        interface IObstacleAction
        {
            void ExecuteAction(Obstacle Obstacle);
        }
        public class CheckIfAttackAction:IObstacleAction
        {
            public Game1 GameRunning { get; set; }
            public void ExecuteAction(Obstacle Obstacle)
            {
                Sprite CurrentTarget = Sprite.GetClosestSprite(GameRunning, Obstacle.Position, Obstacle.ERange, Obstacle.EnemyTeams);
                if (CurrentTarget != null)
                {
                  //  Debug.WriteLine(CurrentTarget.Team.ToString() + "GAH");
                    if (CurrentTarget.GetType() == typeof(Obstacle))
                    {
                        Obstacle.CurrentAttackTarget = (Obstacle)CurrentTarget;
                    }
                }
            }
            public CheckIfAttackAction(Game1 gameRunning)
            {
                GameRunning = gameRunning;
            }
        }


        public void AddForce(Vector2 MoveVector, int DecayDelay)
        {
            Forces.Add(MoveVector);
            Vector2 ForceValue = Forces[Forces.Count - 1];
        }
        public static void DecayForces(Game1 GameRunning)
        {
            List<Sprite> snapshot;
            lock (GameRunning.SpriteListLock)
            {
                snapshot = new List<Sprite>(GameRunning.SpriteList);
            }
            foreach (Sprite sprite in snapshot)
            {
                if (sprite.GetType() == typeof(Obstacle))
                {
                    Obstacle Obstacle = (Obstacle)sprite;
                    if (Obstacle.Forces.Count > 0)
                    {
                        for (int i = 0; i < Obstacle.Forces.Count; i++)
                        {
                            if (i != 0)
                            {
                                if (Math.Abs(Obstacle.Forces[i].X) < 0.1 || Math.Abs(Obstacle.Forces[i].Y) < 0.1)
                                {
                                    Obstacle.Forces[i] = new Vector2(Obstacle.Forces[i].X * 0.9f, Obstacle.Forces[i].Y * 0.9f); // Decay the force by 10%
                                }
                                else
                                {
                                    Obstacle.Forces[i] = new Vector2(0, 0); // Remove the force if it's too small
                                }
                            }
                        }
                    }
                }
            }
        }
        public void Initialise()
        {
            // Debug.WriteLine(Team); // Print the current team
            List < Texture2D >  mAN = new List<Texture2D> { ImageData.Images["FaceDrawing"], ImageData.Images["FaceDrawing2"] };
            //Animations["Idle"] = new List<Texture2D> { ImageData.Images["FaceDrawing"], ImageData.Images["FaceDrawing2"] };
            EnemyTeams.Remove(Team); // Remove the current team from the enemy teams list
            foreach(int item in EnemyTeams)
            {
              //  Debug.WriteLine("EnemyTeam: " + item.ToString());
            }
//Debug.WriteLine("-------");
        }

    }
    public class LineObstacle : Line
    {
        const double sixtieth = 1 / 60.0;
        public Vector2 CentrePos;
        public bool DashOnCooldown = false;
        List<Vector2> Forces = new List<Vector2>();
        int MoveForceIndex = 0;
        bool IsMoving = false;
        List<int> EnemyTeams = new List<int> { 1, 2, 3, 4 }; // Example teams, adjust as needed
        string TextureString;
        public string ObstacleType; // Default Obstacle name, can be changed later
        public Line[] LineObject;
        // Remove the current team from the enemy teams list
        public LineObstacle(Vector2 P1, Vector2 P2) : base (position1 : P1, position2 : P2, player : null)
        {
        }

    }
}
