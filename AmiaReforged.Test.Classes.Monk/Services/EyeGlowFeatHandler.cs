using Anvil.API;
using Anvil.API.Events;
using Anvil.Services;
using NLog;

namespace AmiaReforged.Test.Classes.Monk.Services;

[ServiceBinding(typeof(EyeGlowFeatHandler))]
public class EyeGlowFeatHandler
{
  private readonly Logger Log = LogManager.GetCurrentClassLogger();
  
  public EyeGlowFeatHandler()
  {
    // Register method to listen for the event.
    NwModule.Instance.OnUseFeat += OpenEyeGlowDialog;
    Log.Info("Monk Eye Glow Feat Handler initialized.");
  }
  
  /// <summary>
  /// Opens the dialog menu to set the eye glow color
  /// </summary>
  private static void OpenEyeGlowDialog(OnUseFeat eventData)
  {
    if (eventData.Feat.FeatType is not Feat.PerfectSelf) return; 
    if (!eventData.Creature.IsPlayerControlled(out NwPlayer? player)) return;
    
    // player.ActionStartConversation(monkEyeGlowChooser, "monk_eye_glow_chooser", true, false);
  }
}