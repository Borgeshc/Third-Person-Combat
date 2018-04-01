using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CartoonHeroes{
	[CustomEditor (typeof(RemoveMeshParts_Set))]
	public class RemoveMeshParts_Set_Editor : Editor {

		GUIStyle titleStyle;
		GUIStyle elementTitleStyle;

		public override void OnInspectorGUI(){
			serializedObject.Update ();
			RemoveMeshParts_Set myScript = (RemoveMeshParts_Set)target;
			EditorUtility.SetDirty (myScript);
			//DrawDefaultInspector ();

			//Font Styles
			if (titleStyle == null) {
				titleStyle = new GUIStyle ();
			}
			titleStyle.alignment = TextAnchor.MiddleCenter;
			titleStyle.fontStyle = FontStyle.Bold;
			titleStyle.fontSize = 18;

			if (elementTitleStyle == null) {
				elementTitleStyle = new GUIStyle ();
			}
			elementTitleStyle.alignment = TextAnchor.UpperLeft;
			elementTitleStyle.fontStyle = FontStyle.Bold;
			elementTitleStyle.fontSize = 13;

			GUILayout.Space (15);

			GUILayout.Label ("Set Remove Parts (Set)", titleStyle);

			GUILayout.Space (15);

			GUILayout.Label ("Parts To Remove:", elementTitleStyle);

			if (GUILayout.Button ("Add Part")) {
				myScript.AddMeshPart ();
			}

			if (myScript.removeMeshPartNameList == null) {
				myScript.removeMeshPartNameList = new string[0];
			}

			for (int i = 0; i < myScript.removeMeshPartNameList.Length; i++) {
				GUILayout.BeginHorizontal ();
				myScript.removeMeshPartNameList [i] = EditorGUILayout.TextField (myScript.removeMeshPartNameList [i]);
				if (GUILayout.Button ("Delete Part")) {
					myScript.DeleteMeshPart (i);
				}
				GUILayout.EndHorizontal ();
			}

			GUILayout.Space (20);

			GUILayout.Label ("RemoveMeshParts Component:", elementTitleStyle);

			if (GUILayout.Button ("Find On Children")) {
				myScript.BeginGetComponent ();
			}
			if (myScript.removeMeshParts != null) {
				for (int i = 0; i < myScript.removeMeshParts.Length; i++) {
					myScript.removeMeshParts [i] = EditorGUILayout.ObjectField (myScript.removeMeshParts [i], typeof(RemoveMeshParts), true) as RemoveMeshParts;
				}
			}
			myScript.findRemoveMeshPartsOnStart = EditorGUILayout.Toggle ("Find on Start()", myScript.findRemoveMeshPartsOnStart);

			if (myScript.removeMeshParts == null) {
				if(!myScript.findRemoveMeshPartsOnStart){
					EditorGUILayout.HelpBox("RemoveMeshParts script not found.", MessageType.Warning);
				}
			}

			GUILayout.Space (20);
			GUILayout.Label ("Options:", elementTitleStyle);

			EditorGUILayout.Toggle ("Apply as soon as possible", myScript.applyASAP);

			serializedObject.ApplyModifiedProperties ();
		}
	}
}