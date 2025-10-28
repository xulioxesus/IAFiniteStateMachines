using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Clase Singleton que xestiona o entorno do xogo.
// Mantén unha lista ordenada de todos os checkpoints na escena.
public class GameEnvironment {

    // Instancia única do Singleton
    private static GameEnvironment instance;
    
    // Lista de GameObjects que representan os checkpoints do xogo
    private List<GameObject> checkpoints = new List<GameObject>();

    // Propiedade pública de só lectura para acceder á lista de checkpoints
    public List<GameObject> Checkpoints { get { return checkpoints; } }

    //=========================================================================
    // Propiedade Singleton que proporciona acceso global á instancia única de GameEnvironment.
    // Ao acceder por primeira vez, inicializa a instancia, busca todos os GameObjects co tag "Checkpoint"
    // e ordénaos alfabeticamente por nome.
    //=========================================================================
    public static GameEnvironment Singleton
    {

        get
        {

            // Lazy initialization: crea a instancia só cando se necesita
            if (instance == null)
            {

                instance = new GameEnvironment();
                // Busca todos os GameObjects co tag "Checkpoint" na escena
                instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
                // Ordena os checkpoints alfabeticamente polo seu nome
                instance.checkpoints = instance.checkpoints.OrderBy(waypoint => waypoint.name).ToList();
            }

            return instance;
        }
    }
}