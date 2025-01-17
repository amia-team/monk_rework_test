// Called from the body technique handler when the technique is cast
using AmiaReforged.Test.Classes.Monk.Types;
using Anvil.API;
using Anvil.API.Events;


namespace AmiaReforged.Test.Classes.Monk.Techniques.Body;

public static class WholenessOfBody
{   
    public static void CastWholenessOfBody(OnSpellCast castData)
    {
        NwCreature monk = (NwCreature)castData.Caster;
        PathType? path = MonkUtilFunctions.GetMonkPath(monk);
        const TechniqueType technique = TechniqueType.Wholeness;

        if (path != null)
        {
            PathEffectApplier.ApplyPathEffects(path, technique, castData);
            return;
        }

        int monkLevel = monk.GetClassInfo(ClassType.Monk)!.Level;
        int healAmount = monkLevel*2;
        Effect wholenessEffect = Effect.Heal(healAmount);
        Effect wholenessVfx = Effect.VisualEffect(VfxType.ImpHealingL, false, 0.7f);

        monk.ApplyEffect(EffectDuration.Instant, wholenessEffect);
        monk.ApplyEffect(EffectDuration.Instant, wholenessVfx);
    }
}