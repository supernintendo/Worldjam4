
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System;
using VRC.Udon.Common;

namespace AoinuWorks.VRCDarts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VRCDartsMachine : UdonSharpBehaviour
    {
        [Header("Monitor Objects")]
        [SerializeField] VRCDartsMonitor[] monitors;
        [Header("Entry System")]
        [SerializeField] VRCDartsPlayerEntrySystem entrySystem;
        [Header("Event Listener")]
        [SerializeField] UdonSharpBehaviour eventListener;

        [Header("Animators")]
        [SerializeField] Animator boardAnimator;
        [SerializeField] Animator controllerAnimator;

        [Header("Darts")]
        [SerializeField] VRCDart[] darts;
        [SerializeField] Transform[] dartsResetPositions;

        [Header("Darts Board Profile")]
        public Transform center;
        [SerializeField] float innerBullRadius = 0.008f;
        [SerializeField] float outerBullRadius = 0.022f;
        [SerializeField] float innerSingleRadius = 0.105f;
        [SerializeField] float tripleRadius = 0.124f;
        [SerializeField] float outerSingleRadius = 0.177f;
        [SerializeField] float doubleRadius = 0.197f;

        [Header("Sound Effects")]
        [SerializeField] AudioSource ConfirmSound;
        [SerializeField] AudioSource SelectSound;
        [SerializeField] AudioSource CancelSound;
        [SerializeField] AudioSource SingleHitSound;
        [SerializeField] AudioSource DoubleHitSound;
        [SerializeField] AudioSource TripleHitSound;
        [SerializeField] AudioSource BullHitSound;
        [SerializeField] AudioSource InBullHitSound;

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
        readonly uint[] DATA_SIZES = new uint[]
        {
        3,  // PAGE
        2,  // RULE
        3,  // SETTING
        2,  // PLAYERS
        4,  // ROUND
        2,  // TURN
        2,  // DART
        11, // PLAYER1 SCORE
        11, // PLAYER2 SCORE
        11, // PLAYER3 SCORE
        11, // PLAYER4 SCORE
        1   // IN_GAME
        };
        [UdonSynced] byte[] encodedGameStateData = new byte[8];
        [NonSerialized] public uint[] gameStateData = new uint[12];
        uint[] previousGameStateData = new uint[12];
        bool stateChanged = false;

        [NonSerialized] public int[] player1CricketNumbers = new int[7];
        [NonSerialized] public int[] player2CricketNumbers = new int[7];
        [UdonSynced] int cricketBoardState = 0;
        const int CRICKET_NUMBER_20 = 0;
        const int CRICKET_NUMBER_19 = 1;
        const int CRICKET_NUMBER_18 = 2;
        const int CRICKET_NUMBER_17 = 3;
        const int CRICKET_NUMBER_16 = 4;
        const int CRICKET_NUMBER_15 = 5;
        const int CRICKET_NUMBER_BULL = 6;

        const uint PAGE_INITIAL = 0U;
        const uint PAGE_RULE = 1U;
        const uint PAGE_SETTING = 2U;
        const uint PAGE_ENTRY = 3U;
        const uint PAGE_GAME = 4U;

        const uint RULE_COUNT_UP = 1U;
        const uint RULE_01_GAMES = 2U;
        const uint RULE_CRICKET = 3U;

        const uint SETTING_COUNTUP_5ROUNDS = 1U;
        const uint SETTING_COUNTUP_8ROUNDS = 2U;
        const uint SETTING_COUNTUP_10ROUNDS = 3U;
        const uint SETTING_COUNTUP_15ROUNDS = 4U;
        const uint SETTING_01GAMES_301 = 1U; // (MAX 15ROUNDs)
        const uint SETTING_01GAMES_501 = 2U;
        const uint SETTING_01GAMES_701 = 3U;
        const uint SETTING_01GAMES_1001 = 4U;

        const uint BUTTON_MAIN = 0U;
        const uint BUTTON_1 = 1U;
        const uint BUTTON_2 = 2U;
        const uint BUTTON_3 = 3U;
        const uint BUTTON_4 = 4U;
        const uint BUTTON_CANCEL = 5U;

        const string ANIM_BOOL_ENABLED = "Enabled";
        const string ANIM_BOOL_CRICKET_MODE = "CricketMode";
        const string ANIM_TRIG_BULL = "Bull";
        const string ANIM_TRIG_WIPE = "Wipe";

        const string ANIM_INT_MAIN_BUTTON = "MainButton";
        const string ANIM_INT_SUB_BUTTON = "SubButton";
        const string ANIM_INT_BUTTON1 = "Button1";
        const string ANIM_INT_BUTTON2 = "Button2";
        const string ANIM_INT_BUTTON3 = "Button3";
        const string ANIM_INT_BUTTON4 = "Button4";
        const string ANIM_INT_CRICKET_20 = "Cricket20";
        const string ANIM_INT_CRICKET_19 = "Cricket19";
        const string ANIM_INT_CRICKET_18 = "Cricket18";
        const string ANIM_INT_CRICKET_17 = "Cricket17";
        const string ANIM_INT_CRICKET_16 = "Cricket16";
        const string ANIM_INT_CRICKET_15 = "Cricket15";
        const string ANIM_INT_CRICKET_BULL = "CricketBull";
        const int ANIM_BUTTON_STATE_DISABLED = 0;
        const int ANIM_BUTTON_STATE_FLUSHING = 1;
        const int ANIM_BUTTON_STATE_ENABLED = 2;
        const int ANIM_CRICKET_STATE_NOT_OPENED = 0;
        const int ANIM_CRICKET_STATE_PLAYER1 = 1;
        const int ANIM_CRICKET_STATE_PLAYER2 = 2;
        const int ANIM_CRICKET_STATE_CLOSED = 3;

        [HideInInspector] public uint currentPlayerScore;
        [HideInInspector] public uint currentDart;
        [HideInInspector] public string[] dartResults = new string[] { "", "", "" };
        [HideInInspector] public int[] currentPlayerRoundScores = new int[5];
        [HideInInspector] public string[] playerNames = new string[] { "Player1", "Player2", "Player3", "Player4" };
        [HideInInspector] public string currentPlayerName = "Player";
        [HideInInspector] public string message = "";
        [HideInInspector] public uint maxRounds;
        bool isBursted = false;
        bool inGame = false;

        ushort currentRoundScore;
        [UdonSynced] ushort[] player1RoundScores = new ushort[5];
        [UdonSynced] ushort[] player2RoundScores = new ushort[5];
        [UdonSynced] ushort[] player3RoundScores = new ushort[5];
        [UdonSynced] ushort[] player4RoundScores = new ushort[5];

        const string MESSAGE_INITIAL_PAGE = "PRESS ANY BUTTON";

        void Start()
        {
            currentPlayerRoundScores = new int[5];
            gameStateData = new uint[12];
            previousGameStateData = new uint[12];
            player1CricketNumbers = new int[7];
            player2CricketNumbers = new int[7];
            if (center == null)
            {
                center = transform;
            }
            boardAnimator.SetBool(ANIM_BOOL_ENABLED, false);

            if (eventListener != null)
            {
                Debug.Log("External Event Listener is Enabled.");
                entrySystem.vrcDartsEventListener = (VRCDartsEventListener)eventListener;
            }
            else
            {
                Debug.Log("Listener is not set.");
            }
        }

        public void _OnDartsHit(Vector3 point)
        {
            var dir = point - center.position;
            dir = Vector3.ProjectOnPlane(dir, center.forward);
            var distance = Vector3.Cross(center.forward, dir).magnitude;
            var angle = Vector3.SignedAngle(center.up, dir, center.forward);
            var ring = ComputeRing(distance);
            var number = ComputeNumber(angle);

            if (gameStateData[PAGE] == PAGE_INITIAL)
            {
                message = $"{DartResult(ring, number)}\nDistance from center: {distance * 1000f} mm";
                controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_FLUSHING);
                UpdateMonitors();
            }
            else if (gameStateData[PAGE] == PAGE_GAME)
            {
                NotifyHit(ring, number);
            }
        }

        public void OnPressedMainButton()
        {
            if (Networking.IsOwner(gameObject))
            {
                OnButtonPressed(BUTTON_MAIN);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnPressedMainButton));
            }
            controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_DISABLED);
            UpdateMonitors();
        }

        public void OnPressedCancelButtton()
        {
            if (Networking.IsOwner(gameObject))
            {
                OnButtonPressed(BUTTON_CANCEL);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnPressedCancelButtton));
            }
        }

        public void OnPressedButton1()
        {
            if (Networking.IsOwner(gameObject) || gameStateData[PAGE] == PAGE_ENTRY)
            {
                OnButtonPressed(BUTTON_1);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnPressedButton1));
            }
        }

        public void OnPressedButton2()
        {
            if (Networking.IsOwner(gameObject) || gameStateData[PAGE] == PAGE_ENTRY)
            {
                OnButtonPressed(BUTTON_2);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnPressedButton2));
            }
        }

        public void OnPressedButton3()
        {
            if (Networking.IsOwner(gameObject) || gameStateData[PAGE] == PAGE_ENTRY)
            {
                OnButtonPressed(BUTTON_3);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnPressedButton3));
            }
        }

        public void OnPressedButton4()
        {
            if (Networking.IsOwner(gameObject) || gameStateData[PAGE] == PAGE_ENTRY)
            {
                OnButtonPressed(BUTTON_4);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnPressedButton4));
            }
        }

        public void UpdateNumOfPlayer()
        {
            var nPlayers = entrySystem.GetNumOfEntryPlayers();
            if (nPlayers > gameStateData[PLAYERS]) SelectSound.Play();

            controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_FLUSHING);
            controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_FLUSHING);
            controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_FLUSHING);
            controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_FLUSHING);
            if(nPlayers > 0) controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_ENABLED);
            if(nPlayers > 1) controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_ENABLED);
            if(nPlayers > 2) controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_ENABLED);
            if(nPlayers > 3) controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_ENABLED);

            if (Networking.IsOwner(gameObject))
            {
                gameStateData[PLAYERS] = nPlayers == 0 ? 0U : nPlayers - 1U;
                EncodeGameState();
            }

            UpdatePlayerNames();
        }

        void OnButtonPressed(uint button)
        {
            switch (gameStateData[PAGE])
            {
                case PAGE_INITIAL:
                    gameStateData[PAGE] = PAGE_RULE;
                    EncodeGameState();
                    return;
                case PAGE_RULE:
                    switch (button)
                    {
                        case BUTTON_1:
                            gameStateData[RULE] = RULE_COUNT_UP;
                            gameStateData[PAGE] = PAGE_SETTING;
                            break;
                        case BUTTON_2:
                            gameStateData[RULE] = RULE_01_GAMES;
                            gameStateData[PAGE] = PAGE_SETTING;
                            break;
                        case BUTTON_3:
                            gameStateData[RULE] = RULE_CRICKET;
                            gameStateData[PAGE] = PAGE_ENTRY;
                            _PlayConfirmSound();
                            break;
                        case BUTTON_CANCEL:
                            ClearState();
                            gameStateData[PAGE] = PAGE_INITIAL;
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowInitialPageMessage));
                            break;
                        default:
                            // Nothing to do
                            break;
                    }
                    EncodeGameState();
                    return;
                case PAGE_SETTING:
                    switch (button)
                    {
                        case BUTTON_1:
                            gameStateData[SETTING] = SETTING_COUNTUP_5ROUNDS;
                            gameStateData[PAGE] = PAGE_ENTRY;
                            break;
                        case BUTTON_2:
                            gameStateData[SETTING] = SETTING_COUNTUP_8ROUNDS;
                            gameStateData[PAGE] = PAGE_ENTRY;
                            break;
                        case BUTTON_3:
                            gameStateData[SETTING] = SETTING_COUNTUP_10ROUNDS;
                            gameStateData[PAGE] = PAGE_ENTRY;
                            break;
                        case BUTTON_4:
                            gameStateData[SETTING] = SETTING_COUNTUP_15ROUNDS;
                            gameStateData[PAGE] = PAGE_ENTRY;
                            break;
                        case BUTTON_CANCEL:
                            gameStateData[PAGE] = PAGE_RULE;
                            break;
                        default:
                            // Nothing to do
                            break;
                    }
                    EncodeGameState();
                    return;
                case PAGE_ENTRY:
                    switch (button)
                    {
                        case BUTTON_1:
                            if (entrySystem.InteractSlot(0))
                            {
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(UpdateNumOfPlayer));
                            }
                            else
                            {
                                _PlayCancelSound();
                            }
                            break;
                        case BUTTON_2:
                            if (entrySystem.InteractSlot(1))
                            {
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(UpdateNumOfPlayer));
                            }
                            else
                            {
                                _PlayCancelSound();
                            }
                            break;
                        case BUTTON_3:
                            if (entrySystem.InteractSlot(2))
                            {
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(UpdateNumOfPlayer));
                            }
                            else
                            {
                                _PlayCancelSound();
                            }
                            break;
                        case BUTTON_4:
                            if (entrySystem.InteractSlot(3))
                            {
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(UpdateNumOfPlayer));
                            }
                            else
                            {
                                _PlayCancelSound();
                            }
                            break;
                        case BUTTON_CANCEL:
                            ClearState();
                            gameStateData[PAGE] = PAGE_INITIAL;
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowInitialPageMessage));
                            break;
                        case BUTTON_MAIN:
                            if (
                                gameStateData[RULE] != RULE_CRICKET && entrySystem.GetNumOfEntryPlayers() > 0
                                || gameStateData[RULE] == RULE_CRICKET && entrySystem.GetNumOfEntryPlayers() == 2
                                )
                            {
                                gameStateData[PAGE] = PAGE_GAME;
                                EncodeGameState();
                            }
                            else if (gameStateData[RULE] == RULE_CRICKET && gameStateData[PLAYERS] == 1)
                            {
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowCricketStartError));
                                _PlayCancelSound();
                            }
                            else
                            {
                                _PlayCancelSound();
                            }
                            break;
                    }
                    return;
                case PAGE_GAME:
                    if (button == BUTTON_MAIN)
                    {
                        if (!inGame)
                        {
                            ClearState();
                            gameStateData[PAGE] = PAGE_INITIAL;
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowInitialPageMessage));
                            EncodeGameState();
                        }
                        else
                        {
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(OnPressedMainButtonInGame));
                        }
                        UpdateMonitors();
                    }
                    return;
            }
        }

        byte ComputeScore(int ring, int number)
        {
            if (ring >= 4)
            {
                return 50;
            }
            else
            {
                return (byte)(ring * number);
            }
        }

        string DartResult(int ring, int number)
        {
            if (ring >= 4)
            {
                return "BULL";
            }
            else if (ring == 0)
            {
                return "MISS";
            }
            else
            {
                string ringName = "SINGLE";
                if (ring == 3) ringName = "TRIPLE";
                else if (ring == 2) ringName = "DOUBLE";
                return $"{ringName} {number}";
            }
        }

        int ComputeRing(float distance)
        {
            if (distance <= innerBullRadius)
            {
                return 5;
            }
            else if (distance <= outerBullRadius)
            {
                return 4;
            }
            else if (distance > innerSingleRadius && distance <= tripleRadius)
            {
                return 3;
            }
            else if (distance > outerSingleRadius && distance <= doubleRadius)
            {
                return 2;
            }
            else if (distance > doubleRadius)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        int ComputeNumber(float angle)
        {
            float number = (angle - 9f) / 18f;
            if (number > 0f && number <= 1f)
            {
                return 1;
            }
            else if (number > 1f && number <= 2f)
            {
                return 18;
            }
            else if (number > 2f && number <= 3f)
            {
                return 4;
            }
            else if (number > 3f && number <= 4f)
            {
                return 13;
            }
            else if (number > 4f && number <= 5f)
            {
                return 6;
            }
            else if (number > 5f && number <= 6f)
            {
                return 10;
            }
            else if (number > 6f && number <= 7f)
            {
                return 15;
            }
            else if (number > 7f && number <= 8f)
            {
                return 2;
            }
            else if (number > 8f && number <= 9f)
            {
                return 17;
            }
            else if (number > -1f && number <= 0f)
            {
                return 20;
            }
            else if (number > -2f && number <= -1f)
            {
                return 5;
            }
            else if (number > -3f && number <= -2f)
            {
                return 12;
            }
            else if (number > -4f && number <= -3f)
            {
                return 9;
            }
            else if (number > -5f && number <= -4f)
            {
                return 14;
            }
            else if (number > -6f && number <= -5f)
            {
                return 11;
            }
            else if (number > -7f && number <= -6f)
            {
                return 8;
            }
            else if (number > -8f && number <= -7f)
            {
                return 16;
            }
            else if (number > -9f && number <= -8f)
            {
                return 7;
            }
            else if (number > -10f && number <= -9f)
            {
                return 19;
            }
            else
            {
                return 3;
            }
        }

        void PlayHitSound(int number)
        {
            switch (number)
            {
                case 0:
                    PlayMissHitSound();
                    return;
                case 1:
                    PlaySingleHitSound();
                    return;
                case 2:
                    PlayDoubleHitSound();
                    return;
                default:
                    PlayTripleHitSound();
                    return;
            }
        }

        public void PlaySingleHitSound()
        {
            SingleHitSound.Play();
        }
        public void PlayDoubleHitSound()
        {
            DoubleHitSound.Play();
        }
        public void PlayMissHitSound()
        {
            _PlayCancelSound();
        }
        public void PlayTripleHitSound()
        {
            TripleHitSound.Play();
        }

        void ResetDartsPosition(VRCPlayerApi owner)
        {
            for (int i = 0; i < darts.Length; i++)
            {
                Networking.SetOwner(owner, darts[i].gameObject);
                darts[i].transform.position = dartsResetPositions[i].position;
                darts[i].transform.rotation = dartsResetPositions[i].rotation;
                darts[i].transform.parent = dartsResetPositions[i];
                darts[i].gameObject.transform.localPosition = Vector3.zero;
                darts[i].gameObject.transform.localRotation = Quaternion.identity;
                darts[i].Freeze();
            }
        }

        public void RespawnDarts()
        {
            ResetDartsPosition(Networking.LocalPlayer);
        }

        void OnPageChanged(uint nextPage)
        {
            var isOwner = Networking.IsOwner(gameObject);
            switch (nextPage)
            {
                case PAGE_INITIAL:
                    _PlayCancelSound();
                    boardAnimator.SetBool(ANIM_BOOL_ENABLED, false);
                    boardAnimator.SetBool(ANIM_BOOL_CRICKET_MODE, false);
                    controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_SUB_BUTTON, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_DISABLED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_15, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_16, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_17, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_18, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_19, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_20, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_BULL, ANIM_CRICKET_STATE_CLOSED);
                    if (isOwner)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowInitialPageMessage));
                    }
                    break;
                case PAGE_RULE:
                    _PlayConfirmSound();
                    controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_SUB_BUTTON, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_DISABLED);
                    if (isOwner)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HideMessage));
                    }
                    break;
                case PAGE_SETTING:
                    _PlayConfirmSound();
                    controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_SUB_BUTTON, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_ENABLED);
                    if (isOwner)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HideMessage));
                    }
                    break;
                case PAGE_ENTRY:
                    _PlayConfirmSound();
                    for (int i = 0; i < playerNames.Length; i++)
                    {
                        playerNames[i] = "PRESS TO ENTRY";
                    }
                    InitScores();
                    controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_FLUSHING);
                    controllerAnimator.SetInteger(ANIM_INT_SUB_BUTTON, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_FLUSHING);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_FLUSHING);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_FLUSHING);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_FLUSHING);
                    if (isOwner)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HideMessage));
                    }
                    break;
                case PAGE_GAME:
                    // Start Game
                    _PlayConfirmSound();
                    boardAnimator.SetBool(ANIM_BOOL_ENABLED, true);
                    boardAnimator.SetBool(ANIM_BOOL_CRICKET_MODE, gameStateData[RULE] == RULE_CRICKET);
                    controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_ENABLED);
                    controllerAnimator.SetInteger(ANIM_INT_SUB_BUTTON, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_DISABLED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_15, ANIM_CRICKET_STATE_NOT_OPENED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_16, ANIM_CRICKET_STATE_NOT_OPENED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_17, ANIM_CRICKET_STATE_NOT_OPENED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_18, ANIM_CRICKET_STATE_NOT_OPENED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_19, ANIM_CRICKET_STATE_NOT_OPENED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_20, ANIM_CRICKET_STATE_NOT_OPENED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_BULL, ANIM_CRICKET_STATE_NOT_OPENED);
                    var turn = gameStateData[TURN];
                    currentDart = 0;
                    currentPlayerName = entrySystem.GetEntryPlayerName((int)turn);
                    currentPlayerScore = GetPlayerScore(turn);
                    inGame = true;
                    if (Networking.IsOwner(gameObject))
                    {
                        RespawnDarts();
                        gameStateData[IN_GAME] = 0x1u;
                        EncodeGameState();
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowStartGameMessage));
                    }
                    break;
                default:
                    _PlayConfirmSound();
                    controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_SUB_BUTTON, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_DISABLED);
                    controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_DISABLED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_15, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_16, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_17, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_18, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_19, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_20, ANIM_CRICKET_STATE_CLOSED);
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_BULL, ANIM_CRICKET_STATE_CLOSED);
                    if (isOwner)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HideMessage));
                    }
                    break;
            }
        }

        void OnRuleChanged(uint nextRule)
        {
            if (nextRule == RULE_CRICKET)
            {
                maxRounds = 15;
            }
        }

        void OnNumOfPlayersChanged(uint nextNPlayers)
        {
        }

        void OnSettingChanged(uint nextSetting)
        {
            if (gameStateData[RULE] == RULE_COUNT_UP)
            {
                switch (nextSetting)
                {
                    case SETTING_COUNTUP_5ROUNDS:
                        maxRounds = 5;
                        return;
                    default:
                    case SETTING_COUNTUP_8ROUNDS:
                        maxRounds = 8;
                        return;
                    case SETTING_COUNTUP_10ROUNDS:
                        maxRounds = 10;
                        return;
                    case SETTING_COUNTUP_15ROUNDS:
                        maxRounds = 15;
                        return;
                }
            }
            else if (gameStateData[RULE] == RULE_01_GAMES)
            {
                maxRounds = 15;
                if (Networking.IsOwner(gameObject))
                {
                    switch (nextSetting)
                    {
                        case SETTING_01GAMES_301:
                            gameStateData[PLAYER1_SCORE] = 301;
                            gameStateData[PLAYER2_SCORE] = 301;
                            gameStateData[PLAYER3_SCORE] = 301;
                            gameStateData[PLAYER4_SCORE] = 301;
                            break;
                        default:
                        case SETTING_01GAMES_501:
                            gameStateData[PLAYER1_SCORE] = 501;
                            gameStateData[PLAYER2_SCORE] = 501;
                            gameStateData[PLAYER3_SCORE] = 501;
                            gameStateData[PLAYER4_SCORE] = 501;
                            break;
                        case SETTING_01GAMES_701:
                            gameStateData[PLAYER1_SCORE] = 701;
                            gameStateData[PLAYER2_SCORE] = 701;
                            gameStateData[PLAYER3_SCORE] = 701;
                            gameStateData[PLAYER4_SCORE] = 701;
                            break;
                        case SETTING_01GAMES_1001:
                            gameStateData[PLAYER1_SCORE] = 901;
                            gameStateData[PLAYER2_SCORE] = 901;
                            gameStateData[PLAYER3_SCORE] = 901;
                            gameStateData[PLAYER4_SCORE] = 901;
                            break;
                    }
                    EncodeGameState();
                }
            }
        }

        void OnRoundChanged(uint nextRound)
        {
            if (nextRound + 1U > maxRounds)
            {
                OnMaxRoundsAchieved();
            }
        }

        void OnMaxRoundsAchieved()
        {
            inGame = false;
            if (Networking.IsOwner(gameObject))
            {
                gameStateData[IN_GAME] = 0x0u;
                EncodeGameState();
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowWinner));
            }
        }

        void OnTrunChanged(uint nextTurn)
        {
        }

        void AppendRoundScore(uint player, ushort score)
        {
            if (Networking.IsOwner(gameObject))
            {
                switch (player)
                {
                    case 0:
                        for (int i = player1RoundScores.Length - 1; i > 0; i--)
                        {
                            player1RoundScores[i] = player1RoundScores[i - 1];
                        }
                        player1RoundScores[0] = score;
                        break;
                    case 1:
                        for (int i = player2RoundScores.Length - 1; i > 0; i--)
                        {
                            player2RoundScores[i] = player2RoundScores[i - 1];
                        }
                        player2RoundScores[0] = score;
                        break;
                    case 2:
                        for (int i = player3RoundScores.Length - 1; i > 0; i--)
                        {
                            player3RoundScores[i] = player3RoundScores[i - 1];
                        }
                        player3RoundScores[0] = score;
                        break;
                    case 3:
                        for (int i = player4RoundScores.Length - 1; i > 0; i--)
                        {
                            player4RoundScores[i] = player4RoundScores[i - 1];
                        }
                        player4RoundScores[0] = score;
                        break;
                }
            }
        }

        void OnGameStarted()
        {
            inGame = true;
        }

        void OnGameFinished()
        {
            inGame = false;
        }

        public void OnPressedMainButtonInGame()
        {
            bool isOwner = false;
            if (Networking.IsOwner(gameObject))
            {
                isOwner = true;
            }
            _PlayConfirmSound();

            if (!inGame)
            {
                if (isOwner)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowInitialPageMessage));
                    gameStateData[PAGE] = PAGE_INITIAL;
                    ClearState();
                    EncodeGameState();
                }
                return;
            }
            if (gameStateData[DART] != 3)
            {
                if (isOwner)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HideMessage));
                }
                return;
            }

            // Change Player
            dartResults = new string[] { "", "", "" };
            currentRoundScore = 0;
            currentDart = 0;
            if (isOwner)
            {
                if (gameStateData[TURN] == gameStateData[PLAYERS])
                {
                    gameStateData[ROUND] += 1;
                    gameStateData[TURN] = 0;
                }
                else
                {
                    gameStateData[TURN] += 1;
                }
                gameStateData[DART] = 0;
                EncodeGameState();
                UpdateMonitors();
            }
        }

        void OnDartChanged(uint nextDart)
        {
            if (nextDart == 0 && inGame)
            {
                dartResults = new string[] { "", "", "" };
                currentRoundScore = 0;
                currentDart = 0;
                if (entrySystem.GetPlayerApi(gameStateData[TURN]).playerId == Networking.LocalPlayer.playerId)
                {
                    RespawnDarts();
                }
                ShowPlayerChangedMessage();
                SendCustomEventDelayedSeconds(nameof(HideMessage), 1.5f, VRC.Udon.Common.Enums.EventTiming.Update);
                boardAnimator.SetTrigger(ANIM_TRIG_WIPE);

                currentPlayerName = playerNames[gameStateData[TURN]];
                currentPlayerScore = GetPlayerScore(gameStateData[TURN]);
                for (int i = 0; i < currentPlayerRoundScores.Length; i++)
                {
                    currentPlayerRoundScores[i] = (int)GetPlayerRoundScore(i, gameStateData[TURN]);
                }
            }
            else if (nextDart == 3 && inGame)
            {
                AppendRoundScore(gameStateData[TURN], currentRoundScore);
                var isOwner = Networking.IsOwner(gameObject);
                if (maxRounds <= gameStateData[ROUND]+1 && gameStateData[TURN] == gameStateData[PLAYERS])
                {
                    inGame = false;
                    if (isOwner)
                    {
                        gameStateData[IN_GAME] = 0x0u;
                        EncodeGameState();
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowWinner));
                    }
                }
                else if(isOwner)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowChangePlayerMessage));
                }
                controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_FLUSHING);
            }
        }

        void OnPlayerScoresChanged(uint player, uint newScore)
        {
        }

        void OnHitDartIn01Games(int ring, int number)
        {
            PlayHitSound(ring);
            var turn = gameStateData[TURN];
            if (currentDart > 2) return;
            dartResults[currentDart] = DartResult(ring, number);
            var score = ComputeScore(ring, number);
            if (currentPlayerScore < score)
            {
                controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_FLUSHING);
                if (Networking.IsOwner(gameObject))
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowBurstMessage));
                    gameStateData[DART] = 3;
                    SetPlayerScore(turn, currentPlayerScore + currentRoundScore);
                    EncodeGameState();
                    UpdateMonitors();
                }
                return;
            }
            else
            {
                currentPlayerScore -= score;
                currentRoundScore += score;
            }

            if (currentPlayerScore == 0 && inGame)
            {
                controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_FLUSHING);
                SetPlayerScore(turn, 0);
                inGame = false;
                if (Networking.IsOwner(gameObject))
                {
                    gameStateData[IN_GAME] = 0x0u;
                    EncodeGameState();
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowWinner));
                    UpdateMonitors();
                }
                return;
            }
            NextDart();
            UpdateMonitors();
        }

        void OnHitDartInCountUp(int ring, int number)
        {
            PlayHitSound(ring);
            if (currentDart > 2) return;
            dartResults[currentDart] = DartResult(ring, number);
            var score = ComputeScore(ring, number);
            currentPlayerScore += score;
            currentRoundScore += score;
            NextDart();
            UpdateMonitors();
        }

        void OnHitDartInCricket(int ring, int number)
        {
            if (number >= 15)
            {
                PlayHitSound(ring);
            }
            else if (ring == 4)
            {
                PlayHitSound(1);
            }
            else if (ring == 5)
            {
                PlayHitSound(2);
            }
            else
            {
                PlayHitSound(0);
            }
            var turn = gameStateData[TURN];
            if (currentDart > 2) return;
            dartResults[currentDart] = DartResult(ring, number);
            MarkCricketBoard(turn, ring, number);
            CheckCricketFinished();
            NextDart();
            UpdateMonitors();
        }

        void MarkCricketBoard(uint turn, int ring, int number)
        {
            int marks = 0;
            if (ring == 5)
            {
                ComputeCricketScore(turn, CRICKET_NUMBER_BULL, 2);
                marks = 2;
            }
            else if (ring == 4)
            {
                ComputeCricketScore(turn, CRICKET_NUMBER_BULL, 1);
                marks = 1;
            }
            else if (ring <= 0)
            {
                return;
            }
            else
            {
                switch (number)
                {
                    case 20:
                        ComputeCricketScore(turn, CRICKET_NUMBER_20, ring);
                        marks = ring;
                        break;
                    case 19:
                        ComputeCricketScore(turn, CRICKET_NUMBER_19, ring);
                        marks = ring;
                        break;
                    case 18:
                        ComputeCricketScore(turn, CRICKET_NUMBER_18, ring);
                        marks = ring;
                        break;
                    case 17:
                        ComputeCricketScore(turn, CRICKET_NUMBER_17, ring);
                        marks = ring;
                        break;
                    case 16:
                        ComputeCricketScore(turn, CRICKET_NUMBER_16, ring);
                        marks = ring;
                        break;
                    case 15:
                        ComputeCricketScore(turn, CRICKET_NUMBER_15, ring);
                        marks = ring;
                        break;
                }
            }
            if (gameStateData[DART] == 0)
            {
                currentRoundScore += (ushort)(marks * 100U);
            }
            else if (gameStateData[DART] == 1)
            {
                currentRoundScore += (ushort)(marks * 10U);
            }
            else if (gameStateData[DART] == 2)
            {
                currentRoundScore += (ushort)marks;
            }
        }

        void ComputeCricketScore(uint turn, int cricketNum, int marks)
        {
            int ownMarks = turn == 0U ? player1CricketNumbers[cricketNum] : player2CricketNumbers[cricketNum];
            int opponentMarks = turn == 0U ? player2CricketNumbers[cricketNum] : player1CricketNumbers[cricketNum];

            if (ownMarks > 3 && opponentMarks > 3)
            {
                // Already Closed
                return;
            }

            uint score = 0U;
            switch (cricketNum)
            {
                case CRICKET_NUMBER_20:
                    score = 20U;
                    break;
                case CRICKET_NUMBER_19:
                    score = 19U;
                    break;
                case CRICKET_NUMBER_18:
                    score = 18U;
                    break;
                case CRICKET_NUMBER_17:
                    score = 17U;
                    break;
                case CRICKET_NUMBER_16:
                    score = 16U;
                    break;
                case CRICKET_NUMBER_15:
                    score = 15U;
                    break;
                case CRICKET_NUMBER_BULL:
                    score = 25U;
                    break;
            }

            for (int i = 0; i < marks; i++)
            {
                ownMarks++;
                if (opponentMarks < 3) {
                    if (ownMarks == 3)
                    {
                        if (turn == 0U)
                        {
                            SetCricketAnimParams(cricketNum, ANIM_CRICKET_STATE_PLAYER1);
                        }
                        else
                        {
                            SetCricketAnimParams(cricketNum, ANIM_CRICKET_STATE_PLAYER2);
                        }
                    }
                    if (ownMarks > 3)
                    {
                        currentPlayerScore += score;
                    }
                }
                else
                {
                    if (ownMarks >= 3)
                    {
                        // Closed
                        SetCricketAnimParams(cricketNum, ANIM_CRICKET_STATE_CLOSED);
                        break;
                    }
                }
            }
            if (turn == 0U)
            {
                player1CricketNumbers[cricketNum] = ownMarks > 3 ? 3 : ownMarks;
            }
            else
            {
                player2CricketNumbers[cricketNum] = ownMarks > 3 ? 3 : ownMarks;
            }
            SetPlayerScore(turn, currentPlayerScore);
        }

        void SetCricketAnimParams(int cricketNum, int state)
        {
            switch (cricketNum)
            {
                case CRICKET_NUMBER_20:
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_20, state);
                    break;
                case CRICKET_NUMBER_19:
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_19, state);
                    break;
                case CRICKET_NUMBER_18:
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_18, state);
                    break;
                case CRICKET_NUMBER_17:
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_17, state);
                    break;
                case CRICKET_NUMBER_16:
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_16, state);
                    break;
                case CRICKET_NUMBER_15:
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_15, state);
                    break;
                case CRICKET_NUMBER_BULL:
                    boardAnimator.SetInteger(ANIM_INT_CRICKET_BULL, state);
                    break;
            }
        }

        void CheckCricketFinished()
        {
            bool player1AllOpened = true;
            bool player2AllOpened = true;
            foreach(var m in player1CricketNumbers)
            {
                if (m < 3)
                {
                    player1AllOpened = false;
                }
            }
            foreach (var m in player2CricketNumbers)
            {
                if (m < 3)
                {
                    player2AllOpened = false;
                }
            }
            if (
                (player1AllOpened && gameStateData[PLAYER1_SCORE] > gameStateData[PLAYER2_SCORE]) // Player1 WIN
                || (player2AllOpened && gameStateData[PLAYER2_SCORE] > gameStateData[PLAYER1_SCORE]) // Player2 WIN
                )
            {
                if (inGame)
                {
                    inGame = false;
                    if (Networking.IsOwner(gameObject))
                    {
                        gameStateData[IN_GAME] = 0x0u;
                        EncodeGameState();
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowWinner));
                    }
                }
            }
        }

        void NextDart()
        {
            if (Networking.IsOwner(gameObject))
            {
                SetPlayerScore(gameStateData[TURN], currentPlayerScore);
                if (gameStateData[DART] != 3)
                {
                    gameStateData[DART] += 1;
                }
                EncodeGameState();
            }
            if (currentDart != 3) currentDart += 1;
        }

        public void UpdatePlayerNames()
        {
            playerNames = entrySystem.GetEntryPlayerNames();
            UpdateMonitors();
        }

        void SetPlayerScore(uint turn, uint score)
        {
            switch (turn)
            {
                case 0:
                    gameStateData[PLAYER1_SCORE] = score;
                    break;
                case 1:
                    gameStateData[PLAYER2_SCORE] = score;
                    break;
                case 2:
                    gameStateData[PLAYER3_SCORE] = score;
                    break;
                case 3:
                    gameStateData[PLAYER4_SCORE] = score;
                    break;
            }
            EncodeGameState();
        }

        uint GetPlayerScore(uint turn)
        {
            switch (turn)
            {
                case 0:
                    return gameStateData[PLAYER1_SCORE];
                case 1:
                    return gameStateData[PLAYER2_SCORE];
                case 2:
                    return gameStateData[PLAYER3_SCORE];
                case 3:
                    return gameStateData[PLAYER4_SCORE];
                default:
                    return 0u;
            }
        }

        uint GetPlayerRoundScore(int round, uint player)
        {
            switch (player)
            {
                case 0:
                    return player1RoundScores[round];
                case 1:
                    return player2RoundScores[round];
                case 2:
                    return player3RoundScores[round];
                case 3:
                    return player4RoundScores[round];
                default:
                    return 0;
            }
        }

        public void ForceReset()
        {
            if (Networking.IsOwner(gameObject))
            {
                ClearState();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ForceReset));
            }
        }

        void UpdateMonitors()
        {
            foreach (var m in monitors)
            {
                m._UpdateState();
            }
        }

        public void _PlayCancelSound()
        {
            CancelSound.Play();
        }

        public void _PlayConfirmSound()
        {
            ConfirmSound.Play();
        }

        void ShowMessage(string message)
        {
            foreach (var m in monitors)
            {
                m._ShowMessage(message);
            }
        }

        public void HideMessage()
        {
            foreach (var m in monitors)
            {
                m._HideMessage();
            }
        }

        public void ShowInitialPageMessage()
        {
            ShowMessage(MESSAGE_INITIAL_PAGE);
        }

        public void ShowStartGameMessage()
        {
            ShowMessage("GAME ON");
            SendCustomEventDelayedSeconds(nameof(HideMessage), 1.5f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void ShowChangePlayerMessage()
        {
            if (isBursted)
            {
                isBursted = false;
            }
            else
            {
                ShowMessage("CHANGE PLAYER");
            }
        }

        public void ShowBurstMessage()
        {
            ShowMessage("BURST!!\nCHANGE PLAYER");
            isBursted = true;
        }

        public void ShowCricketStartError()
        {
            ShowMessage("CRICKET can only play by 2 players.");
        }

        public void ShowPlayerChangedMessage()
        {
            var nextPlayer = entrySystem.GetPlayerApi(gameStateData[TURN]);
            ShowMessage($"PLAYER CHANGED\nTHROW DARTS, {nextPlayer.displayName}");
        }

        public void ShowWinner()
        {
            var winner = PLAYER1_SCORE;
            var winnerPlayer = entrySystem.GetPlayerApi(0);
            switch (gameStateData[RULE])
            {
                case RULE_COUNT_UP:
                    if (gameStateData[winner] < gameStateData[PLAYER2_SCORE])
                    {
                        winner = PLAYER2_SCORE;
                        winnerPlayer = entrySystem.GetPlayerApi(1);
                    }
                    if (gameStateData[winner] < gameStateData[PLAYER3_SCORE])
                    {
                        winner = PLAYER3_SCORE;
                        winnerPlayer = entrySystem.GetPlayerApi(2);
                    }
                    if (gameStateData[winner] < gameStateData[PLAYER4_SCORE])
                    {
                        winner = PLAYER4_SCORE;
                        winnerPlayer = entrySystem.GetPlayerApi(3);
                    }
                    break;
                case RULE_01_GAMES:
                    if (gameStateData[winner] > gameStateData[PLAYER2_SCORE])
                    {
                        winner = PLAYER2_SCORE;
                        winnerPlayer = entrySystem.GetPlayerApi(1);
                    }
                    if (gameStateData[winner] > gameStateData[PLAYER3_SCORE])
                    {
                        winner = PLAYER3_SCORE;
                        winnerPlayer = entrySystem.GetPlayerApi(2);
                    }
                    if (gameStateData[winner] > gameStateData[PLAYER4_SCORE])
                    {
                        winner = PLAYER4_SCORE;
                        winnerPlayer = entrySystem.GetPlayerApi(3);
                    }
                    break;
                case RULE_CRICKET:
                    if (gameStateData[PLAYER1_SCORE] < gameStateData[PLAYER2_SCORE])
                    {
                        winnerPlayer = entrySystem.GetPlayerApi(1);
                    }
                    break;
            }
            ShowMessage($"GAME FINISHED!\n{winnerPlayer.displayName} WIN");
            if (entrySystem.udonChips != null && winnerPlayer.playerId == Networking.LocalPlayer.playerId)
            {
                var money = (float)entrySystem.udonChips.GetProgramVariable("money");
                money += entrySystem.GetNumOfEntryPlayers() * (entrySystem.entryFee - entrySystem.commissionFee);
                entrySystem.udonChips.SetProgramVariable("money", money);
            }
            if (eventListener != null)
            {
                ((VRCDartsEventListener)eventListener).OnFinishGame(winnerPlayer);
            }
        }

        #region STATE_SYNC
        void ClearState()
        {
            if (Networking.IsOwner(gameObject))
            {
                gameStateData = new uint[gameStateData.Length];
                previousGameStateData = new uint[previousGameStateData.Length];
                encodedGameStateData = new byte[encodedGameStateData.Length];
                entrySystem.SendCustomNetworkEvent(
                    VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner,
                    nameof(entrySystem.ClearSlots)
                );
                cricketBoardState = 0;
            }
            InitScores();
            inGame = false;
            playerNames = new string[] { "Player1", "Player2", "Player3", "Player4" };
            dartResults = new string[] { "", "", "" };
            currentPlayerName = "Player";
            message = MESSAGE_INITIAL_PAGE;
            maxRounds = 0;
            currentRoundScore = 0;
            currentDart = 0;
            boardAnimator.SetBool(ANIM_BOOL_ENABLED, false);
            boardAnimator.SetBool(ANIM_BOOL_CRICKET_MODE, false);
            controllerAnimator.SetInteger(ANIM_INT_MAIN_BUTTON, ANIM_BUTTON_STATE_DISABLED);
            controllerAnimator.SetInteger(ANIM_INT_SUB_BUTTON, ANIM_BUTTON_STATE_DISABLED);
            controllerAnimator.SetInteger(ANIM_INT_BUTTON1, ANIM_BUTTON_STATE_DISABLED);
            controllerAnimator.SetInteger(ANIM_INT_BUTTON2, ANIM_BUTTON_STATE_DISABLED);
            controllerAnimator.SetInteger(ANIM_INT_BUTTON3, ANIM_BUTTON_STATE_DISABLED);
            controllerAnimator.SetInteger(ANIM_INT_BUTTON4, ANIM_BUTTON_STATE_DISABLED);
            boardAnimator.SetInteger(ANIM_INT_CRICKET_15, ANIM_CRICKET_STATE_CLOSED);
            boardAnimator.SetInteger(ANIM_INT_CRICKET_16, ANIM_CRICKET_STATE_CLOSED);
            boardAnimator.SetInteger(ANIM_INT_CRICKET_17, ANIM_CRICKET_STATE_CLOSED);
            boardAnimator.SetInteger(ANIM_INT_CRICKET_18, ANIM_CRICKET_STATE_CLOSED);
            boardAnimator.SetInteger(ANIM_INT_CRICKET_19, ANIM_CRICKET_STATE_CLOSED);
            boardAnimator.SetInteger(ANIM_INT_CRICKET_20, ANIM_CRICKET_STATE_CLOSED);
            boardAnimator.SetInteger(ANIM_INT_CRICKET_BULL, ANIM_CRICKET_STATE_CLOSED);
            if (Networking.IsOwner(gameObject))
            {
                _SyncState();
            }
            if (eventListener != null)
            {
                ((VRCDartsEventListener)eventListener).OnResetDarts();
            }
        }

        void InitScores()
        {
            currentPlayerScore = 0;
            currentRoundScore = 0;
            currentPlayerRoundScores = new int[currentPlayerRoundScores.Length];
            player1RoundScores = new ushort[player1RoundScores.Length];
            player2RoundScores = new ushort[player2RoundScores.Length];
            player3RoundScores = new ushort[player3RoundScores.Length];
            player4RoundScores = new ushort[player4RoundScores.Length];
            dartResults = new string[dartResults.Length];
            player1CricketNumbers = new int[player1CricketNumbers.Length];
            player2CricketNumbers = new int[player2CricketNumbers.Length];
        }

        void EncodeGameState()
        {
            int bytePos = 0;
            int bitCapacity = 8;
            encodedGameStateData[bytePos] = 0x0;
            for (int i = 0; i < gameStateData.Length; i++)
            {
                for (int size = (int)DATA_SIZES[i]; size > 0;)
                {
                    if (size > bitCapacity)
                    {
                        size -= bitCapacity;
                        encodedGameStateData[bytePos] |= (byte)(gameStateData[i] >> size & 0xFF);
                        bytePos++;
                        encodedGameStateData[bytePos] = 0x0;
                        bitCapacity = 8;
                    }
                    else
                    {
                        bitCapacity -= size;
                        size = 0;
                        encodedGameStateData[bytePos] |= (byte)(gameStateData[i] << bitCapacity & 0xFF);
                        if (bitCapacity == 0)
                        {
                            bytePos++;
                            encodedGameStateData[bytePos] = 0x0;
                            bitCapacity = 8;
                        }
                    }
                }
            }
            EncodeCricketScore();
            if (Networking.IsOwner(gameObject))
            {
                _SyncState();
            }
        }

        void EncodeCricketScore()
        {
            int encoded = 0;
            foreach(var marks in player1CricketNumbers)
            {
                encoded |= marks;
                encoded <<= 2;
            }
            foreach (var marks in player2CricketNumbers)
            {
                encoded |= marks;
                encoded <<= 2;
            }
            cricketBoardState = encoded;
        }

        void DecodeGameState()
        {
            DecodeCricketScore();
            int bytePos = 0;
            int bitCapacity = 8;
            for (int i = 0; i < gameStateData.Length; i++)
            {
                gameStateData[i] = 0U;
                for (int size = (int)DATA_SIZES[i]; size > 0;)
                {
                    if (size > bitCapacity)
                    {
                        gameStateData[i] |= (encodedGameStateData[bytePos] & (0xFFu >> 8 - bitCapacity)) << size - bitCapacity;
                        size -= bitCapacity;
                        bytePos++;
                        bitCapacity = 8;
                    }
                    else
                    {
                        gameStateData[i] |= (encodedGameStateData[bytePos] & (0xFFu >> 8 - bitCapacity)) >> bitCapacity - size;
                        bitCapacity -= size;
                        size = 0;
                        if (bitCapacity == 0)
                        {
                            bytePos++;
                            bitCapacity = 8;
                        }
                    }
                }
            }
        }

        void DecodeCricketScore()
        {
            int tmp = cricketBoardState;
            for(int i = player2CricketNumbers.Length - 1; i >= 0; i--)
            {
                tmp >>= 2;
                player2CricketNumbers[i] = tmp & 0x3;
            }
            for (int i = player1CricketNumbers.Length - 1; i >= 0; i--)
            {
                tmp >>= 2;
                player1CricketNumbers[i] = tmp & 0x3;
            }
        }

        void OnStateChanged(uint state, uint prevValue, uint nextValue)
        {
            switch (state)
            {
                case PAGE:
                    OnPageChanged(nextValue);
                    return;
                case RULE:
                    OnRuleChanged(nextValue);
                    return;
                case SETTING:
                    OnSettingChanged(nextValue);
                    return;
                case PLAYERS:
                    OnNumOfPlayersChanged(nextValue);
                    return;
                case ROUND:
                    OnRoundChanged(nextValue);
                    return;
                case TURN:
                    OnTrunChanged(nextValue);
                    return;
                case DART:
                    OnDartChanged(nextValue);
                    return;
                case PLAYER1_SCORE:
                    OnPlayerScoresChanged(0, nextValue);
                    return;
                case PLAYER2_SCORE:
                    OnPlayerScoresChanged(1, nextValue);
                    return;
                case PLAYER3_SCORE:
                    OnPlayerScoresChanged(2, nextValue);
                    return;
                case PLAYER4_SCORE:
                    OnPlayerScoresChanged(3, nextValue);
                    return;
                case IN_GAME:
                    if (nextValue == 0x1u)
                    {
                        OnGameStarted();
                    }
                    else
                    {
                        OnGameFinished();
                    }
                    return;
                default:
                    return;
            }
        }

        void NotifyStateChanged()
        {
            for (uint state = 0U; state < gameStateData.Length; state++)
            {
                if (previousGameStateData[state] != gameStateData[state])
                {
                    OnStateChanged(state, previousGameStateData[state], gameStateData[state]);
                }
            }
            UpdateMonitors();
            for (uint i = 0U; i < gameStateData.Length; i++)
            {
                previousGameStateData[i] = gameStateData[i];
            }
        }
        #endregion

        private void LateUpdate()
        {
            if (stateChanged && Networking.IsOwner(gameObject))
            {
                NotifyStateChanged();
                stateChanged = false;
            }
        }

        public override void OnDeserialization()
        {
            DecodeGameState();
            NotifyStateChanged();
        }

        public void _SyncState()
        {
            if (!Networking.IsClogged)
            {
                RequestSerialization();
                stateChanged = true;
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(_SyncState), UnityEngine.Random.value / 2, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }

        void NotifyHit(int ring, int number)
        {
            switch (ring)
            {
                case 0:
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, $"OnDartsHitAtOuter");
                    return;
                case 1:
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, $"OnDartsHitAtSingle{number}");
                    return;
                case 2:
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, $"OnDartsHitAtDouble{number}");
                    return;
                case 3:
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, $"OnDartsHitAtTriple{number}");
                    return;
                case 4:
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, $"OnDartsHitAtBull");
                    return;
                case 5:
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, $"OnDartsHitAtInBull");
                    return;
            }
        }

        void OnDartsHitAt(int ring, int number)
        {
            if (ring >= 4)
            {
                boardAnimator.SetTrigger(ANIM_TRIG_BULL);
            }

            switch (gameStateData[RULE])
            {
                case RULE_COUNT_UP:
                    OnHitDartInCountUp(ring, number);
                    break;
                case RULE_01_GAMES:
                    OnHitDartIn01Games(ring, number);
                    break;
                case RULE_CRICKET:
                    OnHitDartInCricket(ring, number);
                    break;
                default:
                    break;
            }
        }

        #region ON_DARTS_HIT_NETWORK_EVENTS
        public void OnDartsHitAtBull()
        {
            OnDartsHitAt(4, 1);
        }

        public void OnDartsHitAtInBull()
        {
            OnDartsHitAt(5, 1);
        }

        public void OnDartsHitAtOuter()
        {
            OnDartsHitAt(0, 1);
        }

        public void OnDartsHitAtSingle1()
        {
            OnDartsHitAt(1, 1);
        }

        public void OnDartsHitAtSingle2()
        {
            OnDartsHitAt(1, 2);
        }

        public void OnDartsHitAtSingle3()
        {
            OnDartsHitAt(1, 3);
        }

        public void OnDartsHitAtSingle4()
        {
            OnDartsHitAt(1, 4);
        }

        public void OnDartsHitAtSingle5()
        {
            OnDartsHitAt(1, 5);
        }

        public void OnDartsHitAtSingle6()
        {
            OnDartsHitAt(1, 6);
        }

        public void OnDartsHitAtSingle7()
        {
            OnDartsHitAt(1, 7);
        }

        public void OnDartsHitAtSingle8()
        {
            OnDartsHitAt(1, 8);
        }

        public void OnDartsHitAtSingle9()
        {
            OnDartsHitAt(1, 9);
        }

        public void OnDartsHitAtSingle10()
        {
            OnDartsHitAt(1, 10);
        }

        public void OnDartsHitAtSingle11()
        {
            OnDartsHitAt(1, 11);
        }

        public void OnDartsHitAtSingle12()
        {
            OnDartsHitAt(1, 12);
        }

        public void OnDartsHitAtSingle13()
        {
            OnDartsHitAt(1, 13);
        }

        public void OnDartsHitAtSingle14()
        {
            OnDartsHitAt(1, 14);
        }

        public void OnDartsHitAtSingle15()
        {
            OnDartsHitAt(1, 15);
        }

        public void OnDartsHitAtSingle16()
        {
            OnDartsHitAt(1, 16);
        }

        public void OnDartsHitAtSingle17()
        {
            OnDartsHitAt(1, 17);
        }

        public void OnDartsHitAtSingle18()
        {
            OnDartsHitAt(1, 18);
        }

        public void OnDartsHitAtSingle19()
        {
            OnDartsHitAt(1, 19);
        }

        public void OnDartsHitAtSingle20()
        {
            OnDartsHitAt(1, 20);
        }

        public void OnDartsHitAtDouble1()
        {
            OnDartsHitAt(2, 1);
        }

        public void OnDartsHitAtDouble2()
        {
            OnDartsHitAt(2, 2);
        }

        public void OnDartsHitAtDouble3()
        {
            OnDartsHitAt(2, 3);
        }

        public void OnDartsHitAtDouble4()
        {
            OnDartsHitAt(2, 4);
        }

        public void OnDartsHitAtDouble5()
        {
            OnDartsHitAt(2, 5);
        }

        public void OnDartsHitAtDouble6()
        {
            OnDartsHitAt(2, 6);
        }

        public void OnDartsHitAtDouble7()
        {
            OnDartsHitAt(2, 7);
        }

        public void OnDartsHitAtDouble8()
        {
            OnDartsHitAt(2, 8);
        }

        public void OnDartsHitAtDouble9()
        {
            OnDartsHitAt(2, 9);
        }

        public void OnDartsHitAtDouble10()
        {
            OnDartsHitAt(2, 10);
        }

        public void OnDartsHitAtDouble11()
        {
            OnDartsHitAt(2, 11);
        }

        public void OnDartsHitAtDouble12()
        {
            OnDartsHitAt(2, 12);
        }

        public void OnDartsHitAtDouble13()
        {
            OnDartsHitAt(2, 13);
        }

        public void OnDartsHitAtDouble14()
        {
            OnDartsHitAt(2, 14);
        }

        public void OnDartsHitAtDouble15()
        {
            OnDartsHitAt(2, 15);
        }

        public void OnDartsHitAtDouble16()
        {
            OnDartsHitAt(2, 16);
        }

        public void OnDartsHitAtDouble17()
        {
            OnDartsHitAt(2, 17);
        }

        public void OnDartsHitAtDouble18()
        {
            OnDartsHitAt(2, 18);
        }

        public void OnDartsHitAtDouble19()
        {
            OnDartsHitAt(2, 19);
        }

        public void OnDartsHitAtDouble20()
        {
            OnDartsHitAt(2, 20);
        }

        public void OnDartsHitAtTriple1()
        {
            OnDartsHitAt(3, 1);
        }

        public void OnDartsHitAtTriple2()
        {
            OnDartsHitAt(3, 2);
        }

        public void OnDartsHitAtTriple3()
        {
            OnDartsHitAt(3, 3);
        }

        public void OnDartsHitAtTriple4()
        {
            OnDartsHitAt(3, 4);
        }

        public void OnDartsHitAtTriple5()
        {
            OnDartsHitAt(3, 5);
        }

        public void OnDartsHitAtTriple6()
        {
            OnDartsHitAt(3, 6);
        }

        public void OnDartsHitAtTriple7()
        {
            OnDartsHitAt(3, 7);
        }

        public void OnDartsHitAtTriple8()
        {
            OnDartsHitAt(3, 8);
        }

        public void OnDartsHitAtTriple9()
        {
            OnDartsHitAt(3, 9);
        }

        public void OnDartsHitAtTriple10()
        {
            OnDartsHitAt(3, 10);
        }

        public void OnDartsHitAtTriple11()
        {
            OnDartsHitAt(3, 11);
        }

        public void OnDartsHitAtTriple12()
        {
            OnDartsHitAt(3, 12);
        }

        public void OnDartsHitAtTriple13()
        {
            OnDartsHitAt(3, 13);
        }

        public void OnDartsHitAtTriple14()
        {
            OnDartsHitAt(3, 14);
        }

        public void OnDartsHitAtTriple15()
        {
            OnDartsHitAt(3, 15);
        }

        public void OnDartsHitAtTriple16()
        {
            OnDartsHitAt(3, 16);
        }

        public void OnDartsHitAtTriple17()
        {
            OnDartsHitAt(3, 17);
        }

        public void OnDartsHitAtTriple18()
        {
            OnDartsHitAt(3, 18);
        }

        public void OnDartsHitAtTriple19()
        {
            OnDartsHitAt(3, 19);
        }

        public void OnDartsHitAtTriple20()
        {
            OnDartsHitAt(3, 20);
        }
        #endregion
    }
}
