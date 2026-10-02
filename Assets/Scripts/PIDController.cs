// from https://github.com/ProdiG66/PID_Controller/blob/main/Assets/_Scripts/PID_Controller.cs
using System;
using UnityEngine;

[Serializable]
public class PIDParameters
{
    public enum DerivativeMeasurement
    {
        Velocity,
        ErrorRateOfChange
    }

    public float proportionalGain;
    public float integralGain;
    public float derivativeGain;

    public float outputMin = -1;
    public float outputMax = 1;
    public float integralSaturation;
    public DerivativeMeasurement derivativeMeasurement;
}

public class PIDController
{
    public PIDParameters pidParams;
    public float valueLast;
    public float errorLast;
    public float integrationStored;
    public float velocity;
    public bool derivativeInitialized;

    public PIDController(PIDParameters pidParams)
    {
        this.pidParams = pidParams;
    }

    public void Reset()
    {
        derivativeInitialized = false;
    }

    public float Update(float dt, float currentValue, float targetValue)
    {
        if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

        float error = targetValue - currentValue;

        float P = pidParams.proportionalGain * error;

        integrationStored = Mathf.Clamp(integrationStored + error * dt, -pidParams.integralSaturation, pidParams.integralSaturation);
        float I = pidParams.integralGain * integrationStored;

        float errorRateOfChange = (error - errorLast) / dt;
        errorLast = error;

        float valueRateOfChange = (currentValue - valueLast) / dt;
        valueLast = currentValue;
        velocity = valueRateOfChange;

        float deriveMeasure = 0;

        if (derivativeInitialized)
        {
            if (pidParams.derivativeMeasurement == PIDParameters.DerivativeMeasurement.Velocity)
                deriveMeasure = -valueRateOfChange;
            else
                deriveMeasure = errorRateOfChange;
        }
        else
        {
            derivativeInitialized = true;
        }

        float D = pidParams.derivativeGain * deriveMeasure;

        float result = P + I + D;

        return Mathf.Clamp(result, pidParams.outputMin, pidParams.outputMax);
    }

    private float AngleDifference(float a, float b)
    {
        return (a - b + 540) % 360 - 180;
    }

    public float UpdateAngle(float dt, float currentAngle, float targetAngle)
    {
        if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));
        float error = AngleDifference(targetAngle, currentAngle);

        float P = pidParams.proportionalGain * error;

        integrationStored = Mathf.Clamp(integrationStored + error * dt, -pidParams.integralSaturation, pidParams.integralSaturation);
        float I = pidParams.integralGain * integrationStored;

        float errorRateOfChange = AngleDifference(error, errorLast) / dt;
        errorLast = error;

        float valueRateOfChange = AngleDifference(currentAngle, valueLast) / dt;
        valueLast = currentAngle;
        velocity = valueRateOfChange;

        float deriveMeasure = 0;

        if (derivativeInitialized)
        {
            if (pidParams.derivativeMeasurement == PIDParameters.DerivativeMeasurement.Velocity)
                deriveMeasure = -valueRateOfChange;
            else
                deriveMeasure = errorRateOfChange;
        }
        else
        {
            derivativeInitialized = true;
        }

        float D = pidParams.derivativeGain * deriveMeasure;

        float result = P + I + D;

        return Mathf.Clamp(result, pidParams.outputMin, pidParams.outputMax);
    }
}