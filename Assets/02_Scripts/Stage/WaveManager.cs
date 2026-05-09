using UnityEngine;

namespace WaterGrow.Stage
{
    public class WaveManager : MonoBehaviour
    {
        public void StartWaveGroup(string waveGroupId)
        {
            Debug.Log($"Start wave group: {waveGroupId}");
        }
    }
}

