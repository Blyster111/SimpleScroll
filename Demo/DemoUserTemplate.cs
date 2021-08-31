using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleScrollUnity;
using UnityEngine.UI;

public class DemoUserTemplate : SimpleScrollItemObject
{
    public Text NickNameText;
    public Image BackgroundImage;

    public override void OnItemRedraw()
    {
        DemoUser user = (DemoUser)MyItem;

        NickNameText.text = user.NickName;
        BackgroundImage.color = user.BackgroundColor;
    }

    public void MoveToTop()
    {
        MyScroll.RepositionItem(MyItem.ItemIndex, 0);
    }

    public void RandomSize()
    {
        MyItem.ItemSize = Random.Range(200, 500);
        MyScroll.Repaint(true);
    }

    public void Delete()
    {
        MyScroll.DeleteItem(MyItem.ItemIndex);
    }

    public void ChangeName()
    {
        DemoUser user = (DemoUser)MyItem;
        user.NickName = "New Name";

        MyScroll.Repaint(false);
    }

    public void ChangeBackground()
    {
        DemoUser user = (DemoUser)MyItem;
        user.BackgroundColor = Color.black;

        MyScroll.Repaint(false);
    }
}
