using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CartoonHeroes{
	[CustomEditor (typeof(HighHeelsFix))]
	public class HighHeelsFix_Editor : Editor {

		GUIStyle titleStyle;
		GUIStyle smallTitleStyle;
		GUIStyle elementTitleStyle;

		public override void OnInspectorGUI(){
			serializedObject.Update ();

			//DrawDefaultInspector ();

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

			HighHeelsFix myScript = (HighHeelsFix)target;

			serializedObject.ApplyModifiedProperties ();

			GUILayout.Space (15);

			GUILayout.Label ("High Heels Modifier", titleStyle);

			GUILayout.Space (15);

			GUILayout.Label ("Character Root", elementTitleStyle);
			myScript.characterRoot_UseFindSetParent = EditorGUILayout.Toggle ("Get From FindSetParent", myScript.characterRoot_UseFindSetParent);
			if (!myScript.characterRoot_UseFindSetParent) {
				myScript.characterRoot = EditorGUILayout.ObjectField ("Character", myScript.characterRoot, typeof(Transform), true) as Transform;
			}

			GUILayout.Space (15);
			GUILayout.Label ("Skeleton Root", elementTitleStyle);
			myScript.skeletonRoot_UseCharacterLargestHierarchy = EditorGUILayout.Toggle ("Use Character's Largest Hierarchy Child", myScript.skeletonRoot_UseCharacterLargestHierarchy);
			if (!myScript.skeletonRoot_UseCharacterLargestHierarchy) {
				myScript.skeletonRoot = EditorGUILayout.ObjectField ("Skeleton", myScript.skeletonRoot, typeof(Transform), true) as Transform; 
			}

			GUILayout.Space (15);
			GUILayout.Label ("Feet", elementTitleStyle);
			myScript.feetLength = EditorGUILayout.Slider ("Feet's Length", myScript.feetLength, 0.1f, 0.5f);
			myScript.heelHeight = EditorGUILayout.Slider ("Heel's Height", myScript.heelHeight, 0.0f, 0.5f); 
			if (myScript.footAxis == Vector3.zero) {
				myScript.footAxis = new Vector3 (0, 0, 1);
			}
			myScript.footAxis = EditorGUILayout.Vector3Field ("Foot's X axis", myScript.footAxis);

			GUILayout.Space (10);

			myScript.footSearchWord = EditorGUILayout.TextField ("Feet Search Word", myScript.footSearchWord);
			if (myScript.footSearchWord == "" || myScript.footSearchWord == null) {
				myScript.leftFoot = EditorGUILayout.ObjectField("Left Foot", myScript.leftFoot, typeof(Transform), true) as Transform;
				myScript.rightFoot = EditorGUILayout.ObjectField ("Right Foot", myScript.rightFoot, typeof(Transform), true) as Transform;
			}
		}
	}
}