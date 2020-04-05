using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour //This script controls the flow of the game, visual info, interactions, and for simplicity, the rudimentary enemy AI.
{
    public Text roundText; //Texts to display visual info.

    public GameObject roundStart, cardSelection, roundOver; //Canvas groups that contain other Game Objects, each representing a phase of the game.

    public int livesPerDuel, cardsPerRound; //Set in the inspector so duels can last as one pleases.

    public Fighter player, ai; //References to both Fighters.

    public AudioSource audioSource; //We'll only be using one Audio Source.

    public AudioClip cardSFX, swordSFX; //Clips for the Audio Source to play.

    private int round; //Counter to keep track of Rounds.

    private List<Card> cardsOnBoard = new List<Card>(); //We keep a list of the cards played so that we may return them to their corresponding hands.
    
    void Start()
    {
        DuelStart();
    }

    private void DuelStart() //The duel starts with maxed out lives.
    {
        player.UpdateLives(livesPerDuel); //Lives are maxed out at the start of each Duel.
        ai.UpdateLives(livesPerDuel);

        RoundStart();
    }

    private void RoundStart()
    {
        NextPhase(roundOver, roundStart); //Displays the current round number and the buttons for choosing a Guard.

        round++; //Increase the round count each time a round starts.

        roundText.text = "Round " + round;

        cardsOnBoard.Clear(); //The list for played cards is cleared.

        player.UpdateHits(0); //Counters for Hits are reset.
        ai.UpdateHits(0);

        player.SwitchGuard("None"); //Both Fighters are rendered Guardless until they choose a Guard.
        ai.SwitchGuard("None");
    }

    private void NextPhase(GameObject endingPhase, GameObject nextPhase) //Deactivates the previous screen and activates the next one.
    {
        endingPhase.SetActive(false);

        nextPhase.SetActive(true);
    }

    public void ChooseGuard(string newGuard) //Called from user interaction. Sets the starting Guard for both player and AI.
    {
        player.SwitchGuard(newGuard); //Cards call upon this same method to switch the Fighter's Guard.

        string[] guards = { "Low", "Mid", "High" }; //An auxiliary array for the next line.

        ai.SwitchGuard(guards[Random.Range(0, 3)]); //The AI begins the round with a random Guard (that isn't Guardless).

        audioSource.PlayOneShot(swordSFX); //Fighters unsheathe.

        CardSelection();
    }

    private void CardSelection() //Displays the player's Cards for them to select.
    {
        NextPhase(roundStart, cardSelection); //Deactivate Guard selection, activate Card selection.

        foreach (Card card in player.cards)
        {
            if (card.Allowed)
            {
                card.gameObject.SetActive(true); //Only Cards that can be played given the current Guard can be played.
            }
            else if (!card.played) //Played Cards won't disappear.
            {
                card.gameObject.SetActive(false);
            }
        }

        audioSource.PlayOneShot(cardSFX); //Cards appear.
    }

    private void RoundOver() //Called when Cards played by both fighters reach the amount indicated by cardsPerRound.
    {
        cardSelection.SetActive(false); //Deactivate Card selection, activate results screen.

        roundOver.SetActive(true);

        if (player.hits == ai.hits) //If both Fighters deal the same amount of Hits to each other...
        {
            if (player.hits != 0) //... and that amount isn't 0.
            {
                player.LivesDown();
                ai.LivesDown();
            }
        }
        else //The Fighter who's dealt the most Hits depletes a Life from the other.
        {
            if (player.hits > ai.hits)
            {
                player.LivesDown();
            }
            else
            {
                ai.LivesDown();
            }
        }
    }

    public void NextRound() //Called upon by user interaction when a Round ends.
    {
        if (player.lives == 0 || ai.lives == 0) //When one or both Fighters lose:
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); //The scene is reloaded so a new Duel can start.
        }
        else
        {
            foreach (Card card in cardsOnBoard)
            {
                card.ReturnToHand(); //Cards know their position in the Card selection screen and return to it upon a new Round start.
            }

            cardsOnBoard.Clear(); //We empty the list so we can fill it anew.

            RoundStart(); //The Game Loop is closed.
        }
    }

    public void CardPlayed(Card card, Fighter fighter)
    {
        cardsOnBoard.Add(card); //We add the Card to the list.

        fighter.SwitchGuard(card.guard);

        if (fighter == player)
        {
            AIPlay(); //Once the player has played, it's the AI's turn.
        }
        else
        {
            Clash(); //Once the AI has played, both cards are compared.
        }

        audioSource.PlayOneShot(cardSFX); //A Card is played.
    }

    private void AIPlay() //The AI's turn.
    {
        List<Card> allowedCards = new List<Card>(); //A new list to store the Cards that the AI can play given its current Guard.

        foreach (Card cardAI in ai.cards) //Populate the list.
        {
            if (cardAI.Allowed && !cardAI.played)
            {
                allowedCards.Add(cardAI);
            }
        }

        List<Card> attackCards = new List<Card>(); //The AI will only defend when strictly necessary, so...

        List<Card> defenseCards = new List<Card>();

        foreach (Card cardAI in allowedCards)
        {
            if (cardAI.attack != "None") //... we populate two lists discriminating by their Attack value.
            {
                attackCards.Add(cardAI);
            }
            else
            {
                defenseCards.Add(cardAI);
            }
        }

        if (attackCards.Count != 0) //Only play defense Cards if there are no attack Cards available.
        {
            attackCards[Random.Range(0, attackCards.Count)].Play();
        }
        else
        {
            defenseCards[Random.Range(0, allowedCards.Count)].Play();
        }
    }

    private void Clash()
    {
        Card cardAI = cardsOnBoard[cardsOnBoard.Count - 1]; //The last Card and the one before are the ones we'll be comparing.

        Card cardPlayer = cardsOnBoard[cardsOnBoard.Count - 2];

        if (cardAI.attack != "None" && //If the AI's isn't playing a Card that has no Attack...
            cardPlayer.special != "Defends All") //... or if the Player isn't parrying (which defends against all Attacks).
        {
            if (cardAI.attack != cardPlayer.guard || cardAI.special == "Ignores Guard") //... and if the AI's attack penetrates the player's Guard, or the AI is playing a Card that ignores Guards...
            {
                player.Hit(); //The player is hit.
            }
        }

        if (cardAI.special != "Defends All" &&//Same conditions but reversed, for the AI.
            cardPlayer.attack != "None")
        {
            if (cardPlayer.attack != cardAI.guard || cardPlayer.special == "Ignores Guard")
            {
                ai.Hit(); //The AI is hit.
            }
        }

        if (cardAI.special == "Defends All") //This is a Special effect from Cards the like of Parry, which change the Guard of the defender to suit that of the attacker.
        {
            if (cardPlayer.attack != "None") //Guard remains the same if the other Card had no Attack.
            {
                ai.SwitchGuard(cardPlayer.attack);
            }
        }

        if (cardPlayer.special == "Defends All")
        {
            if (cardAI.attack != "None") //Guard remains the same if the other Card had no Attack.
            {
                player.SwitchGuard(cardAI.attack);
            }
        }

        if (cardsOnBoard.Count == cardsPerRound * 2) //The Round is over when the max amount of Cards is played.
        {
            RoundOver();
        }
        else
        {
            CardSelection();
        }
    }
}
