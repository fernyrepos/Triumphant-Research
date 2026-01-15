using HarmonyLib;
using RimWorld;
using Verse;

namespace TriumphantResearch
{
    [HarmonyPatch(typeof(MainTabsRoot), nameof(MainTabsRoot.SetCurrentTab))]
    public static class MainTabsRoot_SetCurrentTab_Patch
    {
        public static bool allow;
        public static bool Prefix(MainButtonDef tab, bool playSound)
        {
            if (allow) return true;
            string tabName = tab?.defName ?? "null";
            if (tabName == "CM_Semi_Random_Research_MainButton_Next_Research" && Find.WindowStack.IsOpen<Window_ResearchComplete>())
            {
                return false;
            }
            return true;
        }
    }
}
