using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using UnityEngine.AI;

//get local position of the road in order to place buildings. 
public class VoronoiDemo : MonoBehaviour
{

    public Material land;
    public GameObject plan; 
    public GameObject road;
    public int plane_factor = 10;
    public const int NPOINTS = 300;
    public const int WIDTH = 200;
    public const int HEIGHT = 200;
	public float freqx = 0.02f, freqy = 0.018f, offsetx = 0.43f, offsety = 0.22f;
    public float home_size = 0.05f;
    public float skyscr_size = 0.05f;
    public float ratio_city = 5f;
    public GameObject skyscraper;
    public GameObject house;
    private int[][] edge_table;
    private List<int>[] center_table;
    private int[] isCity = new int[NPOINTS];
    public List<GameObject> list_skys = new List<GameObject>();
    public List<GameObject> list_house = new List<GameObject>();
    public List<GameObject> list_skys2 = new List<GameObject>();
    public List<GameObject> list_house2 = new List<GameObject>();
    public List<GameObject> list_inhabitant = new List<GameObject>();
    public List<GameObject> list_restau = new List<GameObject>();
    public List<GameObject> list_restau2 = new List<GameObject>();
    public List<int[]> list_position_inhabitant = new List<int[]>();
    private AgentControlerInitial ag;
    public GameObject plane;
    public bool to_taff = true; 
    public List<GameObject> park_list = new List<GameObject>();
    public GameObject highway;
    public GameObject restau;
    int nb_rest;
    private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private Texture2D tx;

