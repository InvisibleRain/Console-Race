using System;
using System.Threading;
using System.IO;

namespace Console_Races
{
    public class Globals
    {
        public const char block = '☐';

        // Enemy's cars must appears slowly, so it will appears at first 4 rows and disappears at last 4 rows, that doesn't displayed. Displayed only middle 20.
        public const int screenHeight = 4 + 20 + 4; // Globals.screenHeight
        public const int screenWidth = 10;          // Globals.screenWidth
    }

    public class Game
    {
        static void DrawScreen(char[,] screen)
        {
            //I so dumb to write in really console buffer, so i just write in string
            //I will refract it later (maybe)
            Console.SetCursorPosition(0, 0);

            string consoleBuffer = "";
            for (int col = 4; col < Globals.screenHeight - 4; col++)
            {
                for (int row = 0; row < Globals.screenWidth; row++)
                {
                    consoleBuffer += screen[col, row];
                }
                consoleBuffer += "\n";   //Carriage return symbol
            }

            Console.Write(consoleBuffer);
        }

        static void PutOnScreen(char[,] screen, char[,] array, int posX, int posY)
        {
            for (int col = posY; col < array.GetLength(0) + posY; col++)
            {
                for (int row = posX; row < array.GetLength(1) + posX; row++)
                {
                    if (screen[col, row] != Globals.block)
                    {
                        screen[col, row] = array[col - posY, row - posX]; //These substractions needed to access array from [0,0] position (kostil)
                    }
                }
            }
        }

        static void ClearArray(char[,] screen)
        {
            for (int col = 0; col < Globals.screenHeight; col++)
            {
                for (int row = 0; row < Globals.screenWidth; row++)
                {
                    if (screen[col, row] == Globals.block)
                    {
                        screen[col, row] = '\0';
                    }
                }
            }
        }

        static char[] ArrayShift(char[] ar) //Calls by GenerateWalls
        {
            //Shifts array to one position right
            for (int i = ar.Length - 1; i > 0; i--)
            {
                ar[i] = ar[i - 1];
            }

            //"Delete first item of array"
            ar[0] = '\0';
            return ar;
        }

        public static int count = 0; //For GenerateWalls

