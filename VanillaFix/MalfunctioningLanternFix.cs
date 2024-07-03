using HarmonyLib;
using UnityEngine;

namespace VanillaFix;

/// <summary>
/// CREDIT TO MEGAPIGGY FOR THIS FIX
///
/// don't make conceal or focus sounds for malfunctioning dream lanterns
/// </summary>
[HarmonyPatch(typeof(DreamLanternItem))]
public static class MalfunctioningLanternFix
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(DreamLanternItem.Update))]
	private static bool Update(DreamLanternItem __instance)
	{
		return __instance.GetLanternType() == DreamLanternType.Functioning; // only allow functioning dream lanterns to update
	}
}
