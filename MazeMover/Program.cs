using System.Linq;

namespace MazeMover
{
    public enum Direction
    {
        Left,
        Up,
        Right,
        Down,
        None
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;


            Maze maze = new Maze(100,100);
            maze.GenerateMaze(10);
            maze.Draw(false);
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

        public List<MazeConnection> walls = new List<MazeConnection>();
        public bool[] claimedcells;
        public void CreateWall(Direction direction, int position)
        {
            int bottomleftcorner = position + position / width;
            int topleftcorner = bottomleftcorner + width + 1;
            int toprightcorner = topleftcorner + 1;
            int bottomrightcorner = toprightcorner - width - 1;

            switch (direction)
            {
                case Direction.Left:
                    walls.Add(new MazeConnection(bottomleftcorner, topleftcorner));
                    break;
                case Direction.Up:
                    walls.Add(new MazeConnection(topleftcorner, toprightcorner));
                    break;
                case Direction.Right:
                    walls.Add(new MazeConnection(bottomrightcorner, toprightcorner));
                    break;
                case Direction.Down:
                    walls.Add(new MazeConnection(bottomleftcorner, bottomrightcorner));
                    break;
            }
        }
        public void RemoveWall(Direction direction, int position)
        {
            int bottomleftcorner = position + position / width;
            int topleftcorner = bottomleftcorner + width + 1;
            int toprightcorner = topleftcorner + 1;
            int bottomrightcorner = toprightcorner - width - 1;

            switch (direction)
            {
                case Direction.Left:
                    walls.RemoveAll(m => m.point1 == bottomleftcorner && m.point2 == topleftcorner); 
                    break;
                case Direction.Up:
                    walls.RemoveAll(m => m.point1 == topleftcorner && m.point2 == toprightcorner); 
                    break;
                case Direction.Right:
                    walls.RemoveAll(m=>m.point1 == bottomrightcorner && m.point2 == toprightcorner);
                    break;
                case Direction.Down:
                    walls.RemoveAll(m=>m.point1 == bottomleftcorner && m.point2 == bottomrightcorner);
                    break;
            }
        }
        public Maze(int width, int height)
        {
            this.width = width;
            this.height = height;
            claimedcells = new bool[width*height];
        }
        public void GenerateMaze(int genseed)
        {
            //Begin by generating correct path, starting at 0,0
            bool mazesolved = false;
            Random r = new Random(genseed);
            Direction lastdirection = Direction.None;

            int cellidx = 0;
            //Draw first cell
            CreateWall(Direction.Left, cellidx);
            CreateWall(Direction.Up, cellidx);
            CreateWall(Direction.Right, cellidx);

            int endmazeidx = 0;

            for (int i = 0; i < width * height; ++i)
            {
                //Check to see if there is an available next path
                if (((cellidx + 1) % width == 0 || claimedcells[cellidx + 1]) //Is there a claimed cell to the right? Or on right edge
                  && (cellidx == 0 || (cellidx - 1) % width == width - 1 || claimedcells[cellidx - 1]) //Is there a claimed cell to the left? Or on the left edge
                  && (cellidx + width > width * (height - 1) || claimedcells[cellidx + width]) //Is there a cell to the top? Or on the top edge
                  && (cellidx <= width || claimedcells[cellidx - width]) //Is there a cell to the bottom? Or on the bottom edge
                  ) //No possible next step?
                {
                    return;
                    break;
                }
                Direction nextdirection = Direction.None;
                do
                {
                    var random = r.Next(0, 6);
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
                        case 4:
                        case 5:
                            nextdirection = lastdirection; //Weight more heavily to travelling in the same direction
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
                                                              //Establishes bounds
                int nextcell = 0;
                switch (nextdirection)
                {
                    case Direction.Left:
                        nextcell = cellidx - 1;
                        RemoveWall(Direction.Right, nextcell);
                        CreateWall(Direction.Left, nextcell);
                        CreateWall(Direction.Up, nextcell);
                        CreateWall(Direction.Down, nextcell);
                        break;
                    case Direction.Up:
                        nextcell = cellidx + width;
                        RemoveWall(Direction.Down, nextcell);
                        CreateWall(Direction.Left, nextcell);
                        CreateWall(Direction.Up, nextcell);
                        CreateWall(Direction.Right, nextcell);
                        break;
                    case Direction.Right:
                        nextcell = cellidx + 1;
                        //Our right is the nextcells left, so remove the wall allowing us to travel between the cells
                        RemoveWall(Direction.Left, nextcell);

                        //Create walls on the up/down
                        CreateWall(Direction.Up, nextcell);
                        CreateWall(Direction.Down, nextcell);

                        //The nextcells right creation will create a dead end
                        CreateWall(Direction.Right, nextcell);
                        break;
                    case Direction.Down:
                        nextcell = cellidx - width;
                        //Our down is the nextcells up, so remove the wall allowing us to travel between the cells
                        RemoveWall(Direction.Up, nextcell);

                        //Add walls on the left and right
                        CreateWall(Direction.Left, nextcell);
                        CreateWall(Direction.Right, nextcell);

                        //The nextcells down will be the last wall, creating a dead end that can be changed later if another path is added
                        CreateWall(Direction.Down, nextcell);
                        break;
                    case Direction.None:
                        break;
                } //Find position of nextcell and empty out the position of the new wall
                claimedcells[cellidx] = true;
                cellidx = nextcell;
                //Console.WriteLine();
                //Console.WriteLine("Iteration: " + (i+1).ToString() + " Moved: " + nextdirection.ToString());
                //Draw();

                if (((cellidx % width) == width - 1 && nextdirection == Direction.Right)    //Have we hit the right wall?
                    || ((cellidx / width) == height - 1 && nextdirection == Direction.Up))
                {
                    RemoveWall(nextdirection, cellidx); //Create an end to the maze
                    endmazeidx = cellidx;
                    break;
                }
            }
            //return;
            //Generate incorrect paths
            for (int i = 0; i < width*height; ++i)
            {
                if (i == 77)
                {

                }
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
                        var random = r.Next(0, 6);
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
                            case 4:
                            case 5:
                                nextdirection = lastdirection; //Weight more heavily to travelling in the same direction
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
                            RemoveWall(Direction.Right, nextcell);
                            CreateWall(Direction.Left, nextcell);
                            CreateWall(Direction.Up, nextcell);
                            CreateWall(Direction.Down, nextcell);
                            break;
                        case Direction.Up:
                            nextcell = cellidx + width;
                            RemoveWall(Direction.Down, nextcell);
                            CreateWall(Direction.Left, nextcell);
                            CreateWall(Direction.Up, nextcell);
                            CreateWall(Direction.Right, nextcell);
                            break;
                        case Direction.Right:
                            nextcell = cellidx + 1;
                            //Our right is the nextcells left, so remove the wall allowing us to travel between the cells
                            RemoveWall(Direction.Left, nextcell);

                            //Create walls on the up/down
                            CreateWall(Direction.Up, nextcell);
                            CreateWall(Direction.Down, nextcell);

                            //The nextcells right creation will create a dead end
                            CreateWall(Direction.Right, nextcell);
                            break;
                        case Direction.Down:
                            nextcell = cellidx - width;
                            //Our down is the nextcells up, so remove the wall allowing us to travel between the cells
                            RemoveWall(Direction.Up, nextcell);

                            //Add walls on the left and right
                            CreateWall(Direction.Left, nextcell);
                            CreateWall(Direction.Right, nextcell);

                            //The nextcells down will be the last wall, creating a dead end that can be changed later if another path is added
                            CreateWall(Direction.Down, nextcell);
                            break;
                        case Direction.None:
                            break;
                    } //Find position of nextcell and empty out the position of the new wall
                    claimedcells[cellidx] = true;
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
            public void Draw(bool debug)
        {
            List<MazeConnection> distinctwalls = walls.Copy();

            for (int y = height; y > -1; --y)
            {
                for (int x = 0; x < width + 1; ++x)
                {
                    int i = x + y * (width + 1);

                    if (i % (width + 1) == 0)
                    {
                        Console.Write("\n");
                    }
                    if (i == 13)
                    {

                    }
                    var connections = distinctwalls.Where(w => w.point1 == i).ToList();
                    if (connections.Count() != 0)
                    {
                        MazeConnection priorityconnection = null;
                        foreach (var connection in connections)
                        {
                            //var connection = connections.First();
                            if(Math.Abs(connection.point2 - connection.point1) == width+1) //Bottom below the top, we are point1
                            {
                                priorityconnection = connection;
                                continue; //Prioritize vertical walls
                            }
                            else if (Math.Abs(connection.point1 - connection.point2) == 1) //Horizontal means 1 space apart

                            {
                                //Horizontal
                                if (priorityconnection == null)
                                {
                                    priorityconnection = connection;
                                }
                                continue;
                            }
                            distinctwalls.Remove(connection);
                        }
                        if (Math.Abs(priorityconnection.point2 - priorityconnection.point1) == width+1) //Bottom below the top, we are point1
                        {
                            DrawWall(WallType.Wall, i);
                        }
                        else if (Math.Abs(priorityconnection.point1 - priorityconnection.point2) == 1) //Horizontal means 1 space apart
                        {
                            //Horizontal
                            DrawWall(WallType.DoubleFloor, i);
                        }
                    }
                    else
                    {
                        if (distinctwalls.Where(w => w.point1 == i && w.point2 == i + 1).FirstOrDefault() != null) //next is a wall
                        {
                            //Start drawing start of wall
                            DrawWall(WallType.DoubleFloor, i);
                        }
                        else if (distinctwalls.Where(w => w.point2 == i && w.point1 == i - 1).FirstOrDefault() != null) //next is a wall
                        {
                            //Start drawing start of wall
                            DrawWall(WallType.Floor, i);
                            DrawWall(WallType.Space, i);
                        }
                        else
                        {
                            if (debug)
                            {
                                DrawWall(WallType.DebugDot, i);
                            }
                            else
                            {
                                DrawWall(WallType.Space, i);
                            }
                            DrawWall(WallType.Space, i);
                        }
                    }
                }
            }
            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine("Wall positions: ");
                foreach (var wall in walls)
                {
                    Console.WriteLine(String.Format("{0}:{1}", wall.point1, wall.point2));
                }
            }
        }

        public void DrawWall(WallType wallType, int position)
        {
            if (position == 39)
            {

            }
            bool cell = position>=width*(height-1) || position%(width+1) == width ? true: claimedcells[(width) * ((position / (width + 1))) + (position % (width + 1))]; //Dots on the top row cannot be a floor, will always be a roof, therefore return true to disable full blocks
            bool cellabove = position < (height-1)*width && position % (width + 1) != width ? claimedcells[(width) * ((position / (width + 1)) + 1) + (position % (width + 1))] : true; //values at the top of the maze will always have a claimed roof
            switch (wallType)
            {
                case WallType.Floor:
                    //Start drawing start of wall
                    if (cell == false &&
                        cellabove == true &&
                        (walls.Where(w => w.point1 == position + width + 1 && w.point2 == position + width + 2).FirstOrDefault() != null))
                    {
                           Console.Write("█");
                        //Console.Write("_");
                    }
                    else
                    {
                        Console.Write("▄");
                    }
                    break;
                case WallType.DoubleFloor:
                    if (cell == false &&
                        cellabove == true &&
                        (walls.Where(w => w.point1 == position + width + 1 && w.point2 == position + width + 2).FirstOrDefault() != null))
                    {
                        Console.Write("██");
                       // Console.Write("__");
                    }
                    else
                    {
                        Console.Write("▄▄");
                    }
                    break;
                case WallType.Wall:
                    //Console.Write("|");
                    Console.Write("█");

                    if (walls.Where(w => w.point1 == position && w.point2 == position + 1).FirstOrDefault() != null) //next is a wall
                    {
                        //Start drawing start of wall
                        DrawWall(WallType.Floor, position);
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    break;
                case WallType.DebugDot:
                    Console.Write(".");
                    break;
                case WallType.Space:
                    Console.Write(' ');
                    break;
            }
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