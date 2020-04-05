using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public string guard, attack, special; //Cards have a Guard, an Attack, and a Special effect.

    public Fighter fighter; //A reference to the Fighter that holds the Card.

    public GameObject board, hand; //Cards have a reference to the hand to which they belong and the board at which they're played.

    public GameManager gM; //A reference to the Game Manager script so that Cards may let it know when they're played.

    public bool played; //Cards cannot be played twice, obviously.

    private int siblingIndex; //Cards know their place in hand and when selected.

    public bool Allowed //Returns true if the Card can be played according to the current Guard.
    {
        get
        {
            if (attack == "None") //Cards with no Attack can always be used to transition between Guards.
            {
                return true;
            }
            else
            {
                if (attack == "Low")
                {
                    if (fighter.guard == "Low")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (attack == "Mid")
                {
                    if (fighter.guard == "Mid")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (attack == "High")
                {
                    if (fighter.guard == "High")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }

    void Start() //Initiate some variables.
    {
        siblingIndex = transform.GetSiblingIndex(); //Store the sibling index for further use.
    }

    public void Play() //Called upon user interaction with the Card, or by the Game Manager in the case of an AI Card.
    {
        if (played) //Do nothing if the Card has already been played.
        {
            return;
        }

        played = true; //The Card cannot be played again.

        transform.SetParent(board.transform, false); //Place the Card at its corresponding board. The Horizontal Layout group takes care of its position.

        gM.CardPlayed(this, fighter); //Notify the Game Manager.
    }

    public void ReturnToHand() //Called when another Card is selected and also at the end of the Round.
    {
        played = false; //Cards that have been played...

        transform.SetParent(hand.transform, false); //... return to their hand...

        transform.SetSiblingIndex(siblingIndex); //... in their corresponding order.
    }
}
