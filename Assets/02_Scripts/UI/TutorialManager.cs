using UnityEngine;

namespace WaterGrow.UI
{
    public class TutorialManager : MonoBehaviour
    {
        public void CompleteTutorial(string tutorialId)
        {
            Debug.Log($"Tutorial completed: {tutorialId}");
        }
    }
}

