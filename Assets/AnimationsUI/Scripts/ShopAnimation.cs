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

                anim.CrossFade("Out",0);

	}
}
