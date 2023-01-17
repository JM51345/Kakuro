using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using static System.Console;

namespace Kakuro
{
    static class ApplicationPlay
    {
        static int TOP_X;
        static int TOP_Y;
        static int MARGIN;
        static int HEIGHT;
        static int WIDTH;
        static int TOTAL_MOVES;
        static int INDICATOR_X;
        static int INDICATOR_Y;
        static int INDICATOR_X_PREVIOUS;
        static int INDICATOR_Y_PREVIOUS;
        static ConsoleColor INDICATOR_COLOR_FOREGROUND;
        static ConsoleColor INDICATOR_COLOR_FOREGROUND_PREVIOUS;
        static ConsoleKey KEY;
        static short FONTSIZE;
        
        static Kakuro Board = null;
        static bool Exit = false;
        static bool Running = false;
        static int _SaveNumber = 0;
        const int CONTAINER_WIDTH = 10;
        const int CONTAINER_HEIGHT = 5;
        const string FONTNAME = "Consolas";
        const ConsoleColor INDICATOR_COLOR_BACKGROUND = ConsoleColor.DarkGray;
        const ConsoleColor INDICATOR_ERROR_COLOR_BACKGROUND = ConsoleColor.DarkRed;
        const ConsoleColor REPETION_ERROR_COLOR_FOREGROUND = ConsoleColor.DarkRed;
        const ConsoleColor CHECK_SUM_COLOR_FOREGROUND = ConsoleColor.DarkGreen;

