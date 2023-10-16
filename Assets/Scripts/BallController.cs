using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{

    private GameManagerScript gameManager;

    public bool touchingTop;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManagerScript) GameObject.Find("GameManager").GetComponent("GameManagerScript");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if((collision.gameObject.CompareTag("Ball") || "Top".Equals(collision.gameObject.name)) && GetComponent<Rigidbody>().velocity != Vector3.zero)
        {
            int index = gameManager.setAtClosestPlaceHolder(gameObject);
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameManager.checkBoard(gameObject, index);
        } else if ("Top".Equals(collision.gameObject.name))
        {
            touchingTop = true;
        }
    }

    public void explode(Vector3 center)
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        GetComponent<Collider>().enabled = false;
        rb.AddForce(new Vector3(Random.Range(-1000.0f, 1000.0f), 1000.0f, -1000.0f));
        StartCoroutine("Fall");
    }

    public void fall()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        GetComponent<Collider>().enabled = false;
        StartCoroutine("Fall");
    }

    private IEnumerator Fall()
    {
        while(true)
        {
            yield return new WaitForSeconds(2.0f);
            Destroy(gameObject);
            StopCoroutine("Fall");
        }
    }

}
