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

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
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
        /*
        _controller.CurrentSteeringAngle = vectorAction[0];
        _controller.CurrentAcceleration = vectorAction[1];
        _controller.CurrentBrakeTorque = vectorAction[2];
        */
    }

    public override void Heuristic(float[] actionsOut)
    {
    }

    private void OnCollisionEnter(Collision other)
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToTarget = (ParkingSpots[0].transform.position - transform.position).normalized;

        sensor.AddObservation(transform.position.normalized); // Bilens position
        sensor.AddObservation(transform.forward); // Bilens Z-rotation
        sensor.AddObservation(transform.right); // Bilens X-rotation
        sensor.AddObservation(rb.velocity); // Bilens hastighet ??????? kanske idk ????
        sensor.AddObservation(ParkingSpots[0].transform.position); // Parkeringens position
        sensor.AddObservation(dirToTarget); // Riktning mot parkering

    }


    private void FixedUpdate()
    {
        RequestDecision();


        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        if(Input.GetKey(KeyCode.Space))
        {
            //ResetCar();
        }

    }


    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        currentbreakForce = isBreaking ? breakForce : 0f;

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
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
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
