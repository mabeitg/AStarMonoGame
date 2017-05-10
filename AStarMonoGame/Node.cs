using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;
using MonoGame.Extended;

namespace AStarMonoGame
{
    class Node
    {
        public TypeOfNode Type;
        public int g; //avstånd från start till hit?
        public float h; //fågelvägen till mål!!1 Alltså uppskattat/ungefärligt värde.
        RectangleF square; //Den ritade rutan
        public Vector2 position;
        int size = 15;


        public Node parent;

        //Varje typ har sin färg!
        Color color
        {
            get
            {
                Color color;
                switch (Type)
                {
                    case TypeOfNode.Closed: //Redan undersökt
                        color = Color.Green;
                        break;
                    case TypeOfNode.Goal: //Mål
                        color = Color.Blue;
                        break;
                    case TypeOfNode.Open: //Börjat undersöka
                        color = Color.OrangeRed;
                        break;
                    case TypeOfNode.Start: //Start
                        color = Color.DarkRed;
                        break;
                    case TypeOfNode.Unused: //Inte börjat undersöka
                        color = Color.Gray;
                        break;
                    case TypeOfNode.Wall: //Vägg/Hinder
                        color = Color.Black;
                        break;
                    case TypeOfNode.RightWay: //Del i närmsta vägen från start till mål
                        color = Color.DarkGoldenrod;
                        break;

                    default: color = Color.White; //Ingenting
                        break;
                }
                return color;
            }
        }
        

        //Nodens totala "värde"
        public float f { get { return g + h; } }

        //string med värdena
        public string Info
        {
            get
            {
                return "g: " + g + "\r\n" + "h: " + h + "\r\n" + "f: " + f;
            }
        }

        public Node(Vector2 position, TypeOfNode type)
        {
            this.position = position;
            this.Type = type;
            square = new RectangleF();
            square.X = position.X * (size+1);
            square.Y = position.Y * (size+1);
            square.Width = size;
            square.Height = size;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            ShapeExtensions.DrawRectangle(spriteBatch, square, color, size/2);

            //Skriv inte ut värden för väggar, start och mål
            if(Type!=TypeOfNode.Wall&& Type !=TypeOfNode.Start && Type !=TypeOfNode.Goal)
                spriteBatch.DrawString(font, f.ToString(), square.Position, Color.White);

        }
    }
}
