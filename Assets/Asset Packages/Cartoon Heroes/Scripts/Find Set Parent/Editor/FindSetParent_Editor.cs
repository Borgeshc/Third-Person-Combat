using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CartoonHeroes{
	[CustomEditor (typeof(FindSetParent))]
	public class FindSetParent_Editor : Editor {

		GUIStyle titleStyle;
		GUIStyle smallTitleStyle;
		GUIStyle smallerTitleStyle;
		GUIStyle elementTitleStyle;
		GUIStyle changeColorStyle;

		public override void OnInspectorGUI(){
			//DrawDefaultInspector ();
			serializedObject.Update ();

			FindSetParent myScript = (FindSetParent)target;
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

			GUILayout.Label ("Find Set Parent", titleStyle);

			GUILayout.Space (15);

			GUILayout.Label ("Search Parent Options:", elementTitleStyle);
			GUILayout.BeginHorizontal ();
			myScript.parentName = EditorGUILayout.TextField ("Search Word", myScript.parentName);
			if (GUILayout.Button ("Set Parent Now")) {
				myScript.foundParent = false;
				myScript.findNow = true;
			}
			GUILayout.EndHorizontal ();
			myScript.findOnStart = EditorGUILayout.Toggle ("Find Parent On Start", myScript.findOnStart);
			myScript.closest = EditorGUILayout.Toggle ("Favour Proximity", myScript.closest);
			myScript.parentToRootObject = EditorGUILayout.Toggle ("Parent To Root", myScript.parentToRootObject);
			//if(!myScript.parentToRootObject){
				myScript.use_SearchWord_RootObjet = EditorGUILayout.Toggle ("Use Root Search Word", myScript.use_SearchWord_RootObjet);
				if (myScript.use_SearchWord_RootObjet) {
					myScript.searchWord_RootObjet = EditorGUILayout.TextField ("Root Search Word", myScript.searchWord_RootObjet);
				}
			//}

			myScript.hierarchyType = (FindSetParent.HierarchyType) EditorGUILayout.EnumPopup("Favor Hierarchy Size", myScript.hierarchyType);

			GUILayout.Space (15);

			GUILayout.Label ("Exclude Words From Search:", elementTitleStyle);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add New Exclude Word")) {
				myScript.AddExcludeWord ("Type Exclude Word Here.");
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

			if (myScript.excludeWords != null) {
				for (int i = 0; i < myScript.excludeWords.Length; i++) {
					GUILayout.BeginHorizontal ();
					myScript.excludeWords [i] = EditorGUILayout.TextField (myScript.excludeWords [i]);
					if (GUILayout.Button ("Remove")) {
						myScript.RemoveExcludeWord (i);
					}
					GUILayout.EndHorizontal ();
				}
			}

			GUILayout.Space (15);

			GUILayout.Label ("Post Parenting Options", smallTitleStyle);

			GUILayout.Space (10);

			myScript.setLocalPos = EditorGUILayout.Toggle ("Set Local Position", myScript.setLocalPos);
			if (myScript.setLocalPos) {
				myScript.localPos = EditorGUILayout.Vector3Field ("Local Position", myScript.localPos);
			} 
			myScript.setLocalRot = EditorGUILayout.Toggle ("Set Local Rotation", myScript.setLocalRot);
			if (myScript.setLocalRot) {
				myScript.localRot = EditorGUILayout.Vector3Field ("Local Rotation", myScript.localRot);
			}
			if (myScript.setLocalPos || myScript.setLocalRot) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Store Parent's Local Position and Rotation")) {
					myScript.storeCurrent_Parent = true;
				}
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}

			GUILayout.Space (15);
			myScript.destroyScriptAfterParenting = EditorGUILayout.Toggle ("Destroy After Parenting", myScript.destroyScriptAfterParenting);

			GUILayout.Space (15);

			if (myScript.foundParent) {
				GUILayout.Label ("Parented to: " + myScript.parentedTo.ToString ());
			}

			serializedObject.ApplyModifiedProperties ();
		}
	}
}