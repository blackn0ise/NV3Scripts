using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipController : MonoBehaviour
{
   	private TipSet tipset;
    private List<Tip> tipList;
    private List<Tip> usedTipList;
    public const string tpath = "tips";
    public const string imgpath = "tipImgs/";
    private GameOptions gops;

    void Start()
    {
        tipset = TipSet.Load(tpath);
        tipList = tipset.tips;
        gops = GameOptions.GetGOPS();
        usedTipList = new List<Tip>();
    }

    public Tip GetRandomTip()
    {

        if (!gops)
            gops = FindObjectOfType<GameOptions>();
        if (gops)
        {
            //scripted tips
            if (gops.GetIsTutorialMode())
                return tipList[1];
            else if (gops.GetIsFreshSave())
                return tipList[0];
        }
        List<Tip> templist = new List<Tip>();
        foreach (Tip tip in tipList)
        {
            if (gops.GetSave().highScore >= tip.pbRequired)
            {
                templist.Add(tip);
                //Debug.Log(tip.text + "| is an available tip.");
            }
        }

        if (usedTipList.Count == templist.Count)
            usedTipList.Clear();

        int random = Random.Range(0, templist.Count);
        while (usedTipList.Contains(templist[random]))
            random = Random.Range(0, templist.Count);

        usedTipList.Add(templist[random]);
        return templist[random];
    }

    public Sprite FindSpriteByNumber(int number)
    {
        Sprite sprite = Resources.Load<Sprite>(imgpath + number.ToString());
        return sprite;
    }
}
