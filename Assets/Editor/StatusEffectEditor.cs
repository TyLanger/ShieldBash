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
	stunDurationProp,
	dotDamageTickProp,
	dotTimeIntervalProp,
	dotTotalTimeProp,
	slowPercentProp,
	slowDurationProp,
	decayingSlowProp;

	void OnEnable()
	{
		additionalEffectProp = serializedObject.FindProperty ("additionalEffect");
		//push
		pushDistanceProp = serializedObject.FindProperty ("pushDistance");
		//pull
		pullDistanceProp = serializedObject.FindProperty ("pullDistance");
		//stun
		stunDurationProp = serializedObject.FindProperty ("stunDuration");
		//dot
		dotDamageTickProp = serializedObject.FindProperty ("dotDamageTick");
		dotTimeIntervalProp = serializedObject.FindProperty ("dotTimeInterval");
		dotTotalTimeProp = serializedObject.FindProperty ("dotTotalTime");
		//slow
		slowPercentProp = serializedObject.FindProperty ("slowPercent");
		slowDurationProp = serializedObject.FindProperty ("slowDuration");
		decayingSlowProp = serializedObject.FindProperty ("decayingSlow");

	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update ();
		DrawDefaultInspector ();


		var ability = target as Ability;
		/*
		ability.damage = EditorGUILayout.IntField("Damage", ability.damage);
		ability.cooldown = EditorGUILayout.FloatField ("Cooldown", ability.cooldown);
		*/


		EditorGUILayout.PropertyField (additionalEffectProp);
		Ability.StatusEffect se = (Ability.StatusEffect) additionalEffectProp.intValue;

		switch (se) {
		case Ability.StatusEffect.Pull:
			EditorGUILayout.DelayedFloatField (pullDistanceProp, new GUIContent ("pullDistance"));
			break;

		case Ability.StatusEffect.Push:
			EditorGUILayout.DelayedFloatField (pushDistanceProp, new GUIContent ("pushDistance"));
			break;

		case Ability.StatusEffect.Stun:
			// with this style, when the value field is changed, it stays
			EditorGUILayout.DelayedFloatField (stunDurationProp, new GUIContent ("stunDuration"));
			break;

		case Ability.StatusEffect.DamageOverTime:
			// with this style, when the value field is changed it does NOT stay.
			// You have to click apply for it to save when you run
			//ability.dotDamageTick = EditorGUILayout.IntField ("Damage per tick", ability.dotDamageTick);
			EditorGUILayout.DelayedIntField (dotDamageTickProp, new GUIContent ("Damage per tick"));
			//ability.dotTimeInterval = EditorGUILayout.FloatField ("Time between ticks", ability.dotTimeInterval);
			EditorGUILayout.DelayedFloatField (dotTimeIntervalProp, new GUIContent ("Time between ticks"));
			//ability.dotTotalTime = EditorGUILayout.FloatField ("Total dot time", ability.dotTotalTime);
			EditorGUILayout.DelayedFloatField (dotTotalTimeProp, new GUIContent ("Total dot time"));
			break;

		case Ability.StatusEffect.Slow:
			//ability.slowPercent = EditorGUILayout.Slider ("Slow percent", ability.slowPercent, 0f, 100f);
			EditorGUILayout.Slider (slowPercentProp, 0f, 100f, new GUIContent ("Slow percent"));
			//ability.slowDuration = EditorGUILayout.FloatField ("Slow duration", ability.slowDuration);
			EditorGUILayout.DelayedFloatField (slowDurationProp, new GUIContent ("Slow duration"));
			ability.decayingSlow = EditorGUILayout.Toggle ("Decaying slow", ability.decayingSlow);
			// I don't know how to make it work with toggles
			// where you don't have to click apply
			// not valid
			//EditorGUILayout.Toggle (decayingSlowProp, new GUIContent ("Decaying slow"));
			// same problem
			//ability.decayingSlow = EditorGUILayout.Toggle(new GUIContent("Decaying slow"), ability.decayingSlow);
			break;
		}

		// with this style, when the value field is changed, it stays
		// EditorGUILayout.DelayedFloatField (stunDurationProp, new GUIContent ("stunDuration"));

		// with this style, when the value field is changed it does NOT stay.
		// You have to click apply for it to save when you run
		//ability.dotDamageTick = EditorGUILayout.IntField ("Damage per tick", ability.dotDamageTick);

		serializedObject.ApplyModifiedProperties ();
	}
}
