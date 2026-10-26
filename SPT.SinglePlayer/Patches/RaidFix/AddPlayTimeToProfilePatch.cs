using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

internal class AddPlayTimeToProfilePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseLocalGame<EftGamePlayerOwner>), nameof(BaseLocalGame<>.GameEnd));
    }

    [PatchPrefix]
    protected static void PatchPrefix(BaseLocalGame<EftGamePlayerOwner> __instance)
    {
        FieldInfo sessionStartTimeFieldInfo = AccessTools.Field(typeof(BaseLocalGame<EftGamePlayerOwner>), "SessionStartTime");
        DateTime sessionStartTime = (DateTime)sessionStartTimeFieldInfo.GetValue(__instance);

        // Same calculation steps used in EFT method
        TimeSpan timeInRaid = DateTimeExtensions.Now - sessionStartTime;
        double playTime = timeInRaid.TotalSeconds;

        Logger.LogInfo("Starting TotalInGameTime: " + __instance.Profile.Stats.Eft.TotalInGameTime);
        Logger.LogInfo("PlayTime: " + playTime);

        __instance.Profile.Stats.Eft.TotalInGameTime += Convert.ToInt64(playTime);

        Logger.LogInfo("Ending TotalInGameTime: " + __instance.Profile.Stats.Eft.TotalInGameTime);
    }
}
