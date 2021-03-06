﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProfileMenu : MonoBehaviour {
    bool hasFinishedGame1;
    bool hasFinishedGame2;
    bool hasFinishedGame3;

    public GameObject firstStar;
    public GameObject firstGoldenStar;
    public GameObject secondStar;
    public GameObject secondGoldenStar;
    public GameObject thirdStar;
    public GameObject thirdGoldenStar;
    public Dropdown profileSelector;
    public Text text;

    Profile[] profiles;

    ProfileManager profileManager;

    // Use this for initialization
    void Start () {
        profileSelector.ClearOptions();
        profileSelector.options.Add(new Dropdown.OptionData() { text = "Choisis ton profil" });

        profileManager = GameObject.Find("Navigator").GetComponent<ProfileManager>();

        profiles = profileManager.getProfiles();
        if (profiles == null)
            return;
        else
        {
            foreach (Profile p in profiles)
            {
                profileSelector.options.Add(new Dropdown.OptionData() { text = p.getFirstName() + " " + p.getLastName()[0] });
            }
        }
	}
	

    public void loadSelectedProfile()
    {
        profileManager.setCurrentProfile(profileSelector.value-1);
    }

    public void loadProfileCreator()
    {
        SceneManager.LoadScene("ProfileCreator", LoadSceneMode.Additive);
    }
	// Update is called once per frame
	void Update () {
        if (!profileManager.thereIsAProfile())
            profileSelector.value = 0;
        else if (profileManager.thereIsAProfile())
            profileSelector.value = profileManager.getCurrentProfileIndex() + 1;
        //Update current profile informations
        if (profileManager.getCurrentProfileIndex() != 0)
        {
            hasFinishedGame1 = profiles[profileManager.getCurrentProfileIndex()].getIsGameFinished(1);
            hasFinishedGame2 = profiles[profileManager.getCurrentProfileIndex()].getIsGameFinished(2);
            hasFinishedGame3 = profiles[profileManager.getCurrentProfileIndex()].getIsGameFinished(3);
        }
        if (hasFinishedGame1)
        {
            firstStar.SetActive(false);
            firstGoldenStar.SetActive(true);
        }
        if (hasFinishedGame2)
        {
            secondStar.SetActive(false);
            secondGoldenStar.SetActive(true);
        }
        if (hasFinishedGame3)
        {
            thirdStar.SetActive(false);
            thirdGoldenStar.SetActive(true);
        }



        text.text = profileSelector.options[profileSelector.value].text;
    }
}
