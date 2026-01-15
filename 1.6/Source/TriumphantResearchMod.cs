using System;
using HarmonyLib;
using Verse;

namespace TriumphantResearch
{
    public class TriumphantResearchMod : Mod
    {
        public TriumphantResearchMod(ModContentPack pack) : base(pack)
        {
            new Harmony("TriumphantResearchMod").PatchAll();
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
