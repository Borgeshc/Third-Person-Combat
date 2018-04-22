//Purpose: armor pieces rotation and position based on a master bone's orientation in relation to reference bone.
//Example: Upper arm (master bone) orientation in relation to the spine bone (reference bone) controls shoulder armor's position and rotation.

//There must be stored multiple local points for the reference bone, and for each one establish a corresponding position and rotation for the armor piece object.
//Then script applies by interpolation corresponding position and rotation by checking global proximity from reference pone's local point, to a certain master's bone local point.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CartoonHeroes{
	[ExecuteInEditMode]
	public class TransformReactor : MonoBehaviour {

		//ReactionPoint class stores a local point, and corresponding local position and rotation.
		[Serializable]
		public class ReactionPoint{
			public Vector3 localPoint;
			[Space(30)]
			public float range = 0.5f;
			public bool showRange;
			[HideInInspector]
			public float weight; //Weight is calculated in LateUpdate() as long as updateWeights bool is true.
			[Space(30)]
			public Vector3 localPositionSet;
			public Vector3 localRotationSet;
			[Space(15)]
			//Tools to help configure script.
			public bool storeCurrentTransform; //If set true, stores current armor piece's local position and rotation for this point.
			public bool applyOnEditor; //If updateWeights is false it applies this point's local position and rotation.

			public void StoreCurrentTransform(Transform t){
				localPositionSet = t.localPosition;
				localRotationSet = t.localRotation.eulerAngles;		
			}

			public void ApplyStoredTransform(Transform t){
				t.localPosition = localPositionSet;
				t.localRotation = Quaternion.Euler (localRotationSet);
			}

			public bool foldout_CustomEditor;
		}

		public string findMasterBone = "";
		public string findReactionPointRef = "";
		public bool waitForFindSetParent = true;
		public FindSetParent findAfterFindSetParent;

		[Space(20)]
		public bool affectPosition = true;
		public bool affectRotation = true;
		[Space(20)]
		public Transform masterBone; //Master bone, basically where the armor piece would be physically tied in real life.
		public bool useFirstChild = true;
		public Vector3 boneLocalPoint; //Master bone's local point which will be tested for proximity against reference bone's multiply local points.
		public Transform reactionPointRef; //Reference bone. (Usually higher in hierarchy than master bone)

		public Vector3 masterBoneDefaultLocalPosition;
		public Vector3 masterBoneDefaultLocalRotation;
		public Vector3 masterBoneDefaultLocalPosition_Parent;
		public Vector3 masterBoneDefaultLocalRotation_Parent;

		public bool disableUpdateWhenSelectingMasterBone = true;

		[Space(40)]
		public bool updateWeights = true;
		public float distCurve = .8f;
		public bool updateTransform = true;
		public bool addPointAtEditorPos; //Adds a reference bone's local point, using current master bone's local point's position.
		public ReactionPoint[] reactionPoints; //Reference bone's local points.
		public float rangeMultiplier = 1.0f;
		[Space(40)]
		public Vector3 addRotation;
		public Vector3 addPosition;
		[Space(40)]
		public float debugSize = .03f;
		public bool debug = false;
		GUIStyle labelColor;

		public bool reactionPointsFoldout_CustomEditor = true;



		public void StoreMasterBoneDefaultPos(){
			if (masterBone != null) {
				masterBoneDefaultLocalPosition = masterBone.localPosition;
				masterBoneDefaultLocalRotation = masterBone.localRotation.eulerAngles;

				if (masterBone.parent != null) {
					masterBoneDefaultLocalPosition_Parent = masterBone.parent.localPosition;
					masterBoneDefaultLocalRotation_Parent = masterBone.parent.localRotation.eulerAngles;			
				}
			}
		}

		public void SetMasterBoneDefaultPos(){
			if (masterBone != null) {
				masterBone.localPosition = masterBoneDefaultLocalPosition;
				masterBone.localRotation = Quaternion.Euler (masterBoneDefaultLocalRotation);

				if (masterBone.parent != null) {
					masterBone.parent.localPosition = masterBoneDefaultLocalPosition_Parent;
					masterBone.parent.localRotation = Quaternion.Euler (masterBoneDefaultLocalRotation_Parent);				
				}
			}
		}

		//Adds a reference bone's local point, using current master bone's local point's position.
		public void AddPointAtEditorPos () {
			if (reactionPoints == null) {
				reactionPoints = new ReactionPoint[0];
			}

			List<ReactionPoint> newReactionPoints = new List<ReactionPoint>();
	
			for (int n = 0; n < reactionPoints.Length; n++) {
				newReactionPoints.Add (reactionPoints [n]);
			}


			Vector3 globalMasterBonePointPos = masterBone.TransformPoint (boneLocalPoint);
			ReactionPoint newReactionPoint = new ReactionPoint ();
			newReactionPoint.localPoint = reactionPointRef.InverseTransformPoint (globalMasterBonePointPos);
			newReactionPoint.StoreCurrentTransform (transform);

			newReactionPoints.Add (newReactionPoint);

			reactionPoints = newReactionPoints.ToArray();
		}

		public void RemovePoint(int removeIndex){
			List<ReactionPoint> newReactionPoints = new List<ReactionPoint> (reactionPoints);
			newReactionPoints.RemoveAt (removeIndex);
			reactionPoints = newReactionPoints.ToArray ();
		}

		// Use this for initialization
		void Start () {
			if (Application.isPlaying) {
				if (waitForFindSetParent) {
					ApplyFindSetParent ();
				}

				if (masterBone == null) {
					FindMasterBone ();
				}

				if (reactionPointRef == null) {
					FindReferenceBone ();
				}
			}
		}

		public void ApplyFindSetParent(){
			if (findAfterFindSetParent == null) {
				findAfterFindSetParent = GetComponent<FindSetParent> ();
			}
			if (findAfterFindSetParent != null) {
				findAfterFindSetParent.foundParent = false;
				findAfterFindSetParent.FindAndSetParent ();
			}	
		}

		public void FindMasterBone() {
			Transform currentParent = transform.parent;
			if (!Application.isPlaying && waitForFindSetParent) {
				ApplyFindSetParent ();
			}

			Transform[] allChildren = transform.root.GetComponentsInChildren<Transform> ();
			for (int i = 0; i < allChildren.Length; i++) {
				Transform thisChild = allChildren [i];

				if (thisChild.name.ToLower().Contains (findMasterBone.ToLower ())) {
					masterBone = thisChild;

					break;
				}
			}

			transform.parent = currentParent;
		}

		public void FindReferenceBone(){
			Transform currentParent = transform.parent;
			if (!Application.isPlaying && waitForFindSetParent) {
				ApplyFindSetParent ();
			}

			Transform[] allChildren = transform.root.GetComponentsInChildren<Transform> ();
			for (int i = 0; i < allChildren.Length; i++) {
				Transform thisChild = allChildren [i];

				if (thisChild.name.ToLower().Contains (findReactionPointRef.ToLower ())) {
					reactionPointRef = thisChild;
					break;
				}
			}

			transform.parent = currentParent;
		}
		
		void LateUpdate () {
			//If using first child as bone local point, set the bone local point to be first child's local position.
			if(useFirstChild && masterBone != null){
				if (masterBone.childCount > 0) {
					boneLocalPoint = masterBone.GetChild (0).localPosition;
				} else {
					useFirstChild = false;
				}
			}

			if (masterBone == null || reactionPointRef == null) {
				//this.enabled = false;
				return;
			}

			//Global position of master bone's local point.
			Vector3 masterBonePoint = masterBone.TransformPoint(boneLocalPoint);

			float totalWeight = 0.0f; //Total weight used to normalize all weights, so all weights together add up to 1.0f.

			if (addPointAtEditorPos) { //Apply function in Editor when bool is true.
				AddPointAtEditorPos ();
				addPointAtEditorPos = false;
			}

			if (reactionPoints != null) {
				for (int n = 0; n < reactionPoints.Length; n++) {
					//Store local position and rotation in Editor when bool is true.
					if (reactionPoints [n].storeCurrentTransform) {
						reactionPoints [n].localPositionSet = transform.localPosition;
						reactionPoints [n].localRotationSet = transform.localRotation.eulerAngles;
						reactionPoints [n].storeCurrentTransform = false;
					}

					//Apply point's local position and rotation when applyOnEditor bool is true.
					if (reactionPoints [n].applyOnEditor) {
						if (updateWeights) {
							Debug.Log ("TransformReactor.cs : Disable 'updateWeights' boolean to apply this point's local position and rotation.");
						}
						transform.localPosition = reactionPoints [n].localPositionSet;
						transform.localRotation = Quaternion.Euler (reactionPoints [n].localRotationSet);
						reactionPoints [n].applyOnEditor = false;
					}

					//Set each reaction point's weight, which then will be use to interpolate their position and rotation.
					if (updateWeights) {
						Vector3 reactionPointPos = reactionPointRef.TransformPoint (reactionPoints [n].localPoint); 
						float dist = Vector3.Distance (reactionPointPos, masterBonePoint);
						dist = Mathf.Pow (dist, distCurve);
						float range = reactionPoints [n].range * rangeMultiplier;
						reactionPoints [n].weight = Mathf.Max (0, (range - dist)) / range;
						reactionPoints [n].weight *= reactionPoints [n].weight;// * reactionPoints [n].weight;

						totalWeight += reactionPoints [n].weight;
					}
				}
			}
				
			if (updateTransform && reactionPoints != null) {
				//Divide every point's weight by the total combined weight, so all weights add up to 1.0 so there's a proper lerp between different weights.
				for (int n = 0; n < reactionPoints.Length; n++) {
					if (totalWeight != 0.0) {
						reactionPoints [n].weight /= totalWeight; 
					}
				}
					
				float currentWeight = 0.0f;
				bool firstPoint = true;

				//If for some reason there is only one reaction point, don't interpolate, but use its position and rotation directly.
				if (reactionPoints.Length == 1) {
					if (affectPosition) {
						transform.localPosition = reactionPoints [0].localPositionSet;
					}
					if (affectRotation) {
						transform.localRotation = Quaternion.Euler (reactionPoints [0].localRotationSet);
					}			
				} else {
					//Otherwise, if there are multiple reaction points (as it should) interpolate nomrally.
					for (int n = 0; n < reactionPoints.Length - 1; n++) {
						if (reactionPoints [n].weight == 0 && reactionPoints [n + 1].weight == 0) { //Do not interpolate if both current and next point are out of range (weight is zero)
							continue;
						}

						//Set first point's local position and rotation as starting place, then start interpolating with remaining points using corresponding weights.
						if (firstPoint) {
							firstPoint = false;

							if (affectPosition) {
								transform.localPosition = reactionPoints [n].localPositionSet;
							}
							if (affectRotation) {
								transform.localRotation = Quaternion.Euler (reactionPoints [n].localRotationSet);
							}
						}

						//Calculate lerp towards next point using proportion between weights.
						currentWeight += reactionPoints [n].weight; //To get a correct weight proportion for multiple points, sum each weight in order to compare with next point's weight.
						float lerp = reactionPoints [n + 1].weight / (reactionPoints [n + 1].weight + currentWeight); 


						if (affectPosition) {
							Vector3 newPos = Vector3.Lerp (transform.localPosition, reactionPoints [n + 1].localPositionSet, lerp);
							if (!float.IsNaN (newPos.x) && !float.IsNaN (newPos.y) && !float.IsNaN (newPos.z)) {
								transform.localPosition = newPos;
							}
						}
						if (affectRotation) {
							Vector3 newRot = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (reactionPoints [n + 1].localRotationSet), lerp).eulerAngles;
							if (!float.IsNaN(newRot.x) && !float.IsNaN(newRot.y) && !float.IsNaN(newRot.z)){
								transform.localRotation = Quaternion.Euler(newRot);
							}
						}

						currentWeight += reactionPoints [n].weight;
					}
				}

				//Add rotation and position if tweak is needed.
				transform.Translate(addPosition, Space.Self);
				transform.Rotate(addRotation, Space.Self);
			}
		}



		void OnDrawGizmos(){
			#if UNITY_EDITOR
			if(debug){
				if(masterBone != null){
					Vector3 debugPosBone = masterBone.TransformPoint(boneLocalPoint);
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(debugPosBone, debugSize);
					Debug.DrawLine(debugPosBone, masterBone.position);
					//#if UNITY_EDITOR
					//Handles.Label(debugPosBone, "Master Bone : " + masterBone.name);
					//Handles.Label(reactionPointRef.position, "Reaction Points Reference Bone : " + reactionPointRef.name);
					//#endif

					if( reactionPoints != null){
						for(int n = 0; n < reactionPoints.Length; n++){
							if(reactionPointRef != null){
								Vector3 debugPos = reactionPointRef.TransformPoint(reactionPoints[n].localPoint);
								float dist =  Vector3.Distance(debugPos, debugPosBone) * 2.0f;
								Color weightColor = new Color(1f - dist, 0.0f, dist);
								Gizmos.color = weightColor;
								Gizmos.DrawSphere(debugPos, debugSize * (1+reactionPoints[n].weight));


								Debug.DrawLine(debugPos, debugPosBone, weightColor);
								//Debug.DrawLine(debugPos, reactionPointRef.position);
								if(labelColor == null){
									labelColor = new GUIStyle();
								}
								labelColor.normal.textColor = weightColor;
								labelColor.fontSize = Mathf.RoundToInt(10 + Mathf.Min(15,30 * reactionPoints[n].weight));
								Handles.Label(debugPos, "Reaction Point : " + n.ToString() + " - Weight: " + reactionPoints[n].weight, labelColor);
								if(reactionPoints[n].showRange){
									Handles.color = Color.red;
									Handles.DrawWireDisc(debugPos, SceneView.lastActiveSceneView.camera.transform.forward, reactionPoints[n].range);
								}
							}
						}
					}
				}
			}
			#endif
		}

	}
}

 
