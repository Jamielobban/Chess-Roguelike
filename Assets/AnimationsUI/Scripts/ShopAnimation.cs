using UnityEngine;

public class ShopAnimation : MonoBehaviour
{
    Animator anim;
    bool current = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		anim = GetComponent<Animator>();

	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAnimation()
    {
        if(current)
        {
            current = false;

			if (anim.GetCurrentAnimatorStateInfo(0).IsName("In"))
                anim.CrossFade("Out",0);
        }
        else
		{
			current = true;

			if (anim.GetCurrentAnimatorStateInfo(0).IsName("Out") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
				anim.CrossFade("In", 0);

		}
	}
}
