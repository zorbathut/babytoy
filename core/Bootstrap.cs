using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

class Bootstrap : Node
{
    bool processed = false;

    class ButtonInfo
    {
        public KeyList key;
        public ButtonDef def;
        public Node2D node;

        public RandomCurve r = new RandomCurve(RandomDistribution.Color);
        public RandomCurve g = new RandomCurve(RandomDistribution.Color);
        public RandomCurve b = new RandomCurve(RandomDistribution.Color);

        public RandomCurve x = new RandomCurve(RandomDistribution.Position);
        public RandomCurve y = new RandomCurve(RandomDistribution.Position);
    }
    // this is slow but it doesn't matter because keyboards don't have that many buttons
    List<ButtonInfo> buttons = new List<ButtonInfo>();

    class AudioInfo
    {
        public AudioStream stream;
        public float weight;
    }

    public override void _Ready()
    {
        base._Ready();

        Def.Config.InfoHandler = str => GD.Print("INF: " + str);
        Def.Config.WarningHandler = str => { GD.Print("WRN: " + str); GD.PushWarning(str); };
        Def.Config.ErrorHandler = str => { GD.Print("ERR: " + str); GD.PushError(str); };
        
        // Spool up defs system
        var parser = new Def.Parser();
        foreach (var fname in Util.GetFilesFromDir("res://").Where(fname => !fname.Contains("/.") && fname.EndsWith(".xml")))
        {
            parser.AddString(Util.GetFileAsString(fname), fname);
        }
        parser.Finish();

        if (!Engine.IsEditorHint())
        {
            OS.WindowFullscreen = true;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (!processed)
        {
            var samples = new List<AudioInfo>();

            foreach (var fname in Util.GetFilesFromDir("res://").Where(fname => !fname.Contains("/.") && fname.EndsWith(".wav.import")))
            {
                samples.Add(new AudioInfo() { stream = ResourceLoader.Load(fname.Substring(0, fname.Length - 7)) as AudioStream, weight = 1 });
            }

            var buttonresource = ResourceLoader.Load("res://button/button.tscn") as PackedScene;
            var buttonexample = buttonresource.Instance() as Node2D;
            var buttonsize = (buttonexample.FindNode("image") as Sprite).Texture.GetSize();

            // Init all the buttons
            foreach (var button in Def.Database<ButtonDef>.List)
            {
                var buttoninstance = buttonresource.Instance() as Node2D;
                GetParent().AddChild(buttoninstance);
                buttoninstance.Position = button.position * buttonsize;

                buttoninstance.FindNode<Sprite>("image").Texture = ResourceLoader.Load($"res://button/{(int)(button.size.y * 100)}.{(int)(button.size.x * 100)}.outline.png") as Texture;
                buttoninstance.FindNode<Sprite>("flash").Texture = ResourceLoader.Load($"res://button/{(int)(button.size.y * 100)}.{(int)(button.size.x * 100)}.solid.png") as Texture;

                var sample = samples.RandomElementByWeight(ai => ai.weight);
                samples.Remove(sample);

                buttoninstance.FindNode<AudioStreamPlayer>("audio").Stream = sample.stream;

                KeyList key = (KeyList)0;
                if (button.linkage != (KeyList)0)
                {
                    key = button.linkage;
                }
                else
                {
                    try
                    {
                        key = (KeyList)Enum.Parse(typeof(KeyList), button.defName);
                    }
                    catch (ArgumentException)
                    {
                        Dbg.Inf($"Failed to parse {button.defName}");
                    }
                }

                if (key != (KeyList)0)
                {
                    buttons.Add(new ButtonInfo() { key = key, def = button, node = buttoninstance });
                }
            }
            processed = true;
        }

        foreach (var button in buttons)
        {
            var image = button.node.FindNode<Sprite>("image");
            image.SelfModulate = new Color(button.r.Evaluate(), button.g.Evaluate(), button.b.Evaluate());
            image.Offset = new Vector2(button.x.Evaluate(), button.y.Evaluate());
        }
    }

    public override void _Input(InputEvent eve)
    {
        base._Input(eve);

        if (eve is InputEventKey evekey)
        {
            // stoooop
            var key = (Godot.KeyList)evekey.Scancode;

            if (!evekey.Pressed || evekey.Echo)
                return;
            
            Dbg.Inf($"{key}");

            foreach (var button in buttons)
            {
                if (button.key == key)
                {
                    button.node.FindNode<AudioStreamPlayer>("audio").Play();
                    button.node.FindNode<AnimationPlayer>("player").Play("flash");
                }
            }
        }
    }
}
