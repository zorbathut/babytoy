using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

static class Rand
{
    private static Random state = new Random();

    public static float Value
    {
        get
        {
            return (float)state.NextDouble();
        }
    }
}
