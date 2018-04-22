using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CartoonHeroes{
	[CustomEditor (typeof(TransformReactor))]
	public class TransformReactor_Editor : Editor {

		GUIStyle titleStyle;
		GUIStyle smallTitleStyle;
		GUIStyle elementTitleStyle;


		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();
			TransformReactor myScript = (TransformReactor)target;
			EditorUtility.SetDirty (myScript);

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

			GUILayout.Space (15);

			GUILayout.Label ("Transform Reactor", titleStyle);

			GUILayout.Space (15);


			if (myScript.updateTransform) {
				if (GUILayout.Button ("Disable Transform Update")) {
					myScript.updateTransform = false;
				}
			} else {
				if (GUILayout.Button ("Enable Transform Update")) {
					myScript.updateTransform = true;
				}
			}

			if (!myScript.updateTransform) {
				EditorGUILayout.HelpBox ("Disabled: Can Edit Transform", MessageType.None);
			}

			GUILayout.Space (15);

			GUILayout.Label ("Auto-Find Bones Options", smallTitleStyle);

			GUILayout.Space (10);

			GUILayout.Label ("FindSetParent Options", elementTitleStyle);

			myScript.waitForFindSetParent = EditorGUILayout.Toggle ("Use FindSetParent On Start", myScript.waitForFindSetParent);
			if (myScript.waitForFindSetParent) {
				EditorGUILayout.HelpBox ("Using FindSetParent Changes Search's Root Object", MessageType.Info);
			

				GUILayout.Space (10);

				myScript.findAfterFindSetParent = EditorGUILayout.ObjectField (myScript.findAfterFindSetParent, typeof(FindSetParent), true) as FindSetParent;
				if (GUILayout.Button ("Get FindSetParent Componnent (Local)")) {
					myScript.findAfterFindSetParent = myScript.gameObject.GetComponent<FindSetParent> ();
				}

				if (myScript.findAfterFindSetParent != null) {
					myScript.findAfterFindSetParent.parentName = EditorGUILayout.TextField ("FindSetParent Search Word", myScript.findAfterFindSetParent.parentName);

					if (GUILayout.Button ("Apply FindSetParent")) {
						myScript.ApplyFindSetParent ();
					}
				}
			}

			GUILayout.Space (10);



			GUILayout.Label ("Master Bone", elementTitleStyle);
			myScript.masterBone = EditorGUILayout.ObjectField ("Master Bone", myScript.masterBone, typeof(Transform), true) as Transform;
			if (myScript.masterBone == null) {
				GUILayout.BeginHorizontal ();
				myScript.findMasterBone = EditorGUILayout.TextField ("Search Word", myScript.findMasterBone);
				if (myScript.findMasterBone != "") {
					if (GUILayout.Button ("Search")) {
						myScript.FindMasterBone ();
						if (myScript.masterBone != null) {
							myScript.StoreMasterBoneDefaultPos ();
						}
					}
				} 
				GUILayout.EndHorizontal ();
				EditorGUILayout.HelpBox ("Null Master Bone Allows Search On Start", MessageType.Info);
			} 

			GUILayout.Space (15);

			GUILayout.Label ("Reference Bone", elementTitleStyle);
			myScript.reactionPointRef = EditorGUILayout.ObjectField ("Reference Bone", myScript.reactionPointRef, typeof(Transform), true) as Transform;
			if (myScript.reactionPointRef == null) {
				GUILayout.BeginHorizontal ();
				myScript.findReactionPointRef = EditorGUILayout.TextField ("Search Word", myScript.findReactionPointRef);
				if (myScript.findReactionPointRef != "") {
					if (GUILayout.Button ("Search")) {
						myScript.FindReferenceBone ();
					}
				}
				GUILayout.EndHorizontal ();

				EditorGUILayout.HelpBox ("Null Reference Bone Allows Search On Start", MessageType.Info);
			}

			GUILayout.Space (15);

			GUILayout.Label ("Reaction Options", smallTitleStyle);

			GUILayout.Space (15);

			GUILayout.Label ("Draw Elements", elementTitleStyle);
			myScript.debug = EditorGUILayout.Toggle ("Draw Elements On Viewport", myScript.debug);
			myScript.debugSize = EditorGUILayout.Slider ("Draw Size", myScript.debugSize, .01f, 0.5f);



			GUILayout.Space (10);

			GUILayout.Label ("Enable Transform Reaction", elementTitleStyle);


			//myScript.updateTransform = !EditorGUILayout.Toggle ("Disable Update (Enable Editor)", myScript.updateTransform);

			myScript.affectPosition = EditorGUILayout.Toggle ("Enable Position Reaction", myScript.affectPosition);
			myScript.affectRotation = EditorGUILayout.Toggle ("Enable Position Reaction", myScript.affectRotation);



			GUILayout.Space (10);

			GUILayout.Label ("Master Bone", elementTitleStyle);
			//EditorGUI.BeginDisabledGroup (myScript.masterBone == null || myScript.masterBone.childCount == 0);
			myScript.useFirstChild = EditorGUILayout.Toggle ("Use First Child As Reference Point", myScript.useFirstChild);
			//EditorGUI.EndDisabledGroup ();
			if (!myScript.useFirstChild) {
				myScript.boneLocalPoint = EditorGUILayout.Vector3Field ("Master Bone Local Point", myScript.boneLocalPoint);
			}
			GUILayout.Space (10);

			EditorGUI.BeginDisabledGroup (myScript.masterBone == null);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Select Master Bone")) {
				Selection.activeObject = myScript.masterBone;
				if (myScript.disableUpdateWhenSelectingMasterBone) {
					myScript.updateTransform = false;
				}
			}
			myScript.disableUpdateWhenSelectingMasterBone = EditorGUILayout.ToggleLeft ("Auto-Disable Update When Selecting", myScript.disableUpdateWhenSelectingMasterBone);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if(GUILayout.Button ("Store (Default) Local Position & Rotation")){
				myScript.StoreMasterBoneDefaultPos ();
			}
			if (GUILayout.Button ("Restore Local Position & Rotation")) {
				myScript.SetMasterBoneDefaultPos ();
			}
			GUILayout.EndHorizontal ();

			//myScript.masterBoneDefaultLocalPosition = EditorGUILayout.Vector3Field ("Default Local Position", myScript.masterBoneDefaultLocalPosition);
			//myScript.masterBoneDefaultLocalRotation = EditorGUILayout.Vector3Field ("Default Local Rotation", myScript.masterBoneDefaultLocalRotation);

			EditorGUI.EndDisabledGroup ();

			GUILayout.Space (10);



			int reactionsPointsCount = 0;
			if (myScript.reactionPoints != null) {
				reactionsPointsCount = myScript.reactionPoints.Length;
			}
			string plural = " Points)";
			if (reactionsPointsCount == 1) {
				plural = " Point)";
			}

			GUILayout.Label ("Reaction Points (" + reactionsPointsCount.ToString() + plural, elementTitleStyle);

			myScript.rangeMultiplier = EditorGUILayout.Slider ("Range Multiplier", myScript.rangeMultiplier, .1f, 2.0f);

			if (GUILayout.Button ("Create Reaction Point")) {
				myScript.AddPointAtEditorPos ();
			}


			if (myScript.reactionPoints != null && myScript.reactionPoints.Length > 0) {
				for (int n = 0; n < myScript.reactionPoints.Length; n++) {
					myScript.reactionPoints [n].foldout_CustomEditor = EditorGUILayout.Foldout (myScript.reactionPoints [n].foldout_CustomEditor, "Reaction Point : " + n.ToString ());
					if (myScript.reactionPoints [n].foldout_CustomEditor) {
						EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
						GUILayout.Label ("Reaction Point " + n.ToString (), elementTitleStyle);
						myScript.reactionPoints [n].localPoint = EditorGUILayout.Vector3Field ("Point Local Position", myScript.reactionPoints [n].localPoint);
						myScript.reactionPoints [n].range = EditorGUILayout.FloatField ("Range: ", myScript.reactionPoints [n].range);
						myScript.reactionPoints [n].showRange = EditorGUILayout.Toggle ("Show Range", myScript.reactionPoints [n].showRange);
						GUILayout.Space (10);

						GUILayout.Label ("Local Position & Rotation:", EditorStyles.boldLabel);

						if (GUILayout.Button ("Store Current Local Position & Rotation")) {
							myScript.reactionPoints [n].StoreCurrentTransform (myScript.transform);
						}
						if (GUILayout.Button ("Apply Stored (Disables Transform Update)")) {
							myScript.updateTransform = false;
							myScript.reactionPoints [n].ApplyStoredTransform (myScript.transform);
						}
						myScript.reactionPoints [n].localPositionSet = EditorGUILayout.Vector3Field ("Position", myScript.reactionPoints [n].localPositionSet);
						myScript.reactionPoints [n].localRotationSet = EditorGUILayout.Vector3Field ("Rotation", myScript.reactionPoints [n].localRotationSet);

						GUILayout.Space (10);
						if (GUILayout.Button ("Remove Point " + n.ToString ())) {
							myScript.RemovePoint (n);
						}
						EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
					}
				}
			}

			serializedObject.ApplyModifiedProperties ();
		}
	}
}