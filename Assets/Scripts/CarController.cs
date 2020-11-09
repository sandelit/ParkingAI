using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarController : Agent
{
    private GameObject Car;
    private float currentSteerAngle;
    private float currentAcceleration;
    private float currentbreakForce;
    private bool isBreaking;
    Vector3 directionToSpot;
        
    Rigidbody rb;

    [SerializeField] private GameObject ParkingSpot;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;


    public override void Initialize()
    {
        Car = GameObject.Find("Car");
        rb = Car.GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, 0.3f, 0);
    }

    public override void OnEpisodeBegin()
    {
        ResetCar();
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        currentSteerAngle = vectorAction[0];
        currentAcceleration = vectorAction[1];
        currentbreakForce = vectorAction[2];

        AddReward(-0.001f);
        
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
        actionsOut[2] = Input.GetAxis("Jump");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cars") || other.gameObject.CompareTag("Decoration")
        || other.gameObject.CompareTag("House") || other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
            ResetCar();
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {

        if(GameController.isQuitting)
        {
            return;
        }

        directionToSpot = ParkingSpot.transform.position - transform.position;
        float angleToParkingSpot = Vector3.Angle(directionToSpot, transform.forward);

        sensor.AddObservation(transform.position.normalized); // Bilens position
        sensor.AddObservation(transform.forward); // Bilens Z-rotation
        sensor.AddObservation(transform.right); // Bilens X-rotation
        sensor.AddObservation(rb.velocity); // Bilens hastighet                             ??????? kanske idk ????
        sensor.AddObservation(ParkingSpot.transform.position); // Parkeringens position
        sensor.AddObservation(directionToSpot); // Riktning mot parkering

        AddReward(angleToParkingSpot * -0.0001f);
        if(rb.velocity.x > 1 || rb.velocity.z > 1){
            AddReward(0.005f);
        }
    }


    private void FixedUpdate()
    {
        RequestDecision();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        CheckIfParked();
        CheckIfOnRoad();
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = currentAcceleration * motorForce;
        frontRightWheelCollider.motorTorque = currentAcceleration * motorForce;

        if (currentbreakForce > 0)
        {
            currentbreakForce = breakForce;
        }
        else
        {
            currentbreakForce = 0f;
        }
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        frontLeftWheelCollider.steerAngle = currentSteerAngle * maxSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle * maxSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    public void ResetCar()
    {
        Vector3 position = new Vector3(UnityEngine.Random.Range(-230f, -235f), 0, UnityEngine.Random.Range(0f, 40f));
        Vector3 rotation = new Vector3(0, 0, UnityEngine.Random.Range(-1, 1));

        frontLeftWheelCollider.attachedRigidbody.velocity = new Vector3(0f, 0f, 0f);

        Car.transform.position = position;
        Car.transform.rotation = Quaternion.LookRotation(rotation);
    }

    public void CheckIfParked()
    {
        bool isParked = false;

        float FL = Vector3.Distance(frontLeftWheelTransform.position, ParkingSpot.transform.position);
        float FR = Vector3.Distance(frontRightWheeTransform.position, ParkingSpot.transform.position);
        float RL = Vector3.Distance(rearLeftWheelTransform.position, ParkingSpot.transform.position);
        float RR = Vector3.Distance(rearRightWheelTransform.position, ParkingSpot.transform.position);

        if (FL < 2f && FR < 2f && RL < 2f && RR < 2f)
        {
            isParked = true;
        }

        if(isParked)
        {
            AddReward(5.0f);
            EndEpisode();
            ResetCar();
        }
    }

    public void CheckIfOnRoad()
    {
        bool onRoad = false;

        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Roads");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        float FL = Vector3.Distance(frontLeftWheelTransform.position, closest.transform.position);
        float FR = Vector3.Distance(frontRightWheeTransform.position, closest.transform.position);
        float RL = Vector3.Distance(rearLeftWheelTransform.position, closest.transform.position);
        float RR = Vector3.Distance(rearRightWheelTransform.position, closest.transform.position);

        if(FL < 5f && FR < 5f && RL < 5f && RR < 5f)
        {
            onRoad = true;
        }

        if(onRoad)
        {
            AddReward(0.01f);
        }

    }
}
