namespace Statify;

public static class ConsoleWriter
{
    private const ConsoleColor NeutralColor = ConsoleColor.DarkCyan;

    public static void Title(object title)
    {
        Console.BackgroundColor = NeutralColor;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.WriteLine(title + Environment.NewLine);

        Console.ResetColor();
    }

    public static void TitledMessage(string title, object message, ConsoleColor backgroundColor = NeutralColor,
        ConsoleColor foregroundColor = ConsoleColor.Black)
    {
        Console.BackgroundColor = backgroundColor;
        Console.ForegroundColor = foregroundColor;
        Console.Write(title.ToUpper() + ":");

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = backgroundColor;
        Console.WriteLine(" " + message);

        Console.ResetColor();
    }


    public static void Success(object message)
    {
        TitledMessage("Success", message, ConsoleColor.Green);
    }

    public static void Warning(object message)
    {
        TitledMessage("Warning", message, ConsoleColor.DarkYellow);
    }

    public static void Fatal(object message)
    {
        TitledMessage("Fatal", message, ConsoleColor.Red);
    }
}