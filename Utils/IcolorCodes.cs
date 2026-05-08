using System.Drawing;

namespace MOZ_UPGRADE.Utils
{


    public interface IcolorCodes
    {
        string GetColorCode_string();
        Color GetColorCode_Color();
        string GetFirstLetter(string name);
    }

    public class ColoCodesRepo : IcolorCodes
    {
        public static List<string> colors = new List<string>
        {
            "#F44336",
            "#E91E63",
             "#9C27B0",
            "#673AB7",
             "#3F51B5",
            "#2196F3",
             "#03A9F4",
            "#00BCD4",
             "#009688",
            "#4CAF50",
             "#8BC34A",
            "#CDDC39",
            "#FFC107",
            "#FF9800",
             "#F9A825",
            "#FF5722",
             "#795548",
            "#607D8B",
             "#9E9E9E",
            "#3E2723",
             "#FFAB00",
            "#1B5E20",
             "#827717",
            "#004D40" ,
            "#01579B",
            "#311B92",
             "#0D47A1",
            "#B71C1C",
             "#880E4F",
            "#4A148C",
             "#455A64"
        };

        public Color GetColorCode_Color()
        {
            Random random = new Random();
            string hexColor = GetColorCode_string();
            hexColor = hexColor.TrimStart('#');
            int r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hexColor.Substring(2, 4), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hexColor.Substring(4, 6), System.Globalization.NumberStyles.HexNumber);
            int a = int.Parse(hexColor.Substring(4, 6), System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(r, g, b);

        }

        public string GetColorCode_string()
        {
            Random random = new Random();

            return colors[random.Next(0, colors.Count - 1)];
        }

        public string GetFirstLetter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "N/a";
            }
            char firstChar = char.ToUpper(name[0]);
            return $"{firstChar}";
        }
    }




}
