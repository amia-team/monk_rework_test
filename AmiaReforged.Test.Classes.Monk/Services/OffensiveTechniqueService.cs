// A service for offensive monk techniques
using static AmiaReforged.Test.Classes.Monk.Constants.MonkTechnique;
using AmiaReforged.Test.Classes.Monk.Constants;
using AmiaReforged.Test.Classes.Monk.Techniques.Offensive;
using Anvil.API;
using Anvil.API.Events;
using Anvil.Services;
using NLog;

namespace AmiaReforged.Test.Classes.Monk.Services;

[ServiceBinding(typeof(OffensiveTechniqueService))]
public class OffensiveTechniqueService
{
  private readonly Effect _offensiveEffect = Effect.VisualEffect(VfxType.None);

  private static readonly Logger Log = LogManager.GetCurrentClassLogger();
  
  public OffensiveTechniqueService()
  {
    // Register methods to listen for the events.
    NwModule.Instance.OnUseFeat += OffensiveTechniqueUseFeat;
    NwModule.Instance.OnCombatRoundStart += EnterOffensiveTechnique;
    NwModule.Instance.OnEffectApply += ActivateOffensiveTechniqueIcon;
    NwModule.Instance.OnEffectRemove += DeactivateOffensiveTechniqueIcon;
    NwModule.Instance.OnCreatureAttack += OnHitApplyTechnique;
    Log.Info("Monk Offensive Technique Service initialized.");
  }

  /// <summary>
  /// Sets an offensive technique for activation on next combat round, or instantly if out of combat
  /// </summary>
  private void OffensiveTechniqueUseFeat(OnUseFeat eventData)
  {
    bool isTechnique = eventData.Feat.Id is MonkFeat.StunningStrike or MonkFeat.EagleStrike or MonkFeat.AxiomaticStrike;
    if (!isTechnique) return;

    NwFeat technique = eventData.Feat;
    NwCreature monk = eventData.Creature;
    
    // If monk is in combat, queue offensive technique change for next combat round
    if (monk.IsInCombat)
    {
      LocalVariableInt queuedTechnique = monk.GetObjectVariable<LocalVariableInt>(OffensiveTechnique);

      queuedTechnique.Value = technique.Id switch
      {
          MonkFeat.StunningStrike => StunningValue,
          MonkFeat.EagleStrike => EagleValue,
          MonkFeat.AxiomaticStrike => AxiomaticValue,
          _ => throw new NotImplementedException()
      };
    }
    // Else activate offensive technique straight away
    else
    {
      // Deactivate previous offensive technique
      foreach (Effect effect in monk.ActiveEffects)
        if (effect.Tag is not null && effect.Tag.Contains(OffensiveTechnique)) monk.RemoveEffect(effect);
      
      _offensiveEffect.Tag = technique.Id switch
      {
        MonkFeat.StunningStrike => StunningTag,
        MonkFeat.EagleStrike => EagleTag,
        MonkFeat.AxiomaticStrike => EagleTag,
        _ => throw new NotImplementedException()
      };
      _offensiveEffect.SubType = EffectSubType.Unyielding;
      monk.ApplyEffect(EffectDuration.Permanent, _offensiveEffect);
    }     
  }

  /// <summary>
  /// On combat round start switches into the active offensive technique
  /// </summary>
  private void EnterOffensiveTechnique(OnCombatRoundStart eventData)
  {
    // Creature must be monk
    if (eventData.Creature.GetClassInfo(NwClass.FromClassType(ClassType.Monk)) is null) return;
    
    NwCreature monk = eventData.Creature;
    LocalVariableInt queuedTechnique = monk.GetObjectVariable<LocalVariableInt>(OffensiveTechnique);
    Effect? technique;

    //  If technique is queued up, activate it
    if (queuedTechnique.HasValue)
    {
      _offensiveEffect.Tag = queuedTechnique.Value switch
      {
        StunningValue => StunningTag,
        EagleValue => EagleTag,
        AxiomaticValue => AxiomaticTag,
        _ => ""
      };
      queuedTechnique.Delete();
      _offensiveEffect.SubType = EffectSubType.Unyielding;
      monk.ApplyEffect(EffectDuration.Permanent, _offensiveEffect);
      technique = _offensiveEffect;
    }
    else
    {
      technique = monk.ActiveEffects.FirstOrDefault(effect => effect.Tag is not null && effect.Tag.Contains(OffensiveTechnique));
    }
      
    if (technique is null) return;

    string techniqueName = technique.Tag switch
    {
      StunningTag => "StunningValue Strike",
      EagleTag => "Eagle Strike",
      AxiomaticTag  => "AxiomaticValue Strike",
      _ => ""
    };

    // Technique can only be used unarmored
    bool isArmored = monk.GetItemInSlot(InventorySlot.Chest)?.BaseACValue > 0;
    bool isWieldingShield = monk.GetItemInSlot(InventorySlot.LeftHand)?.BaseItem.Category is BaseItemCategory.Shield;
    if (isArmored || isWieldingShield)
    {
      if (monk.IsPlayerControlled(out NwPlayer? player)) 
          player.SendServerMessage($"{techniqueName} can only be used while unarmored.", MonkColors.MonkColorScheme);

      return;
    }

    // Remove offensive technique from cooldown to allow hits to proc again
    foreach (Effect effect in monk.ActiveEffects)
      if (effect.Tag is OffensiveCooldownTag) monk.RemoveEffect(effect);

    // Remove eagle strike counter
    if (monk.GetObjectVariable<LocalVariableInt>(EagleStrikesCounter).HasValue) 
      monk.GetObjectVariable<LocalVariableInt>(EagleStrikesCounter).Delete();
  }

