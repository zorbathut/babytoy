using Godot;
using System.Linq;

class Bootstrap : Node
{
    bool processed = false;

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

            if (!evekey.Pressed)
                return;
            
            Dbg.Inf($"{key}");
        }
    }
}
