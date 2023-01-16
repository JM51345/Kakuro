using System;

namespace Kakuro
{
    class Container
    {
        //Field variables
        private int? _value;
        private readonly Random rng;

        //Automatic properties
        public ContainerTypes Type { get; private set; }
        public ConsoleColor ContainerForegroundColor { get; private set; }
        public ConsoleColor ContainerBackgroundColor { get; private set; }
        public int? SumLeft { get; }
        public int? SumRight { get; }

        //Properties
        public int? Value
        {
            get { return _value; }
            set
            {
                if ( !(value is null) )
                {
                    if (Type == ContainerTypes.Even && value % 2 != 0)
                    {
                        throw new Exception("Value must be even for type even.");
                    }
                    else if (Type == ContainerTypes.Odd && value % 2 != 1)
                    {
                        throw new Exception("Value must be odd for type odd.");
                    }
                    else if (Type == ContainerTypes.Sum || Type == ContainerTypes.Block)
                    {
                        throw new Exception("Value can not be set for types sum and block.");
                    }
                    else if (value < 1 || value > 9)
                    {
                        throw new Exception("Value must be in range of 1 to 9.");
                    }
                }
                _value = value;
            }
        }

        //Constructors
        // For creating type even, odd or parity container with a type name (value will be null)
        public Container( ContainerTypes Type )
        {
            rng = new Random();
            if ( Type == ContainerTypes.Even )
            {
                this.Type = Type;
                Value = null;
            }
            else if ( Type == ContainerTypes.Odd )
            {
                this.Type = Type;
                Value = null;
            }
            else if ( Type == ContainerTypes.Parity )
            {
                this.Type = Type;
                Value = null;
            }
            else
            {
                throw new Exception("Constructor(Type) can only be used to create type Even, Odd and Parity.");
                
            }
            SetColor();
        }

        // For creating type Even, Odd and Parity containers with a type name and a number
        public Container( ContainerTypes Type, int Value )
        {
            rng = new Random();
            if (Type == ContainerTypes.Even)
            {
                this.Type = Type;
                this.Value = Value;
            }
            else if (Type == ContainerTypes.Odd)
            {
                this.Type = Type;
                this.Value = Value;
            }
            else if (Type == ContainerTypes.Parity)
            {
                this.Type = Type;
                this.Value = Value;
            }
            else
            {
                throw new Exception("Constructor(Type, Value) can only be used to create type Even, Odd and Parity.");
            }
            SetColor();
        }

        // For creating type Sum or Block container
        public Container( int? SumLeft = null, int? SumRight = null )
        {
            if ( SumLeft is null && SumRight is null )
            {
                Type = ContainerTypes.Block;
            }
            else
            {
                if ( SumLeft < 3 && !(SumLeft is null) )
                {
                    throw new Exception($"SumLeft must be larger than 2.");
                }
                else if ( SumRight < 3 && !(SumRight is null) )
                {
                    throw new Exception($"SumRight must be larger than 2.");
                }

                Type = ContainerTypes.Sum;
                this.SumLeft = SumLeft is null ? null : SumLeft;
                this.SumRight = SumRight is null ? null : SumRight;
            }
            SetColor();
        }

        //Methods
        // Randomizes value of the container if it's type even, odd or parity
        public void RandomizeValue()
        {
            int min = 1,
                max = 9;

            switch (Type)
            {
                case ContainerTypes.Even:
                    do
                    {
                        int temp = rng.Next(min, max + 1);
                        if ( temp % 2 == 0 )
                        {
                            Value = temp;
                            break;
                        }
                    } while ( true );
                    break;
                case ContainerTypes.Odd:
                    do
                    {
                        int temp = rng.Next(min, max + 1);
                        if (temp % 2 == 1)
                        {
                            Value = temp;
                            break;
                        }
                    } while ( true );
                    break;
                case ContainerTypes.Parity:
                    Value = rng.Next(min, max + 1);
                    break;
                default:
                    throw new Exception("Method RandomizeValue is only for Container-types even, odd and parity.");
            }
        }
        
