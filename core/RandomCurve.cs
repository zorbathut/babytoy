
using System;
using System.Collections.Generic;
using System.Linq;

[Def.StaticReferences]
public static class RandomDistribution
{
    static RandomDistribution() { Def.StaticReferencesAttribute.Initialized(); }

    public static RandomDistributionDef Color;
    public static RandomDistributionDef Position;
}

public class RandomDistributionDef : Def.Def
{
    public Range min;
    public Range max;
    public Range rate;
}

public class Range
{
    public float min = 0;
    public float max = 1;

    public Range() { }
    public Range(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float Value()
    {
        return Rand.Value(min, max);
    }
}

class ConvertRange : Def.Converter
{
    public override HashSet<Type> GeneratedTypes()
    {
        return new HashSet<Type>() { typeof(Range) };
    }

    public override object FromString(string input, Type type, string inputName, int lineNumber)
    {
        if (input.Length >= 3)
        {
            // Chop off parens
            if (input[0] == '(' && input[input.Length - 1] == ')')
                input = input.Substring(1, input.Length - 2);

            var pieces = input.Split(',').Select(item => item.Trim()).ToArray();
            if (pieces.Length == 2)
            {
                return new Range(float.Parse(pieces[0]), float.Parse(pieces[1]));
            }
        }
        
        Dbg.Err($"{inputName}:{lineNumber}: Failed to parse Range \"{input}\".");

        return null;
    }
}


class RandomCurve
{
    private readonly float min;
    private readonly float max;
    private readonly float rate;
    private readonly float offset;

    public RandomCurve(RandomDistributionDef def)
    {
        min = def.min.Value();
        max = def.max.Value();
        rate = def.rate.Value();
        offset = Rand.Value(0f, 1f);
    }

    public float Evaluate()
    {
        return (float)Math.Sin(Godot.OS.GetTicksMsec() / 1000f / rate * 3.14159f * 2) * (max - min) + min;
    }
}
