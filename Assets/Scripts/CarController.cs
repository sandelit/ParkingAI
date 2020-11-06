using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarController : Agent
{
    private GameObject Car;
    GameObject[] ParkingSpots;
    GameObject ParkingSpot;

    private float currentSteerAngle;
    private float currentAcceleration;
    private float currentbreakForce;
    private bool isBreaking;

    Rigidbody rb;

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
        ParkingSpots = GameObject.FindGameObjectsWithTag("ParkingSpot");
        ParkingSpot = ParkingSpots[0];

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

        AddReward(-0.01f);
        if(currentAcceleration > 0){
            AddReward(0.005f);
        }
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
            AddReward(-0.5f);
            EndEpisode();
            ResetCar();
        }
        if(other.gameObject.CompareTag("Roads")){
            AddReward(0.01f);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // TODO: Kolla om hjulen kolliderar med parkeringsrutan istället för bilens hitbox :))
        if (other.gameObject.CompareTag("ParkingSpot")){
            AddReward(5.0f);
            EndEpisode();
            ResetCar();
        }
        //////////////////////////////////////////////////////////////////////////////////////


    }


    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToTarget = (ParkingSpot.transform.position - transform.position).normalized;

        sensor.AddObservation(transform.position.normalized); // Bilens position
        sensor.AddObservation(transform.forward); // Bilens Z-rotation
        sensor.AddObservation(transform.right); // Bilens X-rotation
        sensor.AddObservation(rb.velocity); // Bilens hastighet                             ??????? kanske idk ????
        sensor.AddObservation(ParkingSpot.transform.position); // Parkeringens position
        sensor.AddObservation(dirToTarget); // Riktning mot parkering
    }


    private void FixedUpdate()
    {
        RequestDecision();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
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
}
