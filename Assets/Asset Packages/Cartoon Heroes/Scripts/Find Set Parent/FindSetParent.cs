//Purpose: Automatically parent armor pieces to character. Finds closest GameObject in scene that contains string in name, and parents to that object. 
//Example: Breastplate searches for closest "Spine1" bone, and parents to it at "Start".
//String doesn't need to match exact name, just contain the string inside the name. Example "Spine1" will parent to "Male Spine1" or "Female Spine1", depending which one's closer.
//Limitation: Currently doesn't set a local position after parenting. So armor must already be placed in correct position before parenting.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CartoonHeroes{
	[ExecuteInEditMode]
	public class FindSetParent : MonoBehaviour {

		public enum HierarchyType {Higher, Lower}

		public bool findOnStart = true;
		public bool findNow;
		[Space (20)]
		public string parentName; //String used to find object.
		public string[] excludeWords;
		public bool exludeOwnHierarchy = true;
		public bool closest = true; //If this is set to false, will parent to first object found. If true, will loop through all objects, and take the closest one.
		public bool parentToRootObject = false;
		public bool use_SearchWord_RootObjet = false;
		public string searchWord_RootObjet;
		public HierarchyType hierarchyType = HierarchyType.Higher;
		[Space (20)]
		public bool setLocalPos = false;
		public bool setLocalRot = false;
		public Vector3 localPos;
		public Vector3 localRot;
		public bool storeCurrent;
		public bool storeCurrent_Parent;
		[Space (20)]
		public bool destroyScriptAfterParenting = true; //This script serves no purpose after parenting, so it can be destroyed.
		const float minDistance = 0.0001f;
		public bool foundParent = false; //As FindAndSetParent() can be called externally as well as locally, to avoid searching for parent more than once, this bool is set TRUE after parenting.
		//Once foundParent is set TRUE, it wont search again even if function is called.
		public string parentedTo;

		// Use this for initialization
		void Start () {
			if (Application.isPlaying) {
				if (findOnStart) {
					FindAndSetParent ();
				}
			}
		}
		
		// Update is called once per frame
		void Update () {
			if (storeCurrent) {
				storeCurrent = false;

				localPos = transform.localPosition;
				localRot = transform.localRotation.eulerAngles;
			}

			if (storeCurrent_Parent) {
				Transform currentParent = transform.parent;
				FindAndSetParent ();
				localPos = transform.localPosition;
				localRot = transform.localRotation.eulerAngles;
				transform.parent = currentParent;
				storeCurrent_Parent = false;
				foundParent = false;
			}

			if(findNow){
				FindAndSetParent ();
				if (!Application.isPlaying) {
					foundParent = false;
				}
				findNow = false;
			}
		}

		Transform HigherHierarchy(Transform obj1, Transform obj2){
			//Count parents for both objects, return the object with less parents (higher in hierarchy)
			int obj1Level = CountParents (obj1);
			int obj2Level = CountParents (obj2);

			if (obj1Level < obj2Level) {
				return obj1;
			}
			if (obj2Level < obj1Level) {
				return obj2;
			}

			return null; 
		}

		Transform LowerHierarchy(Transform obj1, Transform obj2){
			//Count parents for both objects, return the object with less parents (higher in hierarchy)
			int obj1Level = CountParents (obj1);
			int obj2Level = CountParents (obj2);

			if (obj1Level > obj2Level) {
				return obj1;
			}
			if (obj2Level > obj1Level) {
				return obj2;
			}

			return null; 
		}

		public int CountParents(Transform obj){
			int parentCount = 0;
			Transform currentObjectHierarchy = obj;

			while (currentObjectHierarchy.parent != null) {
				parentCount++;
				currentObjectHierarchy = currentObjectHierarchy.parent;
			}

			return parentCount;
		}

		public void AddExcludeWord(string newWord){
			if (excludeWords == null) {
				excludeWords = new string[0];
			}
			string[] newExcludeWords = new string[excludeWords.Length + 1];
			excludeWords.CopyTo (newExcludeWords, 0);
			newExcludeWords [newExcludeWords.Length - 1] = newWord;
			excludeWords = newExcludeWords;
		}

		public void RemoveExcludeWord(int removeIndex){
			if (removeIndex < 0 || removeIndex >= excludeWords.Length) {
				return;
			}

			string[] newExcludeWords = new string[excludeWords.Length - 1];
			int currentIndex = 0;
			for (int i = 0; i < excludeWords.Length; i++) {
				if (i == removeIndex) {
					continue;
				}
				newExcludeWords [currentIndex] = excludeWords [i];
				currentIndex++;
			}
			excludeWords = newExcludeWords;
		}

		public void FindAndSetParent(){
			if (!foundParent) { //Check if function already found a parent, to avoid searching more than once.
				Transform[] allTransforms;
				allTransforms = GameObject.FindObjectsOfType<Transform> ();

				Transform closestParent = null; //Variable used to store closest parent as looping through all scene objects. If global variable "closest" is set to false, it serves no purpose.
				float closestParentDistance = Mathf.Infinity; //Keeps track of current closest's parent distance (toward this object), as looping through all objects.

				for (int i = 0; i < allTransforms.Length; i++) { //Loop through all objects in scene.
					Transform thisTransform = allTransforms [i];
					if (thisTransform == transform) {
						continue;
					}

					if (use_SearchWord_RootObjet) {
						if (!thisTransform.root.name.ToLower ().Contains (searchWord_RootObjet.ToLower ())) {
							continue;
						}
					}

					if (exludeOwnHierarchy) {
						if(IsObjectInHierarchy(thisTransform, transform)){
							continue;
						}
					}

					bool exclude = false;
					if (excludeWords != null) {
						for (int n = 0; n < excludeWords.Length; n++) {
							if (thisTransform.name.ToLower ().Contains (excludeWords [n].ToLower ())) {
								exclude = true;
							}
						}
					}
					if (exclude) {
						continue;	
					}

					if (thisTransform.name.ToLower ().Contains (parentName.ToLower ()) && thisTransform != transform) { 
						//If an object's name constains "parentName", calculate distance. Or set as parent right away (if global variable "closest" is set to false).
					
						if (!closest) {
							//If global variable "closest" is false, set as parent and break loop.
							foundParent = true;
							transform.parent = thisTransform;
							break;
						} else {
							if (closestParent == null) {
								//If there's no current "closestParent", then set this object as the current closest parent.
								closestParent = thisTransform;
								//Store current closest parent's distance (to this object).
								closestParentDistance = Vector3.Distance (transform.position, closestParent.position); 
							} else {
								//If there's a closetParent stored, replace it only if this object is closer than current closest parent.
								float thisTransformDistance = Vector3.Distance (transform.position, thisTransform.position);

								//Distance is "the same". If distance is nearly the same, choose the higher hierarchy object.
								if (Mathf.Abs (thisTransformDistance - closestParentDistance) < minDistance) {
									if (hierarchyType == HierarchyType.Higher) {
										//If the distance is the same, go for higher hierarchy.
										closestParent = HigherHierarchy (thisTransform, closestParent);
										if (closestParent == null) {
											closestParent = thisTransform;
											closestParentDistance = thisTransformDistance;
										}
									}
									if (hierarchyType == HierarchyType.Lower) {
										//If the distance is the same, go for lower hierarchy.
										closestParent = LowerHierarchy (thisTransform, closestParent);
										if (closestParent == null) {
											closestParent = thisTransform;
											closestParentDistance = thisTransformDistance;
										}
										}
								} else {
									//Choose the closest object.
									if (thisTransformDistance < closestParentDistance) {
										closestParent = thisTransform;
										closestParentDistance = thisTransformDistance;
									}
								}
							}
						}
					}
				}

				if (closest && closestParent != null) {
					//if there's a closestParent found, set it as parent.
					if (parentToRootObject) {
						transform.parent = closestParent.root;
					} else {
						transform.parent = closestParent;
					}

					if (transform.parent != null) {
						foundParent = true;
						parentedTo = transform.parent.name;
					}

				}

				//If found parent, set local position and rotation if specified.
				if (Application.isPlaying && foundParent) {
					if (setLocalPos) {
						transform.localPosition = localPos;
					}
					if (setLocalRot) {
						transform.localRotation = Quaternion.Euler (localRot);
					}
				}

				//Log an alert if no parent was found.
				if (!foundParent) {
					Debug.Log (gameObject.name + "'s FindSetParent.cs didn't find any parent object with a name that contains \"" + parentName + "\"");
				}

				//This script serves no purpose after parenting. It can be destroyed afterwards.
				if (Application.isPlaying) {
					if (foundParent && destroyScriptAfterParenting) {
						Destroy (this);
					}
				}
			}
		}

		public bool IsObjectInHierarchy(Transform obj, Transform hierarchyRoot){
			Transform[] hierarchy = hierarchyRoot.GetComponentsInChildren<Transform> ();
			for (int i = 0; i < hierarchy.Length; i++) {
				if (obj == hierarchy [i]) {
					return 	true;
				}
			}
			return false;
		}
	}
}

