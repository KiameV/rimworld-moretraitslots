using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RMTS_Code
{
    public class RMTS : Mod
    {
        public static Settings Settings;
        public override string SettingsCategory() { return "RMTS.RMTS".Translate(); }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
        public RMTS(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            var harmony = HarmonyInstance.Create("rainbeau.rmts");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    public class Settings : ModSettings
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
            traitsMax = list.Slider(traitsMax, 0, 8.25f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + (int)traitsMax);
            Text.Font = GameFont.Small;
            list.Gap(48);
            list.Label("RMTS.traitsNotes".Translate());
            list.End();

            if (traitsMin > traitsMax)
                traitsMax = traitsMin;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref traitsMin, "traitsMin", 2f);
            if (traitsMax < traitsMin) { traitsMax = traitsMin; }
            Scribe_Values.Look(ref traitsMax, "traitsMax", 3f);
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits", null)]
    public static class PawnGenerator_GenerateTraits
    {
        [HarmonyPriority(Priority.VeryHigh)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> l = new List<CodeInstruction>(instructions);
            MethodInfo RangeInclusive = AccessTools.Method(typeof(Rand), "RangeInclusive");
            
            for (int i = 0; i < l.Count; ++i)
            {
                if (i + 2 < l.Count)
                {
                    if (l[i + 2].opcode == OpCodes.Call && l[i + 2].operand == RangeInclusive)
                    {
                        l[i].opcode = GetOp((int)RMTS.Settings.traitsMin);
                        l[i + 1].opcode = GetOp((int)RMTS.Settings.traitsMax);
                        break;
                    }
                }
            }
            return l;
        }

        private static OpCode GetOp(int v)
        {
            switch (v)
            {
                case 0:
                    return OpCodes.Ldc_I4_0;
                case 1:
                    return OpCodes.Ldc_I4_1;
                case 2:
                    return OpCodes.Ldc_I4_2;
                case 3:
                    return OpCodes.Ldc_I4_3;
                case 4:
                    return OpCodes.Ldc_I4_4;
                case 5:
                    return OpCodes.Ldc_I4_5;
                case 6:
                    return OpCodes.Ldc_I4_6;
                case 7:
                    return OpCodes.Ldc_I4_7;
                default:
                    return OpCodes.Ldc_I4_8;
            }
        }
    }
    
    [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard", new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) })]
    public static class CharacterCardUtility_DrawCharacterCard
    {
        [HarmonyPriority(Priority.VeryHigh)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo HighlightOpportunity = AccessTools.Method(typeof(UIHighlighter), "HighlightOpportunity");
            MethodInfo SetTextSize = AccessTools.Method(typeof(CharacterCardUtility_DrawCharacterCard), nameof(SetTextSize));
            MethodInfo SetRectSize = AccessTools.Method(typeof(CharacterCardUtility_DrawCharacterCard), nameof(SetRectSize));
            bool traits = false;
            foreach (CodeInstruction i in instructions)
            {
                if (i.opcode == OpCodes.Ldstr && i.operand.Equals("Traits"))
                {
                    traits = true;
                }
                if (traits && i.opcode == OpCodes.Ldc_I4_1)
                {
                    yield return new CodeInstruction(OpCodes.Call, SetTextSize);//replaces instruction, gets 0 or 1 returned from the method, depending on setting
                    continue;
                }
                if (traits && i.opcode == OpCodes.Ldc_R4 && i.operand.Equals(24f))//replaces rect height
                {
                    yield return new CodeInstruction(OpCodes.Call, SetRectSize);
                    continue;
                }
                if (traits && i.opcode == OpCodes.Ldc_R4 && i.operand.Equals(2f))//replaces rect y calculation
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    traits = false;
                    continue;
                }
                yield return i;
            }
        }

        public static GameFont SetTextSize()
        {
            return (RMTS.Settings.traitsMax < 6) ? GameFont.Small : GameFont.Tiny;
        }

        public static float SetRectSize()
        {
            return 24f;
        }
    }
}