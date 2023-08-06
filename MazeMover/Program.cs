using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
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
        const int totalmovetime = 50;
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
                Console.Clear();
                maze.Draw(false);
                Console.ForegroundColor = ConsoleColor.White;
                //Draw AI position
                Console.CursorLeft = (AI_position % width) * 2; //Maze is double width
                Console.CursorTop = height - (AI_position / width) - 1;
                Console.Write("()");

                //Player movement
                bool selecting = false;
                Task.Factory.StartNew(() =>
                {
                    player_position = (maze.height-1) * maze.width;

                    while (AI_position != maze.mazeendidx)
                    {
                        ConsoleColor backcolor = ConsoleColor.Black;
                        if (!maze.GetCell(player_position)) //Open cell
                        {
                            backcolor = ConsoleColor.White;
                        }
                        ConsoleColor playercolor = ConsoleColor.Cyan;
                        if (selecting)
                        {
                            playercolor = ConsoleColor.Green;
                        }
                        queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "()", playercolor, backcolor));
                        var input = Console.ReadKey(true).Key;
                        queue.Enqueue(new ConsoleWriteInfo((player_position % maze.width) * 2, maze.height - (player_position / maze.width) - 1, "  ", ConsoleColor.Blue, backcolor));
                        switch (input)
                        {
                            case ConsoleKey.W:
                                if (player_position / width < height - 1 && player_position + width != AI_position) //Can we move up?
                                {
                                    player_position += width;
                                }
                                break;
                            case ConsoleKey.A:
                                if (player_position % width >= 1 && player_position - 1  != AI_position) //Can we move left?
                                {
                                    player_position--;
                                }
                                break;
                            case ConsoleKey.S:
                                if (player_position >= width && player_position - width != AI_position) //Can we move down?
                                {
                                    player_position -= width;
                                }
                                break;
                            case ConsoleKey.D:
                                if (player_position % width < width - 1 && player_position + 1 != AI_position) //Can we move right?
                                {
                                    player_position++;
                                }
                                break;
                            case ConsoleKey.Enter:
                                //Show solution
                                if (selecting)
                                {
                                    selecting = false;
                                }
                                else
                                {
                                    selecting = true;
                                }
                                break;
                        }
                    }
                                        
                });
                Task.Factory.StartNew(() =>
                {
                    AI ai = new AI(0, maze.Copy());


                    Stopwatch turntimer = new Stopwatch();

                    while (true)
                    {
                        turntimer.Stop();
                        if (turntimer.ElapsedMilliseconds <= totalmovetime)
                        {
                            Thread.Sleep(totalmovetime - (int)turntimer.ElapsedMilliseconds);
                        }

                        turntimer.Restart();
                        
                        //Remove last one
                        queue.Enqueue(new ConsoleWriteInfo((AI_position % width) * 2, height - (AI_position / width) - 1, "  ", ConsoleColor.Black, ConsoleColor.Black));


                        //AI movement
                        ai.Move();
                        AI_position = ai.position;
                        
                        queue.Enqueue(new ConsoleWriteInfo((AI_position % width) * 2, height - (AI_position / width) - 1, "()", ConsoleColor.White, ConsoleColor.Black));

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

        struct ConsoleWriteInfo
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
        static ConcurrentQueue<ConsoleWriteInfo> queue = new ConcurrentQueue<ConsoleWriteInfo>();
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