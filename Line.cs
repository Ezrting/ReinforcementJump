using System.Collections.Generic;
using System;

using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace ReinforcementJump;

#nullable enable

public class Line
{
    public double Gradient;
    public double YIntercept;
    private Vector2 P1;
    public Vector2 P2;
    public Vector2 MaxP2Offset;
    public double DistanceLimit = 10000;
    bool Visible = true;
    public bool IsHighlighted = false;
    public bool IsVertical;
    public Sprite? AttachedPlayer;
    public Vector2 Position1
    {
        get
        {
            return P1;
        }
        set
        {
            if (P1 != value)
            {
                P1 = value;
                Update();
            }
        }
    }
    public Vector2 Position2
    {
        get
        {
            return P2;
        }
        set
        {
            if (P2 != value)
            {
                P2 = value;
                Update();
            }
        }
    }
    //In a property, the get accessor runs every time a property is read(referencing it in code) whilst set runs every time the property is assigned a value
    //In the property above, P1 is read using the keyword "value"
    public Line(Vector2 position1, Vector2 position2, Player player)
    {
        this.Position1 = position1;
        this.Position2 = position2;
        this.MaxP2Offset = position2 - position1;
        this.Gradient = (position2.Y - position1.Y) / (position2.X - position1.X);
        if (double.IsInfinity(this.Gradient))
        {
            IsVertical = true;
            this.Gradient = 10000;
        }
        this.YIntercept = Position1.Y - Gradient * Position1.X;
        this.AttachedPlayer = player;
    }
    public void Update()
    {
        this.Gradient = (Position2.Y - Position1.Y) / (Position2.X - Position1.X);
        this.YIntercept = Position1.Y - Gradient * Position1.X;
        if (Position1.X == Position2.X)
        {
            IsVertical = true;
        }
        else
        {
            IsVertical = false;
        }
      //  Debug.WriteLine("Line updated: " + Gradient + " " + YIntercept);
    }
    public static (bool, Vector2) FindIntersection(Line line1, Line line2)
    {
        int Constant = 2;
        double GradientDifference = line1.Gradient - line2.Gradient;
        if (GradientDifference == 0)
        {
            if (Math.Abs(line1.YIntercept - line2.YIntercept) <= Constant)
            {
                return (true, line1.Position1);
            }
            return (false, new Vector2(0, 0));
        }
        double XValue;
        double YValue;
        if (line2.IsVertical == true)
        {
            XValue = line2.Position1.X;
            YValue = line1.Gradient * XValue + line1.YIntercept;
        }
        else if (line1.IsVertical == true)
        {
            XValue = line1.Position1.X;
            YValue = line2.Gradient * XValue + line2.YIntercept;
        }
        else
        {
            XValue = (line2.YIntercept - line1.YIntercept) / GradientDifference;
            YValue = line1.Gradient * XValue + line1.YIntercept;
        }


        Vector2 MaxP2_1 = line1.Position1 + line1.MaxP2Offset;
        Vector2 MaxP2_2 = line2.Position1 + line2.MaxP2Offset;
        var (Max_XLine1, Min_XLine1) = (line1.Position1.X > MaxP2_1.X) ?
          (line1.Position1.X, MaxP2_1.X) : (MaxP2_1.X, line1.Position1.X);
        var (Max_XLine2, Min_XLine2) = (line2.Position1.X > MaxP2_2.X) ?
             (line2.Position1.X, MaxP2_2.X) : (MaxP2_2.X, line2.Position1.X);

        var (Max_YLine1, Min_YLine1) = (line1.Position1.Y > MaxP2_1.Y) ?
           (line1.Position1.Y, MaxP2_1.Y) : (MaxP2_1.Y, line1.Position1.Y);
        var (Max_YLine2, Min_YLine2) = (line2.Position1.Y > MaxP2_2.Y) ?
             (line2.Position1.Y, MaxP2_2.Y) : (MaxP2_2.Y, line2.Position1.Y);
        /*
        if (GradientDifference > 1000 || GradientDifference < -1000)
        {
          //  Debug.WriteLine("Vertical line intersection at " + XValue + ", " + YValue);
            if (
             (YValue >= Min_YLine1) &&
             (YValue <= Max_YLine1) &&

             (YValue >= Min_YLine2) &&
             (YValue <= Max_YLine2)
            )
            {
                return (true, new Vector2((float)XValue, (float)YValue));
            }
        }
        */
        if (line1.IsVertical && line2.IsVertical)
        {
            float A_YRange1 = line1.Position1.Y;
            float A_YRange2 = line1.Position2.Y;

            A_YRange1 = Math.Min(A_YRange1, A_YRange2);
            A_YRange2 = Math.Max(A_YRange1, A_YRange2);

            float B_YRange1 = line2.Position1.Y;
            float B_YRange2 = line2.Position2.Y;
            if (
                (B_YRange1 >= A_YRange1 - Constant && B_YRange1 <= A_YRange2 + Constant)
               )
            {
                YValue = B_YRange1;
                return (true, new Vector2((float)XValue, (float)YValue));
            }
            if (
              (B_YRange2 >= A_YRange1 - Constant && B_YRange2 <= A_YRange2 + Constant)
             )
            {
                YValue = B_YRange2;
                return (true, new Vector2((float)XValue, (float)YValue));
            }

            return (false, new Vector2(0, 0));
        }
        else if (
            (
           (XValue >= Min_XLine1 - Constant) &&
           (XValue <= Max_XLine1 + Constant) &&

           (XValue >= Min_XLine2 - Constant) &&
           (XValue <= Max_XLine2 + Constant)
           )
           &&
           (
           (YValue >= Min_YLine1 - Constant) &&
           (YValue <= Max_YLine1 + Constant) &&

           (YValue >= Min_YLine2 - Constant) &&
           (YValue <= Max_YLine2 + Constant)
           )
           )
        {
            return (true, new Vector2((float)XValue, (float)YValue));
        }

        else
        {
            //return (true, new Vector2((float)XValue, (float)YValue));
            return (false, new Vector2(0, 0));
        }
        
      //  return (true, new Vector2((float)XValue, (float)YValue));
    }

}