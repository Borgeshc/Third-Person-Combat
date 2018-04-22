using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CartoonHeroes{
	[ExecuteInEditMode]
	public class RemoveMeshParts_Set : MonoBehaviour {

		public string[] removeMeshPartNameList;

		public int getComponent_FramesLimit = 4; //Amount of frames to use GetComponentInChildren to attempt to find 
		public int getComponent_FramesLeft;

		public RemoveMeshParts[] removeMeshParts;

		public bool applyASAP = true;

		public bool findRemoveMeshPartsOnStart = true;

		// Use this for initialization
		void Start () {
			if (findRemoveMeshPartsOnStart && Application.isPlaying) {
				BeginGetComponent ();
			}
		}
		
		// Update is called once per frame
		void Update () {
			if (removeMeshParts == null || removeMeshParts.Length == 0) {
				if (getComponent_FramesLeft > 0) {
					getComponent_FramesLeft--;

					FindSetParent findSetParent = GetComponent<FindSetParent> ();
					if (findSetParent != null && !Application.isPlaying) {
						Transform currentParent = transform.parent;
						findSetParent.FindAndSetParent ();
						findSetParent.foundParent = false;
						removeMeshParts = transform.parent.GetComponentsInChildren<RemoveMeshParts> ();
						transform.parent = currentParent;
					} else {
						if (transform.parent != null) {
							removeMeshParts = transform.parent.GetComponentsInChildren<RemoveMeshParts> ();
						}				
					}
				}
			}

			if (Application.isPlaying) {
				if (removeMeshParts != null && applyASAP) {
					for (int n = 0; n < removeMeshParts.Length; n++) {
						for (int i = 0; i < removeMeshPartNameList.Length; i++) {
							removeMeshParts[n].MeshPartRemove_Set (removeMeshPartNameList [i], transform, true);
						}
						removeMeshParts[n].updateVisibility = true;
					}

					applyASAP = false;
				}
			}
		}

		public void BeginGetComponent(){
			removeMeshParts = new RemoveMeshParts[0];
			getComponent_FramesLeft = getComponent_FramesLimit;
		}

		public void AddMeshPart(){
			if (removeMeshPartNameList == null) {
				removeMeshPartNameList = new string[0];
			}

			string[] newList = new string[removeMeshPartNameList.Length + 1];
			removeMeshPartNameList.CopyTo (newList, 0);
			newList [newList.Length - 1] = "Type mesh part's name.";
			removeMeshPartNameList = newList;
		}

		public void DeleteMeshPart(int removeIndex){
			if (removeIndex < 0 || removeIndex >= removeMeshPartNameList.Length) {
				return;
			}
			string[] newList = new string[removeMeshPartNameList.Length - 1];

			int currentIndex = 0;
			for (int i = 0; i < removeMeshPartNameList.Length; i++) {
				if (i == removeIndex) {
					continue;
				}
				newList [currentIndex] = removeMeshPartNameList [i];
				currentIndex++;
			}

			removeMeshPartNameList = newList;
		}
	}
}

