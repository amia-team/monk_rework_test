// Utility/helper functions for monk stuff
using Anvil.API;
using AmiaReforged.Classes.Monk.Constants;
using AmiaReforged.Classes.Monk.Types;

namespace AmiaReforged.Classes.Monk;

public static class MonkUtilFunctions
{
    /// <summary>
    /// Returns the monk's path type
    /// </summary>
    public static PathType? GetMonkPath(NwCreature monk)
    {
        NwFeat? pathFeat = monk.Feats.FirstOrDefault(feat => feat.Id is MonkFeat.PathOfTheElements 
            or MonkFeat.PathOfTheHymn or  MonkFeat.PathOfClarity or MonkFeat.PathOfTheMantle 
            or MonkFeat.PathOfTheGolem or MonkFeat.PathOfTorment or MonkFeat.PathOfMists);
        
        return pathFeat?.Id switch
        {
            MonkFeat.PathOfTheElements => PathType.Elements,
            MonkFeat.PathOfTheHymn => PathType.Hymn,
            MonkFeat.PathOfClarity => PathType.Clarity,
            MonkFeat.PathOfTheMantle => PathType.Mantle,
            MonkFeat.PathOfTheGolem => PathType.Golem,
            MonkFeat.PathOfTorment => PathType.Torment,
            MonkFeat.PathOfMists => PathType.Mists,
            _ => null
        };
    }

    /// <summary>
    /// DC 10 + half the monk's character level + the monk's wisdom modifier
    /// </summary>
    /// <returns>The monk ability DC</returns>
    public static int CalculateMonkDc(NwCreature monk)
    {
        return 10 + monk.GetClassInfo(ClassType.Monk)!.Level / 2 + monk.GetAbilityModifier(Ability.Wisdom);
    }

    /// <summary>
    /// Returns a vfx effect resized to your desired size
    /// </summary>
    /// <param name="visualEffect"></param> The visual effect you want to resize
    /// <param name="desiredSize"></param> The size you desire in meters (small 1.67, medium 3.33, large 5, huge 6.67, gargantuan 8.33, colossal 10)
    /// <returns></returns>
    public static Effect ResizedVfx(VfxType visualEffect, float desiredSize)
    {
        float vfxDefaultSize = visualEffect switch
        {
            VfxType.FnfLosEvil10 or (VfxType)1046 => RadiusSize.Medium,
            VfxType.FnfHowlOdd or VfxType.FnfHowlMind => RadiusSize.Colossal,
            _ => RadiusSize.Large
        };

        float vfxScale = desiredSize / vfxDefaultSize;
        return Effect.VisualEffect(visualEffect, false, vfxScale);
    }
    /// <summary>
    /// A simpler version of NWN's spellsIsTarget() adjusted to Amia's difficulty setting. Don't use if it doesn't simplify AoE spell targeting.
    /// </summary>
    /// <param name="creaturesOnly"></param> true if you want to only affect creatures
    /// <param name="affectsSelf"></param> true if you want to affect yourself
    /// <param name="alliesOnly"></param> true if you to affect only allies
    /// <returns>Valid target for spell effect</returns>
    public static bool IsValidTarget(NwObject targetObject, NwCreature caster, bool creaturesOnly, bool affectsSelf, bool alliesOnly)
    {
        if (targetObject == caster)
        {
            return affectsSelf;
        }   
        if (creaturesOnly)
        {
            if (targetObject is not NwCreature targetCreature) return false;
            if (alliesOnly)
            {
                if (caster.IsReactionTypeFriendly(targetCreature)) 
                    return true;
            }
            else if (caster.IsReactionTypeHostile(targetCreature))
                return true;
        }
        else if (targetObject is NwCreature targetCreature && !caster.IsReactionTypeFriendly(targetCreature) 
            || targetObject is NwPlaceable || targetObject is NwDoor) 
            return true;

        return false;
        }

    /// <summary>
    /// Sends debug message to player as "DEBUG: {debugString1} {debugString2}", debugString2 is colored
    /// </summary>
    public static void MonkDebug(NwPlayer player, string debugString1, string debugString2)
    {
        if (!player.IsValid) return;
        if (player.ControlledCreature.GetObjectVariable<LocalVariableInt>("monk_debug").Value != 1) return;

        debugString2.ColorString(MonkColors.MonkColorScheme);
        player.SendServerMessage($"DEBUG: {debugString1} {debugString2}");
    }

    /// <summary>
    /// A helper function for elements monk, gets the damage type based on the chosen element.
    /// </summary>
    public static DamageType GetElementalType(NwCreature monk)
    {
        DamageType elementalType = monk.GetObjectVariable<LocalVariableInt>(MonkElemental.VarName).Value switch
        {
            MonkElemental.Fire => DamageType.Fire,
            MonkElemental.Water => DamageType.Cold,
            MonkElemental.Air => DamageType.Electrical,
            MonkElemental.Earth => DamageType.Acid,
            _ => DamageType.Fire
        };
        return elementalType;
    }
}
