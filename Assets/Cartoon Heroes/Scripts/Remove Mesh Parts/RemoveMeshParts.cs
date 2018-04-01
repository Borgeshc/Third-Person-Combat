using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable 0414

namespace CartoonHeroes{
	[ExecuteInEditMode]
	public class RemoveMeshParts : MonoBehaviour {

		public SkinnedMeshRenderer skinnedMesh;
		public MeshFilter meshFilter;
		public Mesh sourceMesh;

		public enum InitState{None, Triangles, TriangleGroups}

		public InitState initState;

		public float initStartTime;
		public float lastInitDuration;

		const int maxTrianglesPerLoop = 5000;
		public float maxTimePerLoop = 100.0f;

		public enum Mode { Slow, Fast }
		public Mode mode; //Fast or slow mode. Sacrifice accuracy to calculate triangles faster.

		public int currentTriangle;
		public int currentProgress;
		public int progressTotal;

		public Quaternion initRotation;

		public Group[] groups;
		public TriangleGroup[] triangleGroups;

		List<Triangle> triangles;

		List<int> unasignedTriangles_List;
		public int[] unasignedTriangles;

		public bool initRequried = true;
		public int init_groupCount; //Store how many Groups were used at initialization. (If changed, set true initRequired)
		public SkinnedMeshRenderer init_SkinnedMesh; //Store SkinnedMeshRenderer at initialization. (If changed, set true initRequired)
		public MeshFilter init_MeshFilter;
		public Mesh init_SourceMesh;

		public bool updateVisibility;

		[Space(20)]

		public bool debug = true;

		[Space(20)]

		public bool groupsFoldout_CustomEditor = true;
		public bool triangleGroupsFoldout_CustomEditor = true;
		public bool customEditor_SourceMeshFoldout;


		[System.Serializable]
		public class Group{
			public string name;
			public Bounds[] bounds;
			public Color drawColor;
			public bool remove;

			[Space(20)]

			public bool restoreVisibilityIfNoAssignedTransforms;
			public List<Transform> assignedTranforms;

			[Space(20)]

			public Bounds[] init_Bounds; //Store bounds at initialization. (If changed, set true initRequired)

			[Space(20)]

			public bool boundsFoldout_CustomEditor;

			public void StoreInitBounds(){
				init_Bounds = new Bounds[bounds.Length];
				bounds.CopyTo(init_Bounds,0);
			}

			public bool BoundsChangedAfterInit(){
				if (init_Bounds == null || bounds == null) {
					return true;
				}

				if (bounds.Length != init_Bounds.Length) {
					return true;
				}

				for(int i = 0; i < bounds.Length; i++){
					if (bounds [i].center != init_Bounds [i].center || bounds[i].size != init_Bounds[i].size) {
						return true;
					}
				}

				return false;
			}

			public bool Contains(Vector3 point){
				for (int i = 0; i < bounds.Length; i++) {
					if (bounds [i].Contains (point)) {
						return true;
					}
				}
				return false;
			}

			public void AddBound(){
				if (bounds == null) {
					bounds = new Bounds[0];
				}

				Bounds[] newBounds = new Bounds[bounds.Length + 1];
				bounds.CopyTo (newBounds, 0);
				newBounds [newBounds.Length - 1].size = new Vector3 (.5f, .5f, .5f);

				bounds = newBounds;
			}

			public void DeleteBounds(int removeIndex){
				if (removeIndex < 0 || removeIndex >= bounds.Length) {
					return;
				}
				Bounds[] newBounds = new Bounds[bounds.Length - 1];
				int currentIndex = 0;
				for (int i = 0; i < bounds.Length; i++) {
					if (i == removeIndex) {
						continue;
					}

					newBounds [currentIndex] = bounds [i];
					currentIndex++;
				}
				bounds = newBounds;
			}

			public void DuplicateBounds(int index){
				if (index < 0 || index >= bounds.Length) {
					return;
				}

				Bounds[] newBounds = new Bounds[bounds.Length + 1];

				for (int i = 0; i <= index; i++){
					newBounds [i] = bounds [i];
				}

				newBounds [index + 1] = bounds [index];

				for (int i = index + 1; i < bounds.Length; i++) {
					newBounds [i + 1] = bounds [i];
				}
				bounds = newBounds;
			}
		}

		[System.Serializable]
		public class TriangleGroup{
			public int[] groups;

			public int[] triangles;

			public TriangleGroup(){
				groups = new int[0];
				triangles = new int[0];
			}

