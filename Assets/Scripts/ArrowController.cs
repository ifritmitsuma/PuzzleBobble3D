using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowController : MonoBehaviour
{

    public GameObject tip;

    private GameObject ballLoaded;

    public GameObject ballPlacer;

    public GameObject hurryUpDisplay;

    private float angle = 0;

    private GameManagerScript gameManager;

    private AudioManagerScript audioManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.levelEnded && !gameManager.levelPaused) {
            if (Input.GetKeyDown(KeyCode.Space) && ballLoaded != null)
            {
                shoot();
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && angle > -45)
            {
                angle -= 1.0f;
                Vector3 originalPos = ballPlacer.transform.position;
                transform.Rotate(Vector3.right, 1.0f);
                if (ballLoaded != null)
                {
                    ballLoaded.transform.position = ballPlacer.transform.position;
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow) && angle < 45)
            {
                angle += 1.0f;
                transform.Rotate(Vector3.right, -1.0f);
                if (ballLoaded != null)
                {
                    ballLoaded.transform.position = ballPlacer.transform.position;
                }
            }
        } else
        {
            if(ballLoaded == null)
            {
                StopCoroutine("TimeLimit");
                StopCoroutine("HurryUp");
                hurryUp();
            }
        }
    }

    public void shoot()
    {
        ballLoaded.GetComponent<Rigidbody>().velocity = (25 * (tip.transform.position - transform.position).normalized);
        ballLoaded = null;
        audioManager.playSFX("shoot");
        StopCoroutine("TimeLimit");
        StopCoroutine("HurryUp");
        hurryUp();
    }

    public void recharge(GameObject ball)
    {
        if(ballLoaded != null)
        {
            Destroy(ballLoaded);
            ballLoaded = null;
        }
        ballLoaded = ball;
        ball.transform.position = ballPlacer.transform.position;
        StartCoroutine("TimeLimit");
    }

    public void resetPosition()
    {
        transform.rotation = new Quaternion();
        transform.Rotate(Vector3.up, -90.0f);
        angle = 0;
    }

    private void hurryUp(int seconds = -1)
    {
        if (seconds > -1)
        {
            hurryUpDisplay.GetComponent<Text>().text = "HURRY UP!!! " + seconds;
        }
        else
        {
            hurryUpDisplay.GetComponent<Text>().text = "";
        }
    }

    public IEnumerator TimeLimit()
    {
        while (gameManager.levelPaused)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(4.0f);
        while (gameManager.levelPaused)
        {
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine("HurryUp");
    }

    public IEnumerator HurryUp()
    {
        int seconds = 4;
        while (true)
        {
            while(gameManager.levelPaused)
            {
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(1.0f);
            while (gameManager.levelPaused)
            {
                yield return new WaitForSeconds(0.1f);
            }
            --seconds;
            if (seconds == 0)
            {
                shoot();
                hurryUp(-1);
                StopCoroutine("HurryUp");
            }
            else
            {
                hurryUp(seconds);
            }
        }
    }
}
