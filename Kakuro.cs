using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static System.Console;

namespace Kakuro
{
    class Kakuro
    {
        //Field variables

        //Automatic properties
        public Container[,] Data { get; private set; }

        public ArrayList LocationsRS { get; } // Locations of sum-containers that have a sum on the right side of the square (left to right)

        public ArrayList LocationsLS { get; } // Locations of sum-containers that have a sum on the left side of the square (up to down)

        public ArrayList LocationsEOP { get; } // Locations of containers that can have a value

        public int Height { get; }

        public int Width { get; }

        public bool IsSolved { get; private set; }

        public int CombinationsTraversed { get; private set; }

        //Constructors
        // For creating the board
        public Kakuro(string file, string mainSeparator = ";", string secondarySeparator = ",")
        {
            LocationsRS = new ArrayList(); // Squares with left sum (y-axis)
            LocationsLS = new ArrayList(); // Squares with right sum (y-axis)
            LocationsEOP = new ArrayList(); // Squares where even, odd or parity values can be placed
            IsSolved = false;
            CombinationsTraversed = 0;
            StreamReader sr = null;

            try
            {
                sr = new StreamReader(File.Open(file, FileMode.Open));
                string status = "NONE";
                int y = 0;

                while ( !sr.EndOfStream )
                {
                    string[] line = sr.ReadLine().Split(mainSeparator);

                    // Setting whether the properties of the array are read next (heigth and width)
                    // Or the data that is inserted into the array created with the dimensions given in the properties
                    if ( line[0].Equals("PROPERTIES") ) { status = line[0]; }
                    else if ( line[0].Equals("DATA") ) { status = line[0]; }

                    // Currently reading the properties
                    if ( status.Equals("PROPERTIES") && !line[0].Equals("PROPERTIES") )
                    {
                        line = line[0].Split(secondarySeparator);

                        switch (line[0].ToUpper())
                        {
                            case "HEIGHT":
                                Height = int.Parse(line[1]);
                                y = Height - 1;
                                break;
                            case "WIDTH":
                                Width = int.Parse(line[1]);
                                break;
                            default:
                                throw new Exception("Illegal or unknown field for PROPERTIES.");
                        }
                    }
                    // Currently reading data
                    else if ( status.Equals("DATA") && !line[0].Equals("DATA") )
                    {
                        // If the Container-array hasn't been created yet
                        if ( Data is null )
                        {
                            Data = new Container[Width, Height];
                        }

                        // Inserting data into the Container-array, from left to right, moving downwards with every iteration
                        for ( int x = 0; x < Width; x++ )
                        {
                            // If current split segment contains only one value
                            if ( !line[x].Contains(secondarySeparator) )
                            {
                                switch ( line[x] )
                                {
                                    // type Block
                                    case "B":
                                        Data[x, y] = new Container();
                                        break;
                                    // type Even
                                    case "E":
                                        Data[x, y] = new Container(ContainerTypes.Even);
                                        LocationsEOP.Add((x, y));
                                        break;
                                    // type Odd
                                    case "O":
                                        Data[x, y] = new Container(ContainerTypes.Odd);
                                        LocationsEOP.Add((x, y));
                                        break;
                                    // type Parity
                                    case "P":
                                        Data[x, y] = new Container(ContainerTypes.Parity);
                                        LocationsEOP.Add((x, y));
                                        break;
                                    default:
                                        throw new Exception("Error in filedata; data in incorrect form.");
                                }
                            }
                            // Current split segment contains multiple values
                            else
                            {
                                string[] values = line[x].Split(secondarySeparator);
                                switch ( values[0] )
                                {
                                    // type Sum (value on right side of the square)
                                    case "RS":
                                        Data[x, y] = new Container(null, ParseInt(values[1]));
                                        LocationsRS.Add((x, y));
                                        break;
                                    // type Sum (value on left side of the square)
                                    case "LS":
                                        Data[x, y] = new Container(ParseInt(values[1]), null);
                                        LocationsLS.Add((x, y));
                                        break;
                                    // type Sum (value on left and right side of the square)
                                    case "LRS":
                                        Data[x, y] = new Container(ParseInt(values[1]), ParseInt(values[2]));
                                        LocationsLS.Add((x, y));
                                        LocationsRS.Add((x, y));
                                        break;
                                    // type Even with a value
                                    case "E":
                                        Data[x, y] = new Container(ContainerTypes.Even, ParseInt(values[1]));
                                        LocationsEOP.Add((x, y));
                                        break;
                                    // type Odd with a value
                                    case "O":
                                        Data[x, y] = new Container(ContainerTypes.Odd, ParseInt(values[1]));
                                        LocationsEOP.Add((x, y));
                                        break;
                                    // type Parity with a value
                                    case "P":
                                        Data[x, y] = new Container(ContainerTypes.Parity, ParseInt(values[1]));
                                        LocationsEOP.Add((x, y));
                                        break;
                                    default:
                                        throw new Exception("Error in filedata; data in incorrect form.");
                                }
                            }
                        }
                    }
                    // If the status-variable wasn't changed, the file is in wrong format.
                    // The file doesn't define when the properties and data are going to be read.
                    else if ( status.Equals("NONE") )
                    {
                        throw new Exception("Error reading file status.");
                    }

                    if ( status.Equals("DATA") && !line[0].Equals("DATA") )
                    {
                        y--;
                    }
                }
                LocationsRS.Reverse();
                LocationsLS.Sort();
                LocationsLS.Reverse();
                LocationsEOP.Reverse();
            }
            catch (Exception ex)
            {
                WriteLine("Failed to process the file.");
                WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Closing the file
                if ( sr != null )
                {
                    sr.Close();
                }
            }
        }