        private static void FormatVariables()
        {
            var workingDirectory = Environment.CurrentDirectory;
            var builder = new ConfigurationBuilder()
                .SetBasePath(workingDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            var appsettings = configuration.GetSection("AppSettings");
            
            try
            {
                TOP_X = int.Parse(appsettings.GetSection("TOP_X").Value);
                TOP_Y = int.Parse(appsettings.GetSection("TOP_Y").Value);
                MARGIN = int.Parse(appsettings.GetSection("MARGIN").Value);
                FONTSIZE = short.Parse(appsettings.GetSection("FONTSIZE").Value);
                TOTAL_MOVES = 0;
                INDICATOR_X = 0;
                INDICATOR_Y = 0;
                INDICATOR_X_PREVIOUS = -1;
                INDICATOR_Y_PREVIOUS = -1;
                INDICATOR_COLOR_FOREGROUND = ConsoleColor.Black;
                INDICATOR_COLOR_FOREGROUND_PREVIOUS = ConsoleColor.Black;
                Exit = false;
                Running = true;

                if (File.Exists(workingDirectory + "\\Data\\save" + _SaveNumber + ".txt"))
                {
                    Board = new Kakuro(workingDirectory + "\\Data\\save" + _SaveNumber + ".txt");
                    WIDTH = CONTAINER_WIDTH * Board.Width;
                    HEIGHT = CONTAINER_HEIGHT * Board.Height;
                }
                else
                {
                    throw new Exception("File does not exist.");
                }

                // Update indicator location to leftmost and downmost container that isn't Sum or Block.
                while (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Sum || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Block)
                {
                    if (INDICATOR_X == Board.Width)
                    {
                        INDICATOR_X = 0;
                        INDICATOR_Y++;
                    }
                    INDICATOR_X++;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Unable to read configuration data. Error: {e.Message}");
            }
        }

        private static void FormatConsole()
        {
            Clear();
            // Decrease fontsize if WindowHeight exceeds LargestWindowHeight
            while (HEIGHT + TOP_X + MARGIN >= LargestWindowHeight)
            {
                ConsoleHelper.SetCurrentFont(FONTNAME, FONTSIZE--);
            }

            // Set the height and width of the command prompt window
            WindowWidth = WIDTH + TOP_X + MARGIN + 1;
            WindowHeight = HEIGHT + TOP_X + MARGIN;
            BufferWidth = WindowWidth;
            BufferHeight = HEIGHT + TOP_X + MARGIN;
            // Set the command prompt window header
            Title = $"Kakuro --- File[{_SaveNumber}]";
            // Hiding cursor
            CursorVisible = false;
            DrawFrame();
            DrawBoard();
            DrawTotalMoves();
            DrawInfo();
        }

        private static void FormatEventhandlers()
        {
            CancelKeyPress += Console_Cancel_Eventhandler;
        }

        private static char GetCharacter(Characters character)
        {
            var retVal = character switch
            {
                Characters.TopRight => '╗',
                Characters.TopLeft => '╔',
                Characters.BottomRight => '╝',
                Characters.BottomLeft => '╚',
                Characters.Horizontal => '═',
                Characters.Vertical => '║',
                _ => '═',
            };
            return retVal;
        }

        private static void DrawFrame()
        {
            ResetColor();
            // Empty the command line prompt
            Clear();
            // Draw the frame
            // Top row of the frame
            SetCursorPosition(TOP_X, TOP_Y);
            Write(GetCharacter(Characters.TopLeft));
            for (int i = 0; i < WIDTH; i++)
            {
                Write(GetCharacter(Characters.Horizontal));
            }
            Write(GetCharacter(Characters.TopRight));
            // Middle rows of the frame
            for (int i = 1; i <= HEIGHT + 1; i++)
            {
                SetCursorPosition(TOP_X, TOP_Y + i);
                Write(GetCharacter(Characters.Vertical));
                for (int j = 0; j < WIDTH; j++)
                {
                    Write(" ");
                }
                Write(GetCharacter(Characters.Vertical));
            }
            // Bottom row of the frame
            SetCursorPosition(TOP_X, TOP_Y + HEIGHT + 1);
            Write(GetCharacter(Characters.BottomLeft));
            for (int i = 0; i < WIDTH; i++)
            {
                Write(GetCharacter(Characters.Horizontal));
            }
            Write(GetCharacter(Characters.BottomRight));
        }

        private static void DrawBoard()
        {
            SetCursorPosition(TOP_X + 1, TOP_Y + 1);
            Board.Print();
            ResetColor();
        }

        private static void DrawTotalMoves()
        {
            ResetColor();
            SetCursorPosition(TOP_X, TOP_Y + HEIGHT + 2);
            Write($"Total moves: {TOTAL_MOVES}");
        }

        private static void DrawInfo()
        {
            var previousEncoding = OutputEncoding;
            OutputEncoding = System.Text.Encoding.Unicode;

            SetCursorPosition(TOP_X + (int)(0.68 * WIDTH), TOP_Y - 1);
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.White;
            Write("■=Even  ");
            ForegroundColor = ConsoleColor.Gray;
            Write("■=Odd  ");
            ForegroundColor = ConsoleColor.DarkGray;
            Write("■=Parity");
            ResetColor();

            OutputEncoding = previousEncoding;
        }

        private static void HandleInput()
        {
            Write("");

            // Reading pressed button
            ConsoleKeyInfo keyInfo = ReadKey(true);
            KEY = keyInfo.Key;
        }

        private static void Move()
        {
            bool sleep = true;
            INDICATOR_X_PREVIOUS = INDICATOR_X;
            INDICATOR_Y_PREVIOUS = INDICATOR_Y;
            switch (KEY)
            {
                case ConsoleKey.UpArrow:
                    do
                    {
                        INDICATOR_Y += 1;
                        if (INDICATOR_Y == Board.Height)
                        {
                            INDICATOR_Y = -1;
                        }
                        else if (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Even
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Odd
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Parity)
                        {
                            break;
                        }
                    } while (true);
                    break;
                case ConsoleKey.DownArrow:
                    do
                    {
                        INDICATOR_Y -= 1;
                        if (INDICATOR_Y == -1)
                        {
                            INDICATOR_Y = Board.Height - 1;
                        }
                        else if (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Even
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Odd
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Parity)
                        {
                            break;
                        }
                    } while (true);
                    break;
                case ConsoleKey.LeftArrow:
                    do
                    {
                        INDICATOR_X -= 1;
                        if (INDICATOR_X == -1)
                        {
                            INDICATOR_X = Board.Width;
                        }
                        else if (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Even
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Odd
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Parity)
                        {
                            break;
                        }
                    } while (true);
                    break;
                case ConsoleKey.RightArrow:
                    do
                    {
                        INDICATOR_X += 1;
                        if (INDICATOR_X == Board.Width)
                        {
                            INDICATOR_X = 0;
                        }
                        else if (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Even
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Odd
                            || Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Parity)
                        {
                            break;
                        }
                    } while (true);
                    break;
                default:
                    sleep = false;
                    break;
            }
            if (sleep)
            {
                Thread.Sleep(50);
            }
        }

        private static void SetAndUpdateInputValue()
        {
            bool errorPrompt = false;
            bool updateValue = false;
            int? oldValue = Board.Data[INDICATOR_X, INDICATOR_Y].Value;
            ForegroundColor = Board.Data[INDICATOR_X, INDICATOR_Y].ContainerForegroundColor;

            if (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Even)
            {
                switch (KEY)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.D3:
                    case ConsoleKey.D5:
                    case ConsoleKey.D7:
                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.NumPad3:
                    case ConsoleKey.NumPad5:
                    case ConsoleKey.NumPad7:
                    case ConsoleKey.NumPad9:
                        errorPrompt = true;
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 2;
                        updateValue = true;
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 4;
                        updateValue = true;
                        break;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 6;
                        updateValue = true;
                        break;
                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 8;
                        updateValue = true;
                        break;
                    default:
                        break;
                }
            }
            else if (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Odd)
            {
                switch (KEY)
                {
                    case ConsoleKey.D2:
                    case ConsoleKey.D4:
                    case ConsoleKey.D6:
                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.NumPad6:
                    case ConsoleKey.NumPad8:
                        errorPrompt = true;
                        break;
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 1;
                        updateValue = true;
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 3;
                        updateValue = true;
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 5;
                        updateValue = true;
                        break;
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 7;
                        updateValue = true;
                        break;
                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 9;
                        updateValue = true;
                        break;
                    default:
                        break;
                }
            }
            else if (Board.Data[INDICATOR_X, INDICATOR_Y].Type == ContainerTypes.Parity)
            {
                switch (KEY)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 1;
                        updateValue = true;
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 2;
                        updateValue = true;
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 3;
                        updateValue = true;
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 4;
                        updateValue = true;
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 5;
                        updateValue = true;
                        break;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 6;
                        updateValue = true;
                        break;
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 7;
                        updateValue = true;
                        break;
                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 8;
                        updateValue = true;
                        break;
                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                        Board.Data[INDICATOR_X, INDICATOR_Y].Value = 9;
                        updateValue = true;
                        break;
                    default:
                        break;
                }
            }

            if (errorPrompt)
            {
                SetCursorPosition(TOP_X + 5 + CONTAINER_WIDTH * INDICATOR_X, TOP_Y + 3 + HEIGHT - CONTAINER_HEIGHT * (INDICATOR_Y + 1));
                BackgroundColor = INDICATOR_ERROR_COLOR_BACKGROUND;
                Write((Board.Data[INDICATOR_X, INDICATOR_Y].Value is null) ? "  " : $"0{Board.Data[INDICATOR_X, INDICATOR_Y].Value}");
                Thread.Sleep(250);

                // Draw current location
                SetCursorPosition(TOP_X + 5 + CONTAINER_WIDTH * INDICATOR_X, TOP_Y + 3 + HEIGHT - CONTAINER_HEIGHT * (INDICATOR_Y + 1));
                BackgroundColor = INDICATOR_COLOR_BACKGROUND;
                Write((Board.Data[INDICATOR_X, INDICATOR_Y].Value is null) ? "  " : $"0{Board.Data[INDICATOR_X, INDICATOR_Y].Value}");
            }
            else if (updateValue)
            {
                SetCursorPosition(TOP_X + 5 + CONTAINER_WIDTH * INDICATOR_X, TOP_Y + 3 + HEIGHT - CONTAINER_HEIGHT * (INDICATOR_Y + 1));
                BackgroundColor = Board.Data[INDICATOR_X, INDICATOR_Y].ContainerBackgroundColor;
                Write($"0{Board.Data[INDICATOR_X, INDICATOR_Y].Value}");

                // Increment total moves if same value wasn't inserted again
                if (oldValue != Board.Data[INDICATOR_X, INDICATOR_Y].Value) { TOTAL_MOVES++; }
                Thread.Sleep(50);
            }
            ResetColor();
        }

