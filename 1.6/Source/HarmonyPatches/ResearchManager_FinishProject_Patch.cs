using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace TriumphantResearch
{
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
    public static class ResearchManager_FinishProject_Patch
    {
        public static bool adding;
        public static void Prefix(ResearchProjectDef proj, ref bool doCompletionDialog, ref bool doCompletionLetter)
        {
            if (Current.ProgramState != ProgramState.Playing || Find.GameInitData != null) return;
            if (ResearchSnoozeTracker.IsSnoozed)
            {
                doCompletionDialog = false;
                return;
            }
            if (proj.UnlockedDefs != null && proj.UnlockedDefs.Count > 0)
            {
                doCompletionDialog = false;
                doCompletionLetter = false;
                adding = true;
                Find.WindowStack.Add(new Window_ResearchComplete(proj));
                adding = false;
            }
            else
            {
                DefsOf.TR_ResearchComplete.PlayOneShotOnCamera();
            }
        }
    }
}
