using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AStarMonoGame
{
    //"Paket" med en nod och dess grannar/neighbours
    class Pair { public Node node;public List<Node> neighbours; }

    //Olika typer av nod
    public enum TypeOfNode { Unused, Open, Closed, Start, Goal, Wall, RightWay}

    class NodeManager
    {
        bool youHaveReachedYourDestination = false;
        bool diagonalAllowed;
        int distanceRightAngle=10, distanceDiagonal = 15;

        Pair[,] allNodes; //Alla noder alltså
        public List<Node> openSet; //De här har vi börjat undersöka
        public List<Node> closedSet; //De här har vi undersökt färdigt
        public Vector2 minPosition, maxPosition; //Hur många noder?
        public Node start, goal;
        public Pair currentPair; //Npden (inkl neighbours) som vi kollar just nu!
        SpriteFont font;

        //Nytt Pair; nod med tom neighbour-lista
        public void AddNode(Vector2 position, TypeOfNode type)
        {
            allNodes[(int)position.X,(int)position.Y] = new Pair();
            GetPair(position).node = new Node(position, type);
            GetPair(position).neighbours= new List<Node>();
        }

        //Konstruktor. Fyll lista med noder, alla Unused som standard
        public NodeManager(Vector2 maxPosition, bool diagonalAllowed)
        {
            this.diagonalAllowed = diagonalAllowed;
            minPosition = Vector2.Zero;
            this.maxPosition = maxPosition;

            allNodes = new Pair[(int)maxPosition.X + 1, (int)maxPosition.Y + 1];

            for (int x = 0; x <= maxPosition.X; x++)
            {
                for (int y = 0; y <= maxPosition.Y; y++)
                {
                    AddNode(new Vector2(x, y), TypeOfNode.Unused);
                }
            }
        }
        //Konstruktor. Anropar enklare konstruktor.
        public NodeManager(Vector2 maxPosition, Vector2 start, Vector2 goal, bool diagonalAllowed):this(maxPosition,diagonalAllowed)
        {
            ChangeNodeType(start, TypeOfNode.Start);
            ChangeNodeType(goal, TypeOfNode.Goal);
            this.start = GetPair(start).node;
            this.goal= GetPair(goal).node;
        }

        //Startvärden på allt! Eller nästan allt.
        public void Initialize(Vector2 startPosition, Vector2 goalPosition, List<Vector2> walls)
        {
            openSet = new List<Node>();
            closedSet = new List<Node>();

            ChangeNodeType(startPosition, TypeOfNode.Start); //Start
            ChangeNodeType(goalPosition, TypeOfNode.Goal); //Mål
            this.start = GetPair(startPosition).node;
            this.goal = GetPair(goalPosition).node;


            //Väggar
            foreach (Vector2 position in walls)
            {
                ChangeNodeType(position,TypeOfNode.Wall);
            }

            //Fågelvägen till målet för alla noder
            for (int x = 0; x < maxPosition.X; x++)
            {
                for (int y = 0; y < maxPosition.Y; y++)
                {
                    Node node = GetPair(new Vector2(x, y)).node;
                    if (node.Type == TypeOfNode.Unused)
                    {
                        //Avståndsformeln! Högre värde på faktorn i Math.Pow ger mer riktad/snabbare sökning men inte alltid närmsta vägen.
                        node.h = (int)Math.Round(Math.Sqrt(Math.Pow(3 * Math.Abs(x - this.goal.position.X), 2) + Math.Pow(3 * Math.Abs(y - this.goal.position.Y), 2)));
                    }
                }
            }
            currentPair = allNodes[(int)start.position.X, (int)start.position.Y];//Vi startar med den här noden!
        }

        public void LoadContent(SpriteFont font)
        {
            this.font = font;
        }

        //CENTRAL METOD! Beräkna neighbours och kolla vart vi ska härnäst. 
        public void Step()
        {
            closedSet.Add(currentPair.node); //Noden vi är i behöver inte undersökas mer

            //Startnoden är lite speciell; ändra annars typ till Closed
            if(currentPair.node.Type!=TypeOfNode.Start)
                ChangeNodeType(currentPair.node.position, TypeOfNode.Closed);

            Node currentNode = currentPair.node; //För att vi ska slippa skriva currentPair.node hela tiden

            AddNeighbours(currentNode.position); //Lägger till neighbours; se den metoden

            //Uppdatera värden i neighbours
            foreach(Node neighbour in currentPair.neighbours)
            {
                //Undersökt; men är det närmare via currentNode?
                if (neighbour.Type == TypeOfNode.Open)
                {
                    int tempG = neighbour.g; //temp-variabel att jämföra med

                    //Kortflytt
                    if (neighbour.position.X == currentNode.position.X || neighbour.position.Y == currentNode.position.Y)
                        tempG += distanceRightAngle;

                    //Diagonalflytt
                    else
                        tempG += distanceDiagonal;

                    //Om den nya vägen är närmare, ändra g-värde och sätt currentNode som parent
                    if(tempG<neighbour.g)
                    {
                        neighbour.g = tempG;
                        neighbour.parent = currentNode;
                    }

                }

                //Inte undersökt tidigare
                else
                {
                    //FRAMME!
                    if (neighbour.Type == TypeOfNode.Goal)
                    {
                        return;
                    }

                    //Markera som påbörjat undersökt
                    openSet.Add(neighbour);
                    neighbour.Type = TypeOfNode.Open;

                    //Lägg till passerad sträcka
                    neighbour.g += currentNode.g;

                    //Kortflytt; vänster, höger, upp eller ned
                    if (neighbour.position.X == currentNode.position.X || neighbour.position.Y == currentNode.position.Y)
                        neighbour.g += distanceRightAngle;

                    //Diagonalflytt
                    else
                        neighbour.g += distanceDiagonal;

                    neighbour.parent = currentNode;
                }
            }

            //Lägsta f-värde??? Alltså - vart ska vi gå härnäst?
            int closestNodeIndex = 0;
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].f < openSet[closestNodeIndex].f)
                    closestNodeIndex = i;
            }

            currentPair = GetPair(openSet[closestNodeIndex].position);
            openSet.RemoveAt(closestNodeIndex);
        }

        //Plocka fram neighbours till den givna positionen
        public void AddNeighbours(Vector2 position)
        {
            Rectangle screen = new Rectangle(0, 0, (int)maxPosition.X + 1, (int)maxPosition.Y + 1);

            Pair pair = GetPair(position);
            List<Vector2> newNeighbourPositions = new List<Vector2>();
            pair.neighbours = new List<Node>();

            //Lägg till runt om!
            if (diagonalAllowed)
            {
                newNeighbourPositions.Add(new Vector2(position.X - 1, position.Y - 1));
                newNeighbourPositions.Add(new Vector2(position.X, position.Y - 1));
                newNeighbourPositions.Add(new Vector2(position.X + 1, position.Y - 1));

                newNeighbourPositions.Add(new Vector2(position.X - 1, position.Y));
                newNeighbourPositions.Add(new Vector2(position.X + 1, position.Y));

                newNeighbourPositions.Add(new Vector2(position.X - 1, position.Y + 1));
                newNeighbourPositions.Add(new Vector2(position.X, position.Y + 1));
                newNeighbourPositions.Add(new Vector2(position.X + 1, position.Y + 1));
            }

            //Om vi inte tillåter diagonal flytt
            else
            {
                newNeighbourPositions.Add(new Vector2(position.X, position.Y - 1));
                newNeighbourPositions.Add(new Vector2(position.X - 1, position.Y));
                newNeighbourPositions.Add(new Vector2(position.X + 1, position.Y));
                newNeighbourPositions.Add(new Vector2(position.X, position.Y + 1));
            }

            //Kontrollera: Ligger positionen på skärmen eller? I så fall lägg till i neighbours
            foreach(Vector2 pos in newNeighbourPositions)
            {
                if (screen.Contains(pos))
                    pair.neighbours.Add(allNodes[(int)pos.X, (int)pos.Y].node);
            }


            //Ta bort falska grannar
            for (int i = 0; i < pair.neighbours.Count;)
            {
                Node node = pair.neighbours[i];//För att slippa skriva pair.neighbours[i] hela tiden

                //VÄGG!
                if (node.Type == TypeOfNode.Wall)
                    pair.neighbours.RemoveAt(i);

                //Start
                else if (node.Type == TypeOfNode.Start)
                    pair.neighbours.RemoveAt(i);

                //FRAMME!!!
                else if (node.Type == TypeOfNode.Goal)
                {
                    pair.neighbours[i].parent = pair.node;
                    youHaveReachedYourDestination = true;
                    Trace(pair.neighbours[i]);
                    return;
                }

                //REDAN UNDERSÖKT!
                else if (closedSet.Contains(node))
                    pair.neighbours.RemoveAt(i);

                //Grannen är inte falsk! Gå till nästa i listan.
                else
                    i++;
            }
        }

        //Hitta vägen direkt!
        public void AllSteps()
        {
            while(!youHaveReachedYourDestination)
                Step();
        }

        //Följ vägen bakåt. Anropas rekursivt!
        private void Trace(Node node)
        {
            if (node.parent != null)
            {
                node.parent.Type = TypeOfNode.RightWay;
                Trace(node.parent);
            }
        }

        //Ändra typ
        public void ChangeNodeType(Vector2 position, TypeOfNode newType)
        {
            GetPair(position).node.Type = newType;
        }

        //För att slippa en tråkig kodrad
        public Pair GetPair(Vector2 position)
        {
            return allNodes[(int)position.X, (int)position.Y];
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Pair pair in allNodes)
                pair.node.Draw(spriteBatch, font);
        }
    }
}
