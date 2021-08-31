using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleScrollUnity;

public class DemoScene : MonoBehaviour
{
    public SimpleScroll MyScroll;
    
    void Start()
    {
        var list = new List<SimpleScrollItem>();
        for (int i = 0; i < 1000; i++)
        {
            list.Add(new DemoUser()
            {
                NickName = $"User #{i}",
                BackgroundColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f), 1)
            });
        }

        MyScroll.Initialize();
        MyScroll.FillWithData(list);
        MyScroll.Repaint(false);
    }

    private void OnGUI()
    {
        float drawPosition = 100;
        if (GUI.Button(new Rect(50, drawPosition, 150, 35), "Move to Index 100"))
        {
            MyScroll.FastForwardToIndex(100);
        }
        drawPosition += 40;
        if (GUI.Button(new Rect(50, drawPosition, 150, 35), "Smoothly To Index 100"))
        {
            MyScroll.SmoothMoveToIndex(100, 1, true);
        }
        drawPosition += 40;
        if (GUI.Button(new Rect(50, drawPosition, 150, 35), "Insert Item"))
        {
            MyScroll.InsertItem(0, new DemoUser()
            {
                NickName = "New Item",
                BackgroundColor = Color.black
            });
        }
    }
}
