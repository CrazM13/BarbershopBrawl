using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain : MonoBehaviour {

	[SerializeField] private Transform connectionStart;
	[SerializeField] private Transform connectionEnd;


	[SerializeField] private float chainScale = 1;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (!connectionEnd || !connectionStart) return;

		transform.position = (connectionEnd.position + connectionStart.position) / 2;
		transform.localScale = new Vector3(chainScale, chainScale, Vector3.Distance(connectionStart.position, connectionEnd.position));
		transform.rotation = Quaternion.LookRotation(connectionEnd.position - connectionStart.position, Vector3.up);
	}
}
