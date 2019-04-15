
using System;

class RandomCurve
{
    private readonly float min;
    private readonly float max;
    private readonly float rate;
    private readonly float offset;

    public RandomCurve()
    {
        min = Rand.Value(0.6f, 1f);
        max = Rand.Value(0.8f, 1f);
        rate = Rand.Value(3f, 10f);
        offset = Rand.Value(0f, 1f);
    }

    public float Evaluate()
    {
        return (float)Math.Sin(Godot.OS.GetTicksMsec() / 1000f / rate * 3.14159f * 2) * (max - min) + min;
    }
}
