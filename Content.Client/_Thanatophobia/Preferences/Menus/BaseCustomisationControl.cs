using Robust.Client.UserInterface;

namespace Content.Client.Thanatophobia.Preferences.UI.CustomControls;

public abstract partial class TPBaseCustomisationControl : Control
{
    /// <summary>
    /// Used to access data.
    /// </summary>
    public TPHumanoidProfileEditor ProfileEditor;
    public string TabName = "please, give me a (perferably localised) name.";

    public TPBaseCustomisationControl(TPHumanoidProfileEditor profileEditor)
    {
        ProfileEditor = profileEditor;
    }

    public virtual void RefreshControls()
    {

    }
}
