using Godot;
using System.Linq;

class Bootstrap : Node
{
    bool processed = false;

    public override void _Ready()
    {
        base._Ready();

        Def.Config.InfoHandler = str => GD.Print(str);
        Def.Config.WarningHandler = str => GD.PushWarning(str);
        Def.Config.ErrorHandler = str => GD.PushError(str);
                
        // Spool up defs system
        var parser = new Def.Parser();
        foreach (var fname in Util.GetFilesFromDir("res://").Where(fname => fname.EndsWith(".xml")))
        {
            parser.AddString(Util.GetFileAsString(fname), fname);
        }
        parser.Finish();
    }
}
