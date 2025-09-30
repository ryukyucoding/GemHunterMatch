using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 為食材添加圓弧搖擺動畫效果（類似鐘擺）
    /// </summary>
    public class FoodSwayAnimation : MonoBehaviour
    {
        [Header("圓弧搖擺設置")]
        [SerializeField] private readonly float arcRadius = 2f;              // 圓弧半徑
        [SerializeField] private readonly float maxSwayAngle = 60f;            // 最大搖擺角度（度）
        [SerializeField] private readonly float swaySpeed = 2f;               // 搖擺速度
        [SerializeField] private readonly bool randomStartOffset = true;       // 隨機起始偏移
        [SerializeField] private readonly bool playOnStart = true;            // 是否在開始時自動播放

        [Header("高級設置")]
        [SerializeField] private readonly bool useLocalPosition = true;        // 使用本地位置
        [SerializeField] private Vector3 pivotOffset = Vector3.down * 3f;   // 圓心偏移（相對於物體位置）
        [SerializeField] private AnimationCurve swayCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 搖擺曲線

        private Vector3 originalPosition;
        private Vector3 pivotPoint;
        private float timeOffset;
        private bool isPlaying = false;
        private Gem gemComponent;

        void Start()
        {
            // 保存原始位置
            originalPosition = useLocalPosition ? transform.localPosition : transform.position;

            // 計算圓心位置
            pivotPoint = originalPosition + pivotOffset;

            // 隨機起始時間偏移
            if (randomStartOffset)
            {
                timeOffset = Random.Range(0f, 2f * Mathf.PI);
            }

            // 獲取 Gem 組件
            gemComponent = GetComponent<Gem>();

            if (playOnStart)
            {
                StartSwaying();
            }
        }

        void Update()
        {
            if (!isPlaying) return;

            // 只有在寶石靜止時才搖擺
            if (gemComponent != null && gemComponent.CurrentState != Gem.State.Still)
            {
                return;
            }

            // 計算圓弧位置
            Vector3 arcPosition = CalculateArcPosition();

            // 應用位置
            if (useLocalPosition)
            {
                transform.localPosition = arcPosition;
            }
            else
            {
                transform.position = arcPosition;
            }
        }

        private Vector3 CalculateArcPosition()
        {
            // 計算當前時間的角度
            float time = Time.time * swaySpeed + timeOffset;
            float normalizedSin = Mathf.Sin(time);

            // 使用動畫曲線調整搖擺感覺
            float normalizedValue = (normalizedSin + 1f) * 0.5f; // -1~1 轉為 0~1
            float curvedValue = swayCurve.Evaluate(normalizedValue);
            float finalSin = curvedValue * 2f - 1f; // 0~1 轉回 -1~1

            // 計算當前角度（弧度）
            // 從垂直向上位置開始（90度），然後左右搖擺（倒鐘擺效果）
            float currentAngle = finalSin * maxSwayAngle * Mathf.Deg2Rad;
            float baseAngle = 90f * Mathf.Deg2Rad; // 垂直向上
            float finalAngle = baseAngle + currentAngle;

            // 使用極坐標計算圓弧上的位置
            Vector3 arcPosition = pivotPoint + new Vector3(
                Mathf.Cos(finalAngle) * arcRadius,     // X軸位置（左右搖擺）
                Mathf.Sin(finalAngle) * arcRadius,     // Y軸位置（上下變化）
                0f                                     // Z軸保持不變
            );

            return arcPosition;
        }

        /// <summary>
        /// 開始搖擺動畫
        /// </summary>
        public void StartSwaying()
        {
            isPlaying = true;
            // 更新原始位置和圓心
            originalPosition = useLocalPosition ? transform.localPosition : transform.position;
            pivotPoint = originalPosition + pivotOffset;
        }

        /// <summary>
        /// 停止搖擺動畫
        /// </summary>
        public void StopSwaying()
        {
            isPlaying = false;
            // 恢復到原始位置
            if (useLocalPosition)
            {
                transform.localPosition = originalPosition;
            }
            else
            {
                transform.position = originalPosition;
            }
        }

        /// <summary>
        /// 暫停搖擺動畫
        /// </summary>
        public void PauseSwaying()
        {
            isPlaying = false;
        }

        /// <summary>
        /// 恢復搖擺動畫
        /// </summary>
        public void ResumeSwaying()
        {
            isPlaying = true;
        }

        /// <summary>
        /// 設置新的原始位置
        /// </summary>
        public void SetOriginalPosition(Vector3 newPosition)
        {
            originalPosition = newPosition;
            pivotPoint = originalPosition + pivotOffset;
        }

        /// <summary>
        /// 重置到原始位置
        /// </summary>
        public void ResetPosition()
        {
            if (useLocalPosition)
            {
                transform.localPosition = originalPosition;
            }
            else
            {
                transform.position = originalPosition;
            }
        }

        // 在編輯器中顯示搖擺範圍
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Vector3 currentPivot = Application.isPlaying ? pivotPoint :
                ((useLocalPosition ? transform.localPosition : transform.position) + pivotOffset);

            // 畫圓心
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentPivot, 0.05f);

            // 畫圓弧範圍
            Gizmos.color = Color.yellow;
            int segments = 20;
            float baseAngle = 90f * Mathf.Deg2Rad; // 垂直向上
            float startAngle = baseAngle - maxSwayAngle * Mathf.Deg2Rad; // 左邊界
            float endAngle = baseAngle + maxSwayAngle * Mathf.Deg2Rad;   // 右邊界

            Vector3 prevPoint = currentPivot + new Vector3(
                Mathf.Cos(startAngle) * arcRadius,
                Mathf.Sin(startAngle) * arcRadius,
                0f
            );

            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
                Vector3 currentPoint = currentPivot + new Vector3(
                    Mathf.Cos(currentAngle) * arcRadius,
                    Mathf.Sin(currentAngle) * arcRadius,
                    0f
                );

                Gizmos.DrawLine(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }

            // 畫連接線
            Gizmos.color = Color.cyan;
            Vector3 objectPos = useLocalPosition ? transform.localPosition : transform.position;
            Gizmos.DrawLine(currentPivot, objectPos);

            // 標記搖擺邊界點
            Gizmos.color = Color.green;
            Vector3 leftPoint = currentPivot + new Vector3(
                Mathf.Cos(startAngle) * arcRadius,
                Mathf.Sin(startAngle) * arcRadius,
                0f
            );
            Vector3 rightPoint = currentPivot + new Vector3(
                Mathf.Cos(endAngle) * arcRadius,
                Mathf.Sin(endAngle) * arcRadius,
                0f
            );

            Gizmos.DrawWireSphere(leftPoint, 0.03f);
            Gizmos.DrawWireSphere(rightPoint, 0.03f);
        }
        #endif
    }
}