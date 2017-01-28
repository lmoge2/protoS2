﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState
{
    NEUTRAL, HOLD, ACTIVE, FIXED
}

public class Block : MonoBehaviour
{
    [Header("Block's parameters")]
    public int numberOfSignalToUnfix = 3;
    public float timeBeforeBackToNeutral = 5.0f;

    [Header("Block's colors")]
    public Material neutralMaterial;
    public Material activeMaterial;
    public Material holdMaterial;
    public Material fixedMaterial;

    [HideInInspector]
    public BlockState state = BlockState.NEUTRAL;

    private Rigidbody rigid;
    private Renderer renderer;
    private int signalsLeft;
    private bool isTimerRunning = false;

    private void Start()
    {   
        rigid = GetComponent<Rigidbody>();
        renderer = GetComponent<Renderer>();
        signalsLeft = numberOfSignalToUnfix;
        rigid.Sleep();
        renderer.material = neutralMaterial;
    }

    private void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        switch(state)
        {
            case BlockState.NEUTRAL:
                if (rigid.IsSleeping() == false)
                {
                    SwitchState(BlockState.ACTIVE);
                }
                break;

            case BlockState.ACTIVE:
                if (rigid.IsSleeping() && !isTimerRunning)
                {
                    StartCoroutine("FadeToGrey");
                }

                if (rigid.IsSleeping() == false && isTimerRunning)
                {
                    StopAllCoroutines();
                    renderer.material = activeMaterial;
                }
                break;

            case BlockState.FIXED:
                if (signalsLeft <= 0)
                {
                    SwitchState(BlockState.NEUTRAL);
                }
                break;

        }
    }

    private void EnterState()
    {
        StopAllCoroutines();
        isTimerRunning = false;
        switch (state)
        {
            case BlockState.NEUTRAL:
                renderer.material = neutralMaterial;
                rigid.isKinematic = false;
                break;

            case BlockState.ACTIVE:
                renderer.material = activeMaterial;
                rigid.isKinematic = false;
                break;

            case BlockState.FIXED:
                renderer.material = fixedMaterial;
                rigid.isKinematic = true;
                break;

            case BlockState.HOLD:
                renderer.material = holdMaterial;
                rigid.isKinematic = true;
                break;
        }
    }

    private void ExitState()
    {

    }

    public void SwitchState(BlockState newState)
    {
        ExitState();
        state = newState;
        EnterState();
    }

    private IEnumerator FadeToGrey()
    {
        isTimerRunning = true;
        Color baseColor = renderer.material.color;
        Color targetColor = neutralMaterial.color;
        float value = 0.0f;
        while (value < timeBeforeBackToNeutral)
        {
            renderer.material.color = Color.Lerp(baseColor, targetColor, value/timeBeforeBackToNeutral);
            value += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        SwitchState(BlockState.NEUTRAL);
    }

    private void OnSignal()
    {
        if (state == BlockState.ACTIVE)
        {
            SwitchState(BlockState.FIXED);
        }
        else if (state == BlockState.FIXED)
        {
            signalsLeft--;
        }
    }

}
