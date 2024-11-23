using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Graphics data holding fill and outline sprites for panels and buttons
/// </summary>
[CreateAssetMenu(fileName ="Random sprites data", menuName ="Panel data")]
public class PanelData : ScriptableObject
{
    [SerializeField]
    private Sprite[] smallFillGraphics;

    [SerializeField]
    private Sprite[] largeFillGraphics;

    [SerializeField]
    private Sprite[] smallOutlineGraphics;

    [SerializeField]
    private Sprite[] largeOutlineGraphics;

    /// <summary>
    /// Get random matching outline and fill graphics
    /// </summary>
    /// <param name="isLarge">Whether these are large or small sprites</param>
    /// <param name="isStatic">Whether to return the same sprites instead of randomising</param>
    /// <returns>Array where first index is the fill, second index is the outline</returns>
    public Sprite[] GetRandomGraphics(bool isLarge, bool isStatic = false)
    {
        int graphicIndex;
        Sprite[] graphics = new Sprite[2];
        if (isLarge)
        {
            graphicIndex = isStatic ? 0 : Random.Range(0, largeFillGraphics.Length);
            graphics[0] = largeFillGraphics[graphicIndex];
            graphics[1] = largeOutlineGraphics[graphicIndex];
        }
        else
        {
            graphicIndex = isStatic ? 0 : Random.Range(0, smallFillGraphics.Length);
            graphics[0] = smallFillGraphics[graphicIndex];
            graphics[1] = smallOutlineGraphics[graphicIndex];
        }
        return graphics;
    }
}