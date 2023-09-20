using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class BlinkerScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMNeedyModule Needy;

	public KMSelectable[] gridButtons;
	public Animator[] animators;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool needyDeactivated = true;

	private List<int> flashingPos = new List<int>();
	private readonly Coroutine[] flashes = new Coroutine[9];
	private int[] selectedPos;
	private int stage = 1;

	private bool[] pressed = new bool[9];

	void Awake()
    {

		moduleId = moduleIdCounter++;

		foreach (KMSelectable button in gridButtons)
			button.OnInteract += delegate () { buttonPress(button); return false; };

		Needy.OnNeedyActivation += needyActivation;
		Needy.OnNeedyDeactivation += needyDeactivation;
		Needy.OnTimerExpired += needyTimerExpired;

    }

	void Start()
	{
		for (int i = 0; i < 9; i++)
			flashingPos.Add(i);

		flashingPos.Shuffle();
	}

	void needyActivation()
	{
		needyDeactivated = false;
		selectedPos = flashingPos.Take(stage).ToArray();
	}

	void needyDeactivation()
	{
		flashingPos.Shuffle();

		needyDeactivated = true;
    }

	void needyTimerExpired()
	{
		Log($"[Blinker #{moduleId}] You haven't pressed all 9 buttons in time. Strike!");
		Needy.HandleStrike();
	}

	void buttonPress(KMSelectable button)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        var ix = Array.IndexOf(gridButtons, button);

		if (selectedPos.Last() != ix || pressed[ix] || needyDeactivated)
			return;
		else
		{
			stage++;
			pressed[ix] = true;

			if (pressed.All(x => x))
			{
				for (int i = 0; i < 9; i++)
					pressed[i] = false;
				Needy.HandlePass();
			}
		}

	}


	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}





