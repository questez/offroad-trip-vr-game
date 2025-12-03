public class Wheather
{
    private static string dayStatus = "day";

    public static string GetDayStatus() => dayStatus;    

    public static void TurnOnDay()
    {
        dayStatus = "day";        
    }

    public static void TurnOnNight()
    {
        dayStatus = "night";
    }
}
