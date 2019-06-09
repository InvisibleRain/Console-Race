using System;
using System.Threading;
using System.IO;

namespace Console_Races
{
    class Engine
    {
        public static void DrawScreen(char[,] screen)
        {
            //I just write in string and output it like it was realy console buffer. Fortunately it works.
            Console.SetCursorPosition(0, 0);

            string consoleBuffer = "";

            for (int col = 4; col < Game.screenHeight - 4; col++)
            {
                for (int row = 0; row < Game.screenWidth; row++)
                {
                    consoleBuffer += screen[col, row];
                }
                consoleBuffer += "\n";   //Carriage return symbol
            }

            Console.Write(consoleBuffer);
        }

        //Adds one small array to bigger "screen" array at specified coordinates
        public static void PutOnScreen(char[,] screen, char[,] array, int posX, int posY)
        {
            for (int col = posY; col < array.GetLength(0) + posY; col++)
            {
                for (int row = posX; row < array.GetLength(1) + posX; row++)
                {
                    if (screen[col, row] != Game.block)
                    {
                        screen[col, row] = array[col - posY, row - posX]; //These substractions needed to access array from [0,0] position (kostil)
                    }
                }
            }
        }
    }

    class Game
    {
        public const char block = '☐';

        // Enemy's cars must appears slowly, so it will appears at first 4 rows and disappears at last 4 rows, that doesn't displayed. Displayed only middle 20.
        public const int screenHeight = 4 + 20 + 4;
        public const int screenWidth = 10;

        //Player's car
        static readonly char[,] car = new char[4, 3] {{'\0'  , block, '\0'  },
                                                      { block, block, block },
                                                      { '\0' , block, '\0'  },
                                                      { block, '\0' , block }};

        static readonly char[,] enemyCar = car;

        //Screen array is a pseudo-bitmap
        static char[,] screen = new char[screenHeight, screenWidth];

        static char[,] walls = new char[20, 10];

        private static int score = 0;

        private static readonly string bestScorePath = Path.Combine(Directory.GetCurrentDirectory(), "Best_Score");

        private static int bestScore = ReadBestFromDisk();

        static void Main(string[] args)
        {
            Console.SetWindowSize(Console.WindowWidth / 2, Console.WindowHeight); 
            Console.SetBufferSize(Console.BufferWidth / 2, 30);

            //Set square font
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ConsoleFont.ChangeFont("Lucida Console", 16, 16);

            Console.CursorVisible = false;

        start: //Used when player die

            score = 0;

            //Sets start position for left upper corner of car
            int carX = 2;
            int carY = 16;

            //Sets speed in blocks per second (It works a bit not correctly because time of frame depend not only from latency between frames, but also from time of rendering frame)
            int speed = 30;

            //Set enemy car's position
            int enemyX = 0;
            int enemyY = 0;

            enemyX = GetRandomPosition();

            //Shows player countdown
            IntroScript(walls, screen, car, carX, carY);

            while (true)
            {
                //Main frame rendering calls
                walls = GenerateWalls();
                Engine.PutOnScreen(screen, walls, 0, 4); //Watch Globals class for explaining
                Engine.PutOnScreen(screen, car, carX, carY + 4); //Watch Globals class for explaining
                Engine.PutOnScreen(screen, enemyCar, enemyX, enemyY);
                Engine.DrawScreen(screen);

                //Score shows separately from "screen" array
                score++;
                ShowScore(score);
                ShowBest(score);

                if (IsEnemyHitted(carX, carY, enemyX, enemyY) == true)
                {
                    break;
                }

                //Gives player control
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.LeftArrow:
                            if (carX > 3)
                            {
                                carX -= 3;
                                Array.Clear(screen, 0, screen.Length);
                                Engine.PutOnScreen(screen, car, carX, carY + 4);
                                Engine.PutOnScreen(screen, enemyCar, enemyX, enemyY);
                                Engine.PutOnScreen(screen, walls, 0, 4);
                                Thread.Sleep(10); //Removes flickering
                                Engine.DrawScreen(screen);
                                ShowScore(score);
                            }
                            break;

                        case ConsoleKey.RightArrow:
                            if (carX <= 3)
                            {
                                carX += 3;
                                Array.Clear(screen, 0, screen.Length);
                                Engine.PutOnScreen(screen, car, carX, carY + 4);
                                Engine.PutOnScreen(screen, enemyCar, enemyX, enemyY);
                                Engine.PutOnScreen(screen, walls, 0, 4);
                                Thread.Sleep(10); //Removes flickering
                                Engine.DrawScreen(screen);
                                ShowScore(score);
                            }
                            break;

                        default:
                            break;
                    }
                }

                //If enemy car reaches the bottom, spawn new
                enemyY++;
                if (enemyY == screen.GetLength(0) - 1 - 4)
                {
                    enemyY = 0;
                    enemyX = GetRandomPosition();
                }

