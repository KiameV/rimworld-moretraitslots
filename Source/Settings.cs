using UnityEngine;
using Verse;

namespace MoreTraitSlots
{
    class RMTS : Mod
    {
        public static Settings Settings;
        public override string SettingsCategory() { return "RMTS.RMTS".Translate(); }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
        public RMTS(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }
    }

    class Settings : ModSettings
    {
        public float traitsMin = 2f;
        public float traitsMax = 3f;
        public bool smallFont = false;
        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = canvas.width;
            list.Begin(canvas);
            list.Gap();
            list.Label("RMTS.traitsMin".Translate());
            traitsMin = list.Slider(traitsMin, 0, 8.25f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + (int)traitsMin);
            Text.Font = GameFont.Small;
            list.Gap();
            list.Label("RMTS.traitsMax".Translate());
            float orig = traitsMax;
            traitsMax = list.Slider(traitsMax, 0, 8.25f);
            if (traitsMax < traitsMin)
                traitsMin = traitsMax;
            Text.Font = GameFont.Tiny;
            list.Label("          " + (int)traitsMax);
            Text.Font = GameFont.Small;
            list.Gap(48);
            list.Label("RMTS.traitsNotes".Translate());
            list.End();

            if (traitsMin > traitsMax)
                traitsMin = traitsMax;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref traitsMin, "traitsMin", 2f);
            if (traitsMax < traitsMin)
            {
                traitsMax = traitsMin;
            }
            Scribe_Values.Look(ref traitsMax, "traitsMax", 3f);
        }
    }
}
