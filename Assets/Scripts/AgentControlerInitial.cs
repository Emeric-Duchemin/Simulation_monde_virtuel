using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentControlerInitial : MonoBehaviour
{
    // Start is called before the first frame update
    private VoronoiDemo Vorono;
    public int number_inhabitant = 1;
    private int[,] choose_house;
    public GameObject PetitBonhomme;
    private DayNightCycle Daying;
    void Start()
    {
        Vorono = GetComponent<VoronoiDemo>();
        Daying = GetComponent<DayNightCycle>();
    }

    public void initializeAgent() {
        Vorono = GetComponent<VoronoiDemo>();

        int house_nb = Vorono.list_house.Count;
        choose_house = new int[house_nb,2];
        int current_choose_house = house_nb;
        for (int j = 0; j < house_nb; j++) {
            choose_house[j,0] = j;
            choose_house[j,1] = 4;
        }
        for (int i = 0; i < number_inhabitant; i++) {
            int choice = Random.Range(0, current_choose_house);
            int choice_sky = Random.Range(0, Vorono.list_skys.Count);
            int[] sky_house = { choice, choice_sky };
            Vorono.list_position_inhabitant.Add(sky_house);
            Vector3 pos = Vorono.list_house[choice].transform.position;
            pos = new Vector3(pos.x, 0.03039218f, pos.z);
            GameObject PetitBo = Instantiate(PetitBonhomme, pos, Quaternion.identity);
            PetitBo.GetComponent<NavMeshAgent>().Warp(pos);
            //PetitBo.transform.position = new Vector3(PetitBo.transform.position.x, 0.005f, PetitBo.transform.position.z);
            BonhommeBehavior BonhommeScript =  PetitBo.GetComponent<BonhommeBehavior>();
            BonhommeScript.home_sweet_home = Vorono.list_house[choice];
            BonhommeScript.my_taff = Vorono.list_skys[choice_sky];
            BonhommeScript.home_sweet_home2 = Vorono.list_house2[choice];
            BonhommeScript.my_taff2 = Vorono.list_skys2[choice_sky];
            BonhommeScript.Vorono = Vorono;
            BonhommeScript.init_position = pos;
            Vorono.list_inhabitant.Add(PetitBo);
            if (choose_house[choice, 1] == 1) {
                current_choose_house--;
                choose_house[choice, 0] = choose_house[current_choose_house,0];
                choose_house[choice, 1] = choose_house[current_choose_house, 0];
            }
            else
            {
                choose_house[choice, 1]--;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (0.24 <= Daying.currentTimeOfDay && Daying.currentTimeOfDay <= 0.26) {
            Vorono.to_taff = true;
        }
        if (0.79 <= Daying.currentTimeOfDay  && Daying.currentTimeOfDay <= 0.80)
        {
            Vorono.to_taff = false;
        }
    }
}