			public void AddTriangle (int vertex1, int vertex2, int vertex3){
				int[] newTriangles = new int[triangles.Length + 3];
				triangles.CopyTo (newTriangles, 0);
				newTriangles [newTriangles.Length - 3] = vertex1;
				newTriangles [newTriangles.Length - 2] = vertex2;
				newTriangles [newTriangles.Length - 1] = vertex3;
				triangles = newTriangles;
			}

			public bool HasGroup(int index){
				for (int i = 0; i < groups.Length; i++) {
					if (groups [i] == index) {
						return true;
					}
				}
				return false;
			}
		}

		class Triangle{
			public List<int> groups;

			public Triangle(){
				groups = new List<int>();	
			}

			public bool HasGroup(int index){
				for (int i = 0; i < groups.Count; i++) {
					if (groups [i] == index) {
						return true;
					}
				}
				return false;
			}

			public bool GroupArrayMatch(int[] groupArray){
				if (groups.Count != groupArray.Length) {
					return false;
				}

				for (int i = 0; i < groupArray.Length; i++) {
					if (!HasGroup (groupArray [i])) {
						return false;
					}
				}

				return true;
			}
		}

		public void NewGroup(){
			if (groups == null) {
				groups = new Group[0];
			}

			Group[] newGroups = new Group [groups.Length + 1];
			groups.CopyTo (newGroups, 0);

			Group newGroup = new Group ();
			newGroup.name = "Mesh Part " + newGroups.Length.ToString();
			newGroup.AddBound ();
			newGroup.drawColor = Color.white;

			newGroups [newGroups.Length - 1] = newGroup;

			groups = newGroups;
		}

		public void DeleteGroup(int index){
			if (index < 0 || index >= groups.Length) {
				return;
			}

			Group[] newGroups = new Group[groups.Length - 1];

			int currentIndex = 0;
			for (int i = 0; i < groups.Length; i++) {
				if (i == index) {
					continue;
				}

				newGroups [currentIndex] = groups [i];
				currentIndex++;
			}

			groups = newGroups;
		}

		public void Init(){
			if (skinnedMesh != null || meshFilter != null) {
				if (sourceMesh == null) {
					if (skinnedMesh != null) {
						sourceMesh = skinnedMesh.sharedMesh;
					}
					if (meshFilter != null) {
						sourceMesh = meshFilter.sharedMesh;
					}
				}

				initStartTime = Time.realtimeSinceStartup;
				if (!Application.isPlaying) {
					#if UNITY_EDITOR
					initStartTime = (float)EditorApplication.timeSinceStartup;
					#endif
				}

				initState = InitState.Triangles;
				currentTriangle = -3;
				currentProgress = 0;

				if (skinnedMesh != null) {
					initRotation = skinnedMesh.transform.rotation;
					progressTotal = (skinnedMesh.sharedMesh.triangles.Length / 3) * 2;
				}
				if (meshFilter != null) {
					initRotation = meshFilter.transform.rotation;
					progressTotal = (meshFilter.sharedMesh.triangles.Length / 3) * 2;
				}

				triangles = new List<Triangle> ();

				unasignedTriangles_List = new List<int> ();

				#if UNITY_EDITOR
				PrefabUtility.DisconnectPrefabInstance (gameObject);
				#endif

				//Store Bounds at initialization. If they change, set initRequired true.
				initRequried = true;
				for (int i = 0; i < groups.Length; i++) {
					groups [i].StoreInitBounds ();
				}
				init_SkinnedMesh = skinnedMesh;
				init_MeshFilter = meshFilter;
				init_SourceMesh = sourceMesh;
				init_groupCount = groups.Length;
			}
		}

