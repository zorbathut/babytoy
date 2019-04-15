using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

static class Rand
{
    private static Random state = new Random();

    public static float Value()
    {
        return (float)state.NextDouble();
    }

    public static float Value(float min, float max)
    {
        return Value() * (max - min) + min;
    }
}
