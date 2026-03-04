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
        
        AddAutoloadSingleton("ImmediateGizmos2D", "res://addons/immediate_gizmos_net/scripts/ImmediateGizmos2D.cs");
        AddAutoloadSingleton("ImmediateGizmos3D", "res://addons/immediate_gizmos_net/scripts/ImmediateGizmos3D.cs");
    }

    public override void _DisablePlugin()
    {
        RemoveAutoloadSingleton("ImmediateGizmos2D");
        RemoveAutoloadSingleton("ImmediateGizmos3D");
    }
}