		void Init_Triangles(){
			if (skinnedMesh != null || meshFilter != null) {
				int trianglesLength = 0;

				Transform meshTransform = null;
				Mesh sharedMesh = null;

				if (skinnedMesh != null) {
					skinnedMesh.transform.rotation = initRotation;
					trianglesLength = skinnedMesh.sharedMesh.triangles.Length;
					meshTransform = skinnedMesh.transform;
					sharedMesh = skinnedMesh.sharedMesh;
				}
				if (meshFilter != null) {
					meshFilter.transform.rotation = initRotation;
					trianglesLength = meshFilter.sharedMesh.triangles.Length;
					meshTransform = meshFilter.transform;
					sharedMesh = meshFilter.sharedMesh;
				}

				Quaternion currentRotation = meshTransform.rotation;

				//Store starting time of the loop, to break loop if it reaches time limit.
				float loopStartTime = Time.realtimeSinceStartup;
				if (!Application.isPlaying) {
					#if UNITY_EDITOR
					loopStartTime = (float)EditorApplication.timeSinceStartup;
					#endif
				}

				int startIndex = currentTriangle + 3;
				for (int i = startIndex; i < startIndex + maxTrianglesPerLoop; i += 3){
					//Break loop. (Reached end)
					if(i+2 >= trianglesLength){
						initState = InitState.TriangleGroups;
						currentTriangle = -1;

						unasignedTriangles = unasignedTriangles_List.ToArray ();
						unasignedTriangles_List.Clear ();


						triangleGroups = new TriangleGroup[0];

						break;
					}

					//Break Loop. (Time limit)
					if(Application.isPlaying && Time.realtimeSinceStartup > loopStartTime + maxTimePerLoop *  0.001f){
						break;
					}
					#if UNITY_EDITOR
					if (!Application.isPlaying && (float)EditorApplication.timeSinceStartup > loopStartTime + maxTimePerLoop * 0.001f) {
						break;
					}
					#endif

					currentTriangle = i;
					currentProgress++;

					//Create a Triangle class instance for this triagnles
					Triangle thisTriangle = new Triangle ();
					triangles.Add (thisTriangle);



					//Get the vertexs' IDs for this triangle
					int vertex1_index = sharedMesh.triangles [i];
					int vertex2_index = sharedMesh.triangles [i + 1];
					int vertex3_index = sharedMesh.triangles [i + 2];


					bool triangleAssigned = false; //Store if the triangle is assigned in at least one group. (If not, store in unassigned array)

					//Loop groups. See which group contains this triangle's vertices. Add the groups to the Triangle.
					for(int n = 0; n < groups.Length; n++){
						Vector3 vertex1_GlobalPos = meshTransform.localToWorldMatrix.MultiplyPoint3x4 (sharedMesh.vertices [vertex1_index]);
						Vector3 vertex2_GlobalPos = Vector3.zero;
						Vector3 vertex3_GlobalPos = Vector3.zero;

						if (mode == Mode.Slow) {
							vertex2_GlobalPos = meshTransform.localToWorldMatrix.MultiplyPoint3x4 (sharedMesh.vertices [vertex2_index]);
							vertex3_GlobalPos = meshTransform.localToWorldMatrix.MultiplyPoint3x4 (sharedMesh.vertices [vertex3_index]);						
						}

						//If all of this triangle's vertices fall in this group's bounds, add this bound to the Triangle class instance.
						if (groups [n].Contains (vertex1_GlobalPos - meshTransform.position)){
							if (mode == Mode.Fast) {
								thisTriangle.groups.Add (n);
								triangleAssigned = true;			
							}
							if(mode == Mode.Slow){
								if(groups [n].Contains (vertex2_GlobalPos - meshTransform.position) 
								&& groups [n].Contains (vertex3_GlobalPos - meshTransform.position)){
									thisTriangle.groups.Add (n);
									triangleAssigned = true;	
								}
							}
						}
					}

					if (!triangleAssigned) {
						unasignedTriangles_List.Add (sharedMesh.triangles [i]);
						unasignedTriangles_List.Add (sharedMesh.triangles [i+1]);
						unasignedTriangles_List.Add (sharedMesh.triangles [i+2]);
					}
				}

				meshTransform.rotation = currentRotation;
			}
		}

