namespace LegendarySpork9Wiki.Converters
{
    public static class StyleConverter
    {
        public static string GetTopBarDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-color: #2D2D2D; border-bottom: 1px solid #444;";
            }

            return "background-color: #f7f7f7; border-bottom: 1px solid #d6d5d5;";
        }

        public static string GetNavMenuDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-image: linear-gradient(180deg, #1a1a2e 0%, #16213e 70%);";
            }

            return "background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);";
        }

        public static string GetBodyDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-color: #313131; color: #A9A9A9;";
            }

            return "background-color: #ffffff; color: #212529;";
        }

        public static string GetTableDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-color: #3E3E3E; color: #A9A9A9; border-color: #555;";
            }

            return string.Empty;
        }

        public static string GetFormDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-color: #3E3E3E; color: #A9A9A9; border-color: #555;";
            }

            return string.Empty;
        }

        public static string GetInputDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-color: #4E4E4E; color: #D0D0D0; border-color: #666;";
            }

            return string.Empty;
        }

        public static string GetTableRowDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "border-color: #555;";
            }

            return string.Empty;
        }

        public static string GetCardDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-color: #3E3E3E; color: #A9A9A9; border-color: #555;";
            }

            return string.Empty;
        }

        public static string GetLinkDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "color: deepskyblue;";
            }

            return "color: #006bb7;";
        }

        public static string GetButtonDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "background-color: #0d6efd; border-color: #0d6efd; color: #fff;";
            }

            return string.Empty;
        }

        public static string GetTextMutedDarkMode(bool darkMode)
        {
            if (darkMode)
            {
                return "color: #888 !important;";
            }

            return string.Empty;
        }
    }
}
