using LightDemo;

public class Program
{
    public static void Main()
    {
        try
        {
            var game = new Game();
            game.Run();
        }
        catch (Exception ex)
        {
            File.WriteAllText("Error.txt", ex.ToString());
            Console.WriteLine(ex.ToString());
            Console.ReadKey();
        }
    }
}