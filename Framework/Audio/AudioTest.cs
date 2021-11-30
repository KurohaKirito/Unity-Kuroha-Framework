using UnityEngine;
using UnityEngine.UI;

namespace Kuroha.Framework.Audio
{
    public class AudioTest : MonoBehaviour
    {
        public Button playButton;
        public Button stopButton;

        private void Start()
        {
            playButton.onClick.AddListener(()=>
            {
                AudioPlayManager.Instance.Play("Click", true);
            });
 
            stopButton.onClick.AddListener(() =>
            {
                AudioPlayManager.Instance.Stop("Click");
            });
        }
    }
}