using Autodesk.Fbx;
using HarmonyLib;
using ShapeCollision;
using System.Drawing;
using UnityEngine;

namespace VanillaFix;

/// <summary>
/// Adds box colliders to OWTriggerVolume.GetPenetrationDistance
/// </summary>
[HarmonyPatch(typeof(OWTriggerVolume))]
public static class TriggerVolumePenetrationFix
{
	public static float GetDistToSurface(BoxCollider boxCollider, Vector3 worldPosition)
	{
		Vector3 boxCenter = boxCollider.transform.TransformPoint(boxCollider.center);
		Vector3 boxSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);
		Quaternion rotation = boxCollider.transform.rotation;
		Vector3[] axes = new Vector3[3] { rotation * Vector3.right, rotation * Vector3.up, rotation * Vector3.forward };
		Vector3 vector = Penetration.Box(worldPosition, boxCenter, boxSize, axes);
		int index = 0;
		float num = float.PositiveInfinity;
		for (int i = 0; i < 3; i++)
		{
			float num2 = vector[i];
			if (num2 < num)
			{
				index = i;
				num = num2;
			}
		}
		return vector[index];
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(OWTriggerVolume.GetPenetrationDistance))]
	private static bool GetPenetrationDistance(OWTriggerVolume __instance, ref float __result, Vector3 worldPos)
	{
		__result = 0f;
		if (__instance._childEntryways.Count > 0 || __instance._sharedEntryways.Length != 0)
		{
			Debug.LogError("Cannot get penetration distance into OWTriggerVolumes using EntrywayTriggers!");
		}
		else if (__instance._owCollider != null)
		{
			Collider collider = __instance._owCollider.GetCollider();
			if (collider is SphereCollider sphere)
			{
				__result = OWPhysics.GetDistToSurface(sphere, worldPos);
			}
			else if (collider is CapsuleCollider capsule)
			{
				__result = OWPhysics.GetDistToSurface(capsule, worldPos);
			}
			else if (collider is BoxCollider box)
			{
				__result = GetDistToSurface(box, worldPos); // Added box collider
			}
			else
			{
				Debug.LogError("Cannot get penetration distance into an OWCollider that isn't a SphereCollider, CapsuleCollider, or BoxCollider! Collider type is " + collider.GetType().Name);
			}
		}
		else if (__instance._compoundTrigger != null)
		{
			Debug.LogError("Cannot get penetration distance into CompoundTriggers!");
		}
		else if (__instance._shape != null)
		{
			__result = __instance._shape.PenetrationDistance(worldPos);
		}
		return false;
	}
}
