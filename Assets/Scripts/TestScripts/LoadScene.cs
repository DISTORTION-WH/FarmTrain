using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public string sceneToLoad;

    public void Scene()
    {
       
        Debug.Log("��������� �����: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }

}