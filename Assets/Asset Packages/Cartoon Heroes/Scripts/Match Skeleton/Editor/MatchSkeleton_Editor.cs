using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CartoonHeroes{
	[CustomEditor (typeof(MatchSkeleton))]
	public class MatchSkeleton_Editor : Editor {

		GUIStyle titleStyle;
		GUIStyle smallTitleStyle;
		GUIStyle smallerTitleStyle;
		GUIStyle elementTitleStyle;
		GUIStyle changeColorStyle;

		public override void OnInspectorGUI(){
			//DrawDefaultInspector ();
			serializedObject.Update ();
			MatchSkeleton myScript = (MatchSkeleton)target;
			EditorUtility.SetDirty (myScript);

			//Font Styles
			if (titleStyle == null) {
				titleStyle = new GUIStyle ();
			}
			titleStyle.alignment = TextAnchor.MiddleCenter;
			titleStyle.fontStyle = FontStyle.Bold;
			titleStyle.fontSize = 18;

			if (smallTitleStyle == null) {
				smallTitleStyle = new GUIStyle ();
			}
			smallTitleStyle.alignment = TextAnchor.MiddleCenter;
			smallTitleStyle.fontStyle = FontStyle.Bold;
			smallTitleStyle.fontSize = 16;

			if (elementTitleStyle == null) {
				elementTitleStyle = new GUIStyle ();
			}
			elementTitleStyle.alignment = TextAnchor.UpperLeft;
			elementTitleStyle.fontStyle = FontStyle.Bold;
			elementTitleStyle.fontSize = 13;

			if (smallerTitleStyle == null) {
				smallerTitleStyle = new GUIStyle ();
			}
			smallerTitleStyle.alignment = TextAnchor.MiddleCenter;
			smallerTitleStyle.fontStyle = FontStyle.Bold;
			smallerTitleStyle.fontSize = 13;

			if (changeColorStyle == null) {
				changeColorStyle = new GUIStyle ();
			}
			changeColorStyle.fontStyle = FontStyle.Bold;
			changeColorStyle.fontSize = 13;

			GUILayout.Space (15);

			GUILayout.Label ("Match Skeleton", titleStyle);

			GUILayout.Space (15);

			GUILayout.Label ("Master Bones", smallTitleStyle);
			GUILayout.Space (10);

			myScript.useParent = EditorGUILayout.Toggle ("Root Bone Is Parent", myScript.useParent);
			if (myScript.useParent) {
				myScript.waitForFindSetParent = EditorGUILayout.Toggle ("Get And Apply FindSetParent On Start", myScript.waitForFindSetParent);
			} else {
				myScript.masterRoot = EditorGUILayout.ObjectField ("Master Root", myScript.masterRoot, typeof(Transform), true) as Transform;
			}

			GUILayout.Space (15);
			GUILayout.Label ("Slave Bones", smallTitleStyle);
			GUILayout.Space (10);

			if (myScript.slaveRoot == null) {
				myScript.slaveRootIsThisObject = EditorGUILayout.Toggle ("Slave Root Is This Object", myScript.slaveRootIsThisObject);
			} else {
				GUILayout.Label ("");
			}

			if (!myScript.slaveRootIsThisObject) {
				myScript.slaveRoot = EditorGUILayout.ObjectField ("Slave Root", myScript.slaveRoot, typeof(Transform), true) as Transform;
			} else {
				GUILayout.Label ("");
			}

			GUILayout.Space (15);
			GUILayout.Label ("Options", smallTitleStyle);
			GUILayout.Space (10);
			myScript.copyPosition = EditorGUILayout.Toggle ("Match Position", myScript.copyPosition);
			myScript.copyRotation = EditorGUILayout.Toggle ("Match Rotation", myScript.copyRotation);
			myScript.copyScale = EditorGUILayout.Toggle ("Match Scale", myScript.copyScale);


			serializedObject.ApplyModifiedProperties ();
		}
	}
}