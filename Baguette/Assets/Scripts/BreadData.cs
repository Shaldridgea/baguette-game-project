using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Data describing a bread instance and its attributes
/// </summary>
public class BreadData : CacheBehaviour<BreadData>
{
    public Consts.BreadType BreadType { get; private set; }

    public bool Finished { get; private set; }

    public Transform DoughSpriteTransform => spriteRend.transform;
    
    [SerializeField]
    private SpriteRenderer spriteRend;

    [SerializeField]
    private SortingGroup sortGroup;

    [SerializeField]
    private Sprite[] rollingStageSprites;

    [SerializeField]
    private Sprite[] cookedSprites;

    [SerializeField]
    private Sprite[] burntSprites;

    [SerializeField]
    private Sprite[] cookedSlashSprites;

    [SerializeField]
    private Sprite[] burntSlashSprites;

    private Dictionary<Consts.BreadType, int> ingredientCounts = new Dictionary<Consts.BreadType, int>();

    private static int totalBreadCount;

    private const int defaultSortingOrder = -90;

    private CapsuleCollider2D parentCollider;

    private CapsuleCollider2D spriteCollider;

    #region Quality values
    public float RollingStage { get; private set; }

    public const float IDEAL_ROLLING_STAGE = 2f;

    public int SlashCount { get; private set; }

    public float BreadQuality { get; private set; }

    private int overcookTime;

    private const int IDEAL_SLASH_COUNT = 3;

    private const float QUALITY_PENALTY = 1f;

    private const int OVERCOOK_THRESHOLD = 20;
    #endregion Quality values

    private void Start()
    {
        FlowManager.Instance.StateEnd += CheckBakingEnd;
        spriteRend.sortingOrder = defaultSortingOrder + totalBreadCount;
        sortGroup.sortingOrder = defaultSortingOrder + totalBreadCount;

        ++totalBreadCount;

        for (Consts.BreadType i = (Consts.BreadType)1; i < Consts.BreadType.Last; ++i)
            ingredientCounts.Add(i, 0);
        parentCollider = GetComponent<CapsuleCollider2D>();
        spriteCollider = spriteRend.GetComponent<CapsuleCollider2D>();
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateEnd -= CheckBakingEnd;
        --totalBreadCount;
    }

    private void CheckBakingEnd(Consts.GameState newState)
    {
        if (newState != Consts.GameState.Baking)
            return;

        Destroy(gameObject);
    }

    public void KnifeSlash() => ++SlashCount;

    public void SetOvercook(int newOvercookAmount) => overcookTime = newOvercookAmount;

    public void IncreaseRolling(float amount)
    {
        RollingStage += amount;
        if (RollingStage > rollingStageSprites.Length-1)
            RollingStage = rollingStageSprites.Length-1;
        spriteRend.sprite = rollingStageSprites[(int)RollingStage];
    }

    public void UpdateParentCollider()
    {
        parentCollider.size = spriteCollider.size * spriteRend.transform.localScale;
    }

    public void IncreaseIngredientCount(Consts.BreadType ingredientType)
    {
        if (!ingredientCounts.ContainsKey(ingredientType))
            return;

        ++ingredientCounts[ingredientType];
    }

    /// <summary>
    /// Evaluate the bread for its quality and type
    /// </summary>
    public void EvaluateBread()
    {
        float quality;
        if (overcookTime > OVERCOOK_THRESHOLD)
            quality = -QUALITY_PENALTY;
        else
        {
            quality = 1f - CalculateInaccuracy(IDEAL_ROLLING_STAGE, (int)RollingStage, 1f);
            float slashInaccuracy = CalculateInaccuracy(IDEAL_SLASH_COUNT, SlashCount, 1.5f);
            quality -= slashInaccuracy * QUALITY_PENALTY;
        }

        BreadQuality = quality;

        BreadType = Consts.BreadType.Normal;
        int highestCount = 0;
        for(Consts.BreadType i = (Consts.BreadType)1; i < Consts.BreadType.Last; ++i)
        {
            if(ingredientCounts[i] > highestCount)
            {
                highestCount = ingredientCounts[i];
                BreadType = i;
            }
        }
    }

    public void SetFinishedSprites()
    {
        Finished = true;
        bool overcooked = overcookTime > OVERCOOK_THRESHOLD;
        if (overcooked)
            spriteRend.sprite = burntSprites[(int)RollingStage];
        else
            spriteRend.sprite = cookedSprites[(int)RollingStage];

        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            string childTag = child.tag;
            switch (childTag)
            {
                case "Slash":
                child.GetComponent<SpriteRenderer>().sprite =
                    overcooked ? burntSlashSprites[Random.Range(0, burntSlashSprites.Length)] : cookedSlashSprites[Random.Range(0, cookedSlashSprites.Length)];
                break;
            }
        }
    }

    private float CalculateInaccuracy(float goal, float value, float upperPenaltyLimit)
    {
        return Mathf.Clamp(Mathf.Abs(goal - value) / goal, 0f, upperPenaltyLimit);
    }
}