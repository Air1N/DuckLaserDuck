using UnityEngine;
using UnityEngine.UI;

public class FPSScript : MonoBehaviour
{
        public Text fpsText;
        public float deltaTime;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
                deltaTime += (Time.deltaTime / (Time.timeScale + 0.0000001f) - deltaTime) * 0.1f;
                float fps = 1.0f / deltaTime;
                fpsText.text = Mathf.Ceil(fps).ToString();
        }
}
