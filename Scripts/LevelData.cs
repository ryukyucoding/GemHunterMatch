using System;
using UnityEngine;
using UnityEngine.Events;

namespace Match3
{
    /// <summary>
    /// Contains all the data for the Level in which this is : Goals and max number of Moves. This will also  notify the
    /// GameManager that we loaded a level
    /// </summary>
    [DefaultExecutionOrder(12000)]
    public class LevelData : MonoBehaviour
    {
        public static LevelData Instance { get; private set; }
    
        [Serializable]
        public class GemGoal
        {
            public Gem Gem;
            public int Count;
        }

        public string LevelName = "Level";
        public int MaxMove;
        public int LowMoveTrigger = 10;

        [Header("時間制設定")]
        public float MaxTime = 90f;           // 最大時間（秒）
        public float LowTimeWarning = 10f;    // 低時間警告閾值（秒）
        public bool UseTimerMode = true;      // 是否使用時間制模式

        [Header("訂單目標設定")]
        public int RequiredOrderCount = 3;    // 需要完成的訂單數量（通關條件）

        public GemGoal[] Goals;
        
        [Header("Visuals")]
        public float BorderMargin = 0.3f;
        public SpriteRenderer Background;
        
        [Header("Audio")] 
        public AudioClip Music;

        public delegate void GoalChangeDelegate(int gemType,int newAmount);
        public delegate void MoveNotificationDelegate(int moveRemaining);
        public delegate void TimeNotificationDelegate(float timeRemaining);

        public Action OnAllGoalFinished;
        public Action OnNoMoveLeft;
        public UnityEvent OnTimeUp;           // 時間用完事件
        public UnityEvent OnLowTimeWarning;   // 低時間警告事件

        public GoalChangeDelegate OnGoalChanged;
        public MoveNotificationDelegate OnMoveHappened;
        public TimeNotificationDelegate OnTimeChanged;    // 時間變化事件

        public int RemainingMove { get; private set; }
        public float RemainingTime { get; private set; }  // 剩餘時間
        public int GoalLeft { get; private set; }

        private int m_StartingWidth;
        private int m_StartingHeight;
        private bool m_IsLowTimeWarningTriggered = false;    // 是否已觸發低時間警告
        private bool m_IsTimeUp = false;                     // 是否已經時間用完

        private void Awake()
        {
            Instance = this;
            RemainingMove = MaxMove;
            RemainingTime = MaxTime;              // 初始化剩餘時間
            GoalLeft = Goals.Length;
            
            // 重置時間相關標誌（場景重載時確保重置）
            m_IsLowTimeWarningTriggered = false;
            m_IsTimeUp = false;
            
            GameManager.Instance.StartLevel();
        }

        void Start()
        {
            m_StartingWidth = Screen.width;
            m_StartingHeight = Screen.height;

            if (Background != null)
                Background.gameObject.SetActive(false);
        }

        void Update()
        {
            // 時間制模式的計時器
            if (UseTimerMode && !m_IsTimeUp)
            {
                UpdateTimer();
            }

            //to detect device orientation change or resolution change, we check if the screen change since since init
            //and recompute camera zoom
            if (Screen.width != m_StartingWidth || Screen.height != m_StartingHeight)
            {
                GameManager.Instance.ComputeCamera();
            }
        }

        private void UpdateTimer()
        {
            float prevTime = RemainingTime;
            RemainingTime -= Time.deltaTime;

            // 觸發時間變化事件
            OnTimeChanged?.Invoke(RemainingTime);

            // 檢查是否需要觸發低時間警告
            if (!m_IsLowTimeWarningTriggered && prevTime > LowTimeWarning && RemainingTime <= LowTimeWarning)
            {
                m_IsLowTimeWarningTriggered = true;
                OnLowTimeWarning?.Invoke();
                UIHandler.Instance.TriggerCharacterAnimation(UIHandler.CharacterAnimation.LowMove);
            }

            // 檢查時間是否用完
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                m_IsTimeUp = true;
                Debug.LogWarning($"[LevelData] 時間用完！觸發 OnTimeUp 事件");
                OnTimeUp?.Invoke();
                GameManager.Instance.Board.ToggleInput(false);  // 停止輸入
            }
        }

        public bool Matched(Gem gem)
        {
            // 不使用舊的 Gem Goals 系統
            // 遊戲目標完全由訂單系統管理
            // 不顯示匹配特效（因為 AddMatchEffect 依賴於 Goals UI）
            return true;
        }

        public void DarkenBackground(bool darken)
        {
            if (Background == null)
                return;

            Background.gameObject.SetActive(darken);
        }

        public void Moved()
        {
            // 不使用步數限制，保留此方法以避免其他地方的調用出錯
        }

        /// <summary>
        /// 增加遊戲時間（當完成特殊料理時可調用）
        /// </summary>
        /// <param name="seconds">要增加的秒數</param>
        public void AddTime(float seconds)
        {
            if (UseTimerMode)
            {
                float prevTime = RemainingTime;

                // 增加時間，但不能超過 MaxTime
                RemainingTime += seconds;
                RemainingTime = Mathf.Min(RemainingTime, MaxTime);

                Debug.Log($"[LevelData] 增加時間: +{seconds} 秒，從 {prevTime:F1} 秒變為 {RemainingTime:F1} 秒 (MaxTime={MaxTime}, m_IsTimeUp={m_IsTimeUp})");

                // 如果時間增加讓時間變為正數，重置 TimeUp 標誌並重新啟用輸入
                if (RemainingTime > 0 && m_IsTimeUp)
                {
                    m_IsTimeUp = false;
                    Debug.Log($"[LevelData] 時間獎勵使遊戲繼續: {RemainingTime:F1} 秒");

                    // 重新啟用輸入並重置 FinalStretch 標誌
                    if (GameManager.Instance?.Board != null)
                    {
                        GameManager.Instance.Board.ToggleInput(true);
                        GameManager.Instance.Board.ResetFinalStretch();
                        Debug.Log($"[LevelData] 已重置 FinalStretch 並重新啟用輸入");
                    }
                }

                // 如果時間增加後超過低時間警告閾值，重置警告狀態
                if (RemainingTime > LowTimeWarning)
                {
                    m_IsLowTimeWarningTriggered = false;
                }

                // 觸發時間變化事件
                OnTimeChanged?.Invoke(RemainingTime);
            }
        }

        /// <summary>
        /// 暫停/恢復計時器
        /// </summary>
        /// <param name="paused">是否暫停</param>
        public void SetTimerPaused(bool paused)
        {
            // 可以通過設置 m_IsTimeUp 來暫停計時器
            // 但要小心不要破壞遊戲狀態，這裡先留空，可根據需要實作
        }

        /// <summary>
        /// 重置遊戲時間（重新開始關卡時使用）
        /// </summary>
        public void ResetTimer()
        {
            RemainingTime = MaxTime;
            m_IsLowTimeWarningTriggered = false;
            m_IsTimeUp = false;
            OnTimeChanged?.Invoke(RemainingTime);
        }
    }
}