                Array.Clear(screen, 0, screen.Length);
                Thread.Sleep(1000 / speed);
            }

            //If player die
            GameOverScipt(screen);
            goto start;
        }


        public static int count = 0; //For GenerateWalls

        static char[,] GenerateWalls()
        {
            /*This method needs rewriting*/

            //Initialize one one-dimensional wall that will be transforms to final two-dimensional array later (kostil)
            char[] oneWall = new char[20] { block, block, block, block, ' ', block, block, block, block, ' ', block, block, block, block, ' ', block, block, block, block, ' ' };
            char[,] result = new char[20, 10];

            //Sets shiftig level for walls. It not internal variable because we need to store shifting level in RAM continiously, othrewise it nullifies
            Game.count++;
            if (Game.count > 5)
            {
                Game.count = 1;
            }

            //Shifts array to right and give 1st element value of previous last
            for (int i = 1; i <= Game.count; i++)
            {
                char temp = oneWall[oneWall.Length - 1];

                //Shifts array to one position right
                for (int j = oneWall.Length - 1; j > 0; j--)
                {
                    oneWall[j] = oneWall[j - 1];
                }

                oneWall[0] = temp;
            }

            //Create final two-dimensional array
            for (int col = 0; col < oneWall.Length; col++)
            {
                result[col, 0] = oneWall[col];
            }
            for (int col = 0; col < oneWall.Length; col++)
            {
                result[col, 9] = oneWall[col];
            }

            return result;
        }

        static int GetRandomPosition()
        {
            Random rand = new Random();
            int RandNum = rand.Next(2);
            int posX;

            switch (RandNum)
            {
                case 0:
                    posX = 2;
                    break;

                case 1:
                    posX = 5;
                    break;

                default:
                    posX = 2;
                    break;
            }

            return posX;
        }

        static bool IsEnemyHitted(int carX, int carY, int enemyX, int enemyY)
        {
            bool hitted = false;

            for (int row = carY; row < carY + 4; row++)
            {
                for (int col = carX; col < carX + 3; col++)
                {
                    if ((col >= enemyX && col < enemyX + 3) && (row >= enemyY - 4 && row < enemyY))
                    {
                        hitted = true;
                        break;
                    }
                }
            }
            return hitted;
        }


        //Fix bug when key readed in buffer even if Thread.Sleep() already works
        static void ClearKeyBuffer()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(false);
        }


        static void ShowScore(int score)
        {
            Console.SetCursorPosition(12, 0);
            Console.WriteLine($"Score: {score}");
        }

        static int ReadBestFromDisk()
        {
            int best;

            //If file exist, read from it, else best score = 0
            if (File.Exists(bestScorePath))
            {
                best = Convert.ToInt32(File.ReadAllText(bestScorePath)); 
            }
            else
            {
                best = 0;
            }
            return best;
        }
        
        static void ShowBest(int score)
        {
            if (score > bestScore)
            {
                bestScore = score;
            }
            Console.SetCursorPosition(12, 1);
            Console.Write($"Best: {bestScore}");
        }

        static void WriteBestToDisk(int best)
        {
            //Writes "best" in file Best_Score in folder of executable file
            using (StreamWriter outputFile = new StreamWriter(bestScorePath))
            {
                outputFile.WriteLine(best);
            }
        }


        static void IntroScript(char[,] walls, char[,] screen, char[,] car, int carX, int carY)
        {
            walls = GenerateWalls();
        
            for (int i = 3; i >= 1; i--)
            {
                Engine.PutOnScreen(screen, walls, 0, 4); //Watch Globals class for explaining
                Engine.PutOnScreen(screen, car, carX, carY + 4);
                Engine.DrawScreen(screen);
                Console.SetCursorPosition(7, 10);
                Console.WriteLine($"Are you ready? {i}");
                ShowScore(score);
                ShowBest(bestScore);
                Thread.Sleep(1000);
                ClearKeyBuffer();
                Console.Clear();
            }
            Engine.PutOnScreen(screen, walls, 0, 4); //Watch Globals class for explaining
            Engine.PutOnScreen(screen, car, carX, carY + 4);
            Engine.DrawScreen(screen);
            Console.SetCursorPosition(9, 10);
            Console.WriteLine("Go!!!");
            ShowScore(score);
            ShowBest(bestScore);
            Thread.Sleep(1000);
            ClearKeyBuffer();
            Console.Clear();
        }

        static void GameOverScipt(char[,] screen)
        {
            Console.SetCursorPosition(7, 10);
            Console.WriteLine("Game Over!");
            Console.Write("Press any key to restart");
            WriteBestToDisk(bestScore);
            Thread.Sleep(300);
            ClearKeyBuffer();
            Console.ReadKey();
            Array.Clear(screen, 0, screen.Length);
            Console.Clear();
        }
    }
}