        //Methods
        private int ParseInt(string input)
        {
            if ( int.TryParse(input, out int result) )
            {
                return result;
            }
            else
            {
                throw new Exception("Error parsing string to int.");
            }
        }
        
        // Prints the current form of the board
        public void Print()
        {
            ArrayList squares = new ArrayList();
            int cursorX = CursorLeft;

            for ( int y = Height - 1; y >= 0; y-- )
            {
                for ( int x = 0; x < Width; x++ )
                {
                    squares.Add(Data[x, y]);
                }

                int i = 0;
                while ( true )
                {
                    if ( i >= 5 )
                    {
                        break;
                    }

                    // Writing containers one row at a time
                    foreach ( Container square in squares )
                    {
                        BackgroundColor = square.ContainerBackgroundColor;
                        ForegroundColor = square.ContainerForegroundColor;
                        Write(square.GetPrintArray()[i]);
                        ResetColor();
                    }
                    CursorLeft = cursorX;
                    CursorTop++;
                    i++;
                }
                squares.Clear();
            }
        }       

        // Randomizes all the values (type even, odd and parity) without repeating same values in the same row or column
        public void RandomizeValues()
        {
            for ( int y = 0; y < Height; y++ )
            {
                for ( int x = 0; x < Width; x++ )
                {
                    if ( Data[x, y].Type == ContainerTypes.Even || Data[x, y].Type == ContainerTypes.Odd || Data[x, y].Type == ContainerTypes.Parity )
                    {
                        do
                        {
                            Data[x, y].RandomizeValue();
                            if ( CheckRepetionsX(x, y) == true && CheckRepetionsY(x, y) == true )
                            {
                                break;
                            }
                        } while (true);
                    }
                }
            }
        }

        // Every check-for-something function returns true if the check was successful (no errors were found)
        // The four below functions start from the position (x, y) and reverse to the nearest sum-square to the left or up

