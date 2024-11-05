using System.Text.RegularExpressions;
using System.Diagnostics;

public static class CitromKereszt
{
    // Program.cs
    static string programContent =  "\n\n\n\n\n" +
                                    "#region &\n" +
                                    " \n" +
                                    "void Feladat(int i, bool isFirst = false)\n" +
                                    "{\n" +
                                    "   if (isFirst) {\n" +
                                    "       Console.WriteLine($\"-----{i}. feladat-----\");\n" +
                                    "    } else {\n" +
                                    "       Console.WriteLine($\"\\n\\n-----{i}. feladat-----\");\n" +
                                    "   }\n" +
                                    "}\n" +
                                    " \n" +
                                    "string ReadFromConsole(string msg)\n" +
                                    "{\n" +
                                    "   Console.Write(msg);\n" +
                                    "   return Console.ReadLine()!;\n" +
                                    "}\n" +
                                    "\n" +
                                    "#endregion\n";
    static string fileContent = "";
    static string outPut = "";

    public static void Main(string[] args)
    {

        if(args.Length == 0) {
            Console.WriteLine("Parameters required: new | prec");
            return;
        }

        switch (args[0])
        {
            case "new":
                CreateProject();
                break;
            case "prec":
                PreCompile(args);
                break;
        }
    }

    // CREATE PROJECT //

    static void CreateProject()
    {
        // Creating dotnet project
        Console.WriteLine("Creating dotnet project...");

        CreateDotnetProjectCmd();

        Console.WriteLine("Writing Program.cs...");
        File.WriteAllText("Program.cs", programContent);

        Console.WriteLine("Writing run.bat...");
        File.WriteAllText("run.bat", "cls\ndotnet run");

        Console.WriteLine("Succesfully created Console App for Citromkereszt!");
    }

    static void CreateDotnetProjectCmd()
    {
        string cmd = "dotnet new console";
        ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + cmd);
        processInfo.CreateNoWindow = true; // To prevent a console window from showing
        processInfo.UseShellExecute = false; // Required to redirect output
        processInfo.RedirectStandardOutput = true; // Capture the output

        using (Process process = Process.Start(processInfo)!)
        {
            using (System.IO.StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Console.WriteLine(result);
            }
        }
    }

    // PRECOMPILE //
    static void PreCompile(string[] args)
    { 
        if(args.Length >= 2) {
            fileContent = ReadFile(args[1]);
        } else {
            Console.WriteLine("Param 1: Source file required");
            return;
        }

        if(args.Length >= 3) {
            outPut = args[2];
        } else {
            Console.WriteLine("Param 2: Destination file required");
            return;
        }

        ReplaceFirstTask();
        ReplaceSimpleTask();

        ReplaceReadFromConsole();

        RemoveEndSection();

        File.WriteAllText(outPut, fileContent);
    }

    static string ReadFile(string fileName)
    {
        return File.ReadAllText(fileName);
    }

    static void RemoveEndSection()
    {
        int index = fileContent.IndexOf("#region &");

        if(index != -1) {
            fileContent = fileContent.Substring(0, index);
        }
    }

    static void ReplaceFirstTask()
    {
        string pattern = @"Feladat\((\d+), true\);";

        string result = Regex.Replace(fileContent, pattern, match => 
        {
            string number = match.Groups[1].Value;
            
            return $"Console.WriteLine($\"-----{number}. feladat-----\");";
        });

        fileContent = result;
    }
    
    static void ReplaceSimpleTask()
    {
        string pattern = @"Feladat\((\d+)\);";

        string result = Regex.Replace(fileContent, pattern, match => 
        {
            string number = match.Groups[1].Value;
            
            return $"Console.WriteLine($\"\\n\\n-----{number}. feladat-----\");";
        });

        fileContent = result;
    }

    static void ReplaceReadFromConsole()
    {
        List<string> content = fileContent.Split('\n').ToList();
        List<(int index, string str)> inserts = new List<(int, string)>();

        string pattern = @"ReadFromConsole\(""([^""]*)""\)";

        Regex regex = new Regex(pattern);

        for(int i = 0; i < content.Count; i++)
        {
            Match match = regex.Match(content[i]);

            if (match.Success)
            {
                content[i] = regex.Replace(content[i], "Console.ReadLine()!");

                string capturedText = match.Groups[1].Value;
                inserts.Add((i, $"Console.Write(\"{capturedText}\");"));
            }
        }

        for(int i = 0; i < inserts.Count; i++)
        {
            content.Insert(i + inserts[i].index, inserts[i].str);
        }

        fileContent = string.Join('\n', content);
    }
}