        private static void SelectAction()
        {
            bool sleep = true;
            switch (KEY)
            {
                // Solves the kakuro
                case ConsoleKey.S:
                    Board.SolveCombinations(Board);
                    DrawBoard();
                    break;
                // Formats the console again if something breaks
                case ConsoleKey.R:
                    FormatConsole();
                    break;
                // Removes value
                case ConsoleKey.Spacebar:
                    Board.Data[INDICATOR_X, INDICATOR_Y].Value = null;
                    break;
                // Decrease fontsize
                case ConsoleKey.OemMinus:
                case ConsoleKey.Subtract:
                    if (WindowHeight >= 0.40*LargestWindowHeight)
                    {
                        ConsoleHelper.SetCurrentFont(FONTNAME, FONTSIZE--);
                    }
                    break;
                // Increase fontsize
                case ConsoleKey.OemPlus:
                case ConsoleKey.Add:
                    if (WindowHeight <= 0.95*LargestWindowHeight)
                    {
                        ConsoleHelper.SetCurrentFont(FONTNAME, FONTSIZE++);
                    }
                    break;
                default:
                    sleep = false;
                    break;
            }

            if (sleep)
            {
                Thread.Sleep(100);
            }
        }

        private static void UpdateIndicatorOnBoard()
        {
            // Draw current location
            BackgroundColor = Board.Data[INDICATOR_X, INDICATOR_Y].Type != ContainerTypes.Parity ? INDICATOR_COLOR_BACKGROUND : ConsoleColor.Gray;
            ForegroundColor = INDICATOR_COLOR_FOREGROUND;
            SetCursorPosition(TOP_X + 5 + CONTAINER_WIDTH * INDICATOR_X, TOP_Y + 3 + HEIGHT - CONTAINER_HEIGHT * (INDICATOR_Y + 1));
            Write((Board.Data[INDICATOR_X, INDICATOR_Y].Value is null) ? "  " : $"0{Board.Data[INDICATOR_X, INDICATOR_Y].Value}");

            // Revert previous location to original colours
            if (!(INDICATOR_X_PREVIOUS == -1 && INDICATOR_Y_PREVIOUS == -1) && !(INDICATOR_X_PREVIOUS == INDICATOR_X && INDICATOR_Y_PREVIOUS == INDICATOR_Y)) // Move() has been run atleast once
            {
                SetCursorPosition(TOP_X + 5 + CONTAINER_WIDTH * INDICATOR_X_PREVIOUS, TOP_Y + 3 + HEIGHT - CONTAINER_HEIGHT * (INDICATOR_Y_PREVIOUS + 1));
                BackgroundColor = Board.Data[INDICATOR_X_PREVIOUS, INDICATOR_Y_PREVIOUS].ContainerBackgroundColor;
                ForegroundColor = INDICATOR_COLOR_FOREGROUND_PREVIOUS;
                Write((Board.Data[INDICATOR_X_PREVIOUS, INDICATOR_Y_PREVIOUS].Value is null) ? "  " : $"0{Board.Data[INDICATOR_X_PREVIOUS, INDICATOR_Y_PREVIOUS].Value}");
            }
            ResetColor();
        }