        // Checks for repetions in X-Axis (of a single sum)
        public bool CheckRepetionsX(int x, int y)
        {
            // Reversing to a sum-type location
            while ( true )
            {
                if ( Data[x, y].Type == ContainerTypes.Even || Data[x, y].Type == ContainerTypes.Odd || Data[x, y].Type == ContainerTypes.Parity )
                {
                    x--;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum )
                {
                    break;
                }
            }
            x++;

            // Counting occurrences using a dictionary
            Dictionary<int?, int?> occurrences = new Dictionary<int?, int?>();
            while ( true )
            {
                if ( x >= Width )
                {
                    break;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block )
                {
                    break;
                }

                // If value is not null
                if ( Data[x, y].Value != null )
                {
                    // Value exists in the dictionary
                    if (occurrences.TryGetValue(Data[x, y].Value, out _))
                    {
                        return false;
                    }
                    // Value doesn't exist in the dictionary
                    else
                    {
                        occurrences.Add(Data[x, y].Value, 1);
                    }
                }
                x++;
            }
            return true;
        }

        // Checks for repetions in Y-Axis (of a single sum)
        public bool CheckRepetionsY(int x, int y)
        {
            // Reversing to a sum-type location
            while ( true )
            {
                if ( Data[x, y].Type == ContainerTypes.Even || Data[x, y].Type == ContainerTypes.Odd || Data[x, y].Type == ContainerTypes.Parity )
                {
                    y++;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum )
                {
                    break;
                }
            }
            y--;

            // Counting occurrences using a dictionary
            Dictionary<int?, int?> occurrences = new Dictionary<int?, int?>();
            while ( true )
            {
                if ( y < 0 )
                {
                    break;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block )
                {
                    break;
                }

                // If value is not null
                if ( Data[x, y].Value != null )
                {
                    // Value exists in the dictionary
                    if (occurrences.TryGetValue(Data[x, y].Value, out _))
                    {
                        return false;
                    }
                    // Value doesn't exist in the dictionary
                    else
                    {
                        occurrences.Add(Data[x, y].Value, 1);
                    }
                }
                y--;
            }
            return true;
        }

        // Checks the sum of a square in X-Axis
        private bool CheckSumX(int x, int y)
        {
            // Reversing to a sum-type location
            while ( true )
            {
                if ( Data[x, y].Type == ContainerTypes.Even || Data[x, y].Type == ContainerTypes.Odd || Data[x, y].Type == ContainerTypes.Parity )
                {
                    x--;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum )
                {
                    break;
                }
            }

            // Currently at a sum-type location, saving the value sum is compared to
            int? compareSumTo = Data[x, y].SumRight;
            int? sum = 0;
            while ( true )
            {
                x++;
                if ( x >= Width )
                {
                    break;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block )
                {
                    break;
                }
                else if ( Data[x, y].Value == null )
                {
                    return false;
                }

                sum += Data[x, y].Value;
            }

            return sum == compareSumTo;
        }

        // Checks the sum of a square in Y-Axis
        private bool CheckSumY(int x, int y)
        {
            // Reversing to a sum-type location
            while ( true )
            {
                if ( Data[x, y].Type == ContainerTypes.Even || Data[x, y].Type == ContainerTypes.Odd || Data[x, y].Type == ContainerTypes.Parity )
                {
                    y++;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum )
                {
                    break;
                }
            }

            // Currently at a sum-type location, saving the value sum is compared to
            int? compareSumTo = Data[x, y].SumLeft;
            int? sum = 0;
            while ( true )
            {
                y--;
                if ( y < 0 )
                {
                    break;
                }
                else if ( Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block )
                {
                    break;
                }
                else if ( Data[x, y].Value == null )
                {
                    return false;
                }

                sum += Data[x, y].Value;
            }

            return sum == compareSumTo;
        }

        // Checks if the Kakuro has been solved
        public bool CheckAll()
        {
            IsSolved = false;
            // X-Axis
            foreach ( (int, int) XY in LocationsRS )
            {
                if ( !CheckSumX(XY.Item1, XY.Item2) )
                {
                    return false;
                }
                if ( !CheckRepetionsX(XY.Item1, XY.Item2) )
                {
                    return false;
                }
            }

            // Y-Axis
            foreach ( (int, int) XY in LocationsLS )
            {
                if ( !CheckSumY(XY.Item1, XY.Item2) )
                {
                    return false;
                }
                if ( !CheckRepetionsY(XY.Item1, XY.Item2) )
                {
                    return false;
                }
            }

            IsSolved = true;
            return true;
        }

