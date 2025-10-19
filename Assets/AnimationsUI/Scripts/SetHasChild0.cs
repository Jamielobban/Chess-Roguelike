using UnityEngine;

public class SetHasChild0 : MonoBehaviour
{
	Animator anim;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		anim = GetComponentInParent<Animator>();

	}

	// Update is called once per frame
	void Update()
    {
        
    }

    public void SetHasFirstChild()
    {
        Transform parent = this.transform.parent;

		if (this.transform.parent.GetChild(0) == this.transform) 
		{ 
			this.transform.parent = null;
			this.transform.parent = parent;

			this.transform.localPosition = Vector3.zero;


			anim.CrossFade("Out", 0);
		}


	}
}
