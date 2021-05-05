using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace MoreTraitSlots
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.rimworld.mod.moretraitslots");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Log.Message(
            //    "MoreTraitSlots Harmony Patches:" + Environment.NewLine +
            //    "  Prefix:" + Environment.NewLine +
            //    "    PawnGenerator.GenerateTraits [HarmonyPriority(Priority.VeryHigh)]"/* + Environment.NewLine +
            //    "    CharacterCardUtility.DrawCharacterCard [HarmonyPriority(Priority.VeryHigh)]"*/);
        }
    }
    
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits", null)]
    static class PawnGenerator_GenerateTraits
    {
        [HarmonyPriority(Priority.VeryHigh)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> l = new List<CodeInstruction>(instructions);
            MethodInfo RangeInclusive = AccessTools.Method(typeof(Rand), "RangeInclusive");
            MethodInfo mi = AccessTools.Method(typeof(PawnGenerator_GenerateTraits), "GetRandomTraitCount");

            for (int i = 0; i < l.Count; ++i)
            {
                if (l[i].opcode == OpCodes.Call && l[i].operand == RangeInclusive)
                {
                    l[i].operand = mi;
                    break;
                }
            }
            return l;
        }

        private static int GetRandomTraitCount(int min, int max)
        {
            return Rand.RangeInclusive((int)RMTS.Settings.traitsMin, (int)RMTS.Settings.traitsMax);
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
    
    /*[HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard", new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) })]
    static class CharacterCardUtility_DrawCharacterCard
    {
        [HarmonyPriority(Priority.VeryHigh)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo SetTextSize = AccessTools.Method(typeof(CharacterCardUtility_DrawCharacterCard), nameof(SetTextSize));
            List<CodeInstruction> l = new List<CodeInstruction>(instructions);
            for (int i = 0; i < l.Count; ++i)
            {
                if (l[i].opcode == OpCodes.Ldstr && l[i].operand.Equals("Traits"))
                {
                    for (int j = i; j >= i - 20; --j)
                    {
                        if (l[j].opcode == OpCodes.Ldc_R4)
                        {
                            float temp;
                            if (float.TryParse(l[j].operand.ToString(), out temp))
                            {
                                if (temp == 100f)
                                {
                                    // Move "Traits" up 60 pixels
                                    l[j].operand = 80f;
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = i; i >= i - 20; --j)
                    {
                        if (l[j].opcode == OpCodes.Ldc_I4_2)
                        {
                            // Make Traits font small
                            l[j].opcode = OpCodes.Ldc_I4_1;
                            break;
                        }
                    }

                    bool first0 = false,
                         first30 = false,
                         first24 = false;
                    for (; i < l.Count; ++i)
                    {
                        if (l[i].opcode == OpCodes.Ldc_R4 && l[i].operand != null)
                        {
                            float f;
                            if (float.TryParse(l[i].operand.ToString(), out f))
                            {
                                if (!first30 && f == 30f)
                                {
                                    //first30 = true;
                                    l[i].operand = 24f;
                                }
                                else if (!first24 && f == 24f)
                                {
                                    first24 = true;
                                    l[i].operand = 16f;
                                    break;
                                }
                            }
                        }
                        else if (!first0 && l[i].opcode == OpCodes.Ldc_I4_1)
                        {
                            first0 = true;
                            // Make each trait's label font tiny
                            l[i].opcode = OpCodes.Ldc_I4_0;
                        }
                    }
                    // Exit loop
                    i = int.MaxValue - 1;
                }
            }
            return l;
        }
    }*/
}