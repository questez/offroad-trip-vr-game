using UnityEngine;
using UnityEngine.UI;

public class Fuel : MonoBehaviour
{
    private static float fuel_value;
    private const float max_fuel_value = 50;
    [SerializeField] private Slider fuel_bar; // сам слайдер
    [SerializeField] private Image fuel_bar_fill; // компонент Fill у слайдера
    [SerializeField] private Image fuel_bar_backround; // компонент Backround у слайдера

    private const string hex_green = "#0F9800";
    private const string hex_orange = "#BE6900";
    private const string hex_red = "#8A1B00";
    private const string hex_black_red = "#5A0000";
    private const string hex_white = "#FFFFFF";    

    public static bool IsFuelEmpty { get; private set; }

    private void Awake()
    {
        fuel_value = 50;
    }

    private void Update()
    {
        fuel_value = Mathf.Clamp(fuel_value, 0, max_fuel_value);
        fuel_bar.value = fuel_value;

        if (fuel_bar.value > 25)
        {
            UpdateBarColor(hex_green, hex_white);
        }
        else if (fuel_bar.value <= 25 && fuel_bar.value > 5)
        {
            UpdateBarColor(hex_orange, hex_white);
        }
        else if (fuel_bar.value <= 5 && fuel_bar.value > 0)
        {
            UpdateBarColor(hex_red, hex_white);
        }
        else
        {
            UpdateBarColor(hex_red, hex_black_red);
            fuel_bar_fill.enabled = false;
            IsFuelEmpty = true;
        }
    }

    public static void UpdateFuelValue(bool allWheelDriveMode, float CarSpeed)
    {
        if (!allWheelDriveMode)
        {
            fuel_value -= (CarSpeed / 10000);
        }
        else
        {
            fuel_value -= (CarSpeed / 10000) * 2.5f;
        }
    }

    private void UpdateBarColor(string hex_color_fill, string hex_color_backround)
    {
        if (ColorUtility.TryParseHtmlString(hex_color_fill, out Color color1))
        {
            fuel_bar_fill.color = color1;
        }
        if (ColorUtility.TryParseHtmlString(hex_color_fill, out Color color2))
        {
            fuel_bar_fill.color = color2;
        }
        if (ColorUtility.TryParseHtmlString(hex_color_backround, out Color color3))
        {
            fuel_bar_backround.color = color3;
        }        
    }

    public static void AddFuel(float new_fuel_value)
    { 
        fuel_value += new_fuel_value;     
    }

    public static float GetFuel() => fuel_value;  
    public static float GetMaxFuel() => max_fuel_value;  
}
