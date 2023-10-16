using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball
{

    public int index = 0;

    private List<Ball> adjacents = new List<Ball>();

    private GameObject ball;

    private bool remove = false;

    private bool top = false;
    public Ball(int index, GameObject ball, bool top)
    {
        this.index = index;
        this.ball = ball;
        this.top = top;
    }

    public Ball(int index, GameObject ball, bool top, List<Ball> adjacents) : this(index, ball, top)
    {
        this.adjacents = adjacents;
    }

    public GameObject getBall()
    {
        return ball;
    }

    public bool isAtTop()
    {
        return this.top;
    }

    public void addAdjacent(Ball adjacent)
    {
        this.adjacents.Add(adjacent);
    }

    public void setAdjacents(List<Ball> adjacents)
    {
        this.adjacents = adjacents;
    }

    public bool shouldRemove(Dictionary<int, Ball> balls, Dictionary<int, Ball> dont)
    {
        return shouldRemove(balls, dont, null);
    }
    public bool shouldRemove(Dictionary<int, Ball> balls, Dictionary<int, Ball> dont, Ball previous)
    {

        if(dont.ContainsKey(index) || getBall() == null)
        {
            remove = true;
            return true;
        }

        if (isAtTop() || (!dont.ContainsKey(index) && !balls.ContainsKey(index)))
        {
            //dont.Add(index, this);
            balls.Remove(index);
            return false;
        }

        int adjacentsToRemove = 0;

        foreach (Ball adjacent in this.adjacents)
        {
            if((previous == null || !adjacent.Equals(previous)) && !adjacent.shouldRemove(balls, dont, this))
            {
                /*if(balls.ContainsKey(adjacent.index))
                {
                    balls.Remove(adjacent.index);
                }*/
                adjacentsToRemove++;
            }
            /*if(adjacent != null && !adjacent.Equals(previous) && !dont.ContainsKey(adjacent.index) && adjacent.shouldRemove(balls, dont, this))
            {
                if (!balls.ContainsKey(index))
                {
                    balls.Add(index, this);
                }
                adjacentsToRemove.Add(adjacent);   
            }*/
        }
        if(adjacentsToRemove == 0)
        {
            remove = true;
            return true;
        }
        balls.Remove(index);
        return false;
    }
}
