using UnityEngine;

public class SetLayerFromParent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<SpriteRenderer>().sortingOrder = this.transform.parent.GetComponent<SpriteRenderer>().sortingOrder - 1;

	}
}
