using System;
using System.Collections;
using UnityEngine;


public static class ExtensionMethods
{
    /// <summary>
    /// Call function after a delay
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="f">Function to call.</param>
    /// <param name="delay">Wait time before calling function.</param>
    public static void Invoke(this MonoBehaviour monoB, Action targetFunction, float delay)
    {
        monoB.StartCoroutine(InvokeRoutine(targetFunction, delay));
    }

    private static IEnumerator InvokeRoutine(Action targetFunction, float delay)
    {
        yield return new WaitForSeconds(delay);
        targetFunction();
    }


    #region SetParent Extension
    /// <summary>
    /// set keeplocalPos to false so the transfrom's local position becomes 0,0,0
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="parent"></param>
    /// <param name="keepLocalPos"></param>
    /// <param name="keepLocalRot"></param>
    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot)
    {
        if (parent == null)
        {
            Debug.LogWarning("You are trying to set a transform to a parent that doesnt exist, just so you know");
            return;
        }

        trans.SetParent(parent, false);

        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
    }


    /// <summary>
    /// set keeplocalPos to false so the transfrom's local position becomes 0,0,0
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="parent"></param>
    /// <param name="keepLocalPos"></param>
    /// <param name="keepLocalRot"></param>
    /// <param name="keepLocalScale"<>/param>
    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot, bool keepLocalScale)
    {
        if (parent == null)
        {
            Debug.LogWarning("You are trying to set a transform to a parent that doesnt exist, just so you know");
            return;
        }



        trans.SetParent(parent, false);

        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
        if (!keepLocalScale)
        {
            trans.localScale = Vector3.one;
        }
    }
    #endregion
}


public static class VectorLogic
{
    /// <summary>
    /// Instantly move a vector3 towards the new Vector3, up to maxDistance
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="maxDist"></param>
    /// <returns>The new Position</returns>
    public static Vector3 InstantMoveTowards(Vector3 from, Vector3 to, float maxDist)
    {
        // Calculate the direction vector and its magnitude
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        // If the distance is less than or equal to maxDist, move directly to the target
        if (distance <= maxDist)
        {
            return to;
        }

        // Normalize the direction and scale by maxDist
        Vector3 move = direction.normalized * maxDist;

        // Return the new position
        return from + move;
    }


    public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        value.z = Mathf.Clamp(value.z, min.z, max.z);

        return value;
    }
}