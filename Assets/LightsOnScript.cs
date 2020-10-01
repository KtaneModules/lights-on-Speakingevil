using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LightsOnScript : MonoBehaviour {

    public KMAudio Audio;
    public KMNeedyModule needy;
    public KMSelectable[] tiles;
    public Renderer[] tilerends;
    public Material[] mats;

    private bool active;
    private bool[] target = new bool[24];
    private bool[] state = new bool[24];
    private readonly int[][] adj = new int[24][] { new int[3] { 0, 1, 6 }, new int[3] { 0, 1, 2 }, new int[4] { 1, 2, 3, 8 }, new int[3] { 2, 3, 4 }, new int[3] { 3, 4, 10 }, new int[3] { 5, 6, 12 }, new int[4] { 0, 5, 6, 7 }, new int[4] { 6, 7, 8, 14 }, new int[4] { 2, 7, 8, 9 }, new int[4] { 8, 9, 10, 16 }, new int[4] { 4, 9, 10, 11 }, new int[3] { 10, 11, 18 }, new int[3] { 5, 12, 13 }, new int[4] { 12, 13, 14, 19 }, new int[4] { 7, 13, 14, 15 }, new int[4] { 14, 15, 16, 21 }, new int[4] { 9, 15, 16, 17 }, new int[4] { 16, 17, 18, 23 }, new int[3] { 11, 17, 18 }, new int[3] { 13, 19, 20 }, new int[3] { 19, 20, 21 }, new int[4] { 15, 20, 21, 22 }, new int[3] { 21, 22, 23 }, new int[3] { 17, 22, 23 } }; 

	private void Awake()
    {
        needy.OnNeedyActivation = Activate;
        needy.OnTimerExpired = Outtatime;
        foreach (Renderer t in tilerends)
            t.material = mats[0];
        foreach(KMSelectable s in tiles)
        {
            int k = Array.IndexOf(tiles, s);
            s.OnInteract += delegate () { Select(k); return false; };
        }
	}

    private void Activate()
    {
        active = true;
        target = new bool[24];
        state = new bool[24];
        for(int i = 0; i < 12; i++)
        {
            int k = Random.Range(0, 24);
            for (int j = 0; j < adj[k].Length; j++)
                target[adj[k][j]] ^= true;
        }
        for (int i = 0; i < 24; i++)
            tilerends[i].material = mats[target[i] ? 2 : 1];
    }

    private void Select(int k)
    {
        if (active)
        {
            tiles[k].AddInteractionPunch(0.2f);
            Audio.PlaySoundAtTransform("Press" + (state[k] ? "0" : "1"), tiles[k].transform);
            for (int j = 0; j < adj[k].Length; j++)
            {
                int r = adj[k][j];
                state[r] ^= true;
                tilerends[r].material = mats[target[r] ? (state[r] ? 4 : 2) : (state[r] ? 3 : 1)];
            }
        }
    }

    private void Outtatime()
    {
        active = false;
        if (state.SequenceEqual(target))
            needy.HandlePass();
        else
            needy.HandleStrike();
        foreach (Renderer t in tilerends)
            t.material = mats[0];
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <a-d><1-7> [Selects tiles located at the specified coordinates]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (!active)
        {
            yield return "sendtochaterror Tiles are currently inactive";
            yield break;
        }
        string[] commands = command.ToLowerInvariant().Split(' ');
        List<int> coords = new List<int> { };
        for(int i = 0; i < commands.Length; i++)
        {
            int coord = 0;
            if (commands[i].Length != 2)
            {
                yield return "sendtochaterror Invalid coordinate: " + commands[i];
                yield break;
            }
            else
            {
                switch (commands[i][0])
                {
                    case 'a':
                        coord = "12345".IndexOf(commands[i][1]);
                        if (coord == -1)
                        {
                            yield return "sendtochaterror Invalid coordinate: " + commands[i];
                            yield break;
                        }
                        else
                            coords.Add(coord);
                        break;
                    case 'b':
                        coord = "1234567".IndexOf(commands[i][1]);
                        if (coord == -1)
                        {
                            yield return "sendtochaterror Invalid coordinate: " + commands[i];
                            yield break;
                        }
                        else
                        {
                            coord += 5;
                            coords.Add(coord);
                        }
                        break;
                    case 'c':
                        coord = "1234567".IndexOf(commands[i][1]);
                        if (coord == -1)
                        {
                            yield return "sendtochaterror Invalid coordinate: " + commands[i];
                            yield break;
                        }
                        else
                        {
                            coord += 12;
                            coords.Add(coord);
                        }
                        break;
                    case 'd':
                        coord = "12345".IndexOf(commands[i][1]);
                        if (coord == -1)
                        {
                            yield return "sendtochaterror Invalid coordinate: " + commands[i];
                            yield break;
                        }
                        else
                        {
                            coord += 19;
                            coords.Add(coord);
                        }
                        break;
                    default:
                        yield return "sendtochaterror Invalid coordinate: " + commands[i];
                        yield break;
                }
            }
        }
        for(int i = 0; i < coords.Count; i++)
        {
            yield return null;
            tiles[coords[i]].OnInteract();
        }
    }
}
