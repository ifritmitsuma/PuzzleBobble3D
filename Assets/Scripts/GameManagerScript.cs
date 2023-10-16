using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameManagerScript : MonoBehaviour
{

    public GameObject[] placeholders;

    // 0 - None, 1 - Black, 2 - Blue, 3 - Green, 4 - Purple, 5 - Red, 6 - Yellow, 7 - Orange, 8 - White
    public GameObject[] ballPrefabs;

    public GameObject ballsParent;

    public Vector3 ballsParentStartingPoint;

    public Vector3 ballsParentCurrentPoint;

    public int[] colorCount = new int[8];

    public int totalCount = 0;

    public int[] balls = new int[90];

    public Ball[] actualBalls = new Ball[90];

    public ulong score = 0;

    public int round = 0;

    public bool levelEnded = true;

    public bool levelPaused = false;

    public float limit = 0;

    public int lives = 3;

    public int continues = 3;

    public int moveCount = 0;

    public int shakingFactor = 0;

    public bool gameStarted = false;

    public Stopwatch stopWatch;

    private AudioManagerScript audioManager;

    private UIManagerScript uiManager;

    private ColorGrading colorGrading;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManagerScript>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManagerScript>();
        limit = GameObject.Find("LimitBar").transform.position.y;
        Camera.main.GetComponent<PostProcessVolume>().profile.TryGetSettings(out colorGrading);
        ballsParentStartingPoint = ballsParent.transform.position;
        ballsParentCurrentPoint = ballsParentStartingPoint;
        levelEnded = true;
        round = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameStarted)
        {
            startLevel(true);
            gameStarted = true;
        }

        if (!levelEnded)
        {

            if (Input.GetKeyDown(KeyCode.P))
            {
                levelPaused = !levelPaused;
                colorGrading.tonemapper.overrideState = levelPaused;
                if (levelPaused)
                {
                    uiManager.publishMessage("PAUSE");
                    stopWatch.Stop();
                } else
                {
                    uiManager.publishMessage("");
                    stopWatch.Start();
                }
            }
            if (!levelPaused)
            {
                for (int i = 89; i > -1; --i)
                {

                    if (balls[i] > 0 && actualBalls[i].getBall().transform.position.y <= limit)
                    {
                        colorGrading.saturation.overrideState = true;
                        levelEnded = true;
                        audioManager.stopMusic();
                        audioManager.playSFX("lose");
                        lives--;
                        uiManager.setLives(lives);
                        StartCoroutine("LoseLife");
                        break;
                    }

                }

                shake(shakingFactor);
            }

        }
    }

    public int setAtClosestPlaceHolder(GameObject ball)
    {
        float distance = float.PositiveInfinity;
        Vector3 point = Vector3.zero;
        int index = -1;
        for(int i = 0; i < 90; ++i)
        {
            float tempDist = Vector3.Distance(placeholders[i].transform.position, ball.transform.position);
            if (tempDist < distance)
            {
                distance = tempDist;
                point = placeholders[i].transform.position;
                index = i;
            }
        }
        ball.transform.position = point;
        ball.GetComponent<Rigidbody>().isKinematic = true;
        balls[index] = getBallColorByName(ball.GetComponent<Renderer>().material.name);
        List<Ball> adjacentsAtTop = getAdjacentsAtTop(ball);
        actualBalls[index] = new Ball(index, ball, index < 8, adjacentsAtTop);
        /*foreach(Ball i in adjacentsAtTop)
        {
            i.addAdjacent(actualBalls[index]);
            actualBalls[i.index] = i;
        }*/
        ball.transform.parent = ballsParent.transform;
        colorCount[balls[index]-1]++;
        totalCount++;
        return index;
    }
    public int getBallColorByName(string name)
    {
        switch(name.Split(' ')[0])
        {
            case "BlackBall":
                return 1;
            case "BlueBall":
                return 2;
            case "GreenBall":
                return 3;
            case "PurpleBall":
                return 4;
            case "RedBall":
                return 5;
            case "YellowBall":
                return 6;
            case "OrangeBall":
                return 7;
            case "WhiteBall":
                return 8;
            default:
                return 0;
        }
    }

    public bool areAdjacent(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b) < 2.5f;
    }

    public Dictionary<int, GameObject> getAdjacents(GameObject ball)
    {
        Dictionary<int, GameObject> adjacents = new Dictionary<int, GameObject>();

        for(int i = 0; i < 90; ++i)
        {
            if(balls[i] != 0 && areAdjacent(ball.transform.position, placeholders[i].transform.position))
            {
                adjacents.Add(i, actualBalls[i].getBall());
            }
        }
        return adjacents;
    }

    public bool areAdjacentAtTop(Vector3 a, Vector3 b)
    {
        return b.y >= a.y && (b - a).magnitude < 2.5f;
    }

    public List<Ball> getAdjacentsAtTop(GameObject ball)
    {
        List<Ball> adjacents = new List<Ball>();

        for (int i = 0; i < 90; ++i)
        {
            if (actualBalls[i] != null && !actualBalls[i].getBall().Equals(ball) && balls[i] != 0 && areAdjacentAtTop(ball.transform.position, placeholders[i].transform.position))
            {
                adjacents.Add(actualBalls[i]);
            }
        }
        return adjacents;
    }

    public void checkBoard(GameObject ball, int position)
    {
        Dictionary<int, GameObject> adjacents = new Dictionary<int, GameObject>();

        Vector3 center = checkBoard(ball, position, adjacents, new Vector3());
        center /= adjacents.Count;
        
        if (adjacents.Count > 2)
        {

            Dictionary<int, Ball> ballsToRemove = new Dictionary<int, Ball>();
            Dictionary<int, Ball> ballsNotToRemove = new Dictionary<int, Ball>();
            for (int i = 0; i < 90; ++i)
            {
                if (actualBalls[i] != null)
                {
                    ballsToRemove.Add(i, actualBalls[i]);
                }
            }
            audioManager.playSFX("blow");
            foreach (int i in adjacents.Keys)
            {
                GameObject b = adjacents[i];
                ballsToRemove.Remove(i);
                ballsNotToRemove.Add(i, actualBalls[i]);
                actualBalls[i] = null;
                balls[i] = 0;
                colorCount[getBallColorByName(b.GetComponent<Renderer>().material.name) - 1]--;
                totalCount--;
                b.GetComponent<BallController>().explode(center);
                score += 10;
            }
            bool played = false;
            for(int i = 89; i > -1; --i)
            {
                if (actualBalls[i] != null && actualBalls[i].getBall() != null)
                {
                    if(actualBalls[i].shouldRemove(ballsToRemove, ballsNotToRemove))
                    {
                        if (!played)
                        {
                            audioManager.playSFX("excessive_blow");
                            played = true;
                        }
                        Ball looseBall = actualBalls[i];
                        GameObject b = looseBall.getBall();
                        actualBalls[i] = null;
                        balls[i] = 0;
                        colorCount[getBallColorByName(b.GetComponent<Renderer>().material.name) - 1]--;
                        totalCount--;
                        b.GetComponent<BallController>().fall();
                        score += 20;
                    }
                }
            }
            /*Dictionary<int, GameObject> looseBalls = getAdjacents(actualBalls[i]);
            bool topBall = true;
            foreach (int j in looseBalls.Keys)
            {
                if (actualBalls[i].transform.position.y < actualBalls[j].transform.position.y)
                {
                    topBall = false;
                    break;
                }
            }
            if (topBall && !actualBalls[i].GetComponent<BallController>().touchingTop)
            {
                if (!played)
                {
                    audioManager.playSFX("excessive_blow");
                    played = true;
                }
                GameObject b = actualBalls[i];
                actualBalls[i] = null;
                balls[i] = 0;
                colorCount[getBallColorByName(b.GetComponent<Renderer>().material.name) - 1]--;
                totalCount--;
                b.GetComponent<BallController>().fall();
                score += 20;
            }*/
        } else
        {
            audioManager.playSFX("hit");
        }

        moveCount++;

        if (totalCount > 0)
        {
            switch (moveCount)
            {
                case 6:
                    shakingFactor = 1;
                    break;
                case 7:
                    audioManager.playSFX("danger");
                    shakingFactor = 2;
                    break;
                case 8:
                    dropOneLine();
                    moveCount = 0;
                    shakingFactor = 0;
                    break;
                default:
                    break;
            }
            if(!levelEnded)
            {
                arrowRecharge();
            }
        } else
        {
            levelComplete();
        }
        
    }

    private void shake(int speed)
    {
        ballsParent.transform.position = new Vector3(ballsParentCurrentPoint.x + Mathf.Sin(Time.time * speed * 40) * 0.07f, ballsParentCurrentPoint.y, ballsParentCurrentPoint.z);
    }

    private void dropOneLine()
    {
        ballsParent.transform.position = new Vector3(ballsParentCurrentPoint.x, ballsParentCurrentPoint.y - 2.0f, ballsParentCurrentPoint.z);
        ballsParentCurrentPoint = ballsParent.transform.position;
        audioManager.playSFX("drop");
    }

    private void arrowRecharge()
    {

        int index = UnityEngine.Random.Range(1, totalCount);

        GameObject ballPrefab = null;
        int totalVerified = 0;
        for (int i = 0; i < 8; ++i)
        {
            if(index > totalVerified + colorCount[i])
            {
                totalVerified += colorCount[i];
                continue;
            }
            ballPrefab = ballPrefabs[i + 1];
            break;
            
        }
        ((ArrowController)GameObject.Find("Arrow").GetComponent("ArrowController")).recharge(Instantiate(ballPrefab));
    }

    private Vector3 checkBoard(GameObject ball, int index, Dictionary<int, GameObject> adjacents, Vector3 center)
    {
        adjacents[index] = ball;
        center += ball.transform.position;
        for(int i=0; i < 90; ++i)
        {
            if(balls[i] > 0 && 
                areAdjacent(ball.transform.position, placeholders[i].transform.position) && 
                getBallColorByName(ball.GetComponent<Renderer>().material.name) == balls[i] &&
                !adjacents.ContainsValue(actualBalls[i].getBall()))
            {
                checkBoard(actualBalls[i].getBall(), i, adjacents, center);
            }
        }
        return center;
    }

    public void levelComplete()
    {
        audioManager.GetComponent<AudioManagerScript>().playMusic("clear");
        uiManager.publishMessage("ROUND " + round + " CLEAR!");
        levelEnded = true;
        stopWatch.Stop();
        StartCoroutine("EndLevel");
    }

    public void reset()
    {
        colorGrading.saturation.overrideState = false;
        for(int i = 0; i < 90; ++i)
        {
            balls[i] = 0;
            if (actualBalls[i] != null)
            {
                Destroy(actualBalls[i].getBall());
                actualBalls[i] = null;
            }
        }
        foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            Destroy(ball);
        }
        for (int i = 0; i < 8; ++i)
        {
            colorCount[i] = 0;
        }
        totalCount = 0;
        moveCount = 0;
        shakingFactor = 0;
        uiManager.hideButtons();
    }

    public void startLevel(bool newRound, bool isContinue = false)
    {
        if(newRound)
        {
            round++;
            lives = 3;
        }
        if(isContinue) {
            continues--;
            lives = 3;
        }
        if (File.Exists("Assets/Resources/Level" + round.ToString() + ".csv"))
        {
            reset();
            StreamReader reader = new StreamReader("Assets/Resources/Level" + round.ToString() + ".csv");
            string line;
            int i = 0;
            while((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(',');

                foreach(string value in values)
                {
                    balls[i] = int.Parse(value);
                    if (balls[i] > 0)
                    {
                        colorCount[balls[i] - 1]++;
                        totalCount++;
                    }
                    ++i;
                }
                
            }

            for (int j = 0; j < 90; ++j)
            {
                if (balls[j] > 0)
                {
                    GameObject newBall = Instantiate(ballPrefabs[balls[j]]);
                    newBall.transform.position = placeholders[j].transform.position;
                    newBall.GetComponent<Rigidbody>().isKinematic = true;
                    actualBalls[j] = new Ball(j, newBall, j < 8);
                    newBall.transform.parent = ballsParent.transform;
                }
            }

            for(int j = 0; j < 90; ++j)
            {
                if(actualBalls[j] != null) {
                    actualBalls[j].setAdjacents(getAdjacentsAtTop(actualBalls[j].getBall()));
                }
            }

            ballsParent.transform.position = ballsParentStartingPoint;
            ballsParentCurrentPoint = ballsParentStartingPoint;

        }
        else
        {
            audioManager.playMusic("gameover");
            uiManager.publishMessage("You win!\nScore: " + score.ToString() + " points");
            uiManager.displayButton("Restart");
            uiManager.displayButton("Quit");
            return;
        }
        uiManager.setLives(lives);
        uiManager.publishMessage("ROUND " + round.ToString());
        audioManager.playSFX("ready");
        StartCoroutine("StartLevel");

        ((ArrowController)GameObject.Find("Arrow").GetComponent("ArrowController")).resetPosition();
        arrowRecharge();

        audioManager.GetComponent<AudioManagerScript>().playMusic("stage");
    }

    public string getTime()
    {
        TimeSpan time = stopWatch.Elapsed;
        return (time.Minutes > 0 ? time.Minutes.ToString() + " minutes and " : "") + time.Seconds.ToString() + " seconds";
    }

    private IEnumerator StartLevel()
    {
        yield return new WaitForSeconds(1.25f);
        stopWatch = new Stopwatch();
        stopWatch.Start();

        audioManager.playSFX("go");
        uiManager.publishMessage("");
        levelEnded = false;
    }

    private IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(2.0f);
        TimeSpan time = stopWatch.Elapsed;
        float bonus = Math.Max((30.0f - (int)time.TotalSeconds) / 30.0f, 0);
        bonus = (((ulong) (bonus * 50000)) / 100) * 100;
        score += (ulong) bonus;
        uiManager.publishMessage(getTime() + "\n" + (bonus != 0 ? bonus + " points" : "No bonus"));
        yield return new WaitForSeconds(2.0f);
        uiManager.publishMessage(" ");
        startLevel(true);

    }

    public IEnumerator LoseLife()
    {
        yield return new WaitForSeconds(2.0f);
        if (lives > 0)
        {
            startLevel(false);
        }
        else
        {
            audioManager.playMusic("gameover");
            uiManager.publishMessage("GAME OVER");
            if (continues > 1) {
                uiManager.displayButton("Continue");
            } else
            {
                uiManager.displayButton("Restart");
            }
            uiManager.displayButton("Quit");
        }
    }

    public void Restart()
    {
        ballsParentCurrentPoint = ballsParentStartingPoint;
        levelEnded = true;
        continues = 3;
        round = 0;
        score = 0;
        startLevel(true);
    }

    public void Continue()
    {
        startLevel(false, true);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
