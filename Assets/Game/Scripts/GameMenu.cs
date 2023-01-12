using UnityEngine;

namespace Game.Scripts
{
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private float volume = 0.1f;
    
        void Start()
        {
            AudioListener.volume = volume;
        }
    }
}
