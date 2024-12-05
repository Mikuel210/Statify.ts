namespace Statify;

public static class Api
{
    public static Dictionary<string, object> Context { get; private set; }
    
    public static void Write(string content)
    {
        ConsoleWriter.TitledMessage("Write", content, ConsoleColor.Cyan);
    }

    public static void Compile(Dictionary<string, object> context) {
        Context = context;
        
        ConsoleWriter.Warning(Interpreter.Interpret((string)context["input"], Context));
    }
}