        static char[,] GenerateWalls()
        {
            //Initialize one one-dimensional wall that will be transforms to final two-dimensional array later
            char[] oneWall = new char[20] { Globals.block, Globals.block, Globals.block, Globals.block, ' ', Globals.block, Globals.block, Globals.block, Globals.block, ' ', Globals.block, Globals.block, Globals.block, Globals.block, ' ', Globals.block, Globals.block, Globals.block, Globals.block, ' ' };
            char[,] result = new char[20, 10];

            Game.count++;
            if (Game.count > 5)
            {
                Game.count = 1;
            }

            for (int i = 1; i <= Game.count; i++)
            {
                //Record last symbol of oneWall in temp
                char temp = oneWall[oneWall.Length - 1];

                //Move array by one element right and give first element value of temp
                oneWall = ArrayShift(oneWall);
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

        static bool IsHitted(int carX, int carY, int enemyX, int enemyY)
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

        static void ClearKeyBuffer()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(false);
        }

        public static int score = 0;

        static void ShowScore(int score)
        {
            Console.SetCursorPosition(12, 0);
            Console.WriteLine($"Scrore: {score}");
        }

        public static int ReadBest()
        {
            int best;
            string docPath = Directory.GetCurrentDirectory();

            //If file exist, read from it, else best score = 0
            if (File.Exists(Path.Combine(docPath, "Best_Score.txt")))
            {
                best = Convert.ToInt32(File.ReadAllText(Path.Combine(docPath, "Best_Score.txt"))); 
            }
            else
            {
                best = 0;
            }
            return best;
        }

        public static int bestScore = ReadBest(); //For ShowBest

        static void ShowBest(int score) //Доделать
        {
            if (score > bestScore)
            {
                bestScore = score;
            }
            Console.SetCursorPosition(12, 1);
            Console.WriteLine($"Best: {bestScore}");
        }

        static void WriteBestToDisk(int best)
        {
            string docPath = Directory.GetCurrentDirectory();
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "Best_Score.txt")))
            {
                outputFile.WriteLine(best);
            }
        }

        static void IntroScript(char[,] walls, char[,] screen, char[,] car, int carX, int carY)
        {
            walls = GenerateWalls();
            //PutOnScreen(screen, walls, 0, 4); //Watch Globals class for explaining
            //PutOnScreen(screen, car, carX, carY + 4);
            //DrawScreen(screen);
            //Console.Clear();
            for (int i = 3; i >= 1; i--)
            {
                PutOnScreen(screen, walls, 0, 4); //Watch Globals class for explaining
                PutOnScreen(screen, car, carX, carY + 4);
                DrawScreen(screen);
                Console.SetCursorPosition(7, 10);
                Console.WriteLine($"Are you ready? {i}");
                ShowScore(score);
                ShowBest(bestScore);
                Thread.Sleep(1000);
                Console.Clear();
            }
            PutOnScreen(screen, walls, 0, 4); //Watch Globals class for explaining
            PutOnScreen(screen, car, carX, carY + 4);
            DrawScreen(screen);
            Console.SetCursorPosition(9, 10);
            Console.WriteLine("Go!!!");
            ShowScore(score);
            ShowBest(bestScore);
            Thread.Sleep(1000);
            Console.Clear();
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            char[,] car = new char[4, 3] {{ '\0', Globals.block, '\0'                   },
                                         { Globals.block, Globals.block, Globals.block },
                                         { '\0', Globals.block, '\0'                   },
                                         { Globals.block, '\0', Globals.block          }};

            //char[,] enemyCar = new char[4, 3] {{ Globals.block, '\0', Globals.block          },
            //{ '\0', Globals.block, '\0'                   },
            //{ Globals.block, Globals.block, Globals.block },
            //{ '\0', Globals.block, '\0'                   }};

            char[,] enemyCar = car;

            //Sets screen size
            char[,] screen = new char[Globals.screenHeight, Globals.screenWidth];

            //Create walls
            char[,] walls = new char[20, 10];

            int record;

        start: //Used when player die

            score = 0;

            //Sets start position for left upper corner of car
            int carX = 2;
            int carY = 16;

            //Sets speed in blocks per second (or frames per second)
            int speed = 30;


            //Set enemy car's position
            int enemyX = 0;
            int enemyY = 0;

            enemyX = GetRandomPosition();

            //Intro
            IntroScript(walls, screen, car, carX, carY);

            while (true)
            {
                walls = GenerateWalls();
                PutOnScreen(screen, walls, 0, 4); //Watch Globals class for explaining
                PutOnScreen(screen, car, carX, carY + 4);
                PutOnScreen(screen, enemyCar, enemyX, enemyY);
                DrawScreen(screen);

                score++;
                ShowScore(score);
                ShowBest(score);

                if (IsHitted(carX, carY, enemyX, enemyY) == true)
                {
                    break;
                }

                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.LeftArrow:
                            if (carX > 3)
                            {
                                carX -= 3;
                                ClearArray(screen);
                                PutOnScreen(screen, car, carX, carY + 4);
                                PutOnScreen(screen, enemyCar, enemyX, enemyY);
                                PutOnScreen(screen, walls, 0, 4);
                                Thread.Sleep(10); //Removes flickering
                                DrawScreen(screen);
                                ShowScore(score);
                            }
                            break;

                        case ConsoleKey.RightArrow:
                            if (carX <= 3)
                            {
                                carX += 3;
                                ClearArray(screen);
                                PutOnScreen(screen, car, carX, carY + 4);
                                PutOnScreen(screen, enemyCar, enemyX, enemyY);
                                PutOnScreen(screen, walls, 0, 4);
                                Thread.Sleep(10); //Removes flickering
                                DrawScreen(screen);
                                ShowScore(score);
                            }
                            break;

                        default:
                            break;
                    }
                }

                ////Speed up when spacebar is pressed
                //if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                //{
                //    speed = 60;
                //}
                //else
                //{
                //    speed = 30;
                //}

                enemyY++;
                //If enemy car reaches the bottom, spawn new
                if (enemyY == screen.GetLength(0) - 1 - 4)
                {
                    enemyY = 0;
                    enemyX = GetRandomPosition();
                }

                ClearArray(screen);
                Thread.Sleep(1000 / speed);
            }

            //If player die
            Console.SetCursorPosition(7, 10);
            Console.WriteLine("Game Over!");
            Console.WriteLine("Press any key to restart");
            WriteBestToDisk(bestScore);
            Thread.Sleep(300);
            ClearKeyBuffer();
            Console.ReadKey();
            ClearArray(screen);
            Console.Clear();
            goto start;
        }
    }
}
