using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.GraphicsBuffer;

public class bully : MonoBehaviour {

    [SerializeField]
    LiveGraph graph;

    [SerializeField]
    float arm1Length = 3f;
    [SerializeField]
    float arm2Length = 2f;
    [SerializeField]
    float arm3Length = 2f;

    [SerializeField]
    Vector3 desiredLoc = new Vector3(3, 1, 0);

    [SerializeField]
    bool useUpdate = false;
    [SerializeField]
    bool useMousePos = false;

    [SerializeField]
    float speed = 5f;

    Transform arm1;
    Transform arm2;
    Transform arm3;

    public float t1 = 0f;
    public float t2 = 0f;
    public float t3 = 0f;

    InputAction next;

    public int grabbedIndex = 0;

    InputAction mouseDown;

    void Start() {
        arm1 = transform;
        arm2 = transform.GetChild(0);
        arm3 = arm2.GetChild(0);

        next = InputSystem.actions.FindAction("Next");
        next.started += Next;

        t1 = arm1.localEulerAngles.z * Mathf.Deg2Rad;
        t2 = arm2.localEulerAngles.z * Mathf.Deg2Rad;
        t3 = arm3.localEulerAngles.z * Mathf.Deg2Rad;

        mouseDown = InputSystem.actions.FindAction("Attack");
    }

    void Update() {
        if (useMousePos)
            desiredLoc = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;

        if (useUpdate)
            Correct();
    }

    void Next(InputAction.CallbackContext context) {
        Correct();
    }

    void Correct() {
        float3 dq = SolveIKStep1(new float2(desiredLoc.x, desiredLoc.y), grabbedIndex, t1, t2, t3, arm1Length, arm2Length, arm3Length);
        if (mouseDown.ReadValue<float>() == 1f) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
            float3 temp = SolveIKStep(new float2(mousePos.x, mousePos.y), grabbedIndex, t1, t2, t3, arm1Length, arm2Length, arm3Length);
            dq = temp;// (dq + temp) / 2f;
        }

        t1 += dq.x * Time.deltaTime * speed;
        t2 += dq.y * Time.deltaTime * speed;
        t3 += dq.z * Time.deltaTime * speed;

