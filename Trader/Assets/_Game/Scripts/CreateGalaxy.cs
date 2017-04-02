﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeControl;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class CreateGalaxy : MonoBehaviour {

    public LayerMask mapMask;
    public LayerMask solarMask;
    public Gradient sunSizeColor;
    public Gradient planetSizeColor;
    public Vector2 mapField;
    public int starCount;

    public Text infoText;

    internal static float G = .01f;

    private GameObject selectedObj;
    private Camera cam;
    internal SolarController solar;

    internal SolarModel[] stars;
    internal SolarController[] starControllers;

    internal static CreateGalaxy instance;

    private void Awake()
    {
        instance = this;
        CreateStars(starCount);
        LoadStars();
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (hit)
                {
                    selectedObj = hit.transform.gameObject;
                    if (selectedObj.tag == "solar")
                    {
                        solar = selectedObj.GetComponent<SolarController>();
                        SolarView();
                        
                        
                    }

                }
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (cam.cullingMask == solarMask)
                GalaxyView();
            else
                SolarView();         
        }
        if (infoText != null)
            infoText.text = "Timescale: " + Time.timeScale + "\n";
    }

    public void GalaxyView()
    {
        cam.cullingMask = mapMask;
        transform.position = new Vector3(solar.transform.position.x, solar.transform.position.y, -10);
        cam.orthographicSize = 100;
    }

    public void SolarView()
    {

        cam.cullingMask = solarMask;
        transform.position = new Vector3(0, 0, -10);
        cam.orthographicSize = 1000;
    }

    public void CreateStars(int count)
    {
        stars = new SolarModel[count];
        
        for (int i = 0; i < count; i++)
        {
            Vector2 position = new Vector2(UnityEngine.Random.Range(-mapField.x * .5f, mapField.x * .5f), UnityEngine.Random.Range(-mapField.y * .5f, mapField.y * .5f));
            float sunMass = UnityEngine.Random.Range(1f, 1000);
            int numPlanets = UnityEngine.Random.Range(0, (int) Mathf.Sqrt(sunMass / 10));
            stars[i] = new SolarModel("Solar " + i + 1, i, position, sunMass, G, numPlanets, sunSizeColor, sunMass *.001f);
            for (int c = 0; c < i; c++)
            {
                float maxDist = Mathf.Pow(sunMass + stars[c].sun.mass, .5f);
                float actualDist = Vector3.Distance(position, stars[c].position);
                if ( actualDist < maxDist)
                {
                    stars[i].nearStars.Add(stars[c]);
                    stars[c].nearStars.Add(stars[i]);
                }
            }
            
        }
        float connectedness = 0;
        for (int i = 0; i < count; i++)
        {
            float closestStarDist = 1000000000000000;
            int closestStarIndex = 0;
            for (int c = 0; c < count; c++)
            {
                if (c != i)
                {
                    float maxDist = Mathf.Pow(stars[i].sun.mass + stars[c].sun.mass, .5f);
                    float actualDist = Vector3.Distance(stars[i].position, stars[c].position);
                    if (actualDist < closestStarDist)
                    {
                        closestStarDist = actualDist;
                        closestStarIndex = c;
                    }
                }
                
            }
            if (stars[i].nearStars.Count == 0)
            {
                stars[i].nearStars.Add(stars[closestStarIndex]);
                stars[closestStarIndex].nearStars.Add(stars[i]);
                print("added disconnected star");
            }
            connectedness += stars[i].nearStars.Count;
        }
        connectedness /= count;
        print("Connectedness average: " + connectedness);
    }
    public void LoadStars()
    {
        starControllers = new SolarController[stars.Length];
        foreach (SolarModel star in stars)
        {
            starControllers[star.index] = Controller.Instantiate<SolarController>("solar", star);
            starControllers[star.index].name = star.name;
        }
    }
	
}
