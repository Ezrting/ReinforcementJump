using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReinforcementJump
{
    public class Player : Entity
    {
        public Vector2 CentrePos;
        public bool DashOnCooldown = false;
        public List<int> EnemyTeams = new List<int> {2, 3, 4 };
        public Vector2 PreviousVelocity = new(); //Stores the previous velocity on the current frame
        public List<DataPoint> DataBatchToSearch = new();
        public int ActionsSinceLastReward = 0;
        public Player(Texture2D Texture, Vector2 Position, float Speed, Dictionary<string, float> Traits, int Team, int iD, string entityType, Game1 GameRunning) : base(Texture, Position, Speed, Traits, Team, iD, entityType, GameRunning)
        {
            Health = 100;
        }
        public void Move(int Magnitude)
        {
            Position = new Vector2(Position.X, (float)(Magnitude * 10) + 5);
        }
        public override void Update(GameTime gameTime)
        {
            double TimeElapsed = gameTime.ElapsedGameTime.TotalSeconds;
            var (ObjectsToDestroy, CollidingLines) = Sprite.GetCollidingObjects(GameRunning, this);
            foreach (double distance in GameRunning.RaycastDistances)
            {

            }
            if (ObjectsToDestroy.Length > 0 || CollidingLines.Length > 0)
            {
                float ChangedPositionX = (float)(Position.X - PreviousVelocity.X);
                float ChangedPositionY = (float)(Position.Y - PreviousVelocity.Y);
                Position = new Vector2(
                    (float)Math.Round(ChangedPositionX),
                    (float)Math.Round(ChangedPositionY)
                    );
             //   Debug.WriteLine(DataBatchToSearch.Count + " " + ActionsSinceLastReward);
                DataPoint[] BatchToAnalyse = new DataPoint[ActionsSinceLastReward];
                List<DataPoint> SpecificRange = DataBatchToSearch.GetRange(DataBatchToSearch.Count - ActionsSinceLastReward, ActionsSinceLastReward);
                BatchToAnalyse = [.. SpecificRange];
              //  Debug.WriteLine(SpecificRange.Count);
                ActionsSinceLastReward = 0;
                foreach (Sprite sprite in ObjectsToDestroy)
                {
                    GameRunning.PlayerNeuralNetwork.AccumulatedRewards -= 1;
                    for (int i = 0; i < BatchToAnalyse.Length; i++)
                    {
                        GameRunning.PlayerNeuralNetwork.CreateExpectedOutputs(BatchToAnalyse[i], (float)(1f / Math.Sqrt(i)));
                    }
                    GameRunning.PlayerNeuralNetwork.Learn([.. BatchToAnalyse]);
                    GameRunning.PlayerNeuralNetwork.AccumulatedRewards = 0;
                    GameRunning.SpriteList.Remove(sprite);
                    //  GameRunning.PlayerNeuralNetwork.CreateExpectedOutputs();
                }
            }
            PreviousVelocity = new Vector2(0, 0);
            bool CanClick = true;
            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                if (DashOnCooldown == false)
                {
                    DashOnCooldown = true;
                }
                Thread NewThread = new Thread(() =>
                {

                });
                NewThread.Start();
                Position = new Vector2(Position.X, Position.Y + 5);
            }
            Position += new Vector2((float)TimeElapsed * 120,0);
            PreviousVelocity += new Vector2((float)TimeElapsed * 60, 0);
            List<double> MoveDirection = new List<double>{};
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                MoveDirection.Add(0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                MoveDirection.Add(180);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                MoveDirection.Add(270);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                MoveDirection.Add(90);
            }
            for (int g = 0; g < 1; g++)
            {
                DataPoint NewDataPoint = new(GameRunning.RaycastDistances);
                DataBatchToSearch.Add(NewDataPoint);
                //Let's see if it works!
                //  int MaxIndex = neuralNetwork.Classify(new double[] { 1, 1 }); // Classify the inputs 1, 1
                // Debug.WriteLine(MaxIndex);

                //Convert to double array
                var (OutputsRecieved, MaxIndex) = GameRunning.PlayerNeuralNetwork.Classify(NewDataPoint.Inputs);

                //Store these outputs as the expected outputs
                NewDataPoint.ExpectedOutputs = OutputsRecieved;

                GameRunning.PlayerNeuralNetwork.PastOutput = OutputsRecieved;
                double OutputsSum = OutputsRecieved.Sum();
                Random RandomNumber = new();
                double RandomValue = RandomNumber.NextDouble();
                double[] AccumulatedOutputs = new double[OutputsRecieved.Length];
                for (int i = 0; i < OutputsRecieved.Length; i++)
                {
                    OutputsRecieved[i] = OutputsRecieved[i] / OutputsSum;
                    for (int a = 1; a < i + 1; a++)
                    {
                        AccumulatedOutputs[i] += OutputsRecieved[a];
                    }
                }
              //  TrainingData[g] = NewDataPoint;
                int SelectedInput = 0;
                for (int i = 0; i < AccumulatedOutputs.Length; i++)
                {
                    if (RandomValue <= AccumulatedOutputs[i])
                    {
                        SelectedInput = i;
                    }
                }
                NewDataPoint.ChosenAction = SelectedInput;
                switch (SelectedInput)
                {
                    case 0:
                        break;
                    case 1:
                        MoveDirection.Add(270);
                        break;
                    case 2:
                        MoveDirection.Add(90);
                        break;
                    default:
                        throw new Exception("No valid output selected");
                }
                ActionsSinceLastReward++;
            }

            double MD = 0;
            int Count = 0;

            foreach (double direction in MoveDirection)
            {
                MD += direction;
                if (Count == 1 && Math.Abs(MoveDirection[1] - MoveDirection[0]) > 180)
                {
                    MD += 360;
                }   
                Count += 1;
            }
            if (MoveDirection.Count > 0 & MoveDirection.Count < 3)
            {
                MD = MD / MoveDirection.Count;
                //Debug.WriteLine(MD);
                //Debug.WriteLine(Math.Cos(MD) * Speed);
                //  Debug.WriteLine(Math.Sin(MD) * Speed);
                float PositionIncreaseX = (float)(Position.X + Math.Cos(MD * Math.PI / 180 + 0.0000001) * Speed);
                float PositionIncreaseY = (float)(Position.Y + Math.Sin(MD * Math.PI / 180 + 0.0000001) * Speed);
                PreviousVelocity += new Vector2(PositionIncreaseX - Position.X, PositionIncreaseY - Position.Y);
                Position = new Vector2(
                    (float)Math.Round(PositionIncreaseX),
                    (float)Math.Round(PositionIncreaseY)
                    );
            }
            else
            {
                MD = 0;
            }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && CanClick == true)
            {
                CanClick = false;
                while (Mouse.GetState().LeftButton != ButtonState.Released)
                {
                    Thread.Sleep(100);
                }
                CanClick = true;
                Dictionary<string, float> Traits2 = new Dictionary<string, float>();
                Traits2.Add("Size", 0.2f);
              //  Projectile projectile = new Projectile("IDEA", Position, 10, Traits2);
            }
            CentrePos = new Vector2(Position.X + Texture.Width * (int)(SCALE * 100) / 100 / 2, Position.Y + Texture.Height * (int)(SCALE * 100) / 100 / 2);
           // base.Update(gameTime);
        }
    }
}