  /// <summary>
  /// Activates the feat icon when the technique is activated
  /// </summary>
  private static void ActivateOffensiveTechniqueIcon(OnEffectApply eventData)
  {
    if (!eventData.Object.IsPlayerControlled(out NwPlayer? player)) return;
    if (eventData.Effect.Tag is null) return;
    if (!eventData.Effect.Tag.Contains(OffensiveTechnique)) return;

    string technique = eventData.Effect.Tag;
    
    switch (technique)
    {
      case StunningTag :
        player.SetTextureOverride(StunningIconInactive, StunningIconActive);
        break;
      case EagleTag :
        player.SetTextureOverride(EagleIconInactive, EagleIconActive);
        break;
      case AxiomaticTag :
        player.SetTextureOverride(AxiomaticIconInactive, AxiomaticIconActive);
        break;
    }
  }

  /// <summary>
  /// Deactivates the feat icon when the technique is deactivated
  /// </summary>
  private static void DeactivateOffensiveTechniqueIcon(OnEffectRemove eventData)
  {
    if (!eventData.Object.IsPlayerControlled(out NwPlayer? player)) return;
    if (eventData.Effect.Tag is null) return;
    if (!eventData.Effect.Tag.Contains(OffensiveTechnique)) return;

    string technique = eventData.Effect.Tag;
    
    switch (technique)
    {
      case StunningTag :
        player.SetTextureOverride(StunningIconActive, StunningIconInactive);
        break;
      case EagleTag :
        player.SetTextureOverride(EagleIconActive, EagleIconInactive);
        break;
      case AxiomaticTag :
        player.SetTextureOverride(AxiomaticIconActive, AxiomaticIconInactive);
        break;
    }
  }

  /// <summary>
  /// Applies the offensive technique effects and cooldown on hit
  /// </summary>
  private static void OnHitApplyTechnique(OnCreatureAttack attackData)
  {
    // Creature must be monk
    if (attackData.Attacker.GetClassInfo(NwClass.FromClassType(ClassType.Monk)) is null) return;
    NwCreature monk = attackData.Attacker;

    // Can't have the cooldown active for offensive technique procs
    if (monk.ActiveEffects.Any(effect => effect.Tag is OffensiveCooldownTag)) return;
  
    Effect? technique = monk.ActiveEffects.FirstOrDefault(effect => effect.Tag is not null && effect.Tag.Contains(OffensiveTechnique));
    if (technique is null) return;
    
    // Apply technique effects
    switch (technique.Tag)
    {
      case StunningTag : 
        StunningStrike.ApplyStunningStrike(attackData);
        break;
      case EagleTag :
        EagleStrike.ApplyEagleStrike(attackData);
        break; 
      case AxiomaticTag :
        AxiomaticStrike.ApplyAxiomaticStrike(attackData);
        break;
    }

    bool isHit = attackData.AttackResult is AttackResult.Hit or AttackResult.AutomaticHit or AttackResult.CriticalHit;
    int eagleCounter = monk.GetObjectVariable<LocalVariableInt>(EagleStrikesCounter).Value;

    // On hit, apply cooldown  for stunning and eagle
    if (!isHit) return;

    Effect offensiveCooldownEffect = Effect.VisualEffect(VfxType.None);
    offensiveCooldownEffect.SubType = EffectSubType.Unyielding;
    offensiveCooldownEffect.Tag = OffensiveCooldownTag;
    
    if (technique.Tag is StunningTag)
      monk.ApplyEffect(EffectDuration.Permanent, offensiveCooldownEffect);

    if (technique.Tag is not EagleTag) return;
    eagleCounter++;
    if (eagleCounter >= 2) 
      monk.ApplyEffect(EffectDuration.Permanent, offensiveCooldownEffect);
  }
}
