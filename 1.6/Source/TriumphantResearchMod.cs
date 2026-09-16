using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TriumphantResearch
{
    public class TriumphantResearchMod : Mod
    {
        public static TriumphantResearchSettings settings;

        public TriumphantResearchMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<TriumphantResearchSettings>();
            new Harmony("TriumphantResearchMod").PatchAll();
        }

        public override string SettingsCategory() => Content.Name;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("TR_PauseAfterClose".Translate(), ref settings.pauseAfterClose);
            listing.Gap();
            var snoozeLabel = ResearchSnoozeTracker.IsSnoozed ? "TR_CancelSnooze_On".Translate() : "TR_CancelSnooze_Off".Translate();
            if (listing.ButtonText(snoozeLabel))
            {
                ResearchSnoozeTracker.CancelSnooze();
            }
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
