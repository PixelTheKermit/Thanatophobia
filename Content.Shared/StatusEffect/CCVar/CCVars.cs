using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.StatusEffect
{
    public sealed class StatusEffectsCCVars : CVars
    {
        /// <summary>
        /// Update interval of status effects, in seconds
        /// </summary>
        /// <returns></returns>
        public static readonly CVarDef<float> StatusEffectUpdateInterval =
            CVarDef.Create<float>("status_effect.update_interval", 1f, CVar.SERVER);
    }
}
