using Godot;
using System.Collections.Generic;
using System.Linq;

static class Util
{
    public static IEnumerable<string> GetFilesFromDir(string dirname)
    {
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
        file.Open(path, (int)File.ModeFlags.Read);
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

    public static bool Collide(this CollisionShape2D lhs, CollisionShape2D rhs)
    {
        return lhs.GetShape().Collide(lhs.GlobalTransform, rhs.GetShape(), rhs.GlobalTransform);
    }

    public struct IntersectRayResult
    {
        public Vector2 position;
        public Vector2 normal;
        public Object collider;
    }

    public static IntersectRayResult IntersectRayParsed(this Physics2DDirectSpaceState space, Vector2 from, Vector2 to, Godot.Collections.Array exclude = null, int collisionLayer = int.MaxValue, bool collideWithBodies = true, bool collideWithAreas = false)
    {
        Godot.Collections.Dictionary dict = space.IntersectRay(from, to, exclude, collisionLayer, collideWithBodies, collideWithAreas);

        IntersectRayResult result = new IntersectRayResult();
        result.position = (dict.TryGetValue("position") as Vector2?).GetValueOrDefault();
        result.normal = (dict.TryGetValue("normal") as Vector2?).GetValueOrDefault();
        result.collider = dict.TryGetValue("collider") as Godot.Object;
        return result;
    }

    public static void CharacterMovement(this KinematicBody2D body, Comp.KinematicMover mover, float delta)
    {
        bool intendedGrounded = false;

        if (mover.air_time == 0 && mover.linear_vel.y >= 0)
        {
            const float upwardsShift = 1;
            const float stickiness = 5;

            // Shift up a little
            var upresult = body.MoveAndCollide(new Vector2(0, -upwardsShift));
            float upshiftUsed = upresult != null ? -upresult.GetTravel().y : upwardsShift;

            // Move in our intended direction
            var sideresult = body.MoveAndCollide(mover.linear_vel * delta);
            if (sideresult != null)
            {
                mover.linear_vel.x = sideresult.GetTravel().x / delta;
            }

            // Move back down the appropriate amount, plus stickiness
            var downresult = body.MoveAndCollide(new Vector2(0, upshiftUsed + stickiness));
        
            intendedGrounded = downresult != null;
        }
        else
        {
            // Move in our intended direction
            var sideresult = body.MoveAndCollide(mover.linear_vel * delta);
            if (sideresult != null)
            {
                if (sideresult.Normal.y < 0)
                {
                    // If we transition from flying to grounded, just set the flag
                    intendedGrounded = true;

                    // TODO: Move along ground a little bit as well?
                }
                else
                {
                    // Otherwise, we hit something in midair, adjust our X speed appropriately
                    mover.linear_vel.x = sideresult.GetTravel().x / delta;
                }
            }
        }

        if (intendedGrounded)
        {
            mover.air_time = 0;
            mover.linear_vel.y = 0;
        }
        else
        {
            mover.air_time += delta;
        }
        mover.on_floor = intendedGrounded;
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

    // passed into Ghi so it's used on Ghi.Entity.ToString() automatically
    public static string EntityToString(Ghi.Entity entity)
    {
        if (entity.HasComponent<Comp.ActorDef>())
        {
            var actorDef = entity.ComponentRO<Comp.ActorDef>();
            return $"[{actorDef.def}:{actorDef.id}]";
        }
        else
        {
            return "[unknown entity]";
        }
    }
}

public static class Find
{
    public static Ghi.Entity Player
    {
        get
        {
            return Ghi.Environment.List.Where(c => c.HasComponent<Comp.Player>()).FirstOrDefault();
        }
    }

    public static float Delta
    {
        get
        {
            return Ghi.Environment.SingletonRO<Comp.Global>().delta;
        }
    }

    public static Camera2D Camera;
    public static Control UI;
}
