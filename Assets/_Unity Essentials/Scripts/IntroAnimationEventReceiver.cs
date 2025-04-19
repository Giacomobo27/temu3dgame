using UnityEngine;

// This script's only purpose is to exist on the intro character
// to catch Animation Events like OnLand and OnFootstep, preventing
// "no receiver" errors when those methods don't exist elsewhere
// in the Main Menu scene.
public class IntroAnimationEventReceiver : MonoBehaviour
{
    // Animation Event receiver for landing sounds/effects.
    // Does nothing in the intro scene.
    public void OnLand(AnimationEvent animationEvent)
    {
        // Intentionally empty. You could add a Debug.Log("Intro OnLand") here if you wanted to test.
    }

    // Animation Event receiver for footstep sounds/effects.
    // Does nothing in the intro scene.
    public void OnFootstep(AnimationEvent animationEvent)
    {
        // Intentionally empty. You could add a Debug.Log("Intro OnFootstep") here if you wanted to test.
    }

    // Add other empty public void methods here if your animations
    // trigger other events that cause "no receiver" errors.
}