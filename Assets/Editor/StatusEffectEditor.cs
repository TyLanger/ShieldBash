using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Ability), true), CanEditMultipleObjects]
public class StatusEffectEditor : Editor {


	public SerializedProperty
	additionalEffectProp,
	pushDistanceProp,
	pushScaleProp,
	pullDistanceProp,
	pullScaleProp,
	stunDurationProp;

	void OnEnable()
	{
		additionalEffectProp = serializedObject.FindProperty ("additionalEffect");
		pushDistanceProp = serializedObject.FindProperty ("pushDistance");
		pushScaleProp = serializedObject.FindProperty ("pushScale");
		pullDistanceProp = serializedObject.FindProperty ("pullDistance");
		pullScaleProp = serializedObject.FindProperty ("pullScale");
		stunDurationProp = serializedObject.FindProperty ("stunDuration");


	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update ();

		var ability = target as Ability;
		ability.damage = EditorGUILayout.IntField("Damage", ability.damage);
		ability.cooldown = EditorGUILayout.FloatField ("Cooldown", ability.cooldown);

		EditorGUILayout.PropertyField (additionalEffectProp);
		Ability.StatusEffect se = (Ability.StatusEffect) additionalEffectProp.intValue;

		switch (se) {
		case Ability.StatusEffect.Pull:
			EditorGUILayout.DelayedFloatField (pullDistanceProp, new GUIContent ("pullDistance"));
			EditorGUILayout.DelayedFloatField (pullScaleProp, new GUIContent ("pullScale"));
			break;

		case Ability.StatusEffect.Push:
			EditorGUILayout.DelayedFloatField (pushDistanceProp, new GUIContent ("pushDistance"));
			EditorGUILayout.DelayedFloatField (pushScaleProp, new GUIContent ("pushScale"));
			break;

		case Ability.StatusEffect.Stun:
			EditorGUILayout.DelayedFloatField (stunDurationProp, new GUIContent ("stunDuration"));
			break;
		}



		serializedObject.ApplyModifiedProperties ();
	}
}