        // Returns String[]-array of a container containing info how to print it
        public string[] GetPrintArray()
        {
            string[] printArray = new string[5];

            if ( (Type == ContainerTypes.Even || Type == ContainerTypes.Odd || Type == ContainerTypes.Parity) && !(Value is null) )
            {
                printArray[0] = "╒════════╕";
                printArray[1] = "│        │";
                printArray[2] = $"│   0{Value}   │";
                printArray[3] = "│        │";
                printArray[4] = "╘════════╛";
            }
            else if ( Type == ContainerTypes.Sum )
            {
                if ( !(SumLeft is null) && !(SumRight is null) )
                {
                    if ( SumLeft < 10 && SumRight < 10 )
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = $"│  \\\\ 0{SumRight} │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = $"│ 0{SumLeft} \\\\  │";
                        printArray[4] = "╘════════╛";
                    }
                    else if ( SumLeft < 10 && SumRight > 9 )
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = $"│  \\\\ {SumRight} │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = $"│ 0{SumLeft} \\\\  │";
                        printArray[4] = "╘════════╛";
                    }
                    else if ( SumLeft > 9 && SumRight < 10 )
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = $"│  \\\\ 0{SumRight} │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = $"│ {SumLeft} \\\\  │";
                        printArray[4] = "╘════════╛";
                    }
                    else
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = $"│  \\\\ {SumRight} │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = $"│ {SumLeft} \\\\  │";
                        printArray[4] = "╘════════╛";
                    }
                }
                else if ( SumLeft is null )
                {
                    if ( SumRight < 10 )
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = $"│  \\\\ 0{SumRight} │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = "│    \\\\  │";
                        printArray[4] = "╘════════╛";
                    }
                    else
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = $"│  \\\\ {SumRight} │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = "│    \\\\  │";
                        printArray[4] = "╘════════╛";
                    }

                }
                else if ( SumRight is null )
                {
                    if ( SumLeft < 10 )
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = "│  \\\\    │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = $"│ 0{SumLeft} \\\\  │";
                        printArray[4] = "╘════════╛";
                    }
                    else
                    {
                        printArray[0] = "╒════════╕";
                        printArray[1] = "│  \\\\    │";
                        printArray[2] = "│   \\\\   │";
                        printArray[3] = $"│ {SumLeft} \\\\  │";
                        printArray[4] = "╘════════╛";
                    }
                }
                else
                {
                    printArray[0] = ("╒════════╕");
                    printArray[1] = ("│  \\\\    │");
                    printArray[2] = ("│   \\\\   │");
                    printArray[3] = ("│    \\\\  │");
                    printArray[4] = ("╘════════╛");
                }
            }
            else
            {
                printArray[0] = "╒════════╕";
                printArray[1] = "│        │";
                printArray[2] = "│        │";
                printArray[3] = "│        │";
                printArray[4] = "╘════════╛";
            }
            return printArray;
        }

        private void SetColor()
        {
            switch (Type)
            {
                case ContainerTypes.Even:
                    ContainerBackgroundColor = ConsoleColor.White;
                    ContainerForegroundColor = ConsoleColor.Black;
                    break;
                case ContainerTypes.Odd:
                    ContainerBackgroundColor = ConsoleColor.Gray;
                    ContainerForegroundColor = ConsoleColor.Black;
                    break;
                case ContainerTypes.Parity:
                    ContainerBackgroundColor = ConsoleColor.DarkGray;
                    ContainerForegroundColor = ConsoleColor.Black;
                    break;
                case ContainerTypes.Sum:
                    ContainerBackgroundColor = ConsoleColor.Black;
                    ContainerForegroundColor = ConsoleColor.White;
                    break;
                case ContainerTypes.Block:
                    ContainerBackgroundColor = ConsoleColor.Black;
                    ContainerForegroundColor = ConsoleColor.White;
                    break;
                default:
                    break;
            }
        }
    }
}

        