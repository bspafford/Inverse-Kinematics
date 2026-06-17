using System;
using System.Collections;
using UnityEngine;

public class Animations : MonoBehaviour {
    public static IEnumerator AnimateAngle(Angle angle, float startAngle, float endAngle, float duration) {
        float originalAngle = angle.startAngle;

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float logistic = Math.Logistical(percent);

            angle.startAngle = Mathf.Lerp(originalAngle, startAngle, logistic);
            angle.endAngle = Mathf.Lerp(originalAngle, endAngle, logistic);

            yield return null;
        }

        angle.startAngle = startAngle;
        angle.endAngle = endAngle;
    }

    public static IEnumerator AnimateDashedLines(DashedLine line, Vector2 loc, float duration) {
        float dist = loc.magnitude;

        // set line rotation;
        Vector2 dir = loc;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.transform.eulerAngles = new Vector3(0f, 0f, angle);

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            float percent = elapsed / duration;

            float logistic = Math.Logistical(percent);

            line.end.x = Mathf.Lerp(0f, dist, logistic);

            yield return null;
        }

        line.end.x = dist;
    }

    public static IEnumerator Rotate(Transform t, Vector3 rot, float duration, bool local = false) {
        Vector3 startRot = local ? t.localEulerAngles : t.eulerAngles;

        if (Mathf.Abs(rot.x + 360 - startRot.x) < Mathf.Abs(rot.x - startRot.x))
            rot.x += 360f;
        if (Mathf.Abs(rot.y + 360 - startRot.y) < Mathf.Abs(rot.y - startRot.y))
            rot.y += 360f;
        if (Mathf.Abs(rot.z + 360 - startRot.z) < Mathf.Abs(rot.z - startRot.z))
            rot.z += 360f;

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float logistical = Math.Logistical(percent);

            if (local)
                t.localEulerAngles = Vector3.Lerp(startRot, rot, logistical);
            else
                t.eulerAngles = Vector3.Lerp(startRot, rot, logistical);

            yield return null;
        }

        if (local)
            t.localEulerAngles = new Vector3(rot.x, rot.y, rot.z);
        else
            t.eulerAngles = new Vector3(rot.x, rot.y, rot.z);
    }

    public static IEnumerator MoveTo(Transform t, Vector3 loc, float duration, bool useRelativePos = false) {
        Vector3 startLoc = t.position;
        if (useRelativePos)
            startLoc = t.localPosition;

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float logistical = Math.Logistical(percent);

            Vector3 pos = Vector3.Lerp(startLoc, loc, logistical);
            if (!useRelativePos)
                t.position = pos;
            else
                t.localPosition = pos;


            yield return null;
        }

        if (!useRelativePos)
            t.position = loc;
        else
            t.localPosition = loc;

        yield return null;
    }

    public static IEnumerator Fade(Func<Color> getColor, Action<Color> setColor, bool fadeOut, float duration) {
        Color color = getColor();

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float logistical = Math.Logistical(percent);

            color.a = fadeOut ? 1f - logistical : logistical;
            setColor(color);

            yield return null;
        }

        color.a = fadeOut ? 0f : 1f;
        setColor(color);

        yield return null;
    }

    public static IEnumerator SetColor(Func<Color> getColor, Action<Color> setColor, Color targetColor, float duration) {
        Color color = getColor();

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float logistical = Math.Logistical(percent);

            setColor(Color.Lerp(color, targetColor, logistical));

            yield return null;
        }

        setColor(targetColor);
        yield return null;
    }

    public static IEnumerator Scale(Transform t, Vector3 scale, float duration) {
        Vector3 startScale = t.localScale;

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float logistical = Math.Logistical(percent);

            Vector3 newScale = Vector3.Lerp(startScale, scale, logistical);
            t.localScale = newScale;

            yield return null;
        }

        t.localScale = scale;

        yield return null;
    }

    public static IEnumerator CustomFloat(Func<float> getFloat, Action<float> setFloat, float from, float to, float duration, float k = 10f) {
        float startingFloat = getFloat();

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float logistical = Math.Logistical(percent, k);

            startingFloat = Mathf.Lerp(from, to, logistical);
            setFloat(startingFloat);

            yield return null;
        }

        startingFloat = to;
        setFloat(startingFloat);

        yield return null;
    }
}