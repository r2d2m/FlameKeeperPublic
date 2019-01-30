﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : MonoBehaviour
{
    public float activateAnimationSpeed;

    [Header("Connected Triggers")]
    public List<ActivatableObject> connectedTriggers = new List<ActivatableObject>();

    [Header("Emissions")]
    public GameObject emitter;
    public float deactivatedEmitterIntensity = -3.0f;
    public float activatedEmitterIntensity = 3.0f;
    public List<MeshRenderer> renderersInEmission = new List<MeshRenderer>();

    [Header("Lights")]
    public Light pointLight;
    public float deactivatedLightRange;
    public float activatedLightRange;
    public float lightIncrement;

    [Header("Particles")]
    public ParticleSystem particles;
    public float deactivatedParticleEmissionRate;
    public float activatedParticleEmissionRate;
    public Color deactivatedParticleColor;
    public Color activatedParticleColor;

    [Header("Wire")]
    public GameObject wire;
    public Color wireDeactivatedColor;
    public Color wireActivatedColor;

    [Header("Testing, should delete later")]
    public PlayerController player;
    public float activateDistance = 2.0f;

    private bool activated = false;
    private MeshRenderer wireRenderer;
    private MeshRenderer emitterRenderer;

    public int maxLevel;
    private int currLevel;
    private float currLightRange; //only a vague indicator of how much fire is currently used, we should change this

    private void Start()
    {
        wireRenderer = wire.GetComponent<MeshRenderer>();
        emitterRenderer = emitter.GetComponent<MeshRenderer>();
        currLevel = 0;
        currLightRange = deactivatedLightRange;

        if (activated)
        {
            wireRenderer.material.color = wireActivatedColor;
        }
        else
        {
            wireRenderer.material.color = wireDeactivatedColor;
        }
    }

    private void Update()
    {
        float playerDistance = Vector3.Distance(emitter.transform.position, player.transform.position); // TODO: Should have a level controller or whatever where we get the player reference
        if (Input.GetButtonDown(StringConstants.Input.ActivateButton) && playerDistance < activateDistance)
        {
            if (currLevel < maxLevel && player.RequestLanternUse())
            {
                currLevel++;
                ActivatePedestal();
                currLightRange = activatedLightRange + (lightIncrement * currLevel);
            }
        }
        if (Input.GetButtonDown(StringConstants.Input.DeactivateButton) && playerDistance < activateDistance)
        {
            if (activated && player.RequestLanternAddition())
            {
                currLevel--;
                if (currLevel <= 0)
                {
                    DeactivatePedestal();
                    currLevel = 0;
                }
                else
                {
                    ActivatePedestal();
                }
            }
            currLightRange = activatedLightRange + (lightIncrement * currLevel);
        }

        ParticleSystem.MainModule main = particles.main;
        ParticleSystem.EmissionModule particleEmission = particles.emission;
        if (activated)
        {
            wireRenderer.material.color = Color.Lerp(wireRenderer.material.color, wireActivatedColor, Time.deltaTime * activateAnimationSpeed);
            pointLight.range = Mathf.Lerp(pointLight.range, currLightRange, Time.deltaTime * activateAnimationSpeed);
            emitterRenderer.material.SetVector("_EmissionColor", emitterRenderer.material.color * activatedEmitterIntensity * activatedEmitterIntensity * Mathf.Sign(activatedEmitterIntensity));
            main.startColor = activatedParticleColor;
            particleEmission.rateOverTime = activatedParticleEmissionRate;
        }
        else
        {
            wireRenderer.material.color = Color.Lerp(wireRenderer.material.color, wireDeactivatedColor, Time.deltaTime * activateAnimationSpeed);
            pointLight.range = Mathf.Lerp(pointLight.range, deactivatedLightRange, Time.deltaTime * activateAnimationSpeed);
            emitterRenderer.material.SetVector("_EmissionColor", emitterRenderer.material.color * deactivatedEmitterIntensity * deactivatedEmitterIntensity * Mathf.Sign(deactivatedEmitterIntensity));
            main.startColor = deactivatedParticleColor;
            particleEmission.rateOverTime = deactivatedParticleEmissionRate;
        }

        // If we are updating an emission intensity, we have to tell all materials that it touches
        // to re compute their global illumination. There is probably a better method to do this but
        // for now I think this is ok.
        foreach (MeshRenderer rend in renderersInEmission)
        {
            rend.UpdateGIMaterials();
        }
    }

    public bool IsActive()
    {
        return activated;
    }

    public void ActivatePedestal()
    {
        activated = true;
        foreach (ActivatableObject obj in connectedTriggers)
        {
            obj.OnPedestalActivate(this);
        }
    }

    public void DeactivatePedestal()
    {
        foreach (ActivatableObject obj in connectedTriggers)
        {
            obj.OnPedestalDeactivate(this);
        }

        activated = false;
    }

    public int GetCurrLevel()
    {
        return currLevel;
    }
}