		void Init_TriangleGroups(){
			if (skinnedMesh != null || meshFilter != null) {
				//Store starting time of the loop, to break loop if it reaches time limit.
				float loopStartTime = Time.realtimeSinceStartup;
				if (!Application.isPlaying) {
					#if UNITY_EDITOR
					loopStartTime = (float)EditorApplication.timeSinceStartup;
					#endif
				}

				int startIndex = currentTriangle + 1;
				//Loop through all Triangles, and store each triangle in TriangleGroups with the same group array as the each Triangle.
				for (int i = startIndex; i < startIndex + maxTrianglesPerLoop; i++) {

					//Break loop. (Reached end)
					if (i >= triangles.Count) {
						initState = InitState.None;
						currentProgress = 0;
						currentTriangle = 0;
						triangles.Clear ();
						lastInitDuration = Time.realtimeSinceStartup - initStartTime;
						initRequried = false;
						if (!Application.isPlaying) {
							#if UNITY_EDITOR
							lastInitDuration = (float)EditorApplication.timeSinceStartup - initStartTime;
							#endif
						}
						break;
					}

					//Break Loop. (Time limit)
					if(Application.isPlaying && Time.realtimeSinceStartup > loopStartTime + maxTimePerLoop *  0.001f){
						break;
					}
					#if UNITY_EDITOR
					if (!Application.isPlaying && (float)EditorApplication.timeSinceStartup > loopStartTime + maxTimePerLoop * 0.001f) {
						break;
					}
					#endif


					currentTriangle = i;
					currentProgress++;

					//If triangle is unassigned, don't create a TriangleGroup for it.
					if (triangles [i].groups.Count == 0) {
						continue;
					}

					Mesh sharedMesh = null;
					if (skinnedMesh != null) {
						sharedMesh = skinnedMesh.sharedMesh;
					}
					if (meshFilter != null) {
						sharedMesh = meshFilter.sharedMesh;
					}

					int vertex1 = sharedMesh.triangles [i * 3];
					int vertex2 = sharedMesh.triangles [(i * 3) + 1];
					int vertex3 = sharedMesh.triangles [(i * 3) + 2];

					bool foundTriangleGroup = false;

					//Loop through all exiting triangle groups, to see if a TriangleGroup already exists with the same group array.
					for (int n = 0; n < triangleGroups.Length; n++) {
						if (triangles [i].GroupArrayMatch (triangleGroups [n].groups)) {
							//Add this triangle to this triangle group.
							triangleGroups[n].AddTriangle(vertex1, vertex2, vertex3);
							foundTriangleGroup = true;
							break;
						}
					}

					if (!foundTriangleGroup) {
						//If there wasn't a suitable Triangle Group for this triangle, create it and add triangle to it.
						TriangleGroup newTriangleGroup = new TriangleGroup();
						newTriangleGroup.groups = triangles [i].groups.ToArray();
						newTriangleGroup.AddTriangle (vertex1, vertex2, vertex3);
						AddTriangleGroup (newTriangleGroup);
					}
				}
			}
		}

		void AddTriangleGroup(TriangleGroup newTriangleGroup){
			if (triangleGroups == null) {
				triangleGroups = new TriangleGroup[0];
			}

			TriangleGroup[] newTriangleGroups = new TriangleGroup[triangleGroups.Length + 1];

			triangleGroups.CopyTo (newTriangleGroups, 0);
			newTriangleGroups [newTriangleGroups.Length - 1] = newTriangleGroup;
			triangleGroups = newTriangleGroups;
		}

		public void Abort(){
			#if UNITY_EDITOR
			PrefabUtility.DisconnectPrefabInstance (gameObject);
			#endif
			initState = InitState.None;
			currentProgress = 0;
			currentTriangle = 0;
			if (triangles != null) {
				triangles.Clear ();
			}
		}

		public void UpdateVisibility(){
			if (skinnedMesh != null || meshFilter != null) {
				int visibleTrianglesLength = unasignedTriangles.Length;

				for (int i = 0; i < triangleGroups.Length; i++) {
					if (IsTriangleGroupVisible (i)) {
						visibleTrianglesLength += triangleGroups [i].triangles.Length;
					}
				}

				int[] visibleTriangles = new int[visibleTrianglesLength];
				unasignedTriangles.CopyTo (visibleTriangles, 0);

				int currentIndex = unasignedTriangles.Length;

				for (int i = 0; i < triangleGroups.Length; i++) {
					if (IsTriangleGroupVisible (i)) {
						triangleGroups [i].triangles.CopyTo (visibleTriangles, currentIndex);
						currentIndex += triangleGroups [i].triangles.Length;
					}
				}

				SetTriangles (visibleTriangles);
			}
		}

		public void DebugUnasignedTriangles(){
			SetTriangles (unasignedTriangles);
		}

		public void DebugTriangleGroup(int index){
			SetTriangles (triangleGroups [index].triangles);
		}

		void SetTriangles(int[] newTriangles){
			if((skinnedMesh != null || meshFilter != null) && sourceMesh != null){
				Mesh currentMesh = null;

				if (skinnedMesh != null) {
					currentMesh = skinnedMesh.sharedMesh;
					skinnedMesh.sharedMesh = Instantiate (skinnedMesh.sharedMesh);
					skinnedMesh.sharedMesh.name = sourceMesh.name + " (RemoveMeshPars)";
					skinnedMesh.sharedMesh.triangles = newTriangles;
				}
				if(meshFilter != null){
					currentMesh = meshFilter.sharedMesh;
					meshFilter.sharedMesh = Instantiate (meshFilter.sharedMesh);
					meshFilter.sharedMesh.name = sourceMesh.name + " (RemoveMeshPars)";
					meshFilter.sharedMesh.triangles = newTriangles;
				}

				if (currentMesh != sourceMesh) {
					DestroyImmediate (currentMesh);
				}

			}
		}

