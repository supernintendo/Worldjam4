
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace AoinuWorks.VRCDarts
{
    [DefaultExecutionOrder(1)]
    public class VRCDartsMonitor : UdonSharpBehaviour
    {
        [SerializeField] VRCDartsMachine machine;

        [Header("Message Box")]
        [SerializeField] TextMeshProUGUI messageBox;
        [Header("Player Names")]
        [SerializeField] TextMeshProUGUI currentPlayerName;
        [SerializeField] TextMeshProUGUI[] player1Names;
        [SerializeField] TextMeshProUGUI[] player2Names;
        [SerializeField] TextMeshProUGUI[] player3Names;
        [SerializeField] TextMeshProUGUI[] player4Names;
        [Header("Player Scores")]
        [SerializeField] TextMeshProUGUI currentPlayerScore;
        [SerializeField] TextMeshProUGUI player1Score;
        [SerializeField] TextMeshProUGUI player2Score;
        [SerializeField] TextMeshProUGUI player3Score;
        [SerializeField] TextMeshProUGUI player4Score;
        [Header("Round Scores")]
        [SerializeField] TextMeshProUGUI[] roundNumbers;
        [SerializeField] TextMeshProUGUI[] roundScores;
        [Header("Darts Result")]
        [SerializeField] TextMeshProUGUI[] dartsResult;
        [Header("Rules")]
        [SerializeField] TextMeshProUGUI ruleName;
        [SerializeField] TextMeshProUGUI round;
        [SerializeField] TextMeshProUGUI[] settingOptions;

        [Header("Cricket - Score Board Marks")]
        [SerializeField] GameObject[] player1Marks20;
        [SerializeField] GameObject[] player1Marks19;
        [SerializeField] GameObject[] player1Marks18;
        [SerializeField] GameObject[] player1Marks17;
        [SerializeField] GameObject[] player1Marks16;
        [SerializeField] GameObject[] player1Marks15;
        [SerializeField] GameObject[] player1MarksBull;
        [SerializeField] GameObject[] player2Marks20;
        [SerializeField] GameObject[] player2Marks19;
        [SerializeField] GameObject[] player2Marks18;
        [SerializeField] GameObject[] player2Marks17;
        [SerializeField] GameObject[] player2Marks16;
        [SerializeField] GameObject[] player2Marks15;
        [SerializeField] GameObject[] player2MarksBull;

        [Header("Cricket - Round Marks")]
        [SerializeField] GameObject[] slot1Dart1Marks;
        [SerializeField] GameObject[] slot1Dart2Marks;
        [SerializeField] GameObject[] slot1Dart3Marks;

        [SerializeField] GameObject[] slot2Dart1Marks;
        [SerializeField] GameObject[] slot2Dart2Marks;
        [SerializeField] GameObject[] slot2Dart3Marks;

        [SerializeField] GameObject[] slot3Dart1Marks;
        [SerializeField] GameObject[] slot3Dart2Marks;
        [SerializeField] GameObject[] slot3Dart3Marks;

        [SerializeField] GameObject[] slot4Dart1Marks;
        [SerializeField] GameObject[] slot4Dart2Marks;
        [SerializeField] GameObject[] slot4Dart3Marks;

        [SerializeField] GameObject[] slot5Dart1Marks;
        [SerializeField] GameObject[] slot5Dart2Marks;
        [SerializeField] GameObject[] slot5Dart3Marks;

        [Header("Cricket - Closed Panel")]
        [SerializeField] GameObject closedPanel20;
        [SerializeField] GameObject closedPanel19;
        [SerializeField] GameObject closedPanel18;
        [SerializeField] GameObject closedPanel17;
        [SerializeField] GameObject closedPanel16;
        [SerializeField] GameObject closedPanel15;
        [SerializeField] GameObject closedPanelBull;

        Animator animator;

        const uint PAGE = 0U;
        const uint RULE = 1U;
        const uint SETTING = 2U;
        const uint PLAYERS = 3U;
        const uint ROUND = 4U;
        const uint TURN = 5U;
        const uint DART = 6U;
        const uint PLAYER1_SCORE = 7U;
        const uint PLAYER2_SCORE = 8U;
        const uint PLAYER3_SCORE = 9U;
        const uint PLAYER4_SCORE = 10U;
        const uint IN_GAME = 11U;

        const uint RULE_COUNT_UP = 1U;
        const uint RULE_01_GAMES = 2U;
        const uint RULE_CRICKET = 3U;

        void Start()
        {
            animator = gameObject.GetComponent<Animator>();
        }

        public void _UpdateState()
        {
            if (animator == null) return;
            currentPlayerName.text = machine.currentPlayerName;
            foreach (var p in player1Names)
            {
                p.text = machine.playerNames[0];
            }
            foreach (var p in player2Names)
            {
                p.text = machine.playerNames[1];
            }
            foreach (var p in player3Names)
            {
                p.text = machine.playerNames[2];
            }
            foreach (var p in player4Names)
            {
                p.text = machine.playerNames[3];
            }
            currentPlayerScore.text = machine.currentPlayerScore.ToString();
            player1Score.text = machine.gameStateData[PLAYER1_SCORE].ToString();
            player2Score.text = machine.gameStateData[PLAYER2_SCORE].ToString();
            player3Score.text = machine.gameStateData[PLAYER3_SCORE].ToString();
            player4Score.text = machine.gameStateData[PLAYER4_SCORE].ToString();
            var currentRoundNumber = machine.gameStateData[ROUND];
            for (int i = 0, round = (int)currentRoundNumber; i < roundNumbers.Length; i++)
            {
                if (round - i <= 0)
                {
                    roundNumbers[i].text = "-";
                }
                else
                {
                    roundNumbers[i].text = $"R{round - i}";
                }
            }

            if (machine.gameStateData[RULE] == RULE_CRICKET)
            {
                for (int i = 0; i < roundScores.Length; i++)
                {
                    SetRoundMarks(i, machine.currentPlayerRoundScores[i]);
                }

                SetMarks(player1Marks20, machine.player1CricketNumbers[0]);
                SetMarks(player2Marks20, machine.player2CricketNumbers[0]);
                SetClosePanel(closedPanel20, machine.player1CricketNumbers[0], machine.player2CricketNumbers[0]);

                SetMarks(player1Marks19, machine.player1CricketNumbers[1]);
                SetMarks(player2Marks19, machine.player2CricketNumbers[1]);
                SetClosePanel(closedPanel19, machine.player1CricketNumbers[1], machine.player2CricketNumbers[1]);

                SetMarks(player1Marks18, machine.player1CricketNumbers[2]);
                SetMarks(player2Marks18, machine.player2CricketNumbers[2]);
                SetClosePanel(closedPanel18, machine.player1CricketNumbers[2], machine.player2CricketNumbers[2]);

                SetMarks(player1Marks17, machine.player1CricketNumbers[3]);
                SetMarks(player2Marks17, machine.player2CricketNumbers[3]);
                SetClosePanel(closedPanel17, machine.player1CricketNumbers[3], machine.player2CricketNumbers[3]);

                SetMarks(player1Marks16, machine.player1CricketNumbers[4]);
                SetMarks(player2Marks16, machine.player2CricketNumbers[4]);
                SetClosePanel(closedPanel16, machine.player1CricketNumbers[4], machine.player2CricketNumbers[4]);

                SetMarks(player1Marks15, machine.player1CricketNumbers[5]);
                SetMarks(player2Marks15, machine.player2CricketNumbers[5]);
                SetClosePanel(closedPanel15, machine.player1CricketNumbers[5], machine.player2CricketNumbers[5]);

                SetMarks(player1MarksBull, machine.player1CricketNumbers[6]);
                SetMarks(player2MarksBull, machine.player2CricketNumbers[6]);
                SetClosePanel(closedPanelBull, machine.player1CricketNumbers[6], machine.player2CricketNumbers[6]);
            }
            else
            {
                for (int i = 0; i < roundScores.Length; i++)
                {
                    roundScores[i].text = machine.currentPlayerRoundScores[i] == 0 ? "-" : machine.currentPlayerRoundScores[i].ToString();
                }
            }

            for (int i = 0; i < dartsResult.Length; i++)
            {
                dartsResult[i].text = machine.dartResults[i];
            }
            switch (machine.gameStateData[RULE])
            {
                case RULE_COUNT_UP:
                    ruleName.text = "COUNT-UP";
                    settingOptions[0].text = "5 ROUNDS";
                    settingOptions[1].text = "8 ROUNDS";
                    settingOptions[2].text = "10 ROUNDS";
                    settingOptions[3].text = "15 ROUNDS";
                    break;
                case RULE_01_GAMES:
                    ruleName.text = "01 GAMES";
                    settingOptions[0].text = "301";
                    settingOptions[1].text = "501";
                    settingOptions[2].text = "701";
                    settingOptions[3].text = "901";
                    break;
                case RULE_CRICKET:
                    ruleName.text = "CRICKET";
                    foreach (var setting in settingOptions)
                    {
                        setting.text = "";
                    }
                    break;
            }
            var currentRound = machine.gameStateData[ROUND] + 1;
            currentRound = currentRound > machine.maxRounds ? machine.maxRounds : currentRound;
            round.text = $"ROUND {currentRound} /{machine.maxRounds}";

            animator.SetInteger("Page", (int)machine.gameStateData[PAGE]);
            animator.SetInteger("Rule", (int)machine.gameStateData[RULE]);
            animator.SetInteger("nPlayers", (int)machine.gameStateData[PLAYERS] + 1);
            animator.SetInteger("Round", (int)machine.gameStateData[ROUND]);
            animator.SetInteger("Turn", (int)machine.gameStateData[TURN]);
            animator.SetInteger("Darts", (int)machine.gameStateData[DART]);
        }

        public void _ShowMessage(string message)
        {
            messageBox.text = message;
            animator.SetBool("MessageBox", true);
        }

        public void _HideMessage()
        {
            animator.SetBool("MessageBox", false);
        }

        void SetMarks(GameObject[] marksObj, int mark)
        {
            for (int i = 0; i < marksObj.Length; i++)
            {
                if (i == mark)
                {
                    marksObj[i].SetActive(true);
                }
                else
                {
                    marksObj[i].SetActive(false);
                }
            }
        }

        void SetClosePanel(GameObject panel, int player1Marks, int player2Marks)
        {
            if (player1Marks >= 3 && player2Marks >= 3)
            {
                panel.SetActive(true);
            }
            else
            {
                panel.SetActive(false);
            }
        }

        void SetRoundMarks(int slot, int score)
        {
            int dart1Mark = score / 100;
            int dart2Mark = (score - dart1Mark*100) / 10;
            int dart3Mark = (score - dart1Mark*100 - dart2Mark*10);
            switch (slot)
            {
                case 0:
                    SetMarks(slot1Dart1Marks, dart1Mark);
                    SetMarks(slot1Dart2Marks, dart2Mark);
                    SetMarks(slot1Dart3Marks, dart3Mark);
                    break;
                case 1:
                    SetMarks(slot2Dart1Marks, dart1Mark);
                    SetMarks(slot2Dart2Marks, dart2Mark);
                    SetMarks(slot2Dart3Marks, dart3Mark);
                    break;
                case 2:
                    SetMarks(slot3Dart1Marks, dart1Mark);
                    SetMarks(slot3Dart2Marks, dart2Mark);
                    SetMarks(slot3Dart3Marks, dart3Mark);
                    break;
                case 3:
                    SetMarks(slot4Dart1Marks, dart1Mark);
                    SetMarks(slot4Dart2Marks, dart2Mark);
                    SetMarks(slot4Dart3Marks, dart3Mark);
                    break;
                case 4:
                    SetMarks(slot5Dart1Marks, dart1Mark);
                    SetMarks(slot5Dart2Marks, dart2Mark);
                    SetMarks(slot5Dart3Marks, dart3Mark);
                    break;
            }
        }
    }

}
