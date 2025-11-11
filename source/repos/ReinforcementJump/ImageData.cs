using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

namespace ReinforcementJump
{
    public class ImageData
    {
        public static Dictionary<string, Texture2D> Images = new Dictionary<string, Texture2D>
        {
            ["OIP"] = null,
            ["Sunny"] = null,
            ["FaceDrawing"] = null,
            ["FaceDrawing2"] = null,
            ["FaceDrawingAttack"] = null,
            ["FaceDrawingDead"] = null
        };
        public static void AcquireData(ContentManager Content, GraphicsDevice graphicsDevice)
        {
            foreach (KeyValuePair<string, Texture2D> Image in Images)
            {
                if (Image.Value == null)
                {
                    Content.RootDirectory = "Content";
                    Images[Image.Key] = Content.Load<Texture2D>(Image.Key);
                    if (Image.Key != null)
                    {
                      //  Debug.WriteLine(Images[Image.Key].Width + ", " + Images[Image.Key].Height);
                        string p = Path.GetFileNameWithoutExtension(@"C:\Users\ezrat\source\repos\First Game\Project2\Content\" + Image.Key);

                        string FileName = UsefulFunctions.FindFileRegardlessOfExtension(@"C:\Users\ezrat\source\repos\First Game\Project2\Content\", Image.Key);
                        Texture2D texture = Texture2D.FromStream(graphicsDevice, File.OpenRead(FileName));
                        Images[Image.Key] = texture;
                        Debug.WriteLine($"Width: {texture.Width}, Height: {texture.Height}");
                    }
                }
            }
        }
    }


}