        // Checks for repetions in an int[] array
        private bool CheckRepetionsArray(int[] toCheck)
        {
            Dictionary<int, int> occurrences = new Dictionary<int, int>();
            for ( int i = 0; i < toCheck.Length; i++ )
            {
                // Value exists in the dictionary
                if ( occurrences.TryGetValue(toCheck[i], out _) )
                {
                    return false;
                }
                // Value doesn't exist in the dictionary
                else
                {
                    occurrences.Add(toCheck[i], 1);
                }
            }
            return true;
        }

        // Returns all combinations a series consisting of even, gray and parity squares can have
        // types contains E, O and P letters, for example types = "OEO" gets combinations (odd, even, odd) where values are between 1 to 9
        // excludeNumbers: remove combination if it contains a number from this array
        // compareSumTo: produce only combinations that sum up to this value
        private ArrayList CartesianProduct(string types, int[] excludeNumbers, int? compareSumTo)
        {
            int[][] EOP = new int[][]
            {
                new int[] { 2, 4, 6, 8 },
                new int[] { 1, 3, 5, 7, 9 },
                new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9}
            };
            int arrayInUse;
            ArrayList oldCombinations = new ArrayList();
            ArrayList newCombinations = new ArrayList();

            int i = 0;
            /*
             * i = current length of combinations
             * 
             * example: types = "EO"
             * 1. iteration: {2}, {4}, {6}, {8}
             * 2. iteration: {2, 1}, {2, 3}, {2, 5}, {2, 7}, {2, 9},
             *               {4, 1}, {4, 3}, {4, 5}, {4, 7}, {4, 9},
             *               {6, 1}, {6, 3}, {6, 5}, {6, 7}, {6, 9},
             *               {8, 1}, {8, 3}, {8, 5}, {8, 7}, {8, 9}
             *               
             * After the first loop every value from the current array in use (E, O or P) 
             * is separately added to each combination from the previous loop.
             * 
             * New combinations that contain excluded numbers or sums >= compareSumTo (more values to be added still)
             * or sums != compareSumTo (last value has been added) won't be added to the newCombinations list.
            */
            while ( i < types.Length )
            {
                if ( types[i].Equals('E') ) { arrayInUse = 0; }
                else if ( types[i].Equals('O') ) { arrayInUse = 1; }
                else if ( types[i].Equals('P') ) { arrayInUse = 2; }
                else { throw new Exception("Types can only contain E, O and P letters."); }

                // At the first number in combinations.
                if ( i == 0 )
                {
                    for (int j = 0; j < EOP[arrayInUse].Length; j++)
                    {
                        int[] newCombination = { EOP[arrayInUse][j] };

                        bool usedExcludedNumbers = false;
                        for (int k = 0; k < excludeNumbers.Length; k++)
                        {
                            if (newCombination[0] == excludeNumbers[k])
                            {
                                usedExcludedNumbers = true;
                                break;
                            }
                        }

                        if (!usedExcludedNumbers)
                        {
                            // The last value has been added to the combination.
                            if (i == types.Length - 1)
                            {
                                // The combination won't be added to newCombinations if the sum isn't equal to the compared sum.
                                if (newCombination[0] == compareSumTo)
                                {
                                    newCombinations.Add(newCombination);
                                }
                            }
                            // More values will be added to the combination. The combination is added to the newCombinations if the sum is less than the compared sum.
                            else if (newCombination[0] < compareSumTo)
                            {
                                newCombinations.Add(newCombination);
                            }
                        }
                    }
                }
                // After the first number has been added to the combinations
                else
                {
                    foreach ( int[] item1 in oldCombinations )
                    {
                        foreach ( int item2 in EOP[arrayInUse] )
                        {
                            bool usedExcludedNumbers = false;
                            for (int j = 0; j < excludeNumbers.Length; j++)
                            {
                                if (item2 == excludeNumbers[j])
                                {
                                    usedExcludedNumbers = true;
                                    break;
                                }
                            }

                            if (!usedExcludedNumbers)
                            {
                                int[] newCombination = new int[item1.Length + 1];
                                item1.CopyTo(newCombination, 0);
                                newCombination[^1] = item2;

                                // If there are multiple same values.
                                if (CheckRepetionsArray(newCombination) == false)
                                {
                                    continue;
                                }

                                // Calculating the sum of values in temp.
                                int sum = 0;
                                for (int j = 0; j < newCombination.Length; j++)
                                {
                                    sum += newCombination[j];
                                }

                                // If the sum is correct.
                                if (i == types.Length - 1)
                                {
                                    // If the last value has been added.
                                    if (sum == compareSumTo)
                                    {
                                        newCombinations.Add(newCombination);
                                    }
                                }
                                // If there are still more values to be added and the sum is correct.
                                else
                                {
                                    if (sum < compareSumTo)
                                    {
                                        newCombinations.Add(newCombination);
                                    }
                                }
                            }
                        }
                    }
                }

                // If combinations are longer than one and not at the last number in combinations yet
                if ( types.Length != 1 && i != types.Length - 1 )
                {
                    oldCombinations.Clear();
                    foreach (int[] item in newCombinations)
                    {
                        oldCombinations.Add(item);
                    }
                    newCombinations.Clear();
                }
                i++;
            }
            return newCombinations;
        }

