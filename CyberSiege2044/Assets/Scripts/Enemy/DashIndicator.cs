using UnityEngine;

public class DashIndicator : MonoBehaviour
{
    private LineRenderer lr;
    private Transform boss;
    private Transform target;

    public void Init(Transform boss, Transform target)
    {
        this.boss = boss;
        this.target = target;
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (lr == null  || boss == null||  target == null) return;

        lr.positionCount = 2;
        lr.SetPosition(0, boss.position);
        lr.SetPosition(1, target.position);
    }
}