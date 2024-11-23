using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Tray logic for holding bread, changing it, and giving bread to supply
/// </summary>
public class TrayBehaviour : CacheBehaviour<TrayBehaviour>
{
    [SerializeField]
    private TouchableDrag touchDrag;

    [SerializeField]
    private PlayerTool trayTool;

    [Header("Bread spacing")]
    [SerializeField]
    private float minScale;

    [SerializeField]
    private float maxScale;

    [SerializeField]
    private float minY;

    [SerializeField]
    private float maxY;
    
    [SerializeField]
    private int maxBreadSpaces;

    public TouchableDrag TrayDrag => touchDrag;

    public bool IsEmptySpaceAvailable => trayBreadList.Count < maxBreadSpaces;

    private List<BreadData> trayBreadList;

    private OvenBehaviour ovenCache;

    void Start()
    {
        touchDrag.DropEvent += DropCheckForOven;
        trayBreadList = new List<BreadData>();
    }

    private void DropCheckForOven(PlayerHand hand)
    {
        Collider2D collider = CollisionHandler.CheckPositionForCollider(transform.position, Consts.DROP_AREA_MASK);
        
        if (collider == null)
            return;

        if (!collider.CompareTag("Oven"))
        {
            touchDrag.Fall();
            return;
        }
        else
        {
            // Don't put a tray in the oven if there's no bread on it
            if (transform.childCount < 1)
            {
                touchDrag.Fall();
                return;
            }

            if (!ovenCache)
                ovenCache = collider.GetComponent<OvenBehaviour>();

            if (ovenCache.TryConsumeTray(touchDrag.gameObject))
            {
                // Swap out listening for the oven when dropped
                // to instead check for the shop, when the tray is taken out
                touchDrag.DropEvent -= DropCheckForOven;
                touchDrag.DropEvent += DropCheckForShopFront;
                // Put another tray back to use
                trayTool.Replace();

                // Make a used tray appear above unused trays etc.
                ++touchDrag.GetComponent<SpriteRenderer>().sortingOrder;
                ++touchDrag.GetComponent<SortingGroup>().sortingOrder;
            }
            else
                touchDrag.Fall();
        }
    }

    private void DropCheckForShopFront(PlayerHand hand)
    {
        Collider2D collider = CollisionHandler.CheckPositionForCollider(transform.position, Consts.DROP_AREA_MASK);
        if (collider == null)
            return;
        
        if (collider.CompareTag("Shop"))
        {
            // Add bread to supply and delete tray
            for (int i = 0; i < trayBreadList.Count; ++i)
            {
                BreadData currentBread = trayBreadList[i];
                currentBread.EvaluateBread();
                SupplyDemandManager.Instance.IncreaseSupply(1, currentBread.BreadType);
                DayStats.ModifyStat(Consts.DayTracking.BreadQuality, currentBread.BreadQuality);
            }
            Destroy(touchDrag.gameObject);
        }
        else
            touchDrag.Fall();
    }

    public void ConvertDoughToBread()
    {
        for(int i = 0; i < trayBreadList.Count; ++i)
            trayBreadList[i].SetFinishedSprites();
    }

    public void SetOvercookTime(int time)
    {
        for (int i = 0; i < trayBreadList.Count; ++i)
            trayBreadList[i].SetOvercook(time);
    }

    private void OnTransformChildrenChanged()
    {
        if (transform.childCount <= trayBreadList.Count)
            return;

        // Position and scale bread on tray to accommodate multiple
        Transform newBread = transform.GetChild(transform.childCount - 1);
        Vector3 pos = newBread.localPosition;
        float tLerp = trayBreadList.Count / (float)(maxBreadSpaces - 1);
        pos.x = 0f;
        pos.y = Mathf.Lerp(maxY, minY, tLerp);
        newBread.localPosition = pos;
        newBread.localScale *= Mathf.Lerp(minScale, maxScale, tLerp);
        trayBreadList.Add(BreadData.GetCached(newBread.gameObject));
    }
}