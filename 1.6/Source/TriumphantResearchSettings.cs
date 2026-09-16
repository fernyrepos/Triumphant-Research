using Verse;

namespace TriumphantResearch
{
    public class TriumphantResearchSettings : ModSettings
    {
        public bool pauseAfterClose = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref pauseAfterClose, "pauseAfterClose", false);
        }
    }
}
