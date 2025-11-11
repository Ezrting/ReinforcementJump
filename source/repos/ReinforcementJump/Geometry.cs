using System.Collections.Generic;
using System;

using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using ReinforcementJump;
public class Circle : Shape
{
    public float Radius;
    public Circle(Vector2 position, float radius)
    {
        this.Position = position;
        this.Radius = radius;
    }
    public List<Vector2> GetVertices()
    {
        int X = (int)Position.X;
        int Y = (int)Position.Y;
        List<Vector2> Vertices = new List<Vector2>();
        List<Vector2> RotatedVertices = new List<Vector2>();
        // Vector2 RotatedWidth = Vector2.new(Math.Cos(Rotation) * Width;
        // Vector2 RotatedHeight = (float)(1 / Math.Cos(Rotation) * Height);
        //Game1.RotatePointsAroundAxis(Vertices, new Vector2(X, Y), Rotation);
        return Vertices;
    }

}
public class GeometryRect : Shape
{
    public int Width;
    public int Height;
    public GeometryRect(Vector2 Position, int Width, int Height, float Rotation)
    {
        this.Position = Position; //Position is at the centre of the rectangle
        this.Width = Width;
        this.Height = Height;
        this.Rotation = Rotation;
    }
    public List<Vector2> GetVertices()
    {
        int X = (int)Position.X;
        int Y = (int)Position.Y;
        List<Vector2> Vertices = new List<Vector2>();
        List<Vector2> RotatedVertices = new List<Vector2>();
        float DistanceToVertice = (float)Math.Sqrt(Math.Pow(Width / 2, 2) + Math.Pow(Height / 2, 2));
        // Vector2 RotatedWidth = Vector2.new(Math.Cos(Rotation) * Width;
        // Vector2 RotatedHeight = (float)(1 / Math.Cos(Rotation) * Height);
        Vertices.Add(new Vector2(X - Width/2, Y - Height/2));
        Vertices.Add(new Vector2(X + Width/2, Y - Height/2));
        Vertices.Add(new Vector2(X - Width/2, Y + Height/2));
        Vertices.Add(new Vector2(X + Width/2, Y + Height/2));
        //Game1.RotatePointsAroundAxis(Vertices, new Vector2(X, Y), Rotation);
        return Vertices;
    }
    public override Line[] GetLines()
    {
        Line[] LineList = new Line[4];
        List<Vector2> Vertices = GetVertices();
        LineList[0] = new Line(Vertices[0], Vertices[1], null);
        LineList[1] = new Line(Vertices[1], Vertices[3], null);
        LineList[2] = new Line(Vertices[3], Vertices[2], null);
        LineList[3] = new Line(Vertices[2], Vertices[0], null);
        return LineList;
    }

}

public class Shape
{
    public Vector2 Position = new Vector2(0, 0);
    public float Rotation = 0;
    public bool Intersects(Shape Shape1, Shape Shape2)
    {
        if (Shape1.GetType() == typeof(Circle) && Shape2.GetType() == typeof(Circle))
        {
            float Distance = (float)Math.Sqrt(Math.Pow(Shape1.Position.X - Shape2.Position.X, 2) + Math.Pow(Shape1.Position.Y - Shape2.Position.Y, 2));
            if (Distance < ((Circle)Shape1).Radius + ((Circle)Shape2).Radius)
            {
                return true;
            }
        }
        if (Shape1 is GeometryRect && Shape2 is GeometryRect)
        {
            GeometryRect Rect = (GeometryRect)Shape1;
            Line[] Lines = Rect.GetLines();
            foreach (Line line in Lines)
            {
                GeometryRect Rect2 = (GeometryRect)Shape2;
                Line[] Lines2 = Rect2.GetLines();
                foreach (Line line2 in Lines2)
                {
                    var (Success, HitPoint) = Line.FindIntersection(line2, line);
                    if (Success)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
        /*
        int X = (int)Position.X;
        int Y = (int)Position.Y;
        //Convert Rectangle A to world space
        Shape2.Position = Shape2.Position - Shape1.Position;
        Shape2.Rotation = Shape2.Rotation - Shape1.Rotation;
        Shape1.Position = new Vector2(0, 0);
        Shape1.Rotation = 0;
        return true;
        */
    }
    public virtual Line[] GetLines()
    {
        return new Line[0];
    }
}
