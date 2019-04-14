using System;
using System.Collections.Generic;
using System.Linq;

class ConvertVector2 : Def.Converter
{
    public override HashSet<Type> GeneratedTypes()
    {
        return new HashSet<Type>() { typeof(Godot.Vector2) };
    }

    public override object FromString(string input, Type type, string inputName, int lineNumber)
    {
        // first check to see if we're a static
        {
            var property = typeof(Godot.Vector2).GetProperty(input, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (property != null)
            {
                return property.GetValue(null);
            }
        }

        // we're not a static. Try parsing.
        if (input.Length >= 3)
        {
            // Chop off parens
            if (input[0] == '(' && input[input.Length - 1] == ')')
                input = input.Substring(1, input.Length - 2);

            var pieces = input.Split(',').Select(item => item.Trim()).ToArray();
            if (pieces.Length == 2)
            {
                return new Godot.Vector2(float.Parse(pieces[0]), float.Parse(pieces[1]));
            }
        }
        
        Dbg.Err($"{inputName}:{lineNumber}: Failed to parse Vector2 \"{input}\".");

        return null;
    }
}
