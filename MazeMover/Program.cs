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
            Console.OutputEncoding = System.Text.Encoding.Unicode;


            Maze maze = new Maze(100,100);
            maze.GenerateMaze(10, false);
            maze.Draw();
            var input = Console.ReadLine();
            if (input == "solution")
            {
                maze = new Maze(10, 10);
                maze.GenerateMaze(10, true);
                maze.Draw();
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
        public void GenerateMaze(int genseed, bool showsolution)
        {
            //Begin by generating correct path, starting at 0,0
            bool mazesolved = false;
            Random r = new Random(genseed);

            List<Direction> travelledDirections = new List<Direction>();

            int cellidx = 0;
            //Draw first cell
            claimedcells[0] = true;
            drawablecells[0] = true;
            mainpathcells[0] = true;

            int endmazeidx = 0;
            int searchidx = -1;
            
            while(searchidx < width * height)
            {
                ++searchidx;
                //Check to see if there is an available next path
                
                bool cellsaroundleft  = GetCell(cellidx - 2) || GetCell(cellidx - 1 + width) || GetCell(cellidx - 1 - width);
                bool cellsaroundright = GetCell(cellidx + 2) || GetCell(cellidx + 1 + width) || GetCell(cellidx + 1 - width);
                bool cellsaroundup    = GetCell(cellidx + width + width) || GetCell(cellidx + width + 1) || GetCell(cellidx + width - 1);
                bool cellsarounddown  = GetCell(cellidx - width - width) || GetCell(cellidx - width + 1) || GetCell(cellidx - width - 1);

                bool cellsaroundleft2 = GetCell(cellidx - 3) || GetCell(cellidx - 2 + width) || GetCell(cellidx - 2 - width);
                bool cellsaroundright2 = GetCell(cellidx + 3) || GetCell(cellidx + 2 + width) || GetCell(cellidx + 2 - width);
                bool cellsaroundup2 = GetCell(cellidx + width + width + width) || GetCell(cellidx + width + width + 1) || GetCell(cellidx + width + width - 1);
                bool cellsarounddown2 = GetCell(cellidx - width - width - width) || GetCell(cellidx - width - width + 1) || GetCell(cellidx - width - width - 1);

                bool canmoveoneright = ((cellidx + 1) % width != 0 && !claimedcells[cellidx + 1]) && !cellsaroundright;
                bool canmoveoneleft = (cellidx % width >= 1 && !claimedcells[cellidx - 1]) && !cellsaroundleft;
                bool canmoveoneup = (cellidx < width * (height - 1) - 1 && !claimedcells[cellidx + width]) && !cellsaroundup;
                bool canmoveonedown = (cellidx >= width && !claimedcells[cellidx - width]) && !cellsarounddown;

                bool canmovetworight = ((cellidx + 2) % width != 0 && !claimedcells[cellidx + 2] && !claimedcells[cellidx + 1]) && !cellsaroundright2;
                bool canmovetwoleft = (cellidx % width >= 2 && !claimedcells[cellidx - 2] && !claimedcells[cellidx - 1]) && !cellsaroundleft2;
                bool canmovetwoup = (cellidx < width * (height - 2) - 1 && !claimedcells[cellidx + width + width] && !claimedcells[cellidx + width]) && !cellsaroundup2;
                bool canmovetwodown = (cellidx >= (width + width) && !claimedcells[cellidx - width - width] && !claimedcells[cellidx - width]) && !cellsarounddown2;


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
                    allowedDirections = Direction.Up | Direction.Left;
                }

                if (allowedDirections == Direction.None)
                {
                    //return;
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

                        claimedcells[cellidx+directionmodifier] = true;
                        mainpathcells[cellidx + directionmodifier] = false;
                        drawablecells[cellidx + directionmodifier] = false;
                        directionmodifier *= 2;
                    }
                    cellidx += directionmodifier;
                    travelledDirections.RemoveAt(travelledDirections.Count() - 1);
                    continue;
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
                if (travelledDirections.Count() != 0 && nextdirection != travelledDirections.Last())
                {
                    directionmodifier *= 2;
                }
                if (directionmodifier % 2 == 0 && Math.Abs(directionmodifier) != width) //Moving two?
                {
                    claimedcells[cellidx + directionmodifier / 2] = true;
                    mainpathcells[cellidx + directionmodifier / 2] = true;
                    drawablecells[cellidx + directionmodifier / 2] = true;
                }
                claimedcells[cellidx] = true;
                mainpathcells[cellidx] = true;
                drawablecells[cellidx] = true;
                cellidx += directionmodifier;
                travelledDirections.Add(nextdirection);
                //Console.WriteLine();
                //Console.WriteLine("Iteration: " + (i+1).ToString() + " Moved: " + nextdirection.ToString());
                //Draw();

                if (((cellidx % width) == width - 1 && nextdirection == Direction.Right)    //Have we hit the right wall?
                    || ((cellidx / width) == height - 1 && nextdirection == Direction.Up))
                {
                    endmazeidx = cellidx;
                    claimedcells[cellidx] = true;
                    mainpathcells[cellidx] = true;
                    drawablecells[cellidx] = true; 
                    break;
                }
            }
            if (showsolution)
            {
                return;
            }
            return;
            //Generate incorrect paths
            for (int i = 0; i < ((width+height) * (width+height))/2; ++i)
            {
                do {
                    cellidx = r.Next(width * height);
                } while (!claimedcells[cellidx] && cellidx != endmazeidx && cellidx != 0);
                while (true)
                {
                    //Check to see if there is an available next path
                    if ((cellidx == endmazeidx) ||(
                        ((cellidx + 1) % width == 0 || claimedcells[cellidx + 1]) //Is there a claimed cell to the right? Or on right edge
                      && (cellidx == 0 || (cellidx - 1) % width == width-1 || claimedcells[cellidx - 1]) //Is there a claimed cell to the left? Or on the left edge
                      && (cellidx+width > width*(height-1) || claimedcells[cellidx + width]) //Is there a cell to the top? Or on the top edge
                      && (cellidx <= width || claimedcells[cellidx - width])) //Is there a cell to the bottom? Or on the bottom edge
                      ) //No possible next step?
                    {
                        break;
                    }

                    Direction nextdirection = Direction.None;
                    do
                    {
                        var random = r.Next(0, 4);
                        switch (random)
                        {
                            case 0:
                                nextdirection = Direction.Left;
                                break;
                            case 1:
                                nextdirection = Direction.Up;
                                break;
                            case 2:
                                nextdirection = Direction.Down;
                                break;
                            case 3:
                                nextdirection = Direction.Right;
                                break;
                            default:
                                nextdirection = Direction.None;
                                Console.WriteLine("Crashed with direction of: " + random);
                                break;
                        }
                    } while (((cellidx % width) == 0 && nextdirection == Direction.Left)             //For x=0, cannot move left
                          || ((cellidx % width) == width - 1 && nextdirection == Direction.Right)    //For x=width, cannot move right
                          || ((cellidx / width) == 0 && nextdirection == Direction.Down)             //For y=0, cannot move down
                          || ((cellidx / width) == height - 1 && nextdirection == Direction.Up)
                          || WillIntersect(cellidx, nextdirection)
                          || nextdirection == Direction.None);    //For y=height, cannot move up

                    int nextcell = 0;
                    switch (nextdirection)
                    {
                        case Direction.Left:
                            nextcell = cellidx - 1;
                            break;
                        case Direction.Up:
                            nextcell = cellidx + width;
                            break;
                        case Direction.Right:
                            nextcell = cellidx + 1;
                            break;
                        case Direction.Down:
                            nextcell = cellidx - width;
                            break;
                        case Direction.None:
                            break;
                    } //Find position of nextcell and empty out the position of the new wall
                    claimedcells[cellidx] = true;
                    drawablecells[cellidx] = true; //Dont do main path
                    cellidx = nextcell;
                    if (r.Next(0,10) == 5) //Randomly create dead ends
                    {
                        break;
                    }
                }
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
        public void Draw()
        {
            for (int y = height-1; y > -1; --y)
            {
                for (int x = 0; x < width; ++x)
                {
                    if (mainpathcells[x+(y*width)])
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write('█');
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (drawablecells[x + (y*width)])
                    {
                        Console.Write('█');
                    }
                    else if (claimedcells[x + (y*width)])
                    {
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.Write(' ');
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