	private float [,] createMap() 
    {
        float [,] map = new float[WIDTH, HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                map[i, j] = Mathf.PerlinNoise(freqx * i + offsetx, freqy * j + offsety);
        return map;
    }
    float sum_col(int i, float[,] map) {
        float sum = 0;
        for (int j = 0; j < HEIGHT; j++) {
            sum = sum + map[i, j];
        }
        return sum;
    }

    float sum_ligne(int j, float[,] map) {
        float sum = 0;
        for (int i = 0; i < WIDTH; i++) {
            sum = sum + map[i, j];
        }
        return sum;
    }
    
    int[] getRandomValue(float[,] map) {
        float cumul_col = 0;
        float cumul_ligne = 0;
        float rando = Random.Range(0f,0.99f);
        float rando2 = Random.Range(0f, 0.99f);
        int[] col = { 0, 0 };
        int i = 0;
        int j = 0;
        while (cumul_col < rando || cumul_ligne < rando2) {
            cumul_col = cumul_col + sum_col(i,map);
            cumul_ligne = cumul_ligne + sum_ligne(j,map);
            if (cumul_col >= rando && col[1]== 0) {
                col[1] = j;
            }
            if (cumul_ligne >= rando2 && col[0] == 0)
            {
                col[0] = i;
            }
            i += 1;
            j += 1;
        }
        return col;
    }

    private void update_table()
    {

        this.center_table = new List<int>[NPOINTS];
        this.edge_table = new int[m_edges.Count][];
        for (int i = 0; i < m_edges.Count; i++) {
            int nearest1 = -1;
            float value1 = 100000;
            int nearest2 = -1;
            float value2 = 100000;
            LineSegment seg = m_edges[i];
            Vector2 left = (Vector2)seg.p0;
            Vector2 right = (Vector2)seg.p1;
            Vector2 segment = (right + left)/2;
            for (int j = 0; j < m_points.Count; j++) {
                Vector2 point = m_points[j];
                if ((segment - point).magnitude < value1)
                {
                    nearest2 = nearest1;
                    nearest1 = j;
                    value2 = value1;
                    value1 = (segment - point).magnitude;
                }
                else if ((segment - point).magnitude < value2) {
                    value2 = (segment - point).magnitude;
                    nearest2 = j;
                }
            }
            if (center_table[nearest1] == null) {
                center_table[nearest1] = new List<int>();
            }
            if (center_table[nearest2] == null)
            {
                center_table[nearest2] = new List<int>();
            }
            //Debug.Log(m_points[100].x);
            center_table[nearest1].Add(i);
            center_table[nearest2].Add(i);
            int[] tmp = new int[2];

            tmp[0] = nearest1;
            tmp[1] = nearest2;
            edge_table[i] = tmp;
        }
    }

    private void update_table_city() {
        int nb_park = 0;
        for (int i = 0; i < m_points.Count; i++) {
            float mean = 0f;
            for (int j = 0; j < center_table[i].Count; j++) {
                LineSegment seg = m_edges[center_table[i][j]];
                Vector2 left = (Vector2)seg.p0;
                Vector2 right = (Vector2)seg.p1;
                Vector2 segment = (left+right)/2;
                mean += (segment - m_points[i]).magnitude;
            }
            if (mean / center_table[i].Count < ratio_city)
            {
                if (nb_park < 3 && Random.Range(1, 100) < 3) {
                    isCity[i] = 2;
                    nb_park++;
                }
                else { 
                    isCity[i] = 0; 
                }
                    
            }
            else {

                isCity[i] = 1;
            }
        }
    }

    void Start ()
	{
        ag = GetComponent<AgentControlerInitial>();
        float [,] map = createMap();
        float[,] map2 = new float[WIDTH, HEIGHT];
        Color[] pixels = createPixelMap(map);
        float sum = 0;
        for (int i = 0; i <= map.GetUpperBound(0); i++) {
            for (int j = 0; j <= map.GetUpperBound(1); j++) {
                sum = sum + map[i,j];
            }
        }
        for (int i = 0; i <= map.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= map.GetUpperBound(1); j++)
            {
                map2[i,j] = map[i,j]/sum;
            }
        }

        /* Create random points points */

        m_points = new List<Vector2> ();
		List<uint> colors = new List<uint> ();
        for (int i = 0; i < NPOINTS; i++) {
            colors.Add((uint)0);
            int[] col = {0, 0};
            col = getRandomValue(map2);
            //Vector2 vec = new Vector2(Random.Range(0, WIDTH - 1), Random.Range(0, HEIGHT - 1));
            //Debug.Log(vec);
            Vector2 vec = new Vector2(col[0],col[1]);
            //Debug.Log(vec);
            m_points.Add (vec);
		}

		/* Generate Graphs */
		Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, WIDTH, HEIGHT));
		m_edges = v.VoronoiDiagram ();
		m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation ();
		

		/* Shows Voronoi diagram */
		Color color = Color.blue;
        update_table();
        update_table_city();
		for (int i = 0; i < m_edges.Count; i++) {
			LineSegment seg = m_edges [i];			
			Vector2 left = (Vector2)seg.p0;
			Vector2 right = (Vector2)seg.p1;
            Vector2 segment = (right - left) / WIDTH * 10;
            float angle = Vector2.SignedAngle(Vector2.right, right-left);
            // 10 taille du plan et 5 c'est le décalage du plan
            // Changer emission tiling -1 et -1
            GameObject go;
            if (isCity[edge_table[i][0]] == 0)
            {
                go = Instantiate(road, new Vector3(5 - left.y / WIDTH * plane_factor, 0, 5 - left.x / HEIGHT * plane_factor), Quaternion.Euler(0, angle + 90, 0));
            }
            else {
                go = Instantiate(highway, new Vector3(5 - left.y / WIDTH * plane_factor, 0, 5 - left.x / HEIGHT * plane_factor), Quaternion.Euler(0, angle + 90, 0));
            }
            go.transform.localScale = new Vector3(segment.magnitude, 1, 1);
            Vector3 tmpPosition = go.transform.localPosition;
            Vector3 tmpPositionlb = go.transform.localPosition + new Vector3(-0.45f,0f,-0.025f);
            Vector3 tmpPositionrb = go.transform.localPosition + new Vector3(-0.45f, 0f, 0.025f);
            //Vector3 tmpPositionle = go.transform.localPosition + new Vector3(0.45f, 0f, -0.025f);
            //Vector3 tmpPositionre = go.transform.localPosition + new Vector3(0.45f, 0f, 0.025f);
            Vector3 center_road = new Vector3(5 - (left.y + right.y) / (2*WIDTH) * plane_factor, 0f,5 - (left.x + right.x) / (2*HEIGHT) * plane_factor);
            Vector3 perp = Quaternion.Euler(0, angle+80, 0) * new Vector3(0f,0f,0.1f);
            //perp = perp.normalized*0.06f;
            Vector3 tmp_building_posr = center_road;
            Vector3 tmp_building_posl = center_road;
            Vector3 building_posr = new Vector3(tmp_building_posr.x, 0f, tmp_building_posr.z) - perp;
            Vector3 building_posl = new Vector3(tmp_building_posl.x, 0f, tmp_building_posl.z) + perp;
            //building_posr =  go.transform.TransformPoint(building_posr);
            //building_posl = go.transform.TransformPoint(building_posl);
            //Instantiate(road, new Vector3(5 - left.y / WIDTH * plane_factor, 0, 5 - left.x / HEIGHT * plane_factor), Quaternion.Euler(0, angle + 90, 0));
            //Instantiate(road, new Vector3(5 - left.y / WIDTH * plane_factor, 0, 5 - left.x / HEIGHT * plane_factor), Quaternion.Euler(0, angle + 90, 0));

            if (isCity[edge_table[i][0]] == 0)
            {
                if (nb_rest <= 2 && Random.Range(0,100) <3) {
                    nb_rest += 1;
                    GameObject resta = Instantiate(restau, building_posl, Quaternion.Euler(0, angle + 90, 0));
                    list_restau.Add(go);
                    list_restau2.Add(resta);
                }
                else if (segment.magnitude > 0.15)
                {
                    GameObject skyscrapl = Instantiate(skyscraper, building_posl, Quaternion.Euler(0, angle + 90, 0));
                    GameObject skyscrapr = Instantiate(skyscraper, building_posr, Quaternion.Euler(0, angle + 90, 0));
                    list_skys.Add(go);
                    list_skys.Add(go);
                    list_skys2.Add(skyscrapl);
                    list_skys2.Add(skyscrapr);
                    //Destroy(go);
                    if (segment.magnitude < 1.1f * skyscr_size)
                    {

                        float rapp = (segment.magnitude) / (1.1f * skyscr_size);
                        //Debug.Log(rapp);
                        skyscrapl.transform.localScale = skyscrapl.transform.localScale * Mathf.Max(0.2f, rapp);
                        skyscrapr.transform.localScale = skyscrapr.transform.localScale * Mathf.Max(0.2f, rapp);
                    }
                }
            }
            else if (isCity[edge_table[i][0]] == 1)
            {
                /*int nb_house = Mathf.FloorToInt(segment.magnitude / 1.5f * home_size);
                Vector2 single_length = segment / (nb_house*2);
                Vector3 add_length = new Vector3(single_length.x, 0f, single_length.y);*/
                if (segment.magnitude > 0.2)
                {
                    GameObject house1 = Instantiate(house, building_posl + new Vector3(0f, 0.04f, 0f), Quaternion.Euler(0, angle + 90, 0));
                    GameObject house2 = Instantiate(house, building_posr + new Vector3(0f, 0.04f, 0f), Quaternion.Euler(0, angle + 90, 0));
                    list_house.Add(go);
                    list_house.Add(go);
                    list_house2.Add(house1);
                    list_house2.Add(house2);
                }
            }
            else if (isCity[edge_table[i][0]] == 2)
            {
                park_list.Add(go);
                isCity[edge_table[i][0]]++;
            }
                DrawLine (pixels,left, right,color);
		}

		/* Shows Delaunay triangulation */
		/*
 		color = Color.red;
		if (m_delaunayTriangulation != null) {
			for (int i = 0; i < m_delaunayTriangulation.Count; i++) {
					LineSegment seg = m_delaunayTriangulation [i];				
					Vector2 left = (Vector2)seg.p0;
					Vector2 right = (Vector2)seg.p1;
					DrawLine (pixels,left, right,color);
			}
		}*/

		/* Shows spanning tree */
		/*
		color = Color.black;
		if (m_spanningTree != null) {
			for (int i = 0; i< m_spanningTree.Count; i++) {
				LineSegment seg = m_spanningTree [i];				
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				DrawLine (pixels,left, right,color);
			}
		}*/

		/* Apply pixels to texture */
		tx = new Texture2D(WIDTH, HEIGHT);
        land.SetTexture ("_MainTex", tx);
		tx.SetPixels (pixels);
		tx.Apply ();
        
        

        plane.GetComponent<NavMeshSurface>().BuildNavMesh();
        ag.initializeAgent();
    }



    /* Functions to create and draw on a pixel array */
    private Color[] createPixelMap(float[,] map)
    {
        Color[] pixels = new Color[WIDTH * HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                pixels[i * HEIGHT + j] = Color.Lerp(Color.white, Color.black, map[i, j]);
            }
        return pixels;
    }
    private void DrawPoint (Color [] pixels, Vector2 p, Color c) {
		if (p.x<WIDTH&&p.x>=0&&p.y<HEIGHT&&p.y>=0) 
		    pixels[(int)p.x*HEIGHT+(int)p.y]=c;
	}
	// Bresenham line algorithm
	private void DrawLine(Color [] pixels, Vector2 p0, Vector2 p1, Color c) {
		int x0 = (int)p0.x;
		int y0 = (int)p0.y;
		int x1 = (int)p1.x;
		int y1 = (int)p1.y;

		int dx = Mathf.Abs(x1-x0);
		int dy = Mathf.Abs(y1-y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx-dy;
		while (true) {
            if (x0>=0&&x0<WIDTH&&y0>=0&&y0<HEIGHT)
    			pixels[x0*HEIGHT+y0]=c;

			if (x0 == x1 && y0 == y1) break;
			int e2 = 2*err;
			if (e2 > -dy) {
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx) {
				err += dx;
				y0 += sy;
			}
		}
	}
}