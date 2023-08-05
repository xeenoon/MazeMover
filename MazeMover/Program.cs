﻿using System.IO;
using System.Linq;
using System.Net;

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
        public static int placements = 0;
        static List<ConsoleColor> consoleColors = new List<ConsoleColor>() 
        {
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.Gray,
            ConsoleColor.DarkGray,
            ConsoleColor.Blue,
            ConsoleColor.Green,
            ConsoleColor.Cyan,
            ConsoleColor.Red,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
        };
        static List<ConsoleColor> chosenColors = new List<ConsoleColor>();
        static void Main(string[] args)
        {
            int characterposition = 0;
            Console.ReadKey();
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            List<List<int>> setpaths = new List<List<int>>();
            while (true) 
            {
                int width = Console.WindowWidth / 2;
                int height = Console.WindowHeight - 2;
                Random r = new Random(1);
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

                    //Move the maze
                    placements++;
                    if (placements == 29)
                    {

                    }
                    //Find plausible paths from player position
                    //Remove old ones
                    Console.BackgroundColor = ConsoleColor.Black;
                    foreach (var path in setpaths)
                    {
                        foreach (var square in path)
                        {
                            if (square != characterposition)
                            {
                                Console.CursorLeft = (square % width) * 2;
                                Console.CursorTop = (height) - (square / width) - 1;
                                Console.Write("  ");
                            }
                        }
                    }
                    //Add new ones
                    setpaths.Clear();
                    maze.recursions = 0;
                    List<int> fixedpaths = null; //Optional paramater to force include a path
                    maze.FindPlausiblePaths(characterposition, Direction.None, ref setpaths, ref fixedpaths);
                    chosenColors.Clear(); //Used to ensure every path has a unique color
                    foreach (var path in setpaths)
                    {
                        ConsoleColor chosencolor;
                        do
                        {
                            chosencolor = consoleColors[r.Next(0, consoleColors.Count())];
                        } while (chosenColors.Contains(chosencolor));
                        chosenColors.Add(chosencolor);
                        Console.BackgroundColor = chosencolor;
                        foreach (var square in path)
                        {
                            if (square != characterposition)
                            {
                                Console.CursorLeft = (square % width) * 2;
                                Console.CursorTop = (height) - (square / width) - 1;
                                Console.Write("  ");
                            }
                        }
                    }
                    Console.BackgroundColor = ConsoleColor.Black;


              //     Maze.ChangedWall changedwall = maze.MoveWall(characterposition, r); //Random seeding is possible
              //     Console.CursorLeft = (changedwall.removedwall % width) * 2;
              //     Console.CursorTop = height - (changedwall.removedwall / width) - 1;
              //     Console.Write("||"); //Remove the wall
              //
              //     Console.CursorLeft = (changedwall.placedwall % width) * 2;
              //     Console.CursorTop = height - (changedwall.placedwall / width) - 1;
              //
              //     Console.ForegroundColor = ConsoleColor.Green;
              //     Console.Write("██"); //Add the new wall
              //     Console.ForegroundColor = ConsoleColor.White;
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
        public struct ChangedWall
        {
            public int removedwall;
            public int placedwall;
        }
        int width;
        int height;

        int mazeendidx;

        //Enter at 0,0
        //Exit at width,height (top right)

        //public List<MazeConnection> walls = new List<MazeConnection>();
        public bool[] claimedcells;
        public bool[] mainpathcells;
        public bool[] drawablecells;
        public bool[] solutioncells;
        public Maze(int width, int height)
        {
            this.width = width;
            this.height = height;
            claimedcells = new bool[width * height];
            mainpathcells = new bool[width * height];
            drawablecells = new bool[width * height];
            solutioncells = new bool[width * height];
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
        public bool SolveMaze(int cellposition, Direction lastdirection)
        {
            switch (lastdirection)
            {
                case Direction.Left:
                    cellposition--;
                    break;
                case Direction.Up:
                    cellposition += width;
                    break;
                case Direction.Right:
                    cellposition++;
                    break;
                case Direction.Down:
                    cellposition -= width;
                    break;
                case Direction.None:
                    break;
            }
            if (cellposition % width == width - 1 || cellposition >= (height-1) * width)
            {
                //End of path reached
                solutioncells[cellposition] = true;
                return true;
            }
            Direction availableDirections = Direction.None;

            availableDirections |= (lastdirection != Direction.Right && cellposition % width != 0 && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (lastdirection != Direction.Left && cellposition % width != width-1 && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
            availableDirections |= (lastdirection != Direction.Up && cellposition >= width && GetCell(cellposition - width) ? Direction.Down : Direction.None);
            availableDirections |= (lastdirection != Direction.Down && cellposition <= (height - 1) * width && GetCell(cellposition + width) ? Direction.Up : Direction.None);

            var matching = Enum.GetValues(typeof(Direction))
               .Cast<Direction>()
               .Where(c => (availableDirections & c) == c && c != Direction.None)    // or use HasFlag in .NET4
               .ToArray();

            foreach (var match in matching)
            {
                if (SolveMaze(cellposition, match)) //Move to that square
                {
                    //Highlight this square
                    solutioncells[cellposition] = true;
                    return true;
                }
            }
            return false; //Failed path
        }

        public int recursions = 0;
        bool[] visitedcells;
        public bool SolveMaze(int cellposition, Direction lastdirection, ref List<int> result) //Returns a list of indexes
        {
            if (lastdirection == Direction.None) //First time?
            {
                visitedcells = new bool[width*height];
            }
            visitedcells[cellposition] = true; //Mark the cell as visited
            if (recursions > width*height) //Searched every cell to no avail?
            {
                return false;
            }
            switch (lastdirection)
            {
                case Direction.Left:
                    cellposition--;
                    break;
                case Direction.Up:
                    cellposition += width;
                    break;
                case Direction.Right:
                    cellposition++;
                    break;
                case Direction.Down:
                    cellposition -= width;
                    break;
                case Direction.None:
                    break;
            }
            if (cellposition % width == width - 1 || cellposition >= (height - 1) * width)
            {
                result.Add(cellposition);
                return true;
            }
            Direction availableDirections = Direction.None;

            availableDirections |= (!visitedcells[cellposition - 1]     && cellposition % width != 0 && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (!visitedcells[cellposition + 1]     && cellposition % width != width - 1 && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
            availableDirections |= (!visitedcells[cellposition - width] && cellposition >= width && GetCell(cellposition - width) ? Direction.Down : Direction.None);
            availableDirections |= (!visitedcells[cellposition + width] && cellposition <= (height - 1) * width && GetCell(cellposition + width) ? Direction.Up : Direction.None);

            var matching = Enum.GetValues(typeof(Direction))
               .Cast<Direction>()
               .Where(c => (availableDirections & c) == c && c != Direction.None)    // or use HasFlag in .NET4
               .ToArray();

            foreach (var match in matching)
            {
                ++recursions;
                if (SolveMaze(cellposition, match, ref result)) //Move to that square
                {
                    //Highlight this square
                    result.Add(cellposition);
                    //solutioncells[cellposition] = true;
                    return true; //Maze is solved, we can stop searching now
                }
            }
            return false;
        }
        public void FindAllPaths(int cellposition, Direction lastdirection, ref List<int> result) //Returns a list of indexes
        {
            ++recursions;
            if (recursions > width*height)
            {
                return;
            }
            switch (lastdirection)
            {
                case Direction.Left:
                    cellposition--;
                    break;
                case Direction.Up:
                    cellposition += width;
                    break;
                case Direction.Right:
                    cellposition++;
                    break;
                case Direction.Down:
                    cellposition -= width;
                    break;
                case Direction.None:
                    break;
            }
            result.Add(cellposition);
             
            Direction availableDirections = Direction.None;

            availableDirections |= (lastdirection != Direction.Right && cellposition % width != 0 && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (lastdirection != Direction.Left && cellposition % width != width - 1 && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
            availableDirections |= (lastdirection != Direction.Up && cellposition >= width && GetCell(cellposition - width) ? Direction.Down : Direction.None);
            availableDirections |= (lastdirection != Direction.Down && cellposition <= (height - 1) * width && GetCell(cellposition + width) ? Direction.Up : Direction.None);

            var matching = Enum.GetValues(typeof(Direction))
               .Cast<Direction>()
               .Where(c => (availableDirections & c) == c && c != Direction.None)
               .ToArray();

            foreach (var match in matching)
            {
                FindAllPaths(cellposition, match, ref result);
            }
        }
        public bool FindPlausiblePaths(int cellposition, Direction lastdirection, ref List<List<int>> result, ref List<int> currentpathcells, int lastdistancetravelled = 0, int totaldistancetravelled = 0)
        {
            if(totaldistancetravelled == 20) //Dont look ahead more than 10 squares
            {
                return false;
            }
            ++recursions;
            bool placed = false;
            if (recursions > width * height)
            {
                return false;
            }
            switch (lastdirection)
            {
                case Direction.Left:
                    cellposition--;
                    break;
                case Direction.Up:
                    cellposition += width;
                    break;
                case Direction.Right:
                    cellposition++;
                    break;
                case Direction.Down:
                    cellposition -= width;
                    break;
                case Direction.None:
                    break;
            }
            if ((lastdistancetravelled >= 4) && lastdirection != Direction.None)
            {
                currentpathcells.Add(cellposition);
                placed = true;
            }

            Direction availableDirections = Direction.None;

            availableDirections |= (lastdirection != Direction.Right && cellposition % width != 0 && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (lastdirection != Direction.Left && cellposition % width != width - 1 && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
            availableDirections |= (lastdirection != Direction.Up && cellposition >= width && GetCell(cellposition - width) ? Direction.Down : Direction.None);
            availableDirections |= (lastdirection != Direction.Down && cellposition <= (height - 1) * width && GetCell(cellposition + width) ? Direction.Up : Direction.None);

            var matching = Enum.GetValues(typeof(Direction))
               .Cast<Direction>()
               .Where(c => (availableDirections & c) == c && c != Direction.None)
               .ToArray();
            if (matching.Count() != 0)
            {
                ++lastdistancetravelled;
            }
            else
            {
                lastdistancetravelled = 0;
            }
            ++totaldistancetravelled;
            foreach (var match in matching)
            {
                if (lastdirection == Direction.None)
                {
                    List<int> toadd = new List<int>();
                    FindPlausiblePaths(cellposition, match, ref result, ref toadd, lastdistancetravelled, totaldistancetravelled); //Search a new path
                    result.Add(toadd);
                }
                else if (FindPlausiblePaths(cellposition, match, ref result, ref currentpathcells, lastdistancetravelled, totaldistancetravelled)) //Was the one after me set?
                {
                    currentpathcells.Add(cellposition);
                    placed = true; //I should be set to
                }
            }
            return placed;
        }
        public ChangedWall MoveWall(int playerposition, Random r)
        {
            ChangedWall result = new ChangedWall();
            // Steps:
            // Solve maze to find shortest solution from playerposition
            // Choose a random place on the main path, ahead of the player, fill this in with a wall
            // Work backwards from solution to find all paths accessable from the solution
            // Work fowards from the player to find all paths accessable from the player
            // Find two squares that are 1 square away (-2,+2,-2*width,+2*width)

            List<int> playerpath = new List<int>();
            recursions = 0;
            SolveMaze(playerposition, Direction.None, ref playerpath); //Step 1
            playerpath.Remove(playerposition);

            int nextwall = playerpath[r.Next(0,playerpath.Count())];
            claimedcells[nextwall] = false; //Step 2
            result.placedwall = nextwall;

            List<int> solutionbranches = new List<int>();
            recursions = 0;
            FindAllPaths(mazeendidx, Direction.None, ref solutionbranches); //Step 3

            List<int> playerbranches = new List<int>();
            recursions = 0;
            FindAllPaths(playerposition, Direction.None, ref playerbranches); //Step 4

            List<int> possiblewallbreakages = new List<int>();

            //Step 5: Compare the two
            for (int i = 0; i < playerbranches.Count(); ++i)
            {
                int idx = playerbranches[i];
                if (idx%width < width - 2 && solutionbranches.Contains(idx + 2)) //Two to the right?
                {
                    possiblewallbreakages.Add(idx + 1); //Break wall one to the right
                }
                if (idx % width >= 2 && solutionbranches.Contains(idx - 2)) //Two to the Left?
                {
                    possiblewallbreakages.Add(idx - 1); //Break wall one to the left
                }
                if (idx / width <= (height-2) * width && solutionbranches.Contains(idx + width + width)) //Two up?
                {
                    possiblewallbreakages.Add(idx + width); //Break wall one up
                }
                if (idx > width + width && solutionbranches.Contains(idx - width - width)) //Two down?
                {
                    possiblewallbreakages.Add(idx - width); //Break wall one down
                }
            }
            if (possiblewallbreakages.Count() == 0) //Created a path of no return?
            {
                claimedcells[nextwall] = true; //reset the previous step
                return MoveWall(playerposition, r);
            }
            //Choose a random wall to break
            int breakidx = possiblewallbreakages[r.Next(0, possiblewallbreakages.Count())];
            claimedcells[breakidx] = true;
            result.removedwall = breakidx;
            return result;
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
                travelledDirections.Add(nextdirection);

                if (((cellidx % width) == width - 1)    //Have we found a solution
                    || ((cellidx / width) == height - 1))
                {
                    if (issolution)
                    {
                        claimedcells[cellidx] = true;
                        mainpathcells[cellidx] = true;
                        drawablecells[cellidx] = true;
                        mazeendidx = cellidx;
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
                    if (solutioncells[x+(y*width)])
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("██");
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else if (mainpathcells[x+(y*width)])
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