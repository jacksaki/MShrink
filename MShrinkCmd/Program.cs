using ConsoleAppFramework;
using MShrinkCmd;

public class Program
{
    static void Main(string[] args)
    {
        var app = ConsoleApp.Create();
        app.Add<ShrinkCommand>();
        app.Run(args);
    }
}
