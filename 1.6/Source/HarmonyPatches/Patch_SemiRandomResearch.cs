using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace TriumphantResearch
{
    [HarmonyPatch]
    public static class Patch_CM_Semi_Random_Research
    {
        public static bool Prepare()
        {
            return ModsConfig.IsActive("arodoid.semirandomprogression");
        }

        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("CM_Semi_Random_Research.Dialog_ResearchComplete_Patches");
            if (type is null) return null;

            var method = AccessTools.Method(type, "FinishProject_Prefix");
            return method;
        }

        public static bool Prefix(ResearchProjectDef proj, ref bool doCompletionDialog, Pawn researcher, ref bool doCompletionLetter)
        {
            if (Current.ProgramState != ProgramState.Playing || Find.GameInitData != null) return true;
            if (ResearchSnoozeTracker.IsSnoozed) return true;
            if (proj.UnlockedDefs != null && proj.UnlockedDefs.Count > 0)
            {
                doCompletionDialog = false;
                doCompletionLetter = false;
                Find.WindowStack.Add(new Window_ResearchComplete(proj));
                return false;
            }
            else
            {
                DefsOf.TR_ResearchComplete.PlayOneShotOnCamera();
                return true;
            }
        }

        public static void OpenTab()
        {
            var semiRandomResearchButton = DefDatabase<MainButtonDef>.GetNamed ("CM_Semi_Random_Research_MainButton_Next_Research", false);
            if (semiRandomResearchButton != null && Find.MainTabsRoot != null)
            {
                MainTabsRoot_SetCurrentTab_Patch.allow = true;
                Find.MainTabsRoot.SetCurrentTab(semiRandomResearchButton, true);
                MainTabsRoot_SetCurrentTab_Patch.allow = false;
            }
        }

    }
}
