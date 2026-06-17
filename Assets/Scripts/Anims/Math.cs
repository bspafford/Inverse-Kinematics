using UnityEngine;

public static class Math {
    public static float Logistical(float percent, float k = 10f) {
        percent = Mathf.Clamp01(percent);
        
        //return 1 / (1 + Mathf.Exp(-10 * (percent - 0.5f))); // f(0) = 0.00669, f(1) = 0.99331

        // more exact: f(0) = 0, f(1) = 1
        float x = percent;

        float sigmoid(float x) {
            return 1f / (1f + Mathf.Exp(-k * (x - 0.5f)));
        }

        float min = sigmoid(0f);
        float max = sigmoid(1f);

        return (sigmoid(x) - min) / (max - min);
    }
}