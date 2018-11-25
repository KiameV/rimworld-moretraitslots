using RimWorld;
using System;
using System.Linq;
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
        public int MAX_TRAITS = -1;
        public float traitsMin = 2f;
        public float traitsMax = 3f;
        public bool smallFont = false;
        private string minBuffer, maxBuffer;

        public Settings()
        {
            this.SetMinBuffer();
            this.SetMaxBuffer();
        }

        public void DoWindowContents(Rect canvas)
        {
            if (MAX_TRAITS == -1)
                MAX_TRAITS = DefDatabase<TraitDef>.AllDefsListForReading.Count();

            float x = canvas.xMin, y = canvas.yMin, halfWidth = canvas.width * 0.5f;
            Widgets.Label(new Rect(x, y, canvas.width, 30), "RMTS.traitsMin".Translate());
            y += 32;
            float orig = traitsMin;
            traitsMin = Widgets.HorizontalSlider(new Rect(x + 10, y, halfWidth, 32), traitsMin, 0, 8.25f);
            if (orig != traitsMin)
                this.SetMinBuffer();
            y += 32;
            string origString = minBuffer;
            minBuffer = Widgets.TextField(new Rect(x + 10, y, 50, 32), minBuffer);
            if (!minBuffer.Equals(origString))
            {
                this.ParseInput(minBuffer, traitsMin, out traitsMin);
            }

            if (traitsMin > traitsMax)
            {
                traitsMax = traitsMin;
                this.SetMaxBuffer();
            }

            y += 60;
            
            Widgets.Label(new Rect(x, y, canvas.width, 30), "RMTS.traitsMax".Translate());
            y += 32;
            orig = traitsMax;
            traitsMax = Widgets.HorizontalSlider(new Rect(x + 10, y, halfWidth, 32), traitsMax, 0, 8.25f);
            if (orig != traitsMax)
                this.SetMaxBuffer();
            y += 32;
            origString = maxBuffer;
            maxBuffer = Widgets.TextField(new Rect(x + 10, y, 50, 32), maxBuffer);
            if (!maxBuffer.Equals(origString))
            {
                this.ParseInput(maxBuffer, traitsMax, out traitsMax);
            }

            if (traitsMax < traitsMin)
            {
                traitsMin = traitsMax;
                this.SetMinBuffer();
            }

            y += 60;

            Widgets.Label(new Rect(x, y, canvas.width, 100), "RMTS.traitsNotes".Translate());

            if (this.traitsMin > MAX_TRAITS)
            {
                this.traitsMin = MAX_TRAITS;
                this.SetMinBuffer();
            }

            if (this.traitsMax > MAX_TRAITS)
            {
                this.traitsMax = MAX_TRAITS;
                this.SetMaxBuffer();
            }
        }

        private void ParseInput(string buffer, float origValue, out float newValue)
        {
            if (!float.TryParse(buffer, out newValue))
                newValue = origValue;
            if (newValue < 0)
                newValue = origValue;
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
            if (Scribe.mode != LoadSaveMode.Saving)
            {
                this.SetMinBuffer();
                this.SetMaxBuffer();
            }
        }

        private void SetMinBuffer()
        {
            this.minBuffer = ((int)this.traitsMin).ToString();
        }

        private void SetMaxBuffer()
        {
            this.maxBuffer = ((int)this.traitsMax).ToString();
        }
    }
}