		public bool IsTriangleGroupVisible(int index){
			if (index < 0 || index >= triangleGroups.Length) {
				return false;
			}
			for (int i = 0; i < triangleGroups[index].groups.Length; i++) {
				int groupIndex = triangleGroups [index].groups [i];
				if (groups [groupIndex].remove) {
					return false;
				}
			}
			return true;
		}

		public void RestoreMesh(){
			if (sourceMesh != null) {
				if (skinnedMesh != null) {
					skinnedMesh.sharedMesh = sourceMesh;
				}
				if (meshFilter != null) {
					meshFilter.sharedMesh = sourceMesh;
				}
			}
		}

		public void MeshPartRemove_Set (string partName, bool set){
			Group group = GetGroupByName (partName);
			if (group != null) {
				group.remove = set;
			}
		}

		public void MeshPartRemove_Set (string partName, Transform assignTransform, bool set){
			Group group = GetGroupByName (partName);
			if (group != null) {
				group.remove = set;
				group.assignedTranforms.Add (assignTransform);
				group.restoreVisibilityIfNoAssignedTransforms = true;
			}
		}

		public Group GetGroupByName(string partName){
			for (int i = 0; i < groups.Length; i++) {
				if (groups [i].name == partName) {
					return groups[i];
				}
			}
			return null;
		}

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			if (initState == InitState.Triangles) {
				Init_Triangles ();
			}

			if (initState == InitState.TriangleGroups) {
				Init_TriangleGroups ();
			}

			//If init required is set to false after initialization, check if items have been modified to reset the parameter.
			if (initState == InitState.None) {
				Update_InitRequired ();
			}

			if (groups != null) {
				for (int i = 0; i < groups.Length; i++) {
					if (groups [i].assignedTranforms == null) {
						continue;
					}
					//On editor no have no assigned transforms.
					if (!Application.isPlaying) {	
						if (groups [i].assignedTranforms.Count > 0) {
							groups [i].assignedTranforms.Clear ();
						}
					} else {
						if (groups [i].restoreVisibilityIfNoAssignedTransforms) {
							for (int n = groups [i].assignedTranforms.Count - 1; n >= 0; n--) {
								if (groups [i].assignedTranforms [n] == null) {
									groups [i].assignedTranforms.RemoveAt (n);
								}
							}
							if (groups [i].assignedTranforms.Count == 0) {
								groups [i].remove = false;
								UpdateVisibility ();
								groups [i].restoreVisibilityIfNoAssignedTransforms = false;
							}
						}
					}
				}
			}
		}

		void LateUpdate(){
			if (updateVisibility) {
				UpdateVisibility ();
				updateVisibility = false;
			}
		}

		void Update_InitRequired(){
			if (skinnedMesh != null && skinnedMesh != init_SkinnedMesh) {
				initRequried = true;
				return;
			}

			if (meshFilter != null && meshFilter != init_MeshFilter) {
				initRequried = true;
				return;
			}

			if (sourceMesh != init_SourceMesh) {
				initRequried = true;
				return;
			}

			if (groups == null || groups.Length == 0) {
				initRequried = true;
				return;			
			}

			if (groups.Length != init_groupCount) {
				initRequried = true;
				return;
			}

			for (int i = 0; i < groups.Length; i++) {
				if (groups [i].BoundsChangedAfterInit ()) {
					initRequried = true;
					return;
				}
			}

			initRequried = false;
		}

		#if UNITY_EDITOR
		void OnDrawGizmosSelected(){
			if (debug) {
				if (skinnedMesh != null || meshFilter != null) {
					if (groups == null) {
						groups = new Group[0];
					}

					for (int i = 0; i < groups.Length; i++) {
						for (int n = 0; n < groups [i].bounds.Length; n++) {
							Gizmos.color = (groups [i].drawColor);
							if (skinnedMesh != null) {
								Gizmos.DrawWireCube (groups [i].bounds [n].center + skinnedMesh.transform.position, groups [i].bounds [n].size);
							}

							if (meshFilter != null) {
								Gizmos.DrawWireCube (groups [i].bounds [n].center + meshFilter.transform.position, groups [i].bounds [n].size);
							}
						}
					}
				}
								
			}
		}
		#endif
	}
}
