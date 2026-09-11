using Verse;

namespace TriumphantResearch
{
    public static class ResearchSnoozeTracker
    {
        public static int snoozeUntilTick = 0;
        public static bool IsSnoozed => Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame < snoozeUntilTick;
        public static void Snooze(int ticks)
        {
            snoozeUntilTick = Find.TickManager.TicksGame + ticks;
        }
        public static void CancelSnooze()
        {
            snoozeUntilTick = 0;
        }
        public static void ExposeData()
        {
            Scribe_Values.Look(ref snoozeUntilTick, "triumphantResearchSnoozeUntilTick", 0);
        }
    }
}
