using Unity.Mathematics;
using UnityEngine;

public class IK3D : MonoBehaviour {

    [SerializeField]
    float speed = 10f;

    Transform arm1;
    Transform arm2;
    Transform arm3;

    [SerializeField]
    Transform target;

    float t1 = 0;
    float t2 = 0;
    float t3 = 0;

    float arm1Length = 2f;
    float arm2Length = 2f;
    float arm3Length = 2f;

    void Start() {
        arm1 = transform;
        arm2 = transform.GetChild(0);
        arm3 = arm2.GetChild(0);
    }

	void Update() {
        float3 p0 = arm1.position;
        float3 p1 = arm2.position;
        float3 p2 = arm3.position;
        float3 p3 = arm3.GetChild(0).GetChild(0).position;

        float3 axis0 = arm1.TransformDirection(Vector3.up);
        float3 axis1 = arm2.TransformDirection(Vector3.forward);
        float3 axis2 = arm3.TransformDirection(Vector3.forward);

        float3 delta = SolveIKStep3D(target.position, p0, p1, p2, p3, axis0, axis1, axis2);

        print("delta: " + delta);

        float alpha = Time.deltaTime * speed;
        t1 += delta.x * alpha;
        t2 += delta.y * alpha;
        t3 += delta.z * alpha;

        //arm1.localRotation = Quaternion.AngleAxis(t1 * Mathf.Rad2Deg, Vector3.up);
        //arm2.localRotation = Quaternion.AngleAxis(t2 * Mathf.Rad2Deg, Vector3.forward);
        //arm3.localRotation = Quaternion.AngleAxis(t3 * Mathf.Rad2Deg, Vector3.forward);

        arm1.localRotation = Quaternion.Euler(0f, t1 * Mathf.Rad2Deg, -25f);
        arm2.localRotation = Quaternion.Euler(0f, 0f, t2 * Mathf.Rad2Deg);
        arm3.localRotation = Quaternion.Euler(0f, 0f, t3 * Mathf.Rad2Deg);
    }

    float3 SolveIKStep3D(
    float3 target,
    float3 p0, float3 p1, float3 p2, float3 p3,   // joint positions (0=root, 3=end)
    float3 axis0, float3 axis1, float3 axis2,     // joint rotation axes
    float lambda = 0.1f,
    float alpha = 0.5f) {
        // -------------------------
        // 1. error
        // -------------------------
        float3 error = target - p3;

        // -------------------------
        // 2. build Jacobian (3x3)
        // J columns = axis x (end - joint)
        // -------------------------
        float3 c0 = math.cross(axis0, (p3 - p0));
        float3 c1 = math.cross(axis1, (p3 - p1));
        float3 c2 = math.cross(axis2, (p3 - p2));

        float3x3 J = new float3x3(
            c0.x, c1.x, c2.x,
            c0.y, c1.y, c2.y,
            c0.z, c1.z, c2.z
        );

        // -------------------------
        // 3. compute J^T
        // -------------------------
        float3x3 JT = math.transpose(J);

        // -------------------------
        // 4. compute J^T J
        // -------------------------
        float3x3 JTJ = math.mul(JT, J);

        // -------------------------
        // 5. damping matrix
        // -------------------------
        float3x3 damping = new float3x3(
            lambda * lambda, 0, 0,
            0, lambda * lambda, 0,
            0, 0, lambda * lambda
        );

        float3x3 A = JTJ + damping;

        // -------------------------
        // 6. solve linear system:
        // (J^T J + λ²I) Δθ = J^T e
        // -------------------------
        float3 rhs = math.mul(JT, error);

        float3 dTheta = math.mul(math.inverse(A), rhs);

        // -------------------------
        // 7. step scale
        // -------------------------
        return alpha * dTheta;
    }
}