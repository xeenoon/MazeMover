using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MazeMover
{

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
        public int width;
        public int height;

        public int mazeendidx;
        public int seed;

        //Enter at 0,0
        //Exit at width,height (top right)

        //public List<MazeConnection> walls = new List<MazeConnection>();
        public bool[] claimedcells;
        //public bool[] drawablecells;
        public Maze(int width, int height, int seed)
        {
            this.width = width;
            this.height = height;
            this.seed = seed;
            claimedcells = new bool[width * height];
        }
        public Maze Copy()
        {
            Maze m = new Maze(this.width, this.height, seed);
            m.claimedcells = claimedcells.ToList().Copy().ToArray();
            m.mazeendidx = mazeendidx;
            return m;
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
            if (lastdirection == Direction.None) //First time?
            {
                recursions = 0;
            }
            ++recursions;
            if (recursions > width * height) //Searched every cell to no avail?
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
            if (cellposition == mazeendidx)
            {
                //End of path reached
                return true;
            }
            Direction availableDirections = Direction.None;

            availableDirections |= (lastdirection != Direction.Right && cellposition % width != 0 && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (lastdirection != Direction.Left && cellposition % width != width - 1 && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
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
                    return true;
                }
            }
            return false; //Failed path
        }

        public bool SolveMaze(int cellposition, Direction lastdirection, ref int end)
        {
            if (lastdirection == Direction.None) //First time?
            {
                recursions = 0;
            }
            ++recursions;
            if (recursions > width * height) //Searched every cell to no avail?
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
                end = cellposition;
                //End of path reached
                return true;
            }
            Direction availableDirections = Direction.None;

            availableDirections |= (lastdirection != Direction.Right && cellposition % width != 0 && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (lastdirection != Direction.Left && cellposition % width != width - 1 && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
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
                visitedcells = new bool[width * height];
                recursions = 0;
            }
            visitedcells[cellposition] = true; //Mark the cell as visited
            if (recursions > width * height) //Searched every cell to no avail?
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

            availableDirections |= (cellposition % width != 0 && !visitedcells[cellposition - 1] && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (cellposition % width != width - 1 && !visitedcells[cellposition + 1] && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
            availableDirections |= (cellposition >= width && !visitedcells[cellposition - width] && GetCell(cellposition - width) ? Direction.Down : Direction.None);
            availableDirections |= (cellposition <= (height - 1) * width && !visitedcells[cellposition + width] && GetCell(cellposition + width) ? Direction.Up : Direction.None);

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
        public bool SolveMaze(int cellposition, Direction lastdirection, int destination, ref List<int> result) //Returns a list of indexes
        {
            if (lastdirection == Direction.None) //First time?
            {
                visitedcells = new bool[width * height];
                recursions = 0;
            }
            visitedcells[cellposition] = true; //Mark the cell as visited
            if (recursions > width * height) //Searched every cell to no avail?
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
            if (cellposition + width == destination || cellposition - width == destination || cellposition + 1 == destination || cellposition - 1 == destination)
            {
                result.Add(cellposition);
                return true;
            }
            Direction availableDirections = Direction.None;

            availableDirections |= (cellposition % width != 0 && !visitedcells[cellposition - 1] && GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (cellposition % width != width - 1 && !visitedcells[cellposition + 1] && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
            availableDirections |= (cellposition >= width && !visitedcells[cellposition - width] && GetCell(cellposition - width) ? Direction.Down : Direction.None);
            availableDirections |= (cellposition <= (height - 1) * width && !visitedcells[cellposition + width] && GetCell(cellposition + width) ? Direction.Up : Direction.None);

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
            if (recursions > width * height)
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
        public bool FindPlausiblePaths(bool firstiteration, int cellposition, Direction lastdirection, ref List<List<int>> result, ref List<int> currentpathcells, int lastdistancetravelled = 0, int totaldistancetravelled = 0)
        {
            if (lastdirection == Direction.None)
            {
                visitedcells = new bool[width * height];
                recursions = 0;
            }
            if (totaldistancetravelled == 20) //Dont look ahead more than 20 squares
            {
                return false;
            }
            ++recursions;
            bool placed = false;
            if (recursions > width * height)
            {
                return false;
            }
            int lastcellposition = cellposition;
            visitedcells[cellposition] = true;

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
            if ((lastdistancetravelled >= 10 || cellposition == mazeendidx) && !firstiteration)
            {
                currentpathcells.Add(cellposition);
                placed = true;
            }

            Direction availableDirections = Direction.None;

            availableDirections |= (lastdirection != Direction.Right && cellposition % width != 0 && !visitedcells[cellposition-1]&& GetCell(cellposition - 1) ? Direction.Left : Direction.None);
            availableDirections |= (lastdirection != Direction.Left && cellposition % width != width - 1 && !visitedcells[cellposition + 1] && GetCell(cellposition + 1) ? Direction.Right : Direction.None);
            availableDirections |= (lastdirection != Direction.Up && cellposition >= width && !visitedcells[cellposition - width] && GetCell(cellposition - width) ? Direction.Down : Direction.None);
            availableDirections |= (lastdirection != Direction.Down && cellposition <= (height - 1) * width && !visitedcells[cellposition + width] && GetCell(cellposition + width) ? Direction.Up : Direction.None);

            var matching = Enum.GetValues(typeof(Direction))
               .Cast<Direction>()
               .Where(c => (availableDirections & c) == c && c != Direction.None)
               .ToArray();
            if (matching.Count() == 1 )
            {
                ++lastdistancetravelled;
            }
            else if (matching.Count() >= 2) //New intersection
            {
                if (firstiteration)
                {
                    ++lastdistancetravelled;
                }
                else
                {
                    //Search the available paths to determine how many plausible paths there are

                    List<List<int>> paths = new List<List<int>>();
                    List<int> temp = new List<int>();
                    FindPlausiblePaths(true, lastcellposition, lastdirection, ref paths, ref temp, lastdistancetravelled-1, totaldistancetravelled - 1); //Search a new path

                    if (paths.Count() == 1) //Only one good path?
                    {
                        //Add it
                        placed = true;
                        currentpathcells.AddRange(paths[0]);
                        currentpathcells.Add(cellposition);
                    }
                    else if (paths.Count() >= 2) //More than one good path?
                    {
                        //Add the intersection square, and thats it
                        placed = true;
                        currentpathcells.Add(cellposition);
                    }
                    //If there are no good paths, then dont add this square

                    return placed; //End path here
                }
            }
            else
            {
                lastdistancetravelled = 0;
            }
            ++totaldistancetravelled;
            foreach (var match in matching)
            {
                if (firstiteration)
                {
                    List<int> toadd = new List<int>();
                    FindPlausiblePaths(false, cellposition, match, ref result, ref toadd, lastdistancetravelled, totaldistancetravelled); //Search a new path
                    if (toadd.Count() != 0)
                    {
                        result.Add(toadd);
                    }
                }
                else if (FindPlausiblePaths(false, cellposition, match, ref result, ref currentpathcells, lastdistancetravelled, totaldistancetravelled)) //Was the one after me set?
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

            int nextwall = playerpath[r.Next(0, playerpath.Count())];
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
                if (idx % width < width - 2 && solutionbranches.Contains(idx + 2)) //Two to the right?
                {
                    possiblewallbreakages.Add(idx + 1); //Break wall one to the right
                }
                if (idx % width >= 2 && solutionbranches.Contains(idx - 2)) //Two to the Left?
                {
                    possiblewallbreakages.Add(idx - 1); //Break wall one to the left
                }
                if (idx / width <= (height - 2) * width && solutionbranches.Contains(idx + width + width)) //Two up?
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
                    }
                }
                claimedcells[cellidx] = true;
                if (issolution)
                {
                }
                cellidx += directionmodifier;
                travelledDirections.Add(nextdirection);

                if (((cellidx % width) == width - 1)    //Have we found a solution
                    || ((cellidx / width) == height - 1))
                {
                    if (issolution)
                    {
                        claimedcells[cellidx] = true;
                        mazeendidx = cellidx;
                    }
                    return;
                }
            }

        }
        public void GenerateMaze()
        {
            //Begin by generating correct path, starting at 0,0
            Random r = new Random(seed);
            //Draw first cell
            claimedcells[0] = true;


            SearchPath(width, true, r); //Create a solution to the maze

            //Generate incorrect paths
            for (int i = 0; i < ((width + height) * (width + height)) / 2; ++i)
            {
                int cellidx;
                do
                {
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
        }
        public void Draw()
        {
            for (int y = height - 1; y > -1; --y)
            {
                for (int x = 0; x < width; ++x)
                {
                    if (mazeendidx == x+ (y*width))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("++");
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else if (claimedcells[x + (y * width)])
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
        public string GetString()
        {
            string result = "";
            for (int y = height - 1; y > -1; --y)
            {
                for (int x = 0; x < width; ++x)
                {
                    if (claimedcells[x + (y * width)])
                    {

                        result += "+";
                    }
                    else
                    {
                        result += " ";
                    }
                }
                result += "\n";
            }
            return result;
        }

    }
}
