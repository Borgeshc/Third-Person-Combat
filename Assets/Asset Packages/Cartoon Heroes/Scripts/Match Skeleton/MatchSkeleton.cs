using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CartoonHeroes{
	public class MatchSkeleton : MonoBehaviour {

		public Transform masterRoot;
		public bool useParent = true;
		public bool waitForFindSetParent = true;
		public Transform slaveRoot;
		public bool slaveRootIsThisObject = true;

		[Space(20)]

		public bool copyPosition = true;
		public bool copyRotation = true;
		public bool copyScale = true;

		Transform[] masterBones;
		Transform[] slaveBones;

		[Space(20)]
		public HighHeelsFix highHeelsFix;

		[Space(20)]
		public bool destroyIfNoSkinnedMesh = true;
		SkinnedMeshRenderer skinnedMesh;

		// Use this for initialization
		void Start () {
			Init ();

			if (Application.isPlaying) {
				if (destroyIfNoSkinnedMesh) {
					skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer> ();
				}
			}
		}
		
		// Update is called once per frame
		void LateUpdate () {
			if (Application.isPlaying && destroyIfNoSkinnedMesh) {
				if (skinnedMesh == null) {
					Destroy (gameObject);
				}
			}

			if (highHeelsFix != null) {
				highHeelsFix.ApplyFix ();
			}

			if (masterBones != null && slaveBones != null) {
				for (int i = 0; i < masterBones.Length; i++) {
					if (slaveBones [i] == null || masterBones [i] == null) {
						continue;
					}

					if (copyPosition) {
						slaveRoot.position = masterRoot.position;
						slaveBones [i].position = masterBones [i].position;
					}
					if (copyRotation) {
						slaveBones [i].rotation = masterBones [i].rotation;
					}
					if (copyScale) {
						slaveBones [i].localScale = masterBones [i].localScale;
					}
				}
			}
		}

		public void Init(){
			if (slaveRootIsThisObject && slaveRoot == null) {
				slaveRoot = transform;
			}

			if (waitForFindSetParent) {
				FindSetParent findSetParent = GetComponent<FindSetParent> ();
				if (findSetParent != null) {
					findSetParent.FindAndSetParent ();
				}
			}

			if (useParent && transform.parent != null) {
				masterRoot = transform.parent;
			}

			if (masterRoot != null && slaveRoot != null) {
				List<Transform> foundMatches = new List<Transform> ();
				Transform[] allSlaveChildren = slaveRoot.GetComponentsInChildren<Transform> ();
				for (int i = 0; i < allSlaveChildren.Length; i++) {
					if (allSlaveChildren [i] == slaveRoot) {
						continue;
					}

					string path = allSlaveChildren [i].name;

					Transform check = allSlaveChildren [i];

					while (check.parent != slaveRoot) {
						check = check.parent;
						path = check.name + "/" + path;
					}

					Transform masterBone = masterRoot.Find (path);

					if (masterBone != null ) {
						foundMatches.Add (masterBone);
						foundMatches.Add (allSlaveChildren [i]);
					}
				}

				masterBones = new Transform[foundMatches.Count / 2];
				slaveBones = new Transform[foundMatches.Count / 2];
				for (int i = 0; i < foundMatches.Count; i+=2) {
					masterBones [i/2] = foundMatches [i];
					slaveBones [i/2] = foundMatches [i + 1];
				}

				//High Heels fix
				masterRoot.GetComponentInChildren<HighHeelsFix> ();
			}
		}
	}
}
