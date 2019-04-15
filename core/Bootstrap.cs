using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

class Bootstrap : Node
{
    bool processed = false;

    struct ButtonInfo
    {
        public KeyList key;
        public ButtonDef def;
        public Node2D node;
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
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (!processed)
        {
            var samples = new List<AudioInfo>();
            foreach (var fname in Util.GetFilesFromDir("res://").Where(fname => !fname.Contains("/.") && fname.EndsWith(".wav")))
            {
                samples.Add(new AudioInfo() { stream = ResourceLoader.Load(fname) as AudioStream, weight = 1 });
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

                if (button.size.y == 1)
                {
                    buttoninstance.FindNode<Sprite>("image").Texture = ResourceLoader.Load($"res://button/{(int)(button.size.x * 100)}.png") as Texture;
                }
                else
                {
                    buttoninstance.FindNode<Sprite>("image").Texture = ResourceLoader.Load($"res://button/{(int)(button.size.y * 100)}.{(int)(button.size.x * 100)}.png") as Texture;
                }

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
                    catch (ArgumentException e)
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
                }
            }
        }
    }
}
