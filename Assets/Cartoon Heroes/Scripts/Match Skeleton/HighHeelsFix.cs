using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CartoonHeroes{
	public class HighHeelsFix : MonoBehaviour {


		public Transform characterRoot;
		public bool characterRoot_UseFindSetParent = true;

		[Space(20)]
		public Transform skeletonRoot;
		public bool skeletonRoot_UseCharacterLargestHierarchy = true;

		public Vector3 skeletonRoot_DefaultLocalPosition;

		[Space(10)]
		public string footSearchWord = "Foot";

		public Transform leftFoot;
		public Vector3 leftFoot_DefaultLocalRotation;
		public Transform rightFoot;
		public Vector3 rightFoot_DefaultLocalRotation;

		[Space(20)]
		public float heelHeight;
		public float feetLength = .25f;
		public Vector3 footAxis;

		[Space(20)]
		public bool storedDefaultRotations = false;

		[Space(20)]
		public bool fixedThisFrame;

		// Use this for initialization
		void Start () {
			if (characterRoot_UseFindSetParent) {
				FindSetParent findSetParent = GetComponent<FindSetParent> ();
				if (findSetParent != null) {
					if (!findSetParent.foundParent) {
						findSetParent.FindAndSetParent ();
					}
				}

				if (transform.parent != null) {
					characterRoot = transform.parent;
				}
			}

			FindBones ();
		}

		void Update(){
			fixedThisFrame = false;

			FindBones ();

			if (skeletonRoot != null && rightFoot != null && leftFoot != null) {
				//Store default rotations
				if (!storedDefaultRotations) {
					skeletonRoot_DefaultLocalPosition = skeletonRoot.localPosition;
					leftFoot_DefaultLocalRotation = leftFoot.localRotation.eulerAngles;
					rightFoot_DefaultLocalRotation = rightFoot.localRotation.eulerAngles;
					storedDefaultRotations = true;
				}

				skeletonRoot.localPosition = skeletonRoot_DefaultLocalPosition;
				leftFoot.localRotation = Quaternion.Euler (leftFoot_DefaultLocalRotation);
				rightFoot.localRotation = Quaternion.Euler (rightFoot_DefaultLocalRotation);
			}
		}

		void LateUpdate () {
			ApplyFix ();
		}

		public void ApplyFix(){
			if (!fixedThisFrame && skeletonRoot != null && rightFoot != null && leftFoot != null) {

				//Elevate character the hight of the heels.
				skeletonRoot.position = skeletonRoot.position + characterRoot.up * heelHeight;

				//Drop feet so toes touch the ground.
				float angle = Mathf.Atan (heelHeight / feetLength) * Mathf.Rad2Deg;
				leftFoot.RotateAround (leftFoot.position, leftFoot.TransformDirection(footAxis) , angle);
				rightFoot.RotateAround (rightFoot.position, rightFoot.TransformDirection(footAxis), angle);
			}

			fixedThisFrame = true;
		}

		void FindBones(){
			if (characterRoot != null) {
				if (skeletonRoot_UseCharacterLargestHierarchy) {
					Transform largestHierarchyChild = null;
					int largestHierarchySize = 0;

					for (int i = 0; i < characterRoot.childCount; i++) {
						Transform thisChild = characterRoot.GetChild (i);
						Transform[] thisChild_Children = thisChild.GetComponentsInChildren<Transform>();
						int thisHierarchySize = thisChild_Children.Length;

						if (largestHierarchyChild == null) {
							largestHierarchyChild = thisChild;
							largestHierarchySize = thisHierarchySize;
						} else {
							if (thisHierarchySize > largestHierarchySize) {
								largestHierarchyChild = thisChild;
								largestHierarchySize = thisHierarchySize;
							}
						}
					}

					skeletonRoot = largestHierarchyChild;
				}

				if (skeletonRoot != null) {
					Transform[] skeletonHierarchy = skeletonRoot.GetComponentsInChildren<Transform> ();
					for (int i = 0; i < skeletonHierarchy.Length; i++) {
						Transform thisBone = skeletonHierarchy [i];
						if (thisBone.name.ToLower ().Contains (footSearchWord.ToLower ())) {
							if (rightFoot == null) {
								rightFoot = thisBone;
							} else {
								Vector3 characterFootLocalPos_A = characterRoot.InverseTransformPoint (rightFoot.position);
								Vector3 characterFootLocalPos_B = characterRoot.InverseTransformPoint (thisBone.position);
								if (characterFootLocalPos_A.x > characterFootLocalPos_B.x) {
									leftFoot = thisBone;
								} else {
									leftFoot = rightFoot;
									rightFoot = thisBone;
								}
								break;
							}
						}
					}
				}

				MatchSkeleton[] matchSkeletons = characterRoot.GetComponentsInChildren<MatchSkeleton> ();
				for (int i = 0; i < matchSkeletons.Length; i++) {
					matchSkeletons [i].highHeelsFix = this;
				}
			}
		}

		void OnDrawGizmosSelected(){
			#if UNITY_EDITOR
			Gizmos.color = Color.red;
			if(leftFoot != null){
				Gizmos.DrawLine(leftFoot.position, leftFoot.position + leftFoot.TransformDirection(footAxis));
			}
			if(rightFoot != null){
				Gizmos.DrawLine(rightFoot.position, rightFoot.position + rightFoot.TransformDirection(footAxis));
			}
			#endif
		}
	}
}