using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Fighter : MonoBehaviour //A script to hold all variables pertaining the players. HUD elements also belong to each of them.
{
    public Text livesText, hitsText, guardText; //Texts to display visual info.

    public List<Card> cards; //The Cards that the Fighters hold.

    [HideInInspector]
    public int lives, hits; //Counters to keep track of Hits and Lives. Won't be accessed from the editor.

    [HideInInspector]
    public string guard; //The Guards that the Fighters assume. Won't be accessed from the editor.

    public void UpdateLives(int newLives) //Update and display Lives.
    {
        lives = newLives;

        livesText.text = "Lives: " + lives;
    }

    public void UpdateHits(int newHits) //Update and display Hits.
    {
        hits = newHits;

        hitsText.text = "Hits: " + hits;
    }

    public void SwitchGuard(string newGuard) //The strings are provided by either the Card that calls upon the method or the Guard choose buttons.
    {
        if (newGuard == "As attacked") //Parrying only changes Guard upon receiving an Attack.
        {
            return;
        }

        guard = newGuard; //Fighters assume the Card's Guard.

        guardText.text = "Guard: " + guard;
    }

    public void Hit() //Increase Hits to compare at the end of the Round.
    {
        hits++;

        UpdateHits(hits);
    }

    public void LivesDown() //Decrease Lives when a Round is lost.
    {
        lives--;

        UpdateLives(lives);
    }
}
