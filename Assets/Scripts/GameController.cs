using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool isPausa;
    [SerializeField] private GameObject[] UIInterface;
    [SerializeField] private GameObject SettingPanel;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private Slider cameraViewSlider;
    private void Start()
    {
        cameraViewSlider.value = Camera.main.farClipPlane;
        foreach (GameObject obj in UIInterface)
        
        obj.SetActive(false);
}
       UIInterface[0].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
