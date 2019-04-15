using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

static class Util
{
    public static IEnumerable<string> GetFilesFromDir(string dirname)
    {
        if (dirname == "res://")
            dirname = "res:/";
        
        var dir = new Directory();
        dir.Open(dirname);
        dir.ListDirBegin(skipNavigational: true);
        while (true)
        {
            string fname = dir.GetNext();
            if (fname == "")
            {
                break;
            }

            if (dir.CurrentIsDir())
            {
                foreach (var subdirentry in GetFilesFromDir(dirname + "/" + fname))
                {
                    yield return subdirentry;
                }
            }
            else
            {
                yield return dirname + "/" + fname;
            }
        }
    }

    public static string GetFileAsString(string path)
    {
        var file = new File();
        var openerror = file.Open(path, (int)File.ModeFlags.Read);
        if (openerror != Error.Ok)
        {
            Dbg.Err($"Failure opening file {path}: {openerror}");
            return "";
        }
        var result = file.GetAsText();
        file.Close();
        return result;
    }

    public static IEnumerable<Node> GetNodes(this SceneTree sceneTree)
    {
        return sceneTree.GetRoot().GetAllChildren();
    }

    public static IEnumerable<Node> GetDirectChildren(this Node node)
    {
        return node.GetChildren().OfType<Node>();
    }

    public static IEnumerable<Node> GetAllChildren(this Node node)
    {
        yield return node;

        foreach (var child in node.GetDirectChildren())
        {
            foreach (var result in child.GetAllChildren())
            {
                yield return result;
            }
        }
    }

    public static T FindNode<T>(this Node node, string path) where T : Node
    {
        return node.FindNode(path) as T;
    }

    public static V TryGetValue<T, V>(this Dictionary<T, V> dict, T key)
    {
        dict.TryGetValue(key, out V holder);
        return holder;
    }

    public static object TryGetValue(this Godot.Collections.Dictionary dict, object key)
    {
        dict.TryGetValue(key, out object holder);
        return holder;
    }

    public static IEnumerable<Node> GetParents(this Node node)
    {
        while (node.GetParent() != null)
        {
            node = node.GetParent();

            yield return node;
        }
    }

    public static string GetFullPath(this Node node)
    {
        if (node.GetParent() is var parent && parent != null)
        {
            return $"{parent.GetFullPath()}.{node.GetName()}";
        }
        else
        {
            return node.GetName();
        }
    }

    public static T RandomElementByWeight<T>(this IEnumerable<T> container, Func<T, float> weight)
    {
        T current = default(T);
        float cweight = 0;

        foreach (var element in container)
        {
            float tweight = weight(element);
            if (tweight / (cweight + tweight) >= Rand.Value())
            {
                current = element;
            }

            cweight += tweight;
        }

        return current;
    }
}
