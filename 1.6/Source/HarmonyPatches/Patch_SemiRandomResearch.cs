using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

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

        public static bool Prefix()
        {
            return false;
        }
    }
}
