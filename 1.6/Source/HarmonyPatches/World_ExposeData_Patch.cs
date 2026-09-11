using HarmonyLib;
using RimWorld.Planet;

namespace TriumphantResearch
{
    [HarmonyPatch(typeof(World), nameof(World.ExposeData))]
    public static class World_ExposeData_Patch
    {
        public static void Postfix()
        {
            ResearchSnoozeTracker.ExposeData();
        }
    }
}
