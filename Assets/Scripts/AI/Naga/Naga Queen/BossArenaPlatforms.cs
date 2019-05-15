using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArenaPlatforms : MonoBehaviour
{
    public Transform[] platformTpPossitions;
    public GameObject[] platformObjects;
    private Platform[] platforms;

    private Vector2Int[] platformOffsets = { new Vector2Int(0, -6),
                                              new Vector2Int(8, -2),
                                              new Vector2Int(1, -6),
                                              new Vector2Int(0, -10),
                                              new Vector2Int(0, -10),
                                              new Vector2Int(-1, -6),
                                              new Vector2Int(-8, -2)};

    private float stepTime = 1f;

    private void Start()
    {
        platforms = new Platform[platformObjects.Length];
        for (int i = 0; i < platformObjects.Length; i++)
        {
            int maxStepNumber;
            if (i == 0)
                maxStepNumber = 1;
            else
                maxStepNumber = platforms.Length - i + 1;
            platforms[i] = new Platform(Platform.State.Idle, i, maxStepNumber, platformObjects[i], platformObjects[i].transform.position, platformOffsets[i]);
        }
        this.enabled = false;
    }

    public void StartEvent()
    {
        this.enabled = true;
        platforms[0].state = Platform.State.Moving;
        platforms[0].UpdateTargetPosition(platforms[0].finalPos, stepTime);
        for (int i = 1; i < platforms.Length; i++)
            platforms[i].UpdateTargetPosition(platforms[i].startingPos + new Vector2(0, 2), stepTime);
    }

    private void Update()
    {
        foreach (Platform platform in platforms)
        {
            if (platform.state == Platform.State.Moving)
            {
                platform.SetPlatformPosition(Vector2.MoveTowards(platform.platformObject.transform.position, platform.targetPos, platform.speed * Time.deltaTime));
                float distToTarget = Vector2.Distance(platform.platformObject.transform.position, platform.targetPos);
                if (distToTarget < 0.1f)
                {
                    platform.platformObject.transform.position = platform.targetPos;
                    if (platform.index + 1 < platforms.Length && (platform.index == 0 || platform.currStepNumber == 2))
                        platforms[platform.index + 1].state = Platform.State.Moving;

                    platform.currStepNumber++;
                    if (platform.index == 0)
                        platform.state = Platform.State.Idle;
                    else
                    {
                        if (platform.currStepNumber > platform.maxStepNumber)
                            platform.state = Platform.State.Idle;
                        else if(platforms.Length - platform.currStepNumber + 1 >= 0 && platforms.Length - platform.currStepNumber + 1 < platforms.Length)
                        {
                            platform.UpdateTargetPosition(platforms[platforms.Length - platform.currStepNumber + 1].finalPos, stepTime);
                        }
                    }
                }
            }
        }
    }

    private class Platform
    {
        public enum State { Idle, Moving };
        public State state;

        public int index;

        public float speed;

        public int currStepNumber;
        public int maxStepNumber;

        public GameObject platformObject;
        public Vector2 startingPos;
        public Vector2 finalPos;
        public Vector2 targetPos { private set; get; }

        public Platform(State state, int index, int maxStepNumber, GameObject platformObject, Vector2 finalPos, Vector2Int finalPosOffset)
        {
            this.state = state;
            this.platformObject = platformObject;
            this.startingPos = new Vector2(finalPos.x + finalPosOffset.x, finalPos.y + finalPosOffset.y);
            this.finalPos = finalPos;
            this.index = index;
            this.maxStepNumber = maxStepNumber;

            platformObject.transform.position = startingPos;
        }
        public void UpdateTargetPosition(Vector2 position, float stepTime)
        {
            targetPos = position;
            float distance = Vector2.Distance(platformObject.transform.position, targetPos);
            speed = distance / stepTime;
        }

        public void SetPlatformPosition(Vector2 position)
        {
            Vector2 offset = position - (Vector2)platformObject.transform.position;
            platformObject.transform.position = position;
        }
    }
}
