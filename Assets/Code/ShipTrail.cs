//ShipTrail.cs
//Created on 22.04.16 by Aaron C Gaudette
//Orients a particle system to the parent

using UnityEngine;
[ExecuteInEditMode]

public class ShipTrail : MonoBehaviour{
	public float offset = 90;
	public bool flip = true;
	ParticleSystem system = null;
	void Update(){
		if(system==null)system=GetComponent<ParticleSystem>();
		system.startRotation=Mathf.Deg2Rad*((flip?-1:1)*transform.parent.eulerAngles.z+offset);
	}
}