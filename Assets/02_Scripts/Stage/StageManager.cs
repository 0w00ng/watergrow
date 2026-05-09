using UnityEngine;

namespace WaterGrow.Stage
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private string currentStageId = "STAGE_1_01";

        public string CurrentStageId => currentStageId;

        public void MoveNextStage()
        {
            Debug.Log("Next stage flow will be implemented in MVP Alpha 0.3.");
        }
    }
}

