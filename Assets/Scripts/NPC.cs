using UnityEngine;

public class Npc : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 0.01f;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float turnRate = 15;
    private static Terrain _terrain;
    private static TrafficController _trafficController;

    public float Speed { get; private set; } = 0f;
    public float Length { get; private set; } = 0f;
    public float Width { get; private set; } = 0f;
    private float _topSpeedPercentage;
    private Vector3 _target;

    public Vector3 GetVehicleFront()
    {
        Transform t = transform;
        return t.position + t.forward * Length / 2;
    }

    // Start is called before the first frame update
    private void Start()
    {
        _terrain ??= Terrain.activeTerrain;
        _trafficController ??= _terrain.GetComponent<TrafficController>();
        Vector3 size = gameObject.GetComponent<Renderer>().bounds.size;
        Length = size.z;
        Width = size.x;
    }

    // Update is called once per frame
    private void Update()
    {
        _target = _trafficController.CalcDirection(this);
        float currentSpeed = AdjustSpeed(_target);
        
        RotateAndMove();

        void RotateAndMove()
        {
            Quaternion q = Quaternion.LookRotation(_target - transform.position);
            float maxDegreesDelta = (turnRate / _topSpeedPercentage) * Time.deltaTime;
            Quaternion rotation = transform.rotation;
            rotation = Quaternion.RotateTowards(rotation, q, maxDegreesDelta / 2);
            Move(currentSpeed);
            rotation = Quaternion.RotateTowards(rotation, q, maxDegreesDelta / 2);
            transform.rotation = rotation;
        }
    }

    private float AdjustSpeed(Vector3 target)
    {
        float oldSpeed = Speed;

        Vector3 position = transform.position;
        float distanceToTarget = Vector3.Distance(position, target);
        float angleToTarget = Vector3.Angle(position, target);
        const float safetyMargin = 0.8f;
        bool canBrakeInTime = CanBrakeInTime(distanceToTarget, safetyMargin); 
        bool canCorner = CanCorner(angleToTarget, safetyMargin); 
        
        if (canBrakeInTime && canCorner) {
            Speed = Mathf.Min(acceleration * Time.deltaTime + Speed, maxSpeed);
        }
        else
        {
            Speed = Mathf.Max(Speed - acceleration * Time.deltaTime, 0);
        }
        _topSpeedPercentage = Mathf.Clamp(Speed / maxSpeed, 0.3f, 1f);
        
        // During the frame, the speed has changed.
        // This is the average of the speed before and after the acceleration.
        float averageSpeed = (oldSpeed + Speed) / 2;
        return averageSpeed;
    }

    public bool CanCorner(float angleToTarget, float safetyMargin)
    {
        return angleToTarget < turnRate * safetyMargin / _topSpeedPercentage;
    }

    /**
     * Does not take angle/turning into account
     */
    public bool CanBrakeInTime(float distanceToTarget, float safetyMargin)
    {
        return acceleration * safetyMargin > (Speed * Speed) / (2 * distanceToTarget);
    }

    private void Move(float currentSpeed)
    {
        transform.Translate(Vector3.forward * (currentSpeed * Time.deltaTime));
    }
}