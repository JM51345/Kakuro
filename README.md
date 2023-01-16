# Kakuro
A Windows program for playing Kakuro puzzles. Not completely finished yet.

![Example image](/Images/kakuro.png)

# Controls
- Arrow keys to move
- 1 to 9 to input value
- Spacebar to remove value
- S to solve
- R to reset console
- \+ to increase console size
- \- to decrease console size
- CTRL+C to close current game

# Files
The easiest way to create new save files is to use a .csv file with the separator ";" and then changing the file type to .txt.
##### Board dimensions
- HEIGHT,{value}
- WIDHT,{value}
##### Data
- E = even container with no value
- O = odd container with no value
- P = parity container with no value (parity = even or odd value)
- B = empty container
- RS,{sumRight} = sum container with value on the right side of the container
- LS,{sumLeft} = sum container with value on the left side of the container
- LRS,{sumLeft},{sumRight} = sum container with value on the left and right side of the container
- E,{value} = even container with value
- O,{value} = odd container with value
- P,{value} = parity container with value