        // Returns all types a sum-square contains starting from (x, y)
        // useXAxis: Select traversing direction (horizontal or vertical)
        // reverse: Select whether moving direction (x or y) is decremented or incremented with each loop
        // Default for moving direction is from left to right or down to up.
        private string GetTypesInSum(int x, int y, bool useXAxis, bool reverse = false)
        {
            string types = "";
            if (useXAxis)
            {
                do
                {
                    if (!reverse ? x >= Width : x < 0)
                    {
                        break;
                    }
                    else if (Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    else if (Data[x, y].Type == ContainerTypes.Even)
                    {
                        types += "E";
                    }
                    else if (Data[x, y].Type == ContainerTypes.Odd)
                    {
                        types += "O";
                    }
                    else if (Data[x, y].Type == ContainerTypes.Parity)
                    {
                        types += "P";
                    }
                    x = !reverse ? x + 1 : x - 1;
                } while (true);
            }
            else
            {
                do
                {
                    if (!reverse ? y < 0 : y >= Height)
                    {
                        break;
                    }
                    else if (Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    else if (Data[x, y].Type == ContainerTypes.Even)
                    {
                        types += "E";
                    }
                    else if (Data[x, y].Type == ContainerTypes.Odd)
                    {
                        types += "O";
                    }
                    else if (Data[x, y].Type == ContainerTypes.Parity)
                    {
                        types += "P";
                    }
                    y = !reverse ? y - 1 : y + 1;
                } while (true);
            }
            return types;
        }

        // Returns all numbers already placed in a sum starting from (x, y)
        // useXAxis: Select traversing direction (horizontal or vertical)
        // Moving direction is from left to right or down to up.
        private int[] GetNumbersAlreadyUsedInSum(int x, int y, bool useXAxis)
        {
            List<int> numbersFound = new List<int>();
            
            if (useXAxis)
            {
                do
                {
                    if (x >= Width)
                    {
                        break;
                    }
                    else if (Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    else if (Data[x, y].Value != null)
                    {
                        numbersFound.Add(Data[x, y].Value.Value);
                    }
                    x++;
                } while (true);
            }
            else
            {
                do
                {
                    if (y < 0)
                    {
                        break;
                    }
                    else if (Data[x, y].Type == ContainerTypes.Sum || Data[x, y].Type == ContainerTypes.Block)
                    {
                        break;
                    }
                    else if (Data[x, y].Value != null)
                    {
                        numbersFound.Add(Data[x, y].Value.Value);
                    }
                    y--;
                } while (true);
            }
            return numbersFound.ToArray();
        }

        // Returns all possible combinations for a sum-container in X-axis at (x, y)
        private ArrayList GetCombinationsX(int x, int y)
        {
            string types = GetTypesInSum(x + 1, y, useXAxis:true);
            int? sum = Data[x, y].SumRight;

            return CartesianProduct(types, new int[] { }, sum);
        }

        // Returns all possible combinations for a sum-container in Y-axis at (x, y)
        public ArrayList GetCombinationsY(int x, int y)
        {
            string types = GetTypesInSum(x, y - 1, useXAxis:false);
            int? sum = Data[x, y].SumLeft;

            return CartesianProduct(types, new int[] { }, sum);
        }

        // Returns a score from x-axis calculated from average combination length using weights (Even = 0.4, Odd = 0.5, Parial = 0.9)
        // For example if x-axis has combinations: EO, OO, EPP, score = (0.4 + 0.5) + (0.5 + 0.5) + (0.4 + 0.9 + 0.9) / 3
        private double GetAverageCombinationScoreX()
        {
            double score = 0;
            Dictionary<ContainerTypes, double> Weights = new Dictionary<ContainerTypes, double>()
            {
                { ContainerTypes.Even, 0.4 },
                { ContainerTypes.Odd, 0.5 },
                { ContainerTypes.Parity, 0.9 }
            };

            foreach ((int, int) RSxy in LocationsRS)
            {
                int i = 1;
                while (Data[RSxy.Item1 + i, RSxy.Item2].Type == ContainerTypes.Even || Data[RSxy.Item1 + i, RSxy.Item2].Type == ContainerTypes.Odd || Data[RSxy.Item1 + i, RSxy.Item2].Type == ContainerTypes.Parity)
                {
                    if (Weights.TryGetValue(Data[RSxy.Item1 + i, RSxy.Item2].Type, out double weight))
                    {
                        score += weight;
                    }

                    i++;

                    if (RSxy.Item1 + i >= Width)
                    {
                        break;
                    }
                }
            }
            return score / LocationsRS.Count;
        }

        // Returns a score from y-axis calculated from average combination length using weights (Even = 0.4, Odd = 0.5, Parial = 0.9)
        private double GetAverageCombinationScoreY()
        {
            double score = 0;
            Dictionary<ContainerTypes, double> Weights = new Dictionary<ContainerTypes, double>()
            {
                { ContainerTypes.Even, 0.4 },
                { ContainerTypes.Odd, 0.5 },
                { ContainerTypes.Parity, 0.9 }
            };

            foreach ((int, int) LSxy in LocationsLS)
            {
                int i = 1;
                while (Data[LSxy.Item1, LSxy.Item2 - i].Type == ContainerTypes.Even || Data[LSxy.Item1, LSxy.Item2 - i].Type == ContainerTypes.Odd || Data[LSxy.Item1, LSxy.Item2 - i].Type == ContainerTypes.Parity)
                {
                    if (Weights.TryGetValue(Data[LSxy.Item1, LSxy.Item2 - i].Type, out double weight))
                    {
                        score += weight;
                    }

                    i++;

                    if (LSxy.Item2 - i < 0)
                    {
                        break;
                    }
                }
            }
            return score / LocationsLS.Count;
        }

        // Tries to solve the Kakuro by getting every number combination that equals the required sum
        // and trying them together until an answer is found
        public void SolveCombinations(Kakuro board)
        {
            int level = -1;
            double averageCombinationScoreXAxis = GetAverageCombinationScoreX();
            double averageCombinationScoreYAxis = GetAverageCombinationScoreY();
            //averageCombinationScoreYAxis = 0.01;

            if (averageCombinationScoreYAxis < averageCombinationScoreXAxis)
            {
                //WriteLine("Chose Y-Axis!");
                SolveCombinationsUsingYAxisRun(board, level + 1);
            }
            else
            {
                //WriteLine("Chose X-Axis!");
                SolveCombinationsUsingXAxisRun(board, level + 1);
            }
        }

        // Methods for checking if a sum can be still possible with values already inserted into the sum when not every value has been inserted yet.
        // Used for optimizing SolveCombinations method
        private bool CheckIfThereAreStillPossibleCombinationsX(int x, int y)
        {
            /*
             * Values in SolveCombinationsUsingYAxisRun are placed starting from rightmost and uppermost vertical sum-container,
             * moving downwards in that column, then moving left by one and moving downwards again etc.
             * 
             * This means that reverse needs to be set to true in GetTypesInSum to get types in sum from right to left.
             * GetNumbersAlreadyUsedInSum moves from left to right to get which numbers have already been used in a sum.
            */
            string types = GetTypesInSum(x - 1, y, useXAxis: true, reverse: true);
            int[] excludeNumbers = GetNumbersAlreadyUsedInSum(x, y, useXAxis: true);

            // Moving to a sum-container location
            do
            {
                if (x <= 0)
                {
                    break;
                }
                else if (Data[x, y].Type == ContainerTypes.Sum)
                {
                    break;
                }
                x--;
            } while (true);

            int? sum = Data[x, y].SumRight;

            // The new shorter combination no lorger matches the sum value of the sum-container so the excluded numbers are deducted from the sum
            foreach (int excludedNumber in excludeNumbers)
            {
                sum -= excludedNumber;
            }

            // If an empty set is returned, sum has no solutions
            return CartesianProduct(types, excludeNumbers, sum).Count > 0;
        }

        private bool CheckIfThereAreStillPossibleCombinationsY(int x, int y)
        {
            /*
             * Values in SolveCombinationsUsingXAxisRun are placed starting from leftmost and downmost horizontal sum-container,
             * moving rightwards in that row, then moving up by one and moving rightwards again etc.
             * 
             * This means that reverse needs to be set to true in GetTypesInSum to get types in sum from down to up.
             * GetNumbersAlreadyUsedInSum moves from up to down to get which numbers have already been used in a sum.
            */
            string types = GetTypesInSum(x, y + 1, useXAxis: false, reverse: true);
            int[] excludeNumbers = GetNumbersAlreadyUsedInSum(x, y, useXAxis: false);

            // Moving to a sum-container location
            do
            {
                if (y >= Height)
                {
                    break;
                }
                else if (Data[x, y].Type == ContainerTypes.Sum)
                {
                    break;
                }
                y++;
            } while (true);

            int? sum = Data[x, y].SumLeft;

            // The new shorter combination no lorger matches the sum value of the sum-container so the excluded numbers are deducted from the sum
            foreach (int item in excludeNumbers)
            {
                sum -= item;
            }

            // If an empty set is returned, sum has no solutions
            return CartesianProduct(types, excludeNumbers, sum).Count > 0;
        }

        // Inserts combinations into horizontal sum-containers (X-Axis will always be correct)
        // and checks Y-Axis at the last recursion level.
        private static void SolveCombinationsUsingXAxisRun(Kakuro Board, int level)
        {
            (int, int) RSxy = ((int, int))Board.LocationsRS[level];
            ArrayList combinations = Board.GetCombinationsX(RSxy.Item1, RSxy.Item2);

            foreach (int[] combination in combinations)
            {
                if (Board.IsSolved)
                {
                    break;
                }

                // Inserting values to the board
                for (int i = 0; i < combination.Length; i++)
                {
                    Board.Data[RSxy.Item1 + i + 1, RSxy.Item2].Value = combination[i];
                }

                // At the last sum
                if (level >= Board.LocationsRS.Count - 1)
                {
                    // Solution found
                    if (Board.CheckAll())
                    {
                        Board.IsSolved = true;
                        //WriteLine("Solution found!");
                    }
                    // Next combination
                    else
                    {
                        continue;
                    }
                }
                // Checking for errors, if there are none move to the next combination
                else
                {
                    bool skip = false;
                    for (int i = 0; i < combination.Length; i++)
                    {
                        // At sum-type location
                        if (Board.Data[RSxy.Item1 + i + 1, RSxy.Item2 + 1].Type == ContainerTypes.Sum)
                        {
                            // Checking sum and repetions, skip current combination if something is incorrect
                            if (!Board.CheckSumY(RSxy.Item1 + i + 1, RSxy.Item2 + 1) || !Board.CheckRepetionsY(RSxy.Item1 + i + 1, RSxy.Item2 + 1))
                            {
                                skip = true;
                                break;
                            }
                        }
                        // At Even, Odd or Parity-type location
                        else if (Board.Data[RSxy.Item1 + i + 1, RSxy.Item2 + 1].Type == ContainerTypes.Even || Board.Data[RSxy.Item1 + i + 1, RSxy.Item2 + 1].Type == ContainerTypes.Odd || Board.Data[RSxy.Item1 + i + 1, RSxy.Item2 + 1].Type == ContainerTypes.Parity)
                        {
                            // Checking if Y-Axis is still possible to solve with updated value
                            if (!Board.CheckIfThereAreStillPossibleCombinationsY(RSxy.Item1 + i + 1, RSxy.Item2))
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    // No errors, next recursion level
                    if (!skip)
                    {
                        SolveCombinationsUsingXAxisRun(Board, level + 1);
                    }
                }
                Board.CombinationsTraversed++;
            }
        }

        // Inserts combinations into vertical sum-containers (Y-Axis will always be correct)
        // and checks X-Axis at the last recursion level.
        private static void SolveCombinationsUsingYAxisRun(Kakuro Board, int level)
        {
            (int, int) LSxy = ((int, int))Board.LocationsLS[level];
            ArrayList combinations = Board.GetCombinationsY(LSxy.Item1, LSxy.Item2);

            foreach (int[] combination in combinations)
            {
                if (Board.IsSolved)
                {
                    break;
                }

                // Inserting values to the board
                for (int i = 0; i < combination.Length; i++)
                {
                    Board.Data[LSxy.Item1, LSxy.Item2 - i - 1].Value = combination[i];
                }

                // At the last sum
                if (level >= Board.LocationsLS.Count - 1)
                {
                    // Solution found
                    if (Board.CheckAll())
                    {
                        Board.IsSolved = true;
                        WriteLine("Solution found!");
                    }
                    // Next combination
                    else
                    {
                        continue;
                    }
                }
                // Checking for errors, if there are none move to the next combination
                else
                {
                    bool skip = false;
                    for (int i = 0; i < combination.Length; i++)
                    {
                        // At sum-type location
                        if (Board.Data[LSxy.Item1 - 1, LSxy.Item2 - i - 1].Type == ContainerTypes.Sum)
                        {
                            // Checking sum and repetions, skip current combination if something is incorrect
                            if (!Board.CheckSumX(LSxy.Item1 - 1, LSxy.Item2 - i - 1) || !Board.CheckRepetionsX(LSxy.Item1 - 1, LSxy.Item2 - i - 1))
                            {
                                skip = true;
                                break;
                            }
                        }
                        // At Even, Odd or Parity-type location
                        else if (Board.Data[LSxy.Item1 - 1, LSxy.Item2 - i - 1].Type == ContainerTypes.Even || Board.Data[LSxy.Item1 - 1, LSxy.Item2 - i - 1].Type == ContainerTypes.Odd || Board.Data[LSxy.Item1 - 1, LSxy.Item2 - i - 1].Type == ContainerTypes.Parity)
                        {
                            // Checking if X-Axis is still possible to solve with updated value
                            if (!Board.CheckIfThereAreStillPossibleCombinationsX(LSxy.Item1, LSxy.Item2 - i - 1))
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    // No errors, next recursion level
                    if (!skip)
                    {
                        SolveCombinationsUsingYAxisRun(Board, level + 1);
                    }
                }
                Board.CombinationsTraversed++;
            }
        }
    }
}