        arm1.localRotation = Quaternion.Euler(0f, 0f, t1 * Mathf.Rad2Deg);
        arm2.localRotation = Quaternion.Euler(0f, 0f, t2 * Mathf.Rad2Deg);
        arm3.localRotation = Quaternion.Euler(0f, 0f, t3 * Mathf.Rad2Deg);
    }

    float2x3 ComputeJacobianPoint(
    int jointIndex, // 1, 2, or 3
    float t1, float t2, float t3,
    float l1, float l2, float l3) {
        float c1 = Mathf.Cos(t1);
        float s1 = Mathf.Sin(t1);
        float c12 = Mathf.Cos(t1 + t2);
        float s12 = Mathf.Sin(t1 + t2);
        float c123 = Mathf.Cos(t1 + t2 + t3);
        float s123 = Mathf.Sin(t1 + t2 + t3);

        float xt1 = 0, xt2 = 0, xt3 = 0;
        float yt1 = 0, yt2 = 0, yt3 = 0;

        if (jointIndex >= 1) {
            xt1 = -l1 * s1;
            yt1 = l1 * c1;
        }

        if (jointIndex >= 2) {
            xt1 += -l2 * s12;
            yt1 += l2 * c12;

            xt2 = -l2 * s12;
            yt2 = l2 * c12;
        }

        if (jointIndex >= 3) {
            xt1 += -l3 * s123;
            yt1 += l3 * c123;

            xt2 += -l3 * s123;
            yt2 += l3 * c123;

            xt3 = -l3 * s123;
            yt3 = l3 * c123;
        }

        return new float2x3(
            xt1, xt2, xt3,
            yt1, yt2, yt3
        );
    }

    float2 ForwardPoint(
    int jointIndex,
    float t1, float t2, float t3,
    float l1, float l2, float l3) {
        float c1 = Mathf.Cos(t1);
        float s1 = Mathf.Sin(t1);
        float c12 = Mathf.Cos(t1 + t2);
        float s12 = Mathf.Sin(t1 + t2);
        float c123 = Mathf.Cos(t1 + t2 + t3);
        float s123 = Mathf.Sin(t1 + t2 + t3);

        float x = 0;
        float y = 0;

        if (jointIndex >= 1) {
            x += l1 * c1;
            y += l1 * s1;
        }

        if (jointIndex >= 2) {
            x += l2 * c12;
            y += l2 * s12;
        }

        if (jointIndex >= 3) {
            x += l3 * c123;
            y += l3 * s123;
        }

        return new float2(x, y);
    }

    float3 SolveIKStep(
    float2 desiredLoc,
    int grabbedIndex, // 0, 1, 2
    float t1, float t2, float t3,
    float l1, float l2, float l3,
    float alpha = 0.5f,
    float lambda = 0.1f) {

        desiredLoc = new float2(desiredLoc.y, -desiredLoc.x);

        float length = arm1Length;
        if (grabbedIndex >= 1)
            length += arm2Length;
        if (grabbedIndex >= 2)
            length += arm3Length;

        // --- Forward kinematics ---
        float c1 = Mathf.Cos(t1);
        float s1 = Mathf.Sin(t1);
        float c12 = Mathf.Cos(t1 + t2);
        float s12 = Mathf.Sin(t1 + t2);
        float c123 = Mathf.Cos(t1 + t2 + t3);
        float s123 = Mathf.Sin(t1 + t2 + t3);

        float2 current = new float2(
            l1 * c1 + l2 * c12 + l3 * c123,
            l1 * s1 + l2 * s12 + l3 * s123
        );

        float2 current1 = new float2(
            l1 * c1,
            l1 * s1
        );

        float2 current2 = new float2(
            l1 * c1 + l2 * c12,
            l1 * s1 + l2 * s12
        );

        // --- Error ---
        //float2 error = desiredLoc - current;
        float2 error = new float2(0f);
        if (grabbedIndex == 0)
            error = desiredLoc - current1;
        else if (grabbedIndex == 1)
            error = desiredLoc - current2;
        else if (grabbedIndex == 2)
            error = desiredLoc - current;

        if (graph)
            graph.Add(error);

        // --- Jacobian (2x3) ---
        float xt3 = -l3 * s123;
        float xt2 = -l2 * s12 + xt3;
        float xt1 = -l1 * s1 + xt2;

        float yt3 = l3 * c123;
        float yt2 = l2 * c12 + yt3;
        float yt1 = l1 * c1 + yt2;

        float2x3 J = new float2x3(
            xt1, xt2, xt3,
            yt1, yt2, yt3
        );

        // --- Damped pseudoinverse ---
        float3x2 JT = math.transpose(J);                  // 3x2
        float2x2 JJt = math.mul(J, JT);                   // 2x2

        float2x2 damping = new float2x2(
            lambda * lambda, 0,
            0, lambda * lambda
        );

        float2x2 inv = math.inverse(JJt);     // 2x2

        float3x2 J_pinv = math.mul(JT, inv);                // 3x2

        // --- Delta theta ---
        float3 dTheta = math.mul(J_pinv, error);            // 3x1

        // --- Step size ---
        return alpha * dTheta;
    }

    float3 SolveIKStep1(
    float2 desiredLoc,
    int grabbedIndex, // 0, 1, 2
    float t1, float t2, float t3,
    float l1, float l2, float l3,
    float alpha = 0.5f,
    float lambda = 0.1f) {

        desiredLoc = new float2(desiredLoc.y, -desiredLoc.x);

        float length = arm1Length;
        if (grabbedIndex >= 1)
            length += arm2Length;
        if (grabbedIndex >= 2)
            length += arm3Length;

        // --- Forward kinematics ---
        float c1 = Mathf.Cos(t1);
        float s1 = Mathf.Sin(t1);
        float c12 = Mathf.Cos(t1 + t2);
        float s12 = Mathf.Sin(t1 + t2);
        float c123 = Mathf.Cos(t1 + t2 + t3);
        float s123 = Mathf.Sin(t1 + t2 + t3);

        float2 current = new float2(
            l1 * c1 + l2 * c12 + l3 * c123,
            l1 * s1 + l2 * s12 + l3 * s123
        );

        // --- Error ---
        float2 error = desiredLoc - current;

        if (graph)
            graph.Add(error);

        // --- Jacobian (2x3) ---
        float xt3 = -l3 * s123;
        float xt2 = -l2 * s12 + xt3;
        float xt1 = -l1 * s1 + xt2;

        float yt3 = l3 * c123;
        float yt2 = l2 * c12 + yt3;
        float yt1 = l1 * c1 + yt2;

        float2x3 J = new float2x3(
            xt1, xt2, xt3,
            yt1, yt2, yt3
        );

        // --- Damped pseudoinverse ---
        float3x2 JT = math.transpose(J);                  // 3x2
        float2x2 JJt = math.mul(J, JT);                   // 2x2

        float2x2 damping = new float2x2(
            lambda * lambda, 0,
            0, lambda * lambda
        );

        float2x2 inv = math.inverse(JJt);     // 2x2

        float3x2 J_pinv = math.mul(JT, inv);                // 3x2

        // --- Delta theta ---
        float3 dTheta = math.mul(J_pinv, error);            // 3x1

        // --- Step size ---
        return alpha * dTheta;
    }
}