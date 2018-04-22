using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CartoonHeroes{
	[CustomEditor (typeof(RemoveMeshParts))]
	public class RemoveMeshParts_Editor : Editor {

		GUIStyle titleStyle;
		GUIStyle smallTitleStyle;
		GUIStyle smallerTitleStyle;
		GUIStyle elementTitleStyle;
		GUIStyle changeColorStyle;

		public override void OnInspectorGUI(){
			//DrawDefaultInspector ();
			serializedObject.Update ();
			RemoveMeshParts myScript = (RemoveMeshParts)target;
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

			//Last Rect used in Layout GUI
			Rect lastRect;
			string buttonText;
			float progress = 0;
			string progressText = "";

			GUILayout.Space (15);

			GUILayout.Label ("Remove Mesh Parts", titleStyle);

			GUILayout.Space (15);

			if (myScript.skinnedMesh == null && myScript.meshFilter == null) {
				EditorGUILayout.HelpBox("Requires SkinnedMeshRenderer. Use \"Get Component\" button, or drop component on field below.", MessageType.Warning);
			}

			EditorGUI.BeginDisabledGroup (!(myScript.initState == RemoveMeshParts.InitState.None));

			GUILayout.BeginHorizontal ();
			if (myScript.meshFilter == null) {
				EditorGUILayout.HelpBox ("\"public SkinnedMeshRenderer skinnedMesh\"", MessageType.None);
				myScript.skinnedMesh = EditorGUILayout.ObjectField (myScript.skinnedMesh, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
			} else {
				EditorGUILayout.HelpBox ("\"public MeshFilter meshFilter\"", MessageType.None);
				myScript.meshFilter = EditorGUILayout.ObjectField (myScript.meshFilter, typeof(MeshFilter), true) as MeshFilter;		
			}

			GUILayout.EndHorizontal ();

			if (GUILayout.Button ("Get Component (Find On Local GameObject)")) {
				myScript.skinnedMesh = myScript.gameObject.GetComponent<SkinnedMeshRenderer> ();
				if (myScript.skinnedMesh == null) {
					myScript.meshFilter = myScript.gameObject.GetComponent<MeshFilter> ();
				}
			}

			if (myScript.skinnedMesh == null && myScript.meshFilter == null) {
				serializedObject.ApplyModifiedProperties ();
				return;
			}

			if (myScript.skinnedMesh != null || myScript.meshFilter != null) {

				myScript.customEditor_SourceMeshFoldout = EditorGUILayout.Foldout (myScript.customEditor_SourceMeshFoldout, "Source Mesh");

				if (myScript.customEditor_SourceMeshFoldout) {
					if (myScript.sourceMesh == null) {
						EditorGUILayout.HelpBox ("Null Source Mesh: When initializing will be automatically retrieved from SkinnedMeshRenderer", MessageType.Info);
					}
					GUILayout.BeginHorizontal ();
					EditorGUILayout.HelpBox ("\"public Mesh sourceMesh\"", MessageType.None);
					myScript.sourceMesh = EditorGUILayout.ObjectField (myScript.sourceMesh, typeof(Mesh), true) as Mesh;
					GUILayout.EndHorizontal ();
				}

				EditorGUI.EndDisabledGroup ();


				GUILayout.Space (10);

				GUILayout.Label ("Initializing Options", smallTitleStyle);

				GUILayout.Space (10);

				GUILayout.Label ("Frame Duration Before Breaking Every Initializing Loop:");
				myScript.maxTimePerLoop = EditorGUILayout.Slider ("Milliseconds" , myScript.maxTimePerLoop, 5, 250);
				if (myScript.maxTimePerLoop < 30) {
					EditorGUILayout.HelpBox("Slow / Works On Background", MessageType.Info);
				} else {
					if (myScript.maxTimePerLoop > 60) {
						EditorGUILayout.HelpBox("Fast / Choppy Framerate", MessageType.Info);
					} else {
						GUILayout.Space(2);
					}
				}

				GUILayout.Space (6);

				myScript.mode = (RemoveMeshParts.Mode) EditorGUILayout.EnumPopup ("Initialize Mode:", myScript.mode);

				if (myScript.mode == RemoveMeshParts.Mode.Fast) {
					EditorGUILayout.HelpBox("Fast: Less Accurate", MessageType.Info);
				} else {
					GUILayout.Space(2);
				}

				GUILayout.Space (10);

				EditorGUI.BeginDisabledGroup (myScript.groups == null || myScript.groups.Length == 0);
				if (GUILayout.Button ("Initialize")) {
					myScript.Init ();
				}
				EditorGUI.EndDisabledGroup ();

				EditorGUILayout.Space ();
				EditorGUILayout.Space ();

				lastRect = GUILayoutUtility.GetLastRect ();
				lastRect.size = new Vector3 (lastRect.size.x, 18);

				if (myScript.progressTotal != 0) {
					progress = (float)myScript.currentProgress / (float)myScript.progressTotal;
				}

				if (myScript.initState == RemoveMeshParts.InitState.None) {
					progressText = "0%";
				} else {
					progressText = "Initializing " + Mathf.Round (progress * 100).ToString () + "% ...";
				}

				if (!myScript.initRequried) {
					progressText = "Ready To Apply 100%";
					progress = 1.0f;
				}

				EditorGUI.ProgressBar (lastRect, progress, progressText);

				GUILayout.Space (15);

				EditorGUI.BeginDisabledGroup (myScript.initState == RemoveMeshParts.InitState.None);
				if (GUILayout.Button ("Abort")) {
					myScript.Abort ();
				}
				EditorGUI.EndDisabledGroup ();

				GUILayout.Space (25);

				GUILayout.Label ("Apply Mesh Visibility Changes", smallTitleStyle);

				GUILayout.Space (10);

				EditorGUI.BeginDisabledGroup (myScript.initRequried);
				buttonText = "Apply Mesh Visibility";
				if (myScript.initRequried) {
					buttonText += " (Initializing Required)";
				}
				GUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox("\"public void UpdateVisibility()\"", MessageType.None);

				if (GUILayout.Button (buttonText)) {
					myScript.UpdateVisibility ();
				}
				GUILayout.EndHorizontal ();
				EditorGUI.EndDisabledGroup ();

				EditorGUI.BeginDisabledGroup (myScript.sourceMesh == null);
				buttonText = "Restore Original Mesh";
				if(myScript.sourceMesh == null){
					buttonText += " (Initializing Required)";
				}

				GUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox("\"public void RestoreMesh()\"             ", MessageType.None);
				if (GUILayout.Button (buttonText)) {
					myScript.RestoreMesh ();
				}
				GUILayout.EndHorizontal ();
				EditorGUI.EndDisabledGroup ();

				GUILayout.Space (20);

				GUILayout.Label ("Set Mesh Visibility", smallTitleStyle);
				EditorGUILayout.HelpBox("\"public void MeshPartRemove_Set (string partName, bool set)\"", MessageType.None);

				GUILayout.Label ("Visible Mesh Parts", elementTitleStyle);

				for (int i = 0; i < myScript.groups.Length; i++) {
					GUILayout.BeginHorizontal ();
					if (!myScript.groups [i].remove) {
						if (GUILayout.Button ("Set Hidden")) {
							myScript.groups [i].remove = true;
						}
						changeColorStyle.normal.textColor = myScript.groups[i].drawColor;
						GUILayout.Label ("█ ", changeColorStyle);
						GUILayout.Label (myScript.groups[i].name);

						GUILayout.FlexibleSpace ();
					}
					GUILayout.EndHorizontal ();
				}

				GUILayout.Space (10);

				GUILayout.Label ("Hidden Mesh Parts", elementTitleStyle);

				for (int i = 0; i < myScript.groups.Length; i++) {
					GUILayout.BeginHorizontal ();
					if (myScript.groups [i].remove) {
						if (GUILayout.Button ("Set Visible ")) {
							myScript.groups [i].remove = false;
						}
						changeColorStyle.normal.textColor = myScript.groups[i].drawColor;
						GUILayout.Label ("█ ", changeColorStyle);
						GUILayout.Label (myScript.groups[i].name);

						GUILayout.FlexibleSpace ();
					}
					GUILayout.EndHorizontal ();
				}

				GUILayout.Space (20);

				EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);

				GUILayout.Space (20);

				GUILayout.Label ("Mesh Parts", smallTitleStyle);

				if (myScript.groups != null) {
					string pluralSingular = "Mesh Part";
					if (myScript.groups.Length != 1) {
						pluralSingular = "Mesh Parts";
					}
					GUILayout.Label ("(" + myScript.groups.Length.ToString () + " " + pluralSingular + ")", smallerTitleStyle);
				}

				GUILayout.Space (10);

				myScript.debug = EditorGUILayout.Toggle ("Draw Mesh Parts On Viewport", myScript.debug);

				GUILayout.Space (10);

				EditorGUI.BeginDisabledGroup (!(myScript.initState == RemoveMeshParts.InitState.None));

				if (GUILayout.Button ("Add Mesh Part")) {
					myScript.NewGroup ();
				}

				EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);

				string groupsFoldoutText = "";

				if (!myScript.groupsFoldout_CustomEditor) {
					groupsFoldoutText = "Expand Mesh Parts";
				} else {
					groupsFoldoutText = "Collapse Mesh Parts";
				}

				myScript.groupsFoldout_CustomEditor = EditorGUILayout.Foldout (myScript.groupsFoldout_CustomEditor, groupsFoldoutText);

				if (myScript.groupsFoldout_CustomEditor) {

					if (myScript.groups == null) {
						myScript.groups = new RemoveMeshParts.Group[0];
					}
					if (myScript.triangleGroups == null) {
						myScript.triangleGroups = new RemoveMeshParts.TriangleGroup[0];
					}
					for (int i = 0; i < myScript.groups.Length; i++) {
						GUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();
						changeColorStyle.normal.textColor = myScript.groups [i].drawColor;
						GUILayout.Label ("█ ", changeColorStyle);

						GUILayout.Label (myScript.groups [i].name + ": ", smallerTitleStyle);
						GUILayout.FlexibleSpace ();
						GUILayout.EndHorizontal ();

						GUILayout.Space (20);

						lastRect = GUILayoutUtility.GetLastRect ();

						myScript.groups [i].name = EditorGUILayout.TextField ("Group Name: ", myScript.groups [i].name);

						myScript.groups [i].drawColor = EditorGUILayout.ColorField ("Draw Color: ", myScript.groups [i].drawColor);

						GUILayout.Space (15);

						myScript.groups [i].boundsFoldout_CustomEditor = EditorGUILayout.Foldout (myScript.groups [i].boundsFoldout_CustomEditor, "Bounds");
						if (myScript.groups [i].boundsFoldout_CustomEditor) {
							if (GUILayout.Button ("Add Bounds")) {
								myScript.groups [i].AddBound ();
							}
							GUILayout.Space (15);
							for (int n = 0; n < myScript.groups [i].bounds.Length; n++) {
								GUILayout.Label (myScript.groups[i].name + " Bounds " + n.ToString () + ":", elementTitleStyle);
								GUILayout.BeginHorizontal ();
								myScript.groups [i].bounds [n] = EditorGUILayout.BoundsField (myScript.groups [i].bounds [n]);
								if (GUILayout.Button ("Delete")) {
									myScript.groups [i].DeleteBounds (n);
								}

								lastRect = GUILayoutUtility.GetLastRect ();
								lastRect.center = new Vector3 (lastRect.center.x, lastRect.center.y + lastRect.size.y);
								if (GUI.Button (lastRect, "Duplicate")) {
									myScript.groups [i].DuplicateBounds (n);
								}

								GUILayout.EndHorizontal ();
								EditorGUILayout.Space ();
							}
						}

						int groups = 0;
						int groupTriangles = 0;
						for (int n = 0; n < myScript.triangleGroups.Length; n++) {
							if (myScript.triangleGroups [n].HasGroup (i)) {
								groups++;
								groupTriangles += myScript.triangleGroups [n].triangles.Length / 3;
							}
						}

						GUILayout.Space (15);

						if (GUILayout.Button ("Delete " + myScript.groups [i].name)) {
							myScript.DeleteGroup (i);
						}

						GUILayout.Space (15);

						if (Application.isPlaying && myScript.groups [i].assignedTranforms.Count > 0) {
							GUILayout.Label ("Assigned Transforms:", elementTitleStyle);
							for (int n = 0; n < myScript.groups [i].assignedTranforms.Count; n++) {
								GUILayout.Label(n.ToString() + ": " + myScript.groups[i].assignedTranforms[n].name);
								GUILayout.Space (10);
							}

							if (myScript.groups [i].restoreVisibilityIfNoAssignedTransforms) {
								EditorGUILayout.HelpBox ("Transforms assigned. Visibility will restore if Transforms are destoyed.", MessageType.Info);
							}
						}

						EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);

						GUILayout.Space (20);
					}
				}

				EditorGUI.EndDisabledGroup ();


				serializedObject.ApplyModifiedProperties ();
			}
		}

	}
}