        private static void UpdateSolvedSumsColours()
        {
            ArrayList RS = Board.LocationsRS;
            ArrayList LS = Board.LocationsLS;

            foreach ((int, int) element in RS)
            {
                Container temp = Board.Data[element.Item1, element.Item2];
                int? newSum = 0;

                for (int x = element.Item1 + 1, y = element.Item2; x < Board.Width; x++)
                {
                    if (Board.Data[x, y].Type == ContainerTypes.Sum || Board.Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    newSum += Board.Data[x, y].Value;
                }

                if (newSum == temp.SumRight && Board.CheckRepetionsX(element.Item1, element.Item2))
                {
                    BackgroundColor = Board.Data[element.Item1, element.Item2].ContainerBackgroundColor;
                    ForegroundColor = CHECK_SUM_COLOR_FOREGROUND;
                }
                else
                {
                    BackgroundColor = Board.Data[element.Item1, element.Item2].ContainerBackgroundColor;
                    ForegroundColor = Board.Data[element.Item1, element.Item2].ContainerForegroundColor;
                }
                SetCursorPosition(TOP_X + 7 + CONTAINER_WIDTH * element.Item1, TOP_Y + 2 + HEIGHT - CONTAINER_HEIGHT * (element.Item2 + 1));
                Write((Board.Data[element.Item1, element.Item2].SumRight > 9) ? Board.Data[element.Item1, element.Item2].SumRight.ToString() : $"0{Board.Data[element.Item1, element.Item2].SumRight}");
            }

            foreach ((int, int) element in LS)
            {
                Container temp = Board.Data[element.Item1, element.Item2];
                int? newSum = 0;

                for (int x = element.Item1, y = element.Item2 - 1; y > -1; y--)
                {
                    if (Board.Data[x, y].Type == ContainerTypes.Sum || Board.Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    newSum += Board.Data[x, y].Value;
                }

                if (newSum == temp.SumLeft && Board.CheckRepetionsY(element.Item1, element.Item2))
                {
                    BackgroundColor = Board.Data[element.Item1, element.Item2].ContainerBackgroundColor;
                    ForegroundColor = CHECK_SUM_COLOR_FOREGROUND;
                }
                else
                {
                    BackgroundColor = Board.Data[element.Item1, element.Item2].ContainerBackgroundColor;
                    ForegroundColor = Board.Data[element.Item1, element.Item2].ContainerForegroundColor;
                }
                SetCursorPosition(TOP_X + 3 + CONTAINER_WIDTH * element.Item1, TOP_Y + 4 + HEIGHT - CONTAINER_HEIGHT * (element.Item2 + 1));
                Write((Board.Data[element.Item1, element.Item2].SumLeft > 9) ? Board.Data[element.Item1, element.Item2].SumLeft.ToString() : $"0{Board.Data[element.Item1, element.Item2].SumLeft}");
            }
        }

        private static void UpdateRepetiveValuesToRed()
        {
            ArrayList EOP = Board.LocationsEOP;

            foreach ((int, int) element in EOP)
            {
                UpdateRepetiveValueToRed(element.Item1, element.Item2);
            }
        }

        private static void UpdateRepetiveValueToRed(int centerX, int centerY)
        {
            int? compareAgainst = Board.Data[centerX, centerY].Value;
            bool foundRepetion = false;

            if (compareAgainst != null)
            {
                // Up
                for (int x = centerX, y = centerY + 1; y < Board.Height && !foundRepetion; y++)
                {
                    if (Board.Data[x, y].Type == ContainerTypes.Sum || Board.Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    if (Board.Data[x, y].Value == compareAgainst && Board.Data[x, y].Value != null)
                    {
                        foundRepetion = true;
                    }
                }

                // Down
                for (int x = centerX, y = centerY - 1; y > -1 && !foundRepetion; y--)
                {
                    if (Board.Data[x, y].Type == ContainerTypes.Sum || Board.Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    if (Board.Data[x, y].Value == compareAgainst && Board.Data[x, y].Value != null)
                    {
                        foundRepetion = true;
                    }
                }

                // Left
                for (int x = centerX - 1, y = centerY; x > -1 && !foundRepetion; x--)
                {
                    if (Board.Data[x, y].Type == ContainerTypes.Sum || Board.Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    if (Board.Data[x, y].Value == compareAgainst && Board.Data[x, y].Value != null)
                    {
                        foundRepetion = true;
                    }
                }

                // Right
                for (int x = centerX + 1, y = centerY; x < Board.Width && !foundRepetion; x++)
                {
                    if (Board.Data[x, y].Type == ContainerTypes.Sum || Board.Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    if (Board.Data[x, y].Value == compareAgainst && Board.Data[x, y].Value != null)
                    {
                        foundRepetion = true;
                    }
                }
            }

            // Repetions were found: set start location to red
            if (foundRepetion)
            {
                SetCursorPosition(TOP_X + 5 + CONTAINER_WIDTH * centerX, TOP_Y + 3 + HEIGHT - CONTAINER_HEIGHT * (centerY + 1));
                BackgroundColor = Board.Data[centerX, centerY].ContainerBackgroundColor;
                ForegroundColor = REPETION_ERROR_COLOR_FOREGROUND;
                Write($"0{Board.Data[centerX, centerY].Value}");
            }
            // Repetions were not found: set start location to normal colours
            else if (compareAgainst != null)
            {
                SetCursorPosition(TOP_X + 5 + CONTAINER_WIDTH * centerX, TOP_Y + 3 + HEIGHT - CONTAINER_HEIGHT * (centerY + 1));
                BackgroundColor = Board.Data[centerX, centerY].ContainerBackgroundColor;
                ForegroundColor = Board.Data[centerX, centerY].ContainerForegroundColor;
                Write($"0{Board.Data[centerX, centerY].Value}");
            }

            // Saving what color is at indicator location
            if (centerX == INDICATOR_X && centerY == INDICATOR_Y && foundRepetion)
            {
                INDICATOR_COLOR_FOREGROUND = REPETION_ERROR_COLOR_FOREGROUND;
            }
            else if (centerX == INDICATOR_X && centerY == INDICATOR_Y && !foundRepetion)
            {
                INDICATOR_COLOR_FOREGROUND = Board.Data[centerX, centerY].ContainerForegroundColor;
            }

            // Saving what color was at previous indicator location
            if (centerX == INDICATOR_X_PREVIOUS && centerY == INDICATOR_Y_PREVIOUS && foundRepetion)
            {
                INDICATOR_COLOR_FOREGROUND_PREVIOUS = REPETION_ERROR_COLOR_FOREGROUND;
            }
            else if (centerX == INDICATOR_X_PREVIOUS && centerY == INDICATOR_Y_PREVIOUS && !foundRepetion)
            {
                INDICATOR_COLOR_FOREGROUND_PREVIOUS = Board.Data[centerX, centerY].ContainerForegroundColor;
            }
        }

        private static void EndIfSolved()
        {
            Board.CheckAll();
            if (Board.IsSolved)
            {
                ResetColor();
                SetCursorPosition(TOP_X + (int)(WIDTH * 0.65), TOP_Y + HEIGHT + 2);
                Write("Kakuro has been solved!");

                ForegroundColor = ConsoleColor.Green;
                BackgroundColor = ConsoleColor.Black;
                SetCursorPosition(7, 1);
                Write("Press any button.");
                ReadKey();
                Exit = true;

                ResetColor();
            }
        }

        private static void Play()
        {
            do
            {
                UpdateIndicatorOnBoard();
                HandleInput();
                if (Running)
                {
                    SelectAction();
                    SetAndUpdateInputValue();
                    Move();
                }
                UpdateSolvedSumsColours();
                UpdateRepetiveValuesToRed();
                EndIfSolved();
                DrawTotalMoves();
                CursorVisible = false;
            } while (!Exit);
        }

        private static void Console_Cancel_Eventhandler(object sender, ConsoleCancelEventArgs e)
        {
            ForegroundColor = ConsoleColor.Green;
            BackgroundColor = ConsoleColor.Black;
            SetCursorPosition(7, 1);
            Write($"Game interrupted. Press any button.");
            ResetColor();
            Exit = true;
            Running = false;
            e.Cancel = true;
        }
        
        public static void RunGame(int SaveNumber)
        {
            // Formatting game
            _SaveNumber = SaveNumber;
            FormatEventhandlers();
            FormatVariables();
            FormatConsole();
            // Starting game
            Play();
        }
    }
}
