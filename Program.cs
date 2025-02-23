using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class LexicalAnalyzer
{
    static void Main(string[] args)
    {
        string inputFilePath = @"C:\Users\sasha\matran\lab2\INPUT.TXT";
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Файл не найден!");
            return;
        }

        string sourceCode = File.ReadAllText(inputFilePath);

        // Список зарезервированных слов 
        string[] reservedWords = {
            "False", "class", "finally", "is", "return",
            "None", "continue", "for", "lambda", "try",
            "True", "def", "from", "nonlocal", "while",
            "and", "del", "global", "not", "with",
            "as", "elif", "if", "or", "yield",
            "assert", "else", "import", "pass",
            "break", "except", "in", "raise",
            "print", "f", "abs", "all", "any", "ascii", "bin", "bool", "bytearray", "bytes",
            "callable", "chr", "classmethod", "compile", "complex", "delattr",
            "dict", "dir", "divmod", "enumerate", "eval", "exec", "filter", "float",
            "format", "frozenset", "getattr", "globals", "hasattr", "hash", "help",
            "hex", "id", "input", "int", "isinstance", "issubclass", "iter", "len",
            "list", "locals", "map", "max", "memoryview", "min", "next", "object",
            "oct", "open", "ord", "pow", "print", "property", "range", "repr",
            "reversed", "round", "set", "setattr", "slice", "sorted", "staticmethod",
            "str", "sum", "super", "tuple", "type", "vars", "zip", "join", "__init__", "power"
        };

        // Таблицы для хранения данных
        Dictionary<string, int> identifiers = new Dictionary<string, int>();
        Dictionary<string, int> constants = new Dictionary<string, int>();
        Dictionary<string, int> operators = new Dictionary<string, int>();
        Dictionary<string, int> delimiters = new Dictionary<string, int>();
        Dictionary<string, int> keywords = new Dictionary<string, int>();

        List<Regex> tokenPatterns = new List<Regex>
        {
            new Regex(@"#.*$|//.*$|/\*[\s\S]*?\*/", RegexOptions.Multiline), // Комментарии
            new Regex(@"""([^""\\]|\\.)*""|'([^'\\]|\\.)*'"), // Строковые литералы
            new Regex(@"[a-zA-Z_]\w*"), // Идентификаторы (только латинские буквы)
            new Regex(@"\d+(\.\d*)?([eE][+-]?\d+)?"), // Числовые константы
            new Regex(@"[+\-*/=<>!&|%^]+"), // Операторы
            new Regex(@"[(){}\[\];:,]"), // Разделители
            new Regex(@"\s+"), // Пробелы и табуляция
            new Regex(@"\n") // Новая строка
        };

        // Выходной поток лексем
        List<string> tokensOutput = new List<string>();

        int position = 0;
        int idCounter = 1;
        int constCounter = 1;
        int opCounter = 1;
        int delimCounter = 1;
        int kwCounter = 1;
        bool hasErrors = false;

        while (position < sourceCode.Length)
        {
            bool matchFound = false;
            foreach (var pattern in tokenPatterns)
            {
                Match match = pattern.Match(sourceCode, position);
                if (match.Success && match.Index == position)
                {
                    string token = match.Value;

                    if (string.IsNullOrEmpty(token) || token.StartsWith("#") || token.StartsWith("//") || token.StartsWith("/*"))
                    {
                        // Игнорируем комментарии
                        position += match.Length;
                        matchFound = true;
                        break;
                    }

                    if (Array.Exists(reservedWords, word => word.Equals(token, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!keywords.ContainsKey(token))
                        {
                            keywords.Add(token, kwCounter++);
                        }
                        tokensOutput.Add($"<РЕЗЕРВ{keywords[token]}>");
                    }
                    else if (Regex.IsMatch(token, @"^[a-zA-Z_]\w*$")) // Идентификаторы
                    {
                        if (!identifiers.ContainsKey(token))
                        {
                            identifiers.Add(token, idCounter++);
                        }
                        tokensOutput.Add($"<ИД{identifiers[token]}>");
                    }
                    else if (Regex.IsMatch(token, @"^\d+(\.\d*)?([eE][+-]?\d+)?$")) // Числовые константы
                    {
                        if (!constants.ContainsKey(token))
                        {
                            constants.Add(token, constCounter++);
                        }
                        tokensOutput.Add($"<КОНСТ{constants[token]}>");
                    }
                    else if (Regex.IsMatch(token, @"""([^""\\]|\\.)*""|'([^'\\]|\\.)*'")) // Строковые литералы
                    {
                        if (!constants.ContainsKey(token))
                        {
                            constants.Add(token, constCounter++);
                        }
                        tokensOutput.Add($"<СТР{constants[token]}>");
                    }
                    else if (Regex.IsMatch(token, @"[+\-*/=<>!&|%^]+")) 
                    {
                        if (!operators.ContainsKey(token))
                        {
                            operators.Add(token, opCounter++);
                        }
                        tokensOutput.Add(token); 
                    }
                    else if (Regex.IsMatch(token, @"[(){}\[\];:,]")) 
                    {
                        if (!delimiters.ContainsKey(token))
                        {
                            delimiters.Add(token, delimCounter++);
                        }
                        tokensOutput.Add(token); 
                    }
                    else if (Regex.IsMatch(token, @"\s+")) 
                    {
                        tokensOutput.Add(token); 
                    }
                    else if (Regex.IsMatch(token, @"\n")) 
                    {
                        tokensOutput.Add("\n"); 
                    }

                    position += match.Length;
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
            {
                Console.WriteLine($"Лексическая ошибка в позиции {position}: '{sourceCode[position]}'");
                position++;
                hasErrors = true;
            }
        }

        if (!hasErrors)
        {
            Console.WriteLine("Лексических ошибок не обнаружено.");
        }

        Console.WriteLine("Поток лексем:");
        Console.WriteLine(string.Join("", tokensOutput));

        Console.WriteLine("\nТаблица идентификаторов:");
        foreach (var id in identifiers)
        {
            Console.WriteLine($"{id.Value}. {id.Key} - Переменная");
        }

        Console.WriteLine("\nТаблица констант:");
        foreach (var cnst in constants)
        {
            Console.WriteLine($"{cnst.Value}. {cnst.Key} - Константа");
        }

        Console.WriteLine("\nТаблица операторов:");
        foreach (var op in operators)
        {
            Console.WriteLine($"{op.Value}. {op.Key} - Оператор");
        }

        Console.WriteLine("\nТаблица разделителей:");
        foreach (var delim in delimiters)
        {
            Console.WriteLine($"{delim.Value}. {delim.Key} - Разделитель");
        }

        Console.WriteLine("\nТаблица ключевых слов:");
        foreach (var kw in keywords)
        {
            Console.WriteLine($"{kw.Value}. {kw.Key} - Ключевое слово");
        }

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }
}