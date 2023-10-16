using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerScript : MonoBehaviour
{

    public GameObject scoreObject;

    public GameObject centerMessageObject;

    public GameObject livesObject;

    public GameObject lifePrefab;

    private GameManagerScript gameManager;

    private Text score;

    private Text centerMessage;

    public GameObject buttons;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        score = scoreObject.GetComponent<Text>();
        centerMessage = centerMessageObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        score.text = gameManager.score.ToString();

    }

    public void publishMessage(string message)
    {
        centerMessage.text = message;
    }

    public void setLives(int lives)
    {
        foreach(Transform child in livesObject.transform)
        {
            Destroy(child.gameObject);
        }
        for(int i = 0; i < lives; ++i)
        {
            GameObject life = Instantiate(lifePrefab);
            life.name = "Life";
            life.layer = LayerMask.NameToLayer("UI");
            life.transform.parent = livesObject.transform;
            life.transform.localScale = new Vector3(20.0f, 20.0f, 1.0f);
            life.transform.position = new Vector3(livesObject.transform.position.x + (i), livesObject.transform.position.y, livesObject.transform.position.z);
        }
    }

    public void displayButton(string name)
    {
        GameObject button = buttons.transform.Find(name).gameObject;
        button.SetActive(true);
        button.transform.Find("Text").GetComponent<Text>().text = name;
    }

    public void hideButtons()
    {
        foreach(Transform button in buttons.transform)
        {
            button.gameObject.SetActive(false);
        }
    }

}
