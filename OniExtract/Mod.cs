using HarmonyLib;
using PeterHan.PLib.Options;

namespace OniExtract {
  public class Mod : KMod.UserMod2{
    public override void OnLoad(Harmony harmony) {
      base.OnLoad(harmony);
      new POptions().RegisterOptions(this, typeof(ModOptions));
    }
  }
}
