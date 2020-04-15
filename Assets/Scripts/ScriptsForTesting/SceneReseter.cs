using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneReseter : MonoBehaviour
{
    public void ResetScene()
    {
        SceneManager.LoadScene("VRUIDemo");
    }
}
