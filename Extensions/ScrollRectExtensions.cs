using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts
{
public static class ScrollRectExtensions
{
    public static void CenterOnItem(this ScrollRect scroll, RectTransform target)
    {
        scroll.normalizedPosition = GetNormalizedPositionToCenter(scroll, target);
    }

    public static Vector2 GetNormalizedPositionToCenter(this ScrollRect scroll, RectTransform target)
    {
        RectTransform rt = scroll.transform as RectTransform;
        // Item is here
        var itemCenterPositionInScroll = GetWorldPointInWidget(rt, GetWidgetWorldPoint(target));
        // But must be here
        var targetPositionInScroll = GetWorldPointInWidget(rt, GetWidgetWorldPoint(scroll.viewport));
        // So it has to move this distance
        var difference = targetPositionInScroll - itemCenterPositionInScroll;
        difference.z = 0f;
 
        //clear axis data that is not enabled in the scrollrect
        if (!scroll.horizontal)
        {
            difference.x = 0f;
        }
        if (!scroll.vertical)
        {
            difference.y = 0f;
        }
 
        var normalizedDifference = new Vector2(
            difference.x / (scroll.content.rect.size.x - rt.rect.size.x),
            difference.y / (scroll.content.rect.size.y - rt.rect.size.y));
 
        var newNormalizedPosition = scroll.normalizedPosition - normalizedDifference;
        if (scroll.movementType != ScrollRect.MovementType.Unrestricted)
        {
            newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
            newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
        }

        return newNormalizedPosition;
    }
    private static Vector3 GetWidgetWorldPoint(RectTransform target)
    {
        //pivot position + item size has to be included
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            (0.5f - target.pivot.y) * target.rect.size.y,
            0f);
        var localPosition = target.localPosition + pivotOffset;
        return target.parent.TransformPoint(localPosition);
    }
    private static Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
    {
        return target.InverseTransformPoint(worldPoint);
    }
}
}