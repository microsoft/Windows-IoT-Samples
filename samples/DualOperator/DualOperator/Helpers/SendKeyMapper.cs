using System.Text.RegularExpressions;
using DualOperator.Structures;

namespace DualOperator.Helpers
{
    internal static class SendKeyMapper
    {
        public static string GetKeyCode(string RawCode, OperatorKeyState ModifierKeyState)
        {
            // Our check for alphabetic keys
            bool isAlpha = Regex.IsMatch(RawCode, @"^[a-zA-Z]+$");
            bool isNumber = Regex.IsMatch(RawCode, @"^[0-9]+$");

            // Return the lower case value if there are no modifiers present and the key is alphabetic
            if ((!ModifierKeyState.AltPressed && 
                !ModifierKeyState.ControlPressed && 
                !ModifierKeyState.ShiftPressed &&
                (isAlpha || isNumber)))
            {
                return (isAlpha) ? RawCode.ToLower() : RawCode;
            }

            switch (ModifierKeyState.ShiftPressed)
            {
                // Return upper case values if SHIFT is pressed and the key is alphabetic
                case true when isAlpha:
                    return RawCode.ToUpper();

                // Return shift key values if SHIFT is pressed and the key is numeric
                case true when isNumber:
                    return RawCode switch
                    {
                        "0" => ")",
                        "1" => "!",
                        "2" => "@",
                        "3" => "#",
                        "4" => "$",
                        "5" => "%",
                        "6" => "^",
                        "7" => "&",
                        "8" => "*",
                        _ => "("
                    };

                // And the other special characters
                case true when RawCode.Length == 1:
                    return RawCode switch
                    {
                        "`" => "~",
                        "-" => "_",
                        "=" => "+",
                        "[" => "{",
                        "]" => "}",
                        "\\" => "|",
                        ";" => ":",
                        "'" => "\"",
                        "," => "<",
                        "." => ">",
                        "/" => "?"
                    };
            }

            // Add the modifier(s) as needed
            if (ModifierKeyState.AltPressed)
            {
                RawCode = "%" + RawCode;
            }
            if (ModifierKeyState.ControlPressed)
            {
                RawCode = "^" + RawCode;
            }
            if (ModifierKeyState.ShiftPressed)
            {
                RawCode = "+" + RawCode;
            }

            // And finally return the new mapping
            return RawCode;
        }
    }
}
