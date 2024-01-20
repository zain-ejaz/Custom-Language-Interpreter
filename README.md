# Custom-Language-Interpreter

A custom programming language interpreter and compiler built in C#, featuring an AST-based interpreter, custom lexer, and parser.

To build please follow the instructions in the BUILD.txt file 

## How to Run
1. After successfully building the project, you can run it within Visual Studio 
by pressing 'Ctrl + F5' or by clicking on 'Debug' -> 'Start Without Debugging'.

2. Once the program is running, you will be prompted to enter the name of a file you want to open. 
This should be a text file located in the "Example Source Files" folder. Just enter the file name, not the full path.

3. The program will read the file line by line. For lines starting with "//", it will output "Comment: " 
followed by the rest of the line. For other lines, it will try to parse and evaluate them as expressions or statements in the language. 
Results of expressions or print statements will be output to the console.

4. To open another file you must close the console and repeat steps 1-2.
