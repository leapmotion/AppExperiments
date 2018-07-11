using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParticleSim {

  public class FluidBehaviour : MonoBehaviour {
    
    public ComputeShader computeShader;

    private void Start() {
      spawnParticles();
    }

    private const int NUM_PARTICLES = 16000;

    private void spawnParticles() {
      
    }

  }

}
