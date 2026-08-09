using Godot;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnablePlugin()
    {
        if (ProjectSettings.GetSetting("application/run/main_loop_type").AsString() != "SceneTree")
        {
            GD.PushError("To use ImmediateGizmos, the project main loop must be of type 'SceneTree'");
            return;
        }
    }

    public override void _DisablePlugin()
    {
        // ponytail: classes are fully static, no autoload singletons needed (mirrors WithoutSingletons in original)
    }
}
