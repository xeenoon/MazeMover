using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;

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
        static float AI_movespeed = 200;
        static bool unsolvablemaze = false;
        static System.Timers.Timer incAIspeed = new System.Timers.Timer();

        public static bool dieinbackground = false;

        static void Main(string[] args)
        {
            int AI_position = 0;
            int player_position = 0;
            Console.ReadKey();
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            List<List<int>> setpaths = new List<List<int>>();
            while (true) 
            {
                int width = Console.WindowWidth / 2;
                int height = Console.WindowHeight - 2;
                Random r = new Random();
                int seed = r.Next();
                Maze maze = new Maze(width, height, seed);
                maze.GenerateMaze();
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();

                maze.Draw();
                solvingmaze = maze;
                Console.ForegroundColor = ConsoleColor.White;
                //Draw AI position
                Console.CursorLeft = (AI_position % width) * 2; //Maze is double width
                Console.CursorTop = height - (AI_position / width) - 1;
                Console.Write("()");

                //Player movement
                bool selecting = false;
                int selectedwall = -1;
                ConsoleColor playercolor = ConsoleColor.Cyan;
                AI ai = new AI(0, maze.Copy());

                Task.Factory.StartNew(() =>
                {
                    player_position = (maze.height-1) * maze.width;

                    while (AI_position != maze.mazeendidx)
                    {
                        if (dieinbackground)
                        {
                            break;
                        }

                        ConsoleColor backcolor = ConsoleColor.Black;
                        if (!maze.GetCell(player_position)) //Wall
                        {
                            backcolor = ConsoleColor.White;
                            if (selecting)
                            {
                                playercolor = ConsoleColor.Green;
                                unsolvablemaze = false;
                            }
                        }
                        if (player_position == selectedwall) //Selected wall
                        {
                            backcolor = ConsoleColor.Yellow;
                        }

                        if (selecting && maze.GetCell(player_position))
                        {
                            //Create a new maze and simulate the move
                            Maze copy = maze.Copy();
                            copy.claimedcells[selectedwall]    = true;
                            copy.claimedcells[player_position] = false;
                            if (copy.SolveMaze(AI_position, Direction.None)) //Is the maze solveable?
                            {
                                solvingmaze = copy;
                                ShowSolution(AI_position);
                                playercolor = ConsoleColor.Green;
                                unsolvablemaze = false;
                            }
                            else
                            {
                                playercolor = ConsoleColor.Red;
                                HideSolution();
                                solvingmaze = maze;
                                unsolvablemaze = true;
                            }
                        }

                        queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "()", playercolor, backcolor));
                        var input = Console.ReadKey(true).Key;
                        if (player_position == AI_position)
                        {
                            queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "()", ConsoleColor.White, backcolor));
                        }
                        if (solutionpositions.Contains(player_position))
                        {
                            queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "++", ConsoleColor.DarkGreen, backcolor));
                        }
                        else if(player_position == maze.mazeendidx)
                        {
                            queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "++", ConsoleColor.White, backcolor));
                        }
                        else
                        {
                            queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "  ", ConsoleColor.Black, backcolor));
                        }
                        switch (input)
                        {
                            case ConsoleKey.W:
                            case ConsoleKey.UpArrow:
                                if (player_position / width < height - 1 && player_position + width != AI_position && player_position + width != selectedwall) //Can we move up?
                                {
                                    player_position += width;
                                }
                                break;
                            case ConsoleKey.A:
                            case ConsoleKey.LeftArrow:
                                if (player_position % width >= 1 && player_position - 1  != AI_position && player_position - 1 != selectedwall) //Can we move left?
                                {
                                    player_position--;
                                }
                                break;
                            case ConsoleKey.S:
                            case ConsoleKey.DownArrow:
                                if (player_position >= width && player_position - width != AI_position && player_position - width != selectedwall) //Can we move down?
                                {
                                    player_position -= width;
                                }
                                break;
                            case ConsoleKey.D:
                            case ConsoleKey.RightArrow:
                                if (player_position % width < width - 1 && player_position + 1 != AI_position && player_position + 1 != selectedwall) //Can we move right?
                                {
                                    player_position++;
                                }
                                break;
                            case ConsoleKey.Enter:
                                //Show solution
                                if (selecting)
                                {
                                    selecting = false;

                                    queue.Enqueue(new ConsoleWriteInfo((selectedwall % maze.width) * 2, maze.height - (selectedwall / maze.width) - 1, "  ", ConsoleColor.White, ConsoleColor.White));

                                    solvingmaze = maze;
                                    HideSolution();
                                    playercolor = ConsoleColor.Cyan;
                                    queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "()", playercolor, backcolor));


                                    //Check if we can move a wall
                                    //Create a new maze and simulate the move
                                    Maze copy = maze.Copy();
                                    copy.claimedcells[selectedwall] = true;
                                    copy.claimedcells[player_position] = false;
                                    if (maze.GetCell(player_position) && copy.SolveMaze(AI_position, Direction.None)) //Moved a wall?
                                    {
                                        maze.claimedcells[player_position] = false;
                                        maze.claimedcells[selectedwall] = true;
                                        queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "  ", ConsoleColor.White, ConsoleColor.Black));
                                        queue.Enqueue(new ConsoleWriteInfo((selectedwall % maze.width) * 2, maze.height - (selectedwall / maze.width) - 1, "  ", ConsoleColor.Black, ConsoleColor.Black));
                                        ai.ChangeMaze(selectedwall, player_position, maze.Copy());
                                    }
                                    selectedwall = -1;
                                }
                                else
                                {
                                    if (maze.GetCell(player_position)) //Can only select walls
                                    {
                                        break;
                                    }
                                    selectedwall = player_position;

                                    selecting = true;
                                    solvingmaze = maze;
                                    ShowSolution(AI_position);
                                    playercolor = ConsoleColor.Green;
                                    queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "()", playercolor, ConsoleColor.Yellow));
                                }
                                break;
                        }
                    }
                                        
                }); 
                

                Task.Factory.StartNew(() =>
                {
                    incAIspeed = new System.Timers.Timer(1000);
                    incAIspeed.AutoReset = true;
                    incAIspeed.Elapsed += new System.Timers.ElapsedEventHandler(IncAISpeed);
                    incAIspeed.Start();

                    Stopwatch turntimer = new Stopwatch();

                    while (true)
                    {
                        if (dieinbackground)
                        {
                            break;
                        }

                        turntimer.Stop();
                        if (turntimer.ElapsedMilliseconds <= AI_movespeed)
                        {
                            Thread.Sleep((int)AI_movespeed - (int)turntimer.ElapsedMilliseconds);
                            //totalmovetime = totalmovetime * 0.9f;
                        }

                        turntimer.Restart();

                        //Remove last one
                        if (AI_position == player_position)
                        {
                            queue.Enqueue(new ConsoleWriteInfo((AI_position % width) * 2, height - (AI_position / width) - 1, "()", playercolor, ConsoleColor.Black));
                        }
                        else
                        {
                            queue.Enqueue(new ConsoleWriteInfo((AI_position % width) * 2, height - (AI_position / width) - 1, "  ", ConsoleColor.Black, ConsoleColor.Black));
                        }

                        //AI movement
                        ai.Move();
                        AI_position = ai.position;
                        lastAIpaths.Add(ai.position);

                        queue.Enqueue(new ConsoleWriteInfo((AI_position % width) * 2, height - (AI_position / width) - 1, "()", ConsoleColor.White, ConsoleColor.Black));
                        if (selecting && !unsolvablemaze)
                        {
                            ShowSolution(AI_position);
                        }
                        if (ai.position == maze.mazeendidx)
                        {
                            while (true)
                            {
                                //Stop the program
                            }
                        }
                    }
                });
                while (true)
                {
                    ConsoleWriteTick();
                }
            }
        }

        private static void IncAISpeed(object? sender, ElapsedEventArgs e)
        {
            AI_movespeed *= 0.99f;
        }

        private static void HideSolution()
        {
            foreach (int idx in solutionpositions)
            {
                if (solvingmaze.GetCell(idx))
                {
                    if (solvingmaze.mazeendidx == idx)
                    {
                        queue.Enqueue(new ConsoleWriteInfo((idx % solvingmaze.width) * 2, solvingmaze.height - (idx / solvingmaze.width) - 1, "++", ConsoleColor.White, ConsoleColor.Black));
                    }
                    else
                    {
                        queue.Enqueue(new ConsoleWriteInfo((idx % solvingmaze.width) * 2, solvingmaze.height - (idx / solvingmaze.width) - 1, "  ", ConsoleColor.Red, ConsoleColor.Black));
                    }
                }
                else
                {
                    queue.Enqueue(new ConsoleWriteInfo((idx % solvingmaze.width) * 2, solvingmaze.height - (idx / solvingmaze.width) - 1, "  ", ConsoleColor.White, ConsoleColor.White));
                }
            }
            solutionpositions.Clear();
        }

        static List<int> solutionpositions = new List<int>();
        static List<int> lastAIpaths = new List<int>();
        static Maze solvingmaze = null;
        private static void ShowSolution(int position)
        {
            List<int> newpositions = new List<int>();
            solvingmaze.SolveMaze(position, Direction.None, ref newpositions); //Step 1
            newpositions.Remove(position);
            if (newpositions.Count() == 0)
            {
                return;
            }

            foreach (var p in lastAIpaths)
            {
                if (newpositions.Contains(p))
                {
                    queue.Enqueue(new ConsoleWriteInfo((p % solvingmaze.width) * 2, solvingmaze.height - (p / solvingmaze.width) - 1, "++", ConsoleColor.DarkGreen, ConsoleColor.Black));
                }
            }

            foreach (int idx in solutionpositions.Where(p => !newpositions.Contains(p)))
            {
                if (solvingmaze.GetCell(idx))
                {
                    queue.Enqueue(new ConsoleWriteInfo((idx % solvingmaze.width) * 2, solvingmaze.height - (idx / solvingmaze.width) - 1, "  ", ConsoleColor.DarkGreen, ConsoleColor.Black));
                }
                else
                {
                    queue.Enqueue(new ConsoleWriteInfo((idx % solvingmaze.width) * 2, solvingmaze.height - (idx / solvingmaze.width) - 1, "  ", ConsoleColor.White, ConsoleColor.White));
                }
            }

            foreach (int idx in newpositions.Where(p => !solutionpositions.Contains(p)))
            {
                queue.Enqueue(new ConsoleWriteInfo((idx % solvingmaze.width) * 2, solvingmaze.height - (idx / solvingmaze.width) - 1, "++", ConsoleColor.DarkGreen, ConsoleColor.Black));
            }

            queue.Enqueue(new ConsoleWriteInfo((position % solvingmaze.width) * 2, solvingmaze.height - (position / solvingmaze.width) - 1, "()", ConsoleColor.White, ConsoleColor.Black)); //Ensure we did not overwrite the position


            solutionpositions = newpositions;
        }



        public struct ConsoleWriteInfo
        {
            public int top;
            public int left;
            public string data;
            public ConsoleColor forecolor;
            public ConsoleColor backcolor;

            public ConsoleWriteInfo(int left, int top, string data, ConsoleColor forecolor, ConsoleColor backcolor)
            {
                this.top = top;
                this.left = left;
                this.data = data;
                this.forecolor = forecolor;
                this.backcolor = backcolor;
            }
        }
        public static ConcurrentQueue<ConsoleWriteInfo> queue = new ConcurrentQueue<ConsoleWriteInfo>();
        public static void ConsoleWriteTick()
        {
            //Draw first item in the queue
            if (queue.Count() == 0)
            {
                return;
            }
            var towrite = queue.First();
            Console.CursorTop = towrite.top;
            Console.CursorLeft = towrite.left;
            Console.ForegroundColor = towrite.forecolor;
            if (towrite.backcolor == ConsoleColor.Yellow)
            {

            }
            Console.BackgroundColor = towrite.backcolor;
            Console.Write(towrite.data);
            if(queue.TryDequeue(out towrite))
            {

            }
            else
            {

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