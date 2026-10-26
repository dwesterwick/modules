using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

internal class KeepServerTotalInGameTimesPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EftClientBackendSession.CG_GetProfiles), "MoveNext");
    }

    [PatchTranspiler]
    protected static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
    {
        List<CodeInstruction> modifiedInstructions = originalInstructions.ToList();

        FieldInfo totalInGameTimeFieldInfo = AccessTools.Field(typeof(ProfileStats), nameof(ProfileStats.TotalInGameTime));

        for (int i = 0; i < modifiedInstructions.Count; i++)
        {
            // Find `profile.EftStats.TotalInGameTime = eftClientBackendSession.TotalInGameTime;` and remove all related instructions
            if (modifiedInstructions[i].opcode == OpCodes.Stfld && modifiedInstructions[i].operand.Equals(totalInGameTimeFieldInfo))
            {
                // Instructions to remove:
                //      ldloc.s EFT.Profile (8)
                //      callvirt ProfileStats get_EftStats()
                //      ldloc.1
                //      call Int64 get_TotalInGameTime()
                //      stfld System.Int64 TotalInGameTime
                modifiedInstructions.RemoveRange(i - 4, 5);

                break;
            }
        }

        return modifiedInstructions;
    }
}
