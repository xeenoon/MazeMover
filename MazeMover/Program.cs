using System.Linq;

namespace MazeMover
{
    [Flags]
    public enum Direction
    {
        Left = 1,
        Up = 2,
        Right = 4,
        Down = 8,
        None = 0,
    }
    class Program
    {
        static void Main(string[] args)
        {
            int characterposition = 0;
            Console.ReadKey();
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            while (true) 
            {
                int width = Console.WindowWidth / 2;
                int height = Console.WindowHeight - 2;
                Random r = new Random();
                int seed = r.Next();
                Maze maze = new Maze(width, height);
                maze.GenerateMaze(seed, false);
                Console.Clear();
                maze.Draw(false);
                Console.ForegroundColor = ConsoleColor.White;
                //Draw character position and readkey to find new position
                Console.CursorLeft = (characterposition % width) * 2; //Maze is double width
                Console.CursorTop = height - (characterposition / width) - 1;
                Console.Write("()");

                while (true) 
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = height; //Write moves at bottom of board
                    ConsoleKey input = Console.ReadKey().Key;
                    Console.CursorLeft = 0;
                    Console.CursorTop = height; //Write moves at bottom of board
                    Console.Write(' ');

                    //Remove last one
                    Console.CursorLeft = (characterposition % width) * 2; //Maze is double width
                    Console.CursorTop = height - (characterposition / width) - 1;
                    Console.Write("  ");

                    switch (input)
                    {
                        case ConsoleKey.W:
                            if (characterposition / width < height-1 && maze.GetCell(characterposition + width)) //Can we move up?
                            {
                                characterposition += width;
                            }
                            break;
                        case ConsoleKey.A:
                            if (characterposition % width >= 1 && maze.GetCell(characterposition - 1)) //Can we move left?
                            {
                                characterposition--;
                            }
                            break;
                        case ConsoleKey.S:
                            if (characterposition >= width && maze.GetCell(characterposition - width)) //Can we move down?
                            {
                                characterposition -= width;
                            }
                            break;
                        case ConsoleKey.D:
                            if (characterposition % width < width-1 && maze.GetCell(characterposition + 1)) //Can we move right?
                            {
                                characterposition++;
                            }
                            break;
                    }
                    Console.CursorLeft = (characterposition % width) * 2; //Maze is double width
                    Console.CursorTop = height - (characterposition / width) - 1;
                    Console.Write("()");
                }
            }
        }
    }
    public class MazeConnection //Require class for nullable type
    {
        public int point1;
        public int point2;

        public MazeConnection(int point1, int point2)
        {
            this.point1 = point1;
            this.point2 = point2;
        }
    }
    public class Maze
    {
        int width;
        int height;

        //Enter at 0,0
        //Exit at width,height (top right)

        //public List<MazeConnection> walls = new List<MazeConnection>();
        public bool[] claimedcells;
        public bool[] mainpathcells;
        public bool[] drawablecells;
        public Maze(int width, int height)
        {
            this.width = width;
            this.height = height;
            claimedcells = new bool[width * height];
            mainpathcells = new bool[width * height];
            drawablecells = new bool[width * height];
        }
        public bool GetCell(int cellidx)
        {
            if (cellidx <= -1 || cellidx >= claimedcells.Length)
            {
                return false;
            }
            else
            {
                return claimedcells[cellidx];
            }
        }
        public void SolveMaze()
        {

        }
        public void SearchPath(int startidx, bool issolution, Random r)
        { 
            List<Direction> travelledDirections = new List<Direction>();
            claimedcells[startidx] = true;
            int searchidx = 0;
            int cellidx = startidx;
            while (true)
            {
                if (searchidx % 10000 == 0)
                {
                    //Draw();
                }
                ++searchidx;
                //Check to see if there is an available next path

                bool cellsaroundleft = GetCell(cellidx - 2) || GetCell(cellidx - 1 + width) || GetCell(cellidx - 1 - width);
                bool cellsaroundright = GetCell(cellidx + 2) || GetCell(cellidx + 1 + width) || GetCell(cellidx + 1 - width);
                bool cellsaroundup = GetCell(cellidx + width + width) || GetCell(cellidx + width + 1) || GetCell(cellidx + width - 1);
                bool cellsarounddown = GetCell(cellidx - width - width) || GetCell(cellidx - width + 1) || GetCell(cellidx - width - 1);

                bool cellsaroundleft2 = GetCell(cellidx - 3) || GetCell(cellidx - 2 + width) || GetCell(cellidx - 2 - width);
                bool cellsaroundright2 = GetCell(cellidx + 3) || GetCell(cellidx + 2 + width) || GetCell(cellidx + 2 - width);
                bool cellsaroundup2 = GetCell(cellidx + width + width + width) || GetCell(cellidx + width + width + 1) || GetCell(cellidx + width + width - 1);
                bool cellsarounddown2 = GetCell(cellidx - width - width - width) || GetCell(cellidx - width - width + 1) || GetCell(cellidx - width - width - 1);

                bool canmoveoneright = ((cellidx) % width <= width - 2 && !GetCell(cellidx + 1)) && !cellsaroundright;
                bool canmoveoneleft = (cellidx % width >= 2 && !GetCell(cellidx - 1)) && !cellsaroundleft;
                bool canmoveoneup = ((issolution ? (cellidx < width * (height - 1) - 1) : (cellidx < width * (height - 2) - 1)) && !GetCell(cellidx + width)) && !cellsaroundup;
                bool canmoveonedown = (cellidx >= width + width && !GetCell(cellidx - width)) && !cellsarounddown;

                bool canmovetworight = ((cellidx) % width <= width - 3 && !GetCell(cellidx + 2) && !GetCell(cellidx + 1)) && !cellsaroundright2 && !cellsaroundright;
                bool canmovetwoleft = (cellidx % width >= 3 && !GetCell(cellidx - 2) && !GetCell(cellidx - 1)) && !cellsaroundleft2 && !cellsaroundleft;
                bool canmovetwoup = ((issolution ? (cellidx < width * (height - 2) - 1) : (cellidx < width * (height - 3) - 1)) && !GetCell(cellidx + width + width) && !GetCell(cellidx + width)) && !cellsaroundup2 && !cellsaroundup;
                bool canmovetwodown = (cellidx >= (width + width + width) && !GetCell(cellidx - width - width) && !GetCell(cellidx - width)) && !cellsarounddown2 && !cellsarounddown;


                int directionmodifier = 0;

                Direction allowedDirections = Direction.None;
                if (travelledDirections.Count() != 0)
                {
                    switch (travelledDirections.Last())
                    {
                        case Direction.Left:
                            allowedDirections |= (canmoveoneleft) ? Direction.Left : Direction.None;
                            allowedDirections |= (canmovetwoup) ? Direction.Up : Direction.None;
                            allowedDirections |= (canmovetwodown) ? Direction.Down : Direction.None;
                            allowedDirections |= (canmovetworight) ? Direction.Right : Direction.None;
                            break;
                        case Direction.Up:
                            allowedDirections |= (canmoveoneup) ? Direction.Up : Direction.None;
                            allowedDirections |= (canmovetwoleft) ? Direction.Left : Direction.None;
                            allowedDirections |= (canmovetwodown) ? Direction.Down : Direction.None;
                            allowedDirections |= (canmovetworight) ? Direction.Right : Direction.None;
                            break;
                        case Direction.Right:
                            allowedDirections |= (canmoveoneright) ? Direction.Right : Direction.None;
                            allowedDirections |= (canmovetwoup) ? Direction.Up : Direction.None;
                            allowedDirections |= (canmovetwodown) ? Direction.Down : Direction.None;
                            allowedDirections |= (canmovetwoleft) ? Direction.Left : Direction.None;
                            break;
                        case Direction.Down:
                            allowedDirections |= (canmoveonedown) ? Direction.Down : Direction.None;
                            allowedDirections |= (canmovetwoup) ? Direction.Up : Direction.None;
                            allowedDirections |= (canmovetwoleft) ? Direction.Left : Direction.None;
                            allowedDirections |= (canmovetworight) ? Direction.Right : Direction.None;
                            break;
                    }
                }
                else
                {
                    //First time
                    if (canmovetwoup)
                    {
                        allowedDirections |= Direction.Up;
                    }
                    if (canmovetwoleft)
                    {
                        allowedDirections |= Direction.Left;
                    }
                    if (canmovetworight)
                    {
                        allowedDirections |= Direction.Right;
                    }
                    if (canmovetwodown)
                    {
                        allowedDirections |= Direction.Down;
                    }
                }

                if (allowedDirections == Direction.None)
                {
                    if (issolution)
                    {
                        searchidx--; //Allow for an extra search
                        claimedcells[cellidx] = true; //Make sure we dont get here again
                        mainpathcells[cellidx] = false;
                        drawablecells[cellidx] = false;

                        switch (travelledDirections.Last())
                        {
                            case Direction.Left:
                                directionmodifier = 1;
                                break;
                            case Direction.Up:
                                directionmodifier = -width;
                                break;
                            case Direction.Right:
                                directionmodifier = -1;
                                break;
                            case Direction.Down:
                                directionmodifier = width;
                                break;
                            case Direction.None:
                                throw new Exception("How did we get here");
                        }
                        if (travelledDirections[travelledDirections.Count() - 2] != travelledDirections.Last())
                        {
                            //Did we just travel in a new direction?
                            //We will have moved two squares

                            claimedcells[cellidx + directionmodifier] = true;
                            mainpathcells[cellidx + directionmodifier] = false;
                            drawablecells[cellidx + directionmodifier] = false;
                            directionmodifier *= 2;
                        }
                        cellidx += directionmodifier;
                        travelledDirections.RemoveAt(travelledDirections.Count() - 1);
                        continue;
                    }
                    else
                    {
                        return;
                        if (travelledDirections.Count() <= 2) //path is too short?
                        {
                            //trim it
                            for (int i = 0; i < travelledDirections.Count(); ++i)
                            {
                                int listidx = travelledDirections.Count() - i - 1;
                                claimedcells[cellidx] = false;
                                switch (travelledDirections[listidx])
                                {
                                    case Direction.Left:
                                        directionmodifier = 1;
                                        break;
                                    case Direction.Up:
                                        directionmodifier = -width;
                                        break;
                                    case Direction.Right:
                                        directionmodifier = -1;
                                        break;
                                    case Direction.Down:
                                        directionmodifier = width;
                                        break;
                                    case Direction.None:
                                        throw new Exception("How did we get here");
                                }
                                if (listidx == 0 || travelledDirections[listidx-1] != travelledDirections[listidx])
                                {
                                    //Did we just travel in a new direction?
                                    //We will have moved two squares

                                    claimedcells[cellidx + directionmodifier] = false;
                                    mainpathcells[cellidx + directionmodifier] = false;
                                    drawablecells[cellidx + directionmodifier] = false;
                                    directionmodifier *= 2;
                                }
                                cellidx += directionmodifier;
                            }
                        }
                        return; //If not generating solution, then just die
                    }
                }
                Direction nextdirection = Direction.None;

                var matching = Enum.GetValues(typeof(Direction))
                   .Cast<Direction>()
                   .Where(c => (allowedDirections & c) == c && c != Direction.None)    // or use HasFlag in .NET4
                   .ToArray();
                nextdirection = matching[r.Next(matching.Length)];
                switch (nextdirection)
                {
                    case Direction.Left:
                        directionmodifier = -1;
                        break;
                    case Direction.Up:
                        directionmodifier = width;
                        break;
                    case Direction.Right:
                        directionmodifier = 1;
                        break;
                    case Direction.Down:
                        directionmodifier = -width;
                        break;
                }
                if (travelledDirections.Count() == 0 || nextdirection != travelledDirections.Last())
                {
                    directionmodifier *= 2;
                }
                if (directionmodifier % 2 == 0 && Math.Abs(directionmodifier) != width) //Moving two?
                {
                    claimedcells[cellidx + directionmodifier / 2] = true;
                    if (issolution)
                    {
                        mainpathcells[cellidx + directionmodifier / 2] = true;
                    }
                    drawablecells[cellidx + directionmodifier / 2] = true;
                }
                claimedcells[cellidx] = true;
                if (issolution)
                {
                    mainpathcells[cellidx] = true;
                }
                drawablecells[cellidx] = true;
                cellidx += directionmodifier;
                if (cellidx >= 10000)
                {

                }
                travelledDirections.Add(nextdirection);

                if (((cellidx % width) == width - 1)    //Have we found a solution
                    || ((cellidx / width) == height - 1))
                {
                    if (issolution)
                    {
                        claimedcells[cellidx] = true;
                        mainpathcells[cellidx] = true;
                        drawablecells[cellidx] = true;
                    }
                    return;
                }
            }

        }
        public void GenerateMaze(int genseed, bool showsolution)
        {
            //Begin by generating correct path, starting at 0,0
            Random r = new Random(genseed);
            //Draw first cell
            claimedcells[0] = true;
            drawablecells[0] = true;
            mainpathcells[0] = true;


            SearchPath(width, true, r); //Create a solution to the maze
            claimedcells = mainpathcells.ToList().Copy().ToArray();
            if (showsolution)
            {
                //return;
            }
            //Generate incorrect paths
            for (int i = 0; i < ((width + height) * (width + height)) / 2; ++i) 
            {
                int cellidx;
                do {
                    cellidx = r.Next(width * height);
                } while (!claimedcells[cellidx]);
                SearchPath(cellidx, false, r);
            }
        }

        private bool WillIntersect(int cellidx, Direction nextdirection)
        {
            switch (nextdirection)
            {
                case Direction.Left:
                    return claimedcells[cellidx - 1];
                case Direction.Up:
                    return claimedcells[cellidx + width];
                case Direction.Right:
                    return claimedcells[cellidx + 1];
                case Direction.Down:
                    return claimedcells[cellidx - width];
                case Direction.None:
                    return false;
            }
            throw new Exception("Direction uncharted");
            return false;
        }
        public void Draw(bool solution)
        {
            for (int y = height-1; y > -1; --y)
            {
                for (int x = 0; x < width; ++x)
                {
                    if (mainpathcells[x+(y*width)])
                    {
                        if (solution)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("██");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("  ");
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                    }
                    else if (drawablecells[x + (y*width)])
                    {

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("  ");
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("██");
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
    public enum WallType
    {
        Floor,
        DoubleFloor,
        Wall,
        DebugDot,
        Space,
    }
}