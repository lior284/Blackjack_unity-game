using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public GameObject panel;

    public Image DealersOpenCard;
    public Image DealersCloseCard;
    public Image PlayersCard1;
    public Image PlayersCard2;

    public Text balanceText;
    public Text betAmountText;
    public Slider betAmountSlider;
    private int sessionPlayerBet;
    private int playerBet, playerBetIfSplitR; // Same as the other IfSplitR (Check below)
    private int balance;
    public Button backBtn;

    public Button NextHandBtn;
    public Text NextHandText;

    private readonly string[] cardShapes = {"clubs", "diamonds", "hearts", "spades"};
    private List<string> usedCards = new List<string>();
    /*
    The first element in the list is the dealer's open card.
    The second is the dealer's close card.
    The third is the player's first card.
    And the forth is the player's second card.
    */

    public GameObject actionButtonsParent;
    public GameObject nextSkipParent;
    public Button HitBtn;
    public Button StandBtn;
    public Button DoubleBtn;
    public Button SplitBtn;
    public Button NextBtn;
    public Button SkipBtn;


    private GameObject addedCards;
    private List<Image> playersCards = new List<Image>(), dealersCards = new List<Image>();
    private List<Image> playersCardsIfSplitR = new List<Image>(); // If the player split his cards, then the default list is the left and this variable is the right list

    public Text playerSumText, playerSumTextIfSplitR, dealerSumText;

    private int totalOfPlayer, totalOfPlayerIfAce = 0, absolutePlayerTotal;
    private int totalOfPlayerIfSplitR, totalOfPlayerIfAceIfSplitR = 0, absolutePlayerTotalIfSplitR = 22; // If the player split his cards, then the default totals are the left and these variables are the right totals
    // The reason the default of absolutePlayerTotalIfSplitR is 22 is explained in the last if statement of the function DealersTurn()
    private int totalOfDealer, totalOfDealerIfAce = 0, absoluteDealerTotal;
    private bool validForDouble = true, validForSplit = false;
    private bool validForDoubleIfSplitR = true; // Same as the other IfSplitR
    private bool topiaValid = false, todiaValid = false; // topia = totalOfPlayerIfAce | todia = totalOfDealerIfAce
    private bool topiaIfSplitRValid = false; // If the player split his cards, then the default topia is the left and this variable is the right topia

    private int splitIndicator = 0;
    /* 
    Used to determine if the action btns work of the regular tota, the left total (if split), or the right total (if split)
    0 = no split
    1 = left split
    2 = right split
    */

    public Text resultMessage, resultMessageIfSplitR;

    private void Start()
    {
        balance = BalanceManager.Instance.GetBalance();
        NextHandBtn.GetComponent<EventTrigger>().enabled = true;
        BtnHoverAction.SetColorChangeEnabled(true);

        if(balance >= 10 && balance < 10000) // Making the max value of the slider to be the player's balance if it below 10,000
        {
            betAmountSlider.maxValue = balance;
        } else if(balance < 10)
        {
            betAmountSlider.minValue = 0;
            betAmountSlider.maxValue = 0;
            betAmountSlider.value = 0;
            UpdateBetAmount();
            NextHandBtn.interactable = false;
            NextHandText.color = NextHandBtn.colors.disabledColor;
            NextHandBtn.GetComponent<EventTrigger>().enabled = false;
            BtnHoverAction.SetColorChangeEnabled(false);
        }
    }

    public void UpdateBetAmount()
    {
        betAmountSlider.value = System.MathF.Round(betAmountSlider.value / 10) * 10;
        sessionPlayerBet = (int) betAmountSlider.value;
        betAmountText.text = "Bet Amount: " + ShowBalance.FullBalance(sessionPlayerBet);
    }

    public void NextHand()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        NextHandBtn.gameObject.SetActive(false);
        resultMessage.gameObject.SetActive(false);
        resultMessageIfSplitR.gameObject.SetActive(false);
        betAmountSlider.gameObject.SetActive(false);
        betAmountText.gameObject.SetActive(false);
        balanceText.gameObject.SetActive(false);
        backBtn.gameObject.SetActive(false);
        

        HitBtn.gameObject.SetActive(true);
        StandBtn.gameObject.SetActive(true);
        DoubleBtn.gameObject.SetActive(true);
        SplitBtn.gameObject.SetActive(true);

        ResetVariables();

        addedCards = new GameObject("AddedCards");
        addedCards.transform.SetParent(panel.transform, false);

        if(playerBet * 2 > balance)
        {
            validForDouble = false;
            validForDoubleIfSplitR = false;
            validForSplit = false;
        }

        if(playerBet <= balance)
        {
            balance -= playerBet;
            BalanceManager.Instance.AddToBalance(-playerBet);
        }

        CreateDealersCards();
        CreatePlayersCards();
        /*
        If the player has a blackjack then the function "CreatePlayersCards()" goes directly to the result screen, but after everthing
        it will go back to here, and I don't want it to display the action buttons - so if the result screen is activated then not showing
        the action buttons.
        */
        if(!resultMessage.IsActive())
        {
            ShowActionBtns();
        }
    }

    private void ResetVariables()
    {
        DealersOpenCard.sprite = Cardback.Instance.GetSelectedCardBack();
        DealersCloseCard.sprite = Cardback.Instance.GetSelectedCardBack();
        PlayersCard1.sprite = Cardback.Instance.GetSelectedCardBack();
        PlayersCard2.sprite = Cardback.Instance.GetSelectedCardBack();
        PlayersCard1.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 304.92f);
        PlayersCard2.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 304.92f);

        playerBet = sessionPlayerBet;
        playerBetIfSplitR = 0;

        usedCards = new List<string>();

        Destroy(addedCards);
        playersCards = new List<Image>();
        playersCardsIfSplitR = new List<Image>();
        dealersCards = new List<Image>();

        playerSumText.text = "(0)";
        playerSumText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -115);
        playerSumTextIfSplitR.text = "(0)";
        playerSumTextIfSplitR.gameObject.SetActive(false);
        dealerSumText.text = "(0)";

        totalOfPlayer = 0; totalOfPlayerIfSplitR = 0; totalOfPlayerIfAce = 0; totalOfPlayerIfAceIfSplitR = 0; absolutePlayerTotal = 0; absolutePlayerTotalIfSplitR = 22;
        totalOfDealer = 0; totalOfDealerIfAce = 0; absoluteDealerTotal = 0;

        validForDouble = true; validForDoubleIfSplitR = true;
        validForSplit = false;
        topiaValid = false; todiaValid = false;
        topiaIfSplitRValid = false;
        splitIndicator = 0;

        resultMessage.text = "Result Message";
        resultMessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        resultMessageIfSplitR.text = "Result Message";
        resultMessageIfSplitR.gameObject.SetActive(false);
    }

    private void CreateDealersCards()
    {
        int cardNumber, cardShape;
        string cardStr;

        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));

        usedCards.Add(cardStr);
        DealersOpenCard.sprite = Resources.Load<Sprite>("Cards/" + cardStr);
        dealersCards.Add(DealersOpenCard);
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }
        totalOfDealer += cardNumber;

        if(cardNumber == 1)
        {
            dealerSumText.text = "(1/11)";
        } else {
            dealerSumText.text = "(" + cardNumber + ")";
        }
        
        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));

        usedCards.Add(cardStr);
        DealersCloseCard.sprite = Cardback.Instance.GetSelectedCardBack();
        dealersCards.Add(DealersCloseCard);
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }
        totalOfDealer += cardNumber;

        AlignElementsFromList(dealersCards, 350);

        if(GetNumberFromCard(usedCards[0]) == 1 || GetNumberFromCard(usedCards[1]) == 1)
        {
            totalOfDealerIfAce = totalOfDealer + 10;
            todiaValid = true;
        } else
        {
            todiaValid = false;
        }

        dealerSumText.gameObject.SetActive(true);
        DealersOpenCard.gameObject.SetActive(true);
        DealersCloseCard.gameObject.SetActive(true);
    }

    private void CreatePlayersCards()
    {
        int cardNumber, cardShape;
        string cardStr;

        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));

        usedCards.Add(cardStr);
        PlayersCard1.sprite = Resources.Load<Sprite>("Cards/" + cardStr);
        playersCards.Add(PlayersCard1);
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }
        totalOfPlayer += cardNumber;
        
        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));
        
        usedCards.Add(cardStr);
        PlayersCard2.sprite = Resources.Load<Sprite>("Cards/" + cardStr);
        playersCards.Add(PlayersCard2);
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }
        totalOfPlayer += cardNumber;

        AlignElementsFromList(playersCards, -350);

        if(GetNumberFromCard(usedCards[2]) == GetNumberFromCard(usedCards[3])) // If the vards are the same then the user can split
        {
            validForSplit = true;
        } else if(GetNumberFromCard(usedCards[2]) >= 10 && GetNumberFromCard(usedCards[3]) >= 10) // If the number is different but both are above or equal to 10 (for example 10 and 12) then the split option should be valid
        {
            validForSplit = true;
        } else {
            validForSplit = false;
        }

        // If one of the cards is an ace then making topiaValid = true, updating the topia, and displaying the sum to the player accordingly
        if(GetNumberFromCard(usedCards[2]) == 1 || GetNumberFromCard(usedCards[3]) == 1)
        {
            totalOfPlayerIfAce = totalOfPlayer + 10;
            topiaValid = true;
            playerSumText.text = "(" + totalOfPlayer + "/" + totalOfPlayerIfAce + ")";
        } else
        {
            topiaValid = false;
            playerSumText.text = "(" + totalOfPlayer + ")";
        }

        PlayersCard1.gameObject.SetActive(true);
        PlayersCard2.gameObject.SetActive(true);
        playerSumText.gameObject.SetActive(true);

        if(totalOfPlayerIfAce == 21 || totalOfDealerIfAce == 21)
        {
            DealersCloseCard.sprite = Resources.Load<Sprite>("Cards/" + usedCards[1]);
            DealersTurn();
            ResultScreen();
        }
    }

    private bool AlreadyUsedCard(string card)
    {
        for(int i=0;i<usedCards.Count;i++)
        {
            if(usedCards[i] == card)
            {
                return true;
            }
        }
        return false;
    }

    private void ShowActionBtns()
    {
        actionButtonsParent.SetActive(true); // Showing all the buttons

        /* Creating a list that contains the buttons that neeeds to be shown.
        Everytime the function is called the Hit and Stand options are valid - so creating the list with these defalut values.
        Later, checking if the Double or Split options needs to be added
        */
        List<Button> buttons = new List<Button>() {HitBtn, StandBtn};

        if(splitIndicator == 0 || splitIndicator == 1)
        {
            if(validForDouble) // Adding the double option
            {
                buttons.Add(DoubleBtn);
                DoubleBtn.gameObject.SetActive(true);
            } else { // If double isn't valid then hiding it.
                DoubleBtn.gameObject.SetActive(false);
            }
        } else {
            if(validForDoubleIfSplitR) // Adding the double option
            {
                buttons.Add(DoubleBtn);
                DoubleBtn.gameObject.SetActive(true);
            } else { // If double isn't valid then hiding it.
                DoubleBtn.gameObject.SetActive(false);
            }
        }

        if(validForSplit) // Adding the split option
        {
            buttons.Add(SplitBtn);
        } else { // If split isn't valid then hiding it.
            SplitBtn.gameObject.SetActive(false);
        }

        for(int i=0;i<buttons.Count;i++) // Making all the buttons black -> because of random bugs
        {
            buttons[i].GetComponentInChildren<Text>().color = Color.black;
        }

        // Displaying the buttons centered on the screen
        AlignElementsFromList(buttons, 0);
    }

    private int GetNumberFromCard(string card)
    {
        string cardNumber = "";

        for(int i=0;i<card.Length;i++)
        {
            if(card[i] != '_')
            {
                cardNumber += card[i];
            } else {
                break;
            }
        }
        return int.Parse(cardNumber);
    }

    public void HitBtnClick()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Randomizing a new card that hasn't been used yet
        int cardNumber, cardShape;
        string cardStr;

        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));

        usedCards.Add(cardStr); // Adding the newly added card to the usedCard list
        // Calculating the new sum of the player based on the added card
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }

        if(splitIndicator == 0)
        {
            totalOfPlayer += cardNumber;
            if(cardNumber == 1)
            {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                } else {
                    topiaValid = true;
                    totalOfPlayerIfAce = totalOfPlayer + 10;
                }
            } else {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                }
            }
            // Creating the new card
            GameObject addedCard = new GameObject("AddedCard" + (playersCards.Count + 1)); // Creating a new game object for this card
            addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
            Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
            addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
            addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
            playersCards.Add(addedCardImage); // Adding the new card to the list of the player's card

            // Displaying the cards centered on the screen
            AlignElementsFromList(playersCards, -350);

            // Disabling the double and split options for later
            validForDouble = false;
            validForSplit = false;

            // Checking the new sum
            if(CheckPlayersSum())
            {
                ShowActionBtns();
                // Updating the displayed sum of player
                if(topiaValid)
                {
                    playerSumText.text = "(" + totalOfPlayer + "/" + totalOfPlayerIfAce + ")";
                } else {
                    playerSumText.text = "(" + totalOfPlayer + ")";
                }
            }
        } else if(splitIndicator == 1) {
            totalOfPlayer += cardNumber;
            if(cardNumber == 1)
            {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                } else {
                    topiaValid = true;
                    totalOfPlayerIfAce = totalOfPlayer + 10;
                }
            } else {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                }
            }
            // Creating the new card
            GameObject addedCard = new GameObject("AddedCard" + (playersCards.Count + 1) + "L"); // Creating a new game object for this card
            addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
            Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
            addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
            addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
            playersCards.Add(addedCardImage); // Adding the new card to the list of the player's card

            // Displaying the cards centered on the screen
            AlignElementsFromList(playersCards, -350, false);

            // Disabling the double option for later
            validForDouble = false;

            // Checking the new sum
            if(CheckPlayersSum())
            {
                ShowActionBtns();
                // Updating the displayed sum of player
                if(topiaValid)
                {
                    playerSumText.text = "(" + totalOfPlayer + "/" + totalOfPlayerIfAce + ")";
                } else {
                    playerSumText.text = "(" + totalOfPlayer + ")";
                }
            }
        } else {
            totalOfPlayerIfSplitR += cardNumber;
            if(cardNumber == 1)
            {
                if(topiaIfSplitRValid)
                {
                    totalOfPlayerIfAceIfSplitR += cardNumber;
                } else {
                    topiaIfSplitRValid = true;
                    totalOfPlayerIfAceIfSplitR = totalOfPlayerIfSplitR + 10;
                }
            } else {
                if(topiaIfSplitRValid)
                {
                    totalOfPlayerIfAceIfSplitR += cardNumber;
                }
            }
            // Creating the new card
            GameObject addedCard = new GameObject("AddedCard" + (playersCardsIfSplitR.Count + 1) + "R"); // Creating a new game object for this card
            addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
            Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
            addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
            addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
            playersCardsIfSplitR.Add(addedCardImage); // Adding the new card to the list of the player's card

            // Displaying the cards centered on the screen
            AlignElementsFromList(playersCardsIfSplitR, -350, true);

            // Disabling the double option for later
            validForDoubleIfSplitR = false;

            // Checking the new sum
            if(CheckPlayersSum())
            {
                ShowActionBtns();
                // Updating the displayed sum of player
                if(topiaIfSplitRValid)
                {
                    playerSumTextIfSplitR.text = "(" + totalOfPlayerIfSplitR + "/" + totalOfPlayerIfAceIfSplitR + ")";
                } else {
                    playerSumTextIfSplitR.text = "(" + totalOfPlayerIfSplitR + ")";
                }
            }
        }
    }

    public void StandBtnClick()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        switch(splitIndicator)
        {
            case 0: // No split
                DealersTurn();
                break;
            case 1:
                // From left split moving to the right
                splitIndicator = 2;
                if(totalOfPlayerIfAceIfSplitR == 21) // If the player stands the left split and has a blackjack in the right then ending his turn
                {
                    DealersTurn();
                    break;
                }
                // The action buttons work now for the right side
                playerSumText.color = Color.black;
                playerSumTextIfSplitR.color = Color.red;
                ShowActionBtns();
                if(topiaValid) // Having one variable for the final player sum of cards - if either topia valid or not
                {
                    absolutePlayerTotal = totalOfPlayerIfAce;
                } else {
                    absolutePlayerTotal = totalOfPlayer;
                }
                playerSumText.text = "(" + absolutePlayerTotal + ")";
                break;
            case 2: // Ending the player's turn
                DealersTurn();
                break;
        }
    }

    public void DoubleBtnClick()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        switch(splitIndicator)
        {
            case 0:
                balance -= playerBet;
                BalanceManager.Instance.AddToBalance(-playerBet);
                playerBet *= 2;
                break;
            case 1:
                balance -= playerBet;
                BalanceManager.Instance.AddToBalance(-playerBet);
                playerBet *= 2;
                break;
            case 2:
                balance -= playerBetIfSplitR;
                BalanceManager.Instance.AddToBalance(-playerBetIfSplitR);
                playerBetIfSplitR *= 2;
                break;
        }

        // Randomizing a new card that hasn't been used yet
        int cardNumber, cardShape;
        string cardStr;

        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));

        usedCards.Add(cardStr); // Adding the newly added card to the usedCard list
        // Calculating the new sum of the player based on the added card
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }

        if(splitIndicator == 0)
        {
            totalOfPlayer += cardNumber;
            if(cardNumber == 1)
            {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                } else {
                    topiaValid = true;
                    totalOfPlayerIfAce = totalOfPlayer + 10;
                }
            } else {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                }
            }

            // Creating the new card
            GameObject addedCard = new GameObject("AddedCard" + (playersCards.Count + 1)); // Creating a new game object for this card
            addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
            Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
            addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
            addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
            playersCards.Add(addedCardImage); // Adding the new card to the list of the player's card

            // Displaying the cards centered on the screen
            AlignElementsFromList(playersCards, -350);

            // Changing the topiaValid and displaying the sum of the player accordingly
            if(topiaValid && totalOfPlayerIfAce <= 21)
            {
                playerSumText.text = "(" + totalOfPlayerIfAce + ")";
            } else // Entering here in two cases: topiaValid == false or topia > 21 ---> both cases end up the same for topiaValid and playerSumText
            {
                topiaValid = false;
                playerSumText.text = "(" + totalOfPlayer + ")";
            }

            // Finishing the player's turn.
            DealersTurn();
        } else if(splitIndicator == 1)
        {
            totalOfPlayer += cardNumber;
            if(cardNumber == 1)
            {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                } else {
                    topiaValid = true;
                    totalOfPlayerIfAce = totalOfPlayer + 10;
                }
            } else {
                if(topiaValid)
                {
                    totalOfPlayerIfAce += cardNumber;
                }
            }

            // Creating the new card
            GameObject addedCard = new GameObject("AddedCard" + (playersCards.Count + 1) + "L"); // Creating a new game object for this card
            addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
            Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
            addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
            addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
            playersCards.Add(addedCardImage); // Adding the new card to the list of the player's card

            // Displaying the cards centered on the screen
            AlignElementsFromList(playersCards, -350, false);

            // Changing the topiaValid and displaying the sum of the player accordingly
            if(topiaValid && totalOfPlayerIfAce <= 21)
            {
                playerSumText.text = "(" + totalOfPlayerIfAce + ")";
            } else // Entering here in two cases: topiaValid == false or topia > 21 ---> both cases end up the same for topiaValid and playerSumText
            {
                topiaValid = false;
                playerSumText.text = "(" + totalOfPlayer + ")";
            }

            splitIndicator = 2;
            // The action buttons work now for the right side
            playerSumText.color = Color.black;
            playerSumTextIfSplitR.color = Color.red;
        } else {
            totalOfPlayerIfSplitR += cardNumber;
            if(cardNumber == 1)
            {
                if(topiaIfSplitRValid)
                {
                    totalOfPlayerIfAceIfSplitR += cardNumber;
                } else {
                    topiaIfSplitRValid = true;
                    totalOfPlayerIfAceIfSplitR = totalOfPlayerIfSplitR + 10;
                }
            } else {
                if(topiaIfSplitRValid)
                {
                    totalOfPlayerIfAceIfSplitR += cardNumber;
                }
            }

            // Creating the new card
            GameObject addedCard = new GameObject("AddedCard" + (playersCardsIfSplitR.Count + 1) + "R"); // Creating a new game object for this card
            addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
            Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
            addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
            addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
            playersCardsIfSplitR.Add(addedCardImage); // Adding the new card to the list of the player's card

            // Displaying the cards centered on the screen
            AlignElementsFromList(playersCardsIfSplitR, -350, true);

            // Changing the topiaValid and displaying the sum of the player accordingly
            if(topiaIfSplitRValid && totalOfPlayerIfAceIfSplitR <= 21)
            {
                playerSumTextIfSplitR.text = "(" + totalOfPlayerIfAceIfSplitR + ")";
            } else // Entering here in two cases: topiaValid == false or topia > 21 ---> both cases end up the same for topiaValid and playerSumText
            {
                topiaIfSplitRValid = false;
                playerSumTextIfSplitR.text = "(" + totalOfPlayerIfSplitR + ")";
            }
            // Finishing the player's turn.
            DealersTurn();
        }
    }

    public void SplitBtnClick()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        validForSplit = false;
        
        totalOfPlayer /= 2;
        totalOfPlayerIfSplitR = totalOfPlayer;

        playersCardsIfSplitR.Add(playersCards[1]);
        playersCards.RemoveAt(1);

        playerSumText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-480, playerSumText.GetComponent<RectTransform>().anchoredPosition.y);
        playerSumTextIfSplitR.GetComponent<RectTransform>().anchoredPosition = new Vector2(480, playerSumText.GetComponent<RectTransform>().anchoredPosition.y);
        playerSumTextIfSplitR.gameObject.SetActive(true);
        playerSumText.color = Color.red; // The action buttons work for the left side

        playerBetIfSplitR = playerBet;
        balance -= playerBetIfSplitR;

        topiaValid = GetNumberFromCard(usedCards[2]) == 1;
        topiaIfSplitRValid = GetNumberFromCard(usedCards[3]) == 1;

        if(topiaValid)
        {
            totalOfPlayerIfAce = 11;
        }
        if(topiaIfSplitRValid)
        {
            totalOfPlayerIfAceIfSplitR = 11;
        }

        
        // LEFT SIDE
        splitIndicator = 1;

        // Randomizing a new card that hasn't been used yet
        int cardNumber, cardShape;
        string cardStr;

        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));

        usedCards.Add(cardStr); // Adding the newly added card to the usedCard list
        // Calculating the new sum of the player based on the added card
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }
        totalOfPlayer += cardNumber;
        if(cardNumber == 1)
        {
            if(topiaValid)
            {
                totalOfPlayerIfAce += cardNumber;
            } else {
                topiaValid = true;
                totalOfPlayerIfAce = totalOfPlayer + 10;
            }
        } else {
            if(topiaValid)
            {
                totalOfPlayerIfAce += cardNumber;
            }
        }

        // Creating the new card
        GameObject addedCard = new GameObject("AddedCard" + (playersCards.Count + 1) + "L"); // Creating a new game object for this card
        addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
        Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
        addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
        addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
        playersCards.Add(addedCardImage); // Adding the new card to the list of the player's card

        // Displaying the cards centered on the screen
        AlignElementsFromList(playersCards, -350, false);

        // Checking the new sum
        if(CheckPlayersSum())
        {
            ShowActionBtns();
            // Updating the displayed sum of player
            if(topiaValid)
            {
                playerSumText.text = "(" + totalOfPlayer + "/" + totalOfPlayerIfAce + ")";
            } else {
                playerSumText.text = "(" + totalOfPlayer + ")";
            }
        }

        // RIGHT SIDE
        splitIndicator = 2;

        // Randomizing a new card that hasn't been used yet

        do {
            cardNumber = Random.Range(1, 14);
            cardShape = Random.Range(0, 4);
            cardStr = cardNumber + "_of_" + cardShapes[cardShape];
        } while (AlreadyUsedCard(cardStr));

        usedCards.Add(cardStr); // Adding the newly added card to the usedCard list
        // Calculating the new sum of the player based on the added card
        if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
        {
            cardNumber = 10;
        }
        totalOfPlayerIfSplitR += cardNumber;
        if(cardNumber == 1)
        {
            if(topiaIfSplitRValid)
            {
                totalOfPlayerIfAceIfSplitR += cardNumber;
            } else {
                topiaIfSplitRValid = true;
                totalOfPlayerIfAceIfSplitR = totalOfPlayerIfSplitR + 10;
            }
        } else {
            if(topiaIfSplitRValid)
            {
                totalOfPlayerIfAceIfSplitR += cardNumber;
            }
        }

        // Creating the new card
        addedCard = new GameObject("AddedCard" + (playersCardsIfSplitR.Count + 1) + "R"); // Creating a new game object for this card
        addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
        addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
        addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
        addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
        playersCardsIfSplitR.Add(addedCardImage); // Adding the new card to the list of the player's card

        // Displaying the cards centered on the screen
        AlignElementsFromList(playersCardsIfSplitR, -350, true);

        // Checking the new sum
        if(CheckPlayersSum())
        {
            ShowActionBtns();
            // Updating the displayed sum of player
            if(topiaIfSplitRValid)
            {
                playerSumTextIfSplitR.text = "(" + totalOfPlayerIfSplitR + "/" + totalOfPlayerIfAceIfSplitR + ")";
            } else {
                playerSumTextIfSplitR.text = "(" + totalOfPlayerIfSplitR + ")";
            }
        }

        if(totalOfPlayerIfAce != 21) // If the player gets a blackjack in the left split then moving the split Indicator should stay 2.
        {
            // The player starts with the left side
            splitIndicator = 1;
        }
    }

    // Checking the player's sum. Returning true if the game goes on (for the player), false if it's the dealer's turn (in most cases).
    private bool CheckPlayersSum()
    {
        switch(splitIndicator)
        {
            case 0:
                if(topiaValid) // If the player has an ace
                {
                    if(totalOfPlayerIfAce > 21) // If the sum of the ace is more then 21 then we focus on the regular sum (ace as 1) and the player can still hit
                    {
                        topiaValid = false;
                    } else if(totalOfPlayerIfAce == 21) // If it's equalls 21 then the player has reached the maximum possible and we focus on the special sum (ace as 11)
                    {
                        topiaValid = true;
                        DealersTurn();
                        return false;
                    } else // If it's still below 21 then still focusing on the special sum and the player can still hit
                    {
                        topiaValid = true;
                    }
                } else { // If the player doesn't has an ace
                    if(totalOfPlayer >= 21) // If he reached 21 or higher then he can no longer hit
                    {
                        DealersTurn();
                        return false;
                    }
                }
                break;
            case 1:
                if(topiaValid) // If the player has an ace
                {
                    if(totalOfPlayerIfAce > 21) // If the sum of the ace is more then 21 then we focus on the regular sum (ace as 1) and the player can still hit
                    {
                        topiaValid = false;
                    } else if(totalOfPlayerIfAce == 21) // If it's equalls 21 then the player has reached the maximum possible and we focus on the special sum (ace as 11)
                    {
                        topiaValid = true;
                        splitIndicator = 2;
                        // The action buttons work now for the right side
                        playerSumText.color = Color.black;
                        playerSumTextIfSplitR.color = Color.red;
                        playerSumText.text = "(21)";
                        return false;
                    } else // If it's below 21 then focusing on the special sum and the player can hit
                    {
                        topiaValid = true;
                    }
                } else { // If the player doesn't has an ace
                    if(totalOfPlayer >= 21) // If he reached 21 or higher then he can no longer hit
                    {
                        splitIndicator = 2;
                        // The action buttons work now for the right side
                        playerSumText.color = Color.black;
                        playerSumTextIfSplitR.color = Color.red;
                        playerSumText.text = "(" + totalOfPlayer + ")";
                        return false;
                    }
                }
                break;
            case 2:
                if(topiaIfSplitRValid) // If the player has an ace
                {
                    if(totalOfPlayerIfAceIfSplitR > 21) // If the sum of the ace is more then 21 then we focus on the regular sum (ace as 1) and the player can still hit
                    {
                        topiaIfSplitRValid = false;
                    } else if(totalOfPlayerIfAceIfSplitR == 21) // If it's equalls 21 then the player has reached the maximum possible and we focus on the special sum (ace as 11)
                    {
                        topiaIfSplitRValid = true;
                        if(totalOfPlayerIfAce != 21) // If the player gets a blackjack in the right side and not in the left at the creation of the split cards
                        {
                            splitIndicator = 1;
                            playerSumTextIfSplitR.text = "(21)";
                        } else {
                            DealersTurn();
                        }
                        return false;
                    } else // If it's  below 21 then  focusing on the special sum and the player can hit
                    {
                        topiaIfSplitRValid = true;
                    }
                } else { // If the player doesn't has an ace
                    if(totalOfPlayerIfSplitR >= 21) // If he reached 21 or higher then he can no longer hit
                    {
                        DealersTurn();
                        return false;
                    }
                }
                break;
        }
        return true;
    }

    // Checking the dealer's sum. Returning true if the game goes on (for the dealer), false if the game has ended.
    private bool CheckDealersSum()
    {
        if(todiaValid) // If the dealer has an ace
        {
            if(totalOfDealerIfAce > 21) // If the sum of the ace is more then 21 then we focus on the regular sum (ace as 1) and the dealer can still get cards
            {
                todiaValid = false;
            } else if(totalOfDealerIfAce >= 17 && totalOfDealerIfAce <= 21) // If it's in the range 17-21 then the dealer has its final sum (todia) and the game has ended
            {
                todiaValid = true;
                ResultScreen();
                return false;
            } else // If it's still 16 and below then still focusing on the special sum and the dealet can still get cards
            {
                todiaValid = true;
            }
        } else { // If the dealer doesn't has an ace
            if(totalOfDealer >= 17) // If he reached 17 or higher then he can no longer get cards
            {
                ResultScreen();
                return false;
            }
        }
        return true;
    }

    private void AlignElementsFromList<T>(List<T> elements, int yPosition) where T : Component
    {
        // Displaying the elements in the list on the screen - while centering them nicely.

        // Calculating the width of the elements including the spaces between each two elements
        float totalWidth = 0;
        for(int i=0;i<elements.Count;i++)
        {
            totalWidth += elements[i].GetComponent<RectTransform>().sizeDelta.x;
        }
        totalWidth += 50 * (elements.Count - 1);

        // The start x position for the first element (negative because it starts left then the middle)
        float startX = totalWidth / -2;

        // Displaying the elements
        for(int i=0;i<elements.Count;i++)
        {
            elements[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + elements[i].GetComponent<RectTransform>().sizeDelta.x / 2, yPosition);
            startX += elements[i].GetComponent<RectTransform>().sizeDelta.x + 50; // Calculating the x position for the next button
        }
    }
    
    private void AlignElementsFromList<T>(List<T> elements, int yPosition, bool rightHalf) where T : Component
    {
        // Displaying the elements in the list on half of the screen - while centering them nicely (inside the half).

        float spacing = 50;
        // Calculate new height and width of the cards. Because its supposed to be only on half the screen I have a limited space.
        float newHeight = 304.92f;
        if(elements.Count == 2 || elements.Count == 3)
        {
            // Need this empty block so that the else block wont be activated for elements.Count = 2
        } else if(elements.Count == 4)
        {
            newHeight *= 0.75f;
            spacing *= 0.75f;
        } else if(elements.Count == 5)
        {
            newHeight *= 2f/3f;
            spacing *= 2f/3f;
        } else {
            newHeight *= 0.5f;
            spacing *= 0.5f;
        }
        // The height:width ratio of the cards is 1.452
        float newWidth = newHeight / 1.452f;

        // Calculating the width of the elements including the spaces between each two elements
        float totalWidth = 0;
        for(int i=0;i<elements.Count;i++)
        {
            totalWidth += newWidth;
        }
        totalWidth += spacing * (elements.Count - 1);

        // The start x position for the first element. The x = 0 in the right hlaf is 480, and in the left is -480.
        float startX;
        if(rightHalf)
        {
            startX = 480 - (totalWidth / 2);
        } else {
            startX = -480 - (totalWidth / 2);
        }

        RectTransform rectTransform;

        // Displaying the elements
        for(int i=0;i<elements.Count;i++)
        {
            rectTransform = elements[i].GetComponent<RectTransform>();
            
            rectTransform.anchoredPosition = new Vector2(startX + newWidth / 2, yPosition);

            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            
            // Calculating the x position for the next button
            startX += newWidth + spacing;
        }
    }

    private void DealersTurn()
    {
        actionButtonsParent.SetActive(false);
        nextSkipParent.SetActive(true);

        List<Button> nextSkipButtons = new List<Button>() {NextBtn, SkipBtn};
        NextBtn.GetComponentInChildren<Text>().color = Color.black;
        SkipBtn.GetComponentInChildren<Text>().color = Color.black;

        AlignElementsFromList(nextSkipButtons, 0);

        // Making sure both are black
        playerSumTextIfSplitR.color = Color.black;
        playerSumText.color = Color.black;

        if(splitIndicator == 0 || splitIndicator == 1)
        {
            if(topiaValid) // Having one variable for the final player sum of cards - if either topia valid or not
            {
                absolutePlayerTotal = totalOfPlayerIfAce;
            } else {
                absolutePlayerTotal = totalOfPlayer;
            }
            playerSumText.text = "(" + absolutePlayerTotal + ")";
        } else {
            if(topiaIfSplitRValid) // Having one variable for the final player sum of cards - if either topia valid or not
            {
                absolutePlayerTotalIfSplitR = totalOfPlayerIfAceIfSplitR;
            } else {
                absolutePlayerTotalIfSplitR = totalOfPlayerIfSplitR;
            }
            playerSumTextIfSplitR.text = "(" + absolutePlayerTotalIfSplitR + ")";
        }

        if(absolutePlayerTotal > 21 && absolutePlayerTotalIfSplitR > 21) // absolutePlayerTotalIfSplitR default is 22, so if there is no split the right side of the statement would always be true
        {
            DealersCloseCard.sprite = Resources.Load<Sprite>("Cards/" + usedCards[1]);
            ResultScreen();
        }
    }

    public void NextBtnClick()
    {
        if(DealersCloseCard.sprite == Cardback.Instance.GetSelectedCardBack())
        {
            DealersCloseCard.sprite = Resources.Load<Sprite>("Cards/" + usedCards[1]);
        }
        else if(todiaValid && totalOfDealerIfAce <= 16 || !todiaValid && totalOfDealer <= 16)
        {
            // Randomizing a new card that hasn't been used yet
            int cardNumber, cardShape;
            string cardStr;

            do {
                cardNumber = Random.Range(1, 14);
                cardShape = Random.Range(0, 4);
                cardStr = cardNumber + "_of_" + cardShapes[cardShape];
            } while (AlreadyUsedCard(cardStr));

            usedCards.Add(cardStr); // Adding the newly added card to the usedCard list
            // Calculating the new sum of the player based on the added card
            if(cardNumber == 11 || cardNumber == 12 || cardNumber == 13)
            {
                cardNumber = 10;
            }
            totalOfDealer += cardNumber;

            if(cardNumber == 1)
            {
                if(todiaValid)
                {
                    totalOfDealerIfAce += cardNumber;
                } else {
                    todiaValid = true;
                    totalOfDealerIfAce = totalOfDealer + 10;
                }
            } else {
                if(todiaValid)
                {
                    totalOfDealerIfAce += cardNumber;
                }
            }

            if(totalOfDealerIfAce > 21)
            {
                todiaValid = false;
            }

            // Creating the new card
            GameObject addedCard = new GameObject("AddedCard" + (dealersCards.Count + 1) + "D"); // Creating a new game object for this card
            addedCard.transform.SetParent(addedCards.transform, false); // Assigning the game object in the "AddedCards" game object (as it's child)
            Image addedCardImage = addedCard.AddComponent<Image>(); // Creating an image component to the added card game object
            addedCardImage.sprite = Resources.Load<Sprite>("Cards/" + cardStr); // Setting the image of the added card game object to be the card
            addedCard.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 304.92f); // Changing the size of the ocject
            dealersCards.Add(addedCardImage); // Adding the new card to the list of the player's card

            // Displaying the cards centered on the screen
            AlignElementsFromList(dealersCards, 350);
        }

        if(CheckDealersSum())
        {
            // Updating the displayed sum of the dealer
            if(todiaValid)
            {
                dealerSumText.text = "(" + totalOfDealer + "/" + totalOfDealerIfAce + ")";
            } else {
                dealerSumText.text = "(" + totalOfDealer + ")";
            }   
        }
    }

    public void SkipBtnClick()
    {
        while(!resultMessage.gameObject.activeSelf)
        {
            NextBtnClick();
        }
    }

    private void ResultScreen()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if(todiaValid) // Having one variable for the final dealer sum of cards - if either todia valid or not
        {
            absoluteDealerTotal = totalOfDealerIfAce;
        } else {
            absoluteDealerTotal = totalOfDealer;
        }
        dealerSumText.text = "(" + absoluteDealerTotal + ")";

        nextSkipParent.SetActive(false);
        resultMessage.gameObject.SetActive(true);
        backBtn.gameObject.SetActive(true);
        if(splitIndicator == 0)
        {
            backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(815.9f, -465.4f);
        } else {
            backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -115f);
        }

        if(topiaValid && totalOfPlayerIfAce == 21 && playersCards.Count == 2)
        {
            resultMessage.color = Color.green;
            resultMessage.text = "You Won!";
            balance += (int) (playerBet * 2.5);
        }
        else if(absolutePlayerTotal > 21)
        {
            resultMessage.color = new Color(104f / 255f, 0, 0);
            resultMessage.text = "You lost!";
        }
        else if(absoluteDealerTotal > 21 || absoluteDealerTotal < absolutePlayerTotal)
        {
            resultMessage.color = Color.green;
            resultMessage.text = "You Won!";
            balance += playerBet * 2;
        } else if(absoluteDealerTotal > absolutePlayerTotal)
        {
            resultMessage.color = new Color(104f / 255f, 0, 0);
            resultMessage.text = "You lost!";
        } else if(absoluteDealerTotal == absolutePlayerTotal)
        {
            resultMessage.color = new Color(82f / 255f, 82f / 255f, 82f / 255f);
            resultMessage.text = "It's a tie!";
            balance += playerBet;
        }

        if(splitIndicator == 2) {
            resultMessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-480, 0);
            resultMessageIfSplitR.GetComponent<RectTransform>().anchoredPosition = new Vector2(480, 0);
            resultMessageIfSplitR.gameObject.SetActive(true);

            if(topiaIfSplitRValid && totalOfPlayerIfAceIfSplitR == 21)
            {
                resultMessageIfSplitR.color = Color.green;
                resultMessageIfSplitR.text = "You Won!";
                balance += (int) (playerBetIfSplitR * 2.5);
            }
            else if(absolutePlayerTotalIfSplitR > 21)
            {
                resultMessageIfSplitR.color = new Color(104f / 255f, 0, 0);
                resultMessageIfSplitR.text = "You lost!";
            }
            else if(absoluteDealerTotal > 21 || absoluteDealerTotal < absolutePlayerTotalIfSplitR)
            {
                resultMessageIfSplitR.color = Color.green;
                resultMessageIfSplitR.text = "You Won!";
                balance += playerBetIfSplitR * 2;
            } else if(absoluteDealerTotal > absolutePlayerTotalIfSplitR)
            {
                resultMessageIfSplitR.color = new Color(104f / 255f, 0, 0);
                resultMessageIfSplitR.text = "You lost!";
            } else if(absoluteDealerTotal == absolutePlayerTotalIfSplitR)
            {
                resultMessageIfSplitR.color = new Color(82f / 255f, 82f / 255f, 82f / 255f);
                resultMessageIfSplitR.text = "It's a tie!";
                balance += playerBetIfSplitR;
            }
        }

        BalanceManager.Instance.SetBalance(balance);

        NextHandBtn.gameObject.SetActive(true);
        if(splitIndicator == 0)
        {
            NextHandBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-666, 0);
        } else {
            NextHandBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-666, 115);
        }
        NextHandBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(546, NextHandBtn.GetComponent<RectTransform>().sizeDelta.y);
        NextHandText.fontSize = 150;
        NextHandText.text = "Next Hand";
        NextHandText.color = Color.black;
        if(playerBet > balance)
        {
            NextHandBtn.interactable = false;
            NextHandText.color = NextHandBtn.colors.disabledColor;
            NextHandBtn.GetComponent<EventTrigger>().enabled = false;
            BtnHoverAction.SetColorChangeEnabled(false);
        }

        balanceText.gameObject.SetActive(true);
        balanceText.text = "Balance: " + ShowBalance.FullBalance(balance);
        if(splitIndicator == 0)
        {
            balanceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(789, 0);
        } else {
            balanceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(789, 115);
        }
        balanceText.fontSize = 130;
        balanceText.alignment = TextAnchor.MiddleRight;
    }
}