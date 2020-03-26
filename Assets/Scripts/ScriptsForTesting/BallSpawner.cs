using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballToSpawn;
    public Material materialOfBall;
    public bool randomizeScale;
    public bool randomizeMovement;
    public float power;

    public void SpawnBall()
    {
        GameObject ball = Instantiate(ballToSpawn, transform.position, Quaternion.identity);
        if (randomizeScale)
            ball.transform.localScale = Vector3.one * Random.Range(0.1f, 1f);
        if (randomizeMovement && ball.GetComponent<Rigidbody>())
            ball.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)) * power, ForceMode.Impulse);
        ball.GetComponent<MeshRenderer>().material = materialOfBall;
        Destroy(ball, 1f);
    }

    public void SpawnBallWithOwnMaterial(Material material)
    {
        GameObject ball = Instantiate(ballToSpawn, transform.position, Quaternion.identity);
        if (randomizeScale)
            ball.transform.localScale = Vector3.one * Random.Range(0.1f, 1f);
        if (randomizeMovement && ball.GetComponent<Rigidbody>())
        {
            ball.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)) * power, ForceMode.Impulse);
        }
        ball.GetComponent<MeshRenderer>().material = material;
        Destroy(ball, 3f);
    }
}
