using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scrolling menu background controller
/// </summary>
public class BaguetteMenuBackground : MonoBehaviour
{
    [SerializeField]
    private GameObject baguetteBase;

    [SerializeField]
    private Vector3 startPos;

    [SerializeField]
    private Vector3 endPos;

    private Camera sceneCam;

    void Start()
    {
        // Create a bunch of baguettes to scroll
        for(int i = -20; i <= 20; i += 4)
        {
            for(int j = -20; j <= 20; j += 4)
            {
                Instantiate(baguetteBase, baguetteBase.transform.position + new Vector3(i, j), baguetteBase.transform.rotation, transform);
            }
        }
        transform.position = startPos;
        LeanTween.move(gameObject, endPos, 20f).setLoopClamp();
        sceneCam = Camera.main;
    }

    private void OnMouseDown()
    {
        Collider2D coll = Physics2D.OverlapPoint(sceneCam.ScreenToWorldPoint(Input.mousePosition));
        if (coll == null)
            return;

        if (LeanTween.isTweening(coll.gameObject))
            return;

        // Animate a baguette that's been clicked on
        float origAngle = coll.transform.localEulerAngles.z;
        LeanTween.rotateZ(coll.gameObject, origAngle - 20f, 0.25f).setEase(LeanTweenType.easeInSine);
        LeanTween.rotateZ(coll.gameObject, origAngle + 25f, 0.3f).setDelay(0.25f).setEase(LeanTweenType.easeInSine);
        LeanTween.rotateZ(coll.gameObject, origAngle, 0.25f).setDelay(0.55f).setEase(LeanTweenType.easeOutBack);
    }
}