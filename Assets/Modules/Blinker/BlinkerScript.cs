using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

	private bool[] pressed = new bool[9], currentlyFlashing = new bool[9];

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

		displayingLights();
        Log($"[Blinker #{moduleId}] The buttons to press for this activation in order are: {flashingPos.Select(x => x + 1).Join(", ")}");
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
		stage = 1;
		flashingPos.Shuffle();

		foreach (var c in flashes)
		{
			if (c != null)
				StopCoroutine(c);
		}

		for (int i = 0; i < 9; i++)
		{
			if (currentlyFlashing[i])
				animators[i].SetTrigger("Unlit");

			pressed[i] = false;
			currentlyFlashing[i] = false;
		}
	}

	void buttonPress(KMSelectable button)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		button.AddInteractionPunch(0.4f);

        var ix = Array.IndexOf(gridButtons, button);

		if (pressed[ix] || needyDeactivated)
			return;
		else
		{
			if (selectedPos.Last() != ix)
				return;

			pressed[ix] = true;

			foreach (var c in flashes)
			{
				if (c != null)
					StopCoroutine(c);
			}

			if (pressed.All(x => x))
			{
                for (int i = 0; i < 9; i++)
				{
                    pressed[i] = false;

					if (currentlyFlashing[i])
                        animators[i].SetTrigger("Unlit");
                    currentlyFlashing[i] = false;
					
                }
				Needy.HandlePass();
				stage = 1;
				Log($"[Blinker #{moduleId}] You pressed all 9 buttons, and it is now deactivated.");
			}
			else
			{
                stage++;
                displayingLights();
                Log($"[Blinker #{moduleId}] {ix + 1} is pressed correctly.");
            }
		}

	}

	void displayingLights()
	{
        if (stage != 1)
            StartCoroutine(stopBlinking());
		else
		{
            selectedPos = flashingPos.Take(stage).ToArray();

            for (int i = 0; i < 9; i++)
            {

                if (selectedPos.Contains(i))
                {
                    flashes[i] = StartCoroutine(blinkingLight(i));
                }

            }
        }

	}

	IEnumerator stopBlinking()
	{
        selectedPos = flashingPos.Take(stage).ToArray();

		for (int i = 0; i < 9; i++)
			if (pressed[i])
			{
				if (!currentlyFlashing[i])
					continue;
				else
				{
                    animators[i].SetTrigger("Unlit");
					currentlyFlashing[i] = false;
                }
			}


		yield return new WaitForSeconds(1);

		for (int i = 0; i < 9; i++)
		{
			if (selectedPos.Contains(i))
				flashes[i] = StartCoroutine(blinkingLight(i));
		}
	}

	IEnumerator blinkingLight(int pos)
	{

		while (true)
		{
			currentlyFlashing[pos] = !currentlyFlashing[pos];
			animators[pos].SetTrigger(currentlyFlashing[pos] ? "Lit" : "Unlit");
			yield return new WaitForSeconds(currentlyFlashing[pos] ? 0.5f : 2);
        }
	}


	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press 123456789/TL TM TR ML MM MR BL BM BR presses the position you want to press.";
#pragma warning restore 414
	string[] validPhrases = { "TL", "TM", "TR", "ML", "MM", "MR", "BL", "BM", "BR" };
	int[] validNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };


	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        yield return null;

        if (needyDeactivated)
        {
            yield return "sendtochaterror The needy is not activated yet!";
            yield break;
        }


		if (split[0].EqualsIgnoreCase("PRESS"))
		{
			if (split.Length == 1)
			{
				yield return "returntochaterror Please specify either a position or a number!";
				yield break;
			}

			if (validPhrases.Contains(split[1]))
			{
				if (split[1].Length > 2)
					yield break;

				gridButtons[Array.IndexOf(validPhrases, split[1])].OnInteract();
				yield return new WaitForSeconds(0.1f);
				yield break;
			}
			else if (validNumbers.Contains(int.Parse(split[1])))
			{
				if (split[1].Length > 1)
					yield break;

				gridButtons[int.Parse(split[1]) - 1].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}

		}

		
    }

	void TwitchHandleForcedSolve()
    {
		StartCoroutine(needyAutosolve());
    }

	IEnumerator needyAutosolve()
	{
		while (true)
		{
			while (needyDeactivated)
				yield return null;

			foreach (var num in flashingPos)
			{
				gridButtons[num].OnInteract();
				yield return new WaitForSeconds(1);
			}
		}
	}

}





