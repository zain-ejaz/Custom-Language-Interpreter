using System;
using System.IO;
using LanguageDesign;

public static class Program
{
    public static void Main()
    {
        Console.Write("Enter the name of the file you want to open: ");
        var fileName = Console.ReadLine();
        if (string.IsNullOrEmpty(fileName))
        {
            Console.WriteLine("Invalid file name.");
            return;
        }

        // Get current working directory and its parent directory.
        var workingDirectory = Directory.GetCurrentDirectory();
        var finalSubmissionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.Parent.FullName;
        var filePath = Path.Combine(finalSubmissionDirectory, @"Example Source Files\", fileName);

        if (!File.Exists(filePath))
        {
            Console.WriteLine("File does not exist.");
            return;
        }

        // Instantiate the SymbolTable
        var symbolTable = new SymbolTable();

        // Read all lines of the file
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("//"))
            {
                // Comment line, skip it and continue to the next line
                Console.WriteLine("Comment: " + line.TrimStart().Substring(2));
                continue;
            }
            // Instantiate the Lexer and Parser
            var lexer = new Lexer(line);
            var parser = new Parser(lexer);

            AstNode ast;
            try
            {
                // Try parsing the line as a statement
                ast = parser.ParseStatement();
                var result = ast.Evaluate(symbolTable);
                // Check if the result is a PrintNode
                if (result is PrintNode printNode)
                {
                    // Evaluate the expression in the print statement
                    var printResult = printNode.Expression.Evaluate(symbolTable);
                    Console.WriteLine(printResult);
                }
            }
            catch (Exception)
            {
                // Parsing as a statement failed, try parsing as an expression
                ast = parser.ParseExpression();
                var result = ast.Evaluate(symbolTable);
                Console.WriteLine(result);
            }
        }
    }
}
