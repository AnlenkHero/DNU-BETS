using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingController : MonoBehaviour
{
    [SerializeField] Volume volume;
    private FilmGrain _filmGrain;
    private ChromaticAberration _chromaticAberration;

    public void ToggleEffectsOnInternetConnection(bool connectionState)
    {
        if(volume.profile.TryGet<FilmGrain>(out _filmGrain) && volume.profile.TryGet<ChromaticAberration>(out _chromaticAberration))
        {
            _filmGrain.active = connectionState;
            _chromaticAberration.active = connectionState;
        }
    }
    
}
