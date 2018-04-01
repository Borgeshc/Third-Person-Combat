//Purpsose: Used for armor prefab's root object. Armor parts are parented to a root object. Once each armor part has been automatically parented to a character (by FindSetParent.cs) the empty root object serves no purpose.
//Check every frame for children parented to this object. If no children found, destroy useless GameObject.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CartoonHeroes{
	public class DestroyIfNoChildren : MonoBehaviour {
		
		// Update is called once per frame
		void Update () {
			if (transform.childCount == 0) {
				//Check if any children parented to this object. Destroy if no children found.
				Destroy (gameObject);
			}
		}
	}
}
