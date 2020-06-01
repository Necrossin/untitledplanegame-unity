using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameBehaviour : MonoBehaviour
{

    public static GameBehaviour Instance;

    public GameObject PlayerEntity;
    public AudioMixer mainMixer;
    public AudioMixerGroup mainMixerGroup;

    [SerializeField]
    private GameObject cloudPrefab;

    [SerializeField]
    private GameObject darkCloudPrefab;

    [SerializeField]
    private Camera camera_obj;

    [SerializeField]
    private GameObject scoreTextPrefab;

    [SerializeField]
    private List<GameObject> projectileTypes;

    [SerializeField]
    private List<GameObject> enemyTypes;

    [SerializeField]
    private GameObject pauseMenu;
    [SerializeField]
    private GameObject optionsMenu;
    [SerializeField]
    private GameObject resultsPanel;

    private int maxEnemies = 35;
    private int curMaxEnemies = 0;
    private float maxEnemiesSpawnTime = 90f;
    private float curMaxEnemiesSpawnTime = 0;
    private int enemyCount = 0;
    private float nextEnemySpawn = 0;

    public float level_width = 1000;
    public float level_height = 500;

    private Vector2 level_mins;
    private Vector2 level_maxs;

    private float sky_level = 0.15f;
    private float water_level = 0.15f;

    private bool player_dead = false;
    private bool didRestart = false;

    private float teleport_fraction = 0.25f;

    private float cloud_z = 5;
    private bool gameStarted = false;

    private int score = 0;
    private int temp_score = 0;
    private int cur_combo = 0;
    private float combo_time = 0;
    private float combo_dur = 7;

    private bool gamePaused = false;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.visible = false;
        setupLevelBounds();
        createClouds();

        PlayerEntity.transform.position = new Vector3(0, getWaterLevel() + 4, 0);
        PlayerEntity.SetActive(false);

        float vol = PlayerPrefs.GetFloat("MasterVolume", 0);
        mainMixer.SetFloat("masterVolume", vol);
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseGame(!isGamePaused());
        }

        if (!gameStarted)
        {
            if (Input.GetAxisRaw("Vertical") > 0)
                startGame();
            return;
        }
            

        float spawn_delta = Mathf.Clamp(1 - (curMaxEnemiesSpawnTime - Time.time) / maxEnemiesSpawnTime, 0, 1);
        int calculated_max = Mathf.CeilToInt((maxEnemies - 3) * spawn_delta);

        curMaxEnemies = 3 + calculated_max;

        spawnEnemiesOverTime( 1.2f );

        handleScore();

        if ( player_dead && !didRestart )
        {
            didRestart = true;
            forceFinishTempScore();
            Invoke("showResultsScreen", 0.5f);
            //showResultsScreen();
            //Invoke("restartGame", 6);
        }

    }

    public void startGame()
    {
        gameStarted = true;
        PlayerEntity.SetActive(true);
        PlayerEntity.GetComponent<Rigidbody2D>().AddForce( new Vector2( 0, 630 ), ForceMode2D.Impulse);
        curMaxEnemiesSpawnTime = Time.time + maxEnemiesSpawnTime;
    }

    private void setupLevelBounds()
    {
        level_mins.x = -1 * level_width / 2;
        level_mins.y = -1 * level_height / 2;

        level_maxs.x = level_width / 2;
        level_maxs.y = level_height / 2;
    }

    private void createClouds()
    {
        int amount = Random.Range(30, 40);

        for ( int i=1; i<=amount; i++)
        {
            float scale = Random.Range(3f, 13f);
            var pos = new Vector3(Random.Range(levelMins().x * 0.9f, levelMaxs().x * 0.9f), Random.Range(getWaterLevel() * 0.8f, levelMaxs().y), cloud_z);
            GameObject cloud = Instantiate(cloudPrefab, pos, Quaternion.identity);
            cloud.transform.GetChild(0).transform.localScale = new Vector3(scale * Random.Range(1.2f, 1.5f), scale, 1);
        }

        amount = Random.Range(60, 90);

        for (int i = 1; i <= amount; i++)
        {
            float scale = Random.Range(1f, 3f);
            var pos = new Vector3(Random.Range(levelMins().x * 0.9f, levelMaxs().x * 0.9f), Random.Range(getWaterLevel() * 0.8f, levelMaxs().y), cloud_z + 1);
            GameObject cloud = Instantiate(darkCloudPrefab, pos, Quaternion.identity);
            cloud.transform.GetChild(0).transform.localScale = new Vector3(scale * Random.Range(1.2f, 2f), scale, 1);
            var cloud_logic = cloud.GetComponent<CloudLogic>();
            cloud_logic.setBackLayer(true);
        }

    }

    public Vector2 levelMins()
    {
        return level_mins;
    }

    public Vector2 levelMaxs()
    {
        return level_maxs;
    }

    public float getSkyLevel()
    {
        return level_maxs.y - level_height * sky_level;
    }

    public float getSkyLevelHeight()
    {
        return level_height * sky_level;
    }

    public float getWaterLevel()
    {
        return level_mins.y + getWaterLevelHeight();
    }

    public float getWaterLevelHeight()
    {
        return 45;// level_height * water_level;
    }

    public float getCameraX()
    {
        return camera_obj.transform.position.x;
    }

    public float getCameraY()
    {
        return camera_obj.transform.position.y;
    }

    // object on left side, PLAYER is on right side
    private bool isOnLeftSide( float x, bool isCamera = false )
    {
        if ( isCamera )
            return x < (level_mins.x + level_width * teleport_fraction);

        return isOnRightSide( getCameraX(), true ) && x < (level_mins.x + level_width * teleport_fraction); 
    }

    // object on right side, PLAYER is on left side
    private bool isOnRightSide(float x, bool isCamera = false)
    {
        if (isCamera)
            return x > (level_maxs.x - level_width * teleport_fraction);

        return isOnLeftSide( getCameraX(), true ) && x > (level_maxs.x - level_width * teleport_fraction);
    }

    public float calculateLoopingX( float x, bool resetOutOfBounds = false )
    {
        if (isOnLeftSide(x))
            return x + level_width;

        if (isOnRightSide(x))
            return x - level_width;

        if (resetOutOfBounds)
        {
            if (x < level_mins.x && !isOnLeftSide(getCameraX(), true))
                return x + level_width;

            if (x > level_maxs.x && !isOnRightSide(getCameraX(), true))
                return x - level_width;
        }

        return x;
    }

    public void doScreenshake(Vector3 pos, float am, float dur)
    {
        camera_obj.GetComponent<CameraLogic>().doScreenshake(pos, am, dur);
    }

    public void createProjectile( GameObject proj_prefab, Vector3 pos, Vector3 dir, float sp, float dmg, int tm, Vector3 extra_vel )
    {
        GameObject proj = Instantiate( proj_prefab, pos, Quaternion.identity);
        var proj_data = proj.GetComponent<ProjectileLogic>();
        proj_data.setupProjectile(dir, sp, dmg, tm, extra_vel);
    }

    public void createEnemy( GameObject enemy_prefab, Vector3 pos )
    {
        GameObject enemy = Instantiate(enemy_prefab, pos, Quaternion.identity);
    }

    private void spawnEnemiesOverTime( float delay )
    {
        if ( getEnemyCount() < curMaxEnemies && nextEnemySpawn <= Time.time )
        {
            int type = 0;
            // small ship
            int rand = Random.Range(1, 8);
            if ( rand == 1 )
                type = 1;

            // jet
            rand = Random.Range(1, 6);
            if (rand == 1 && type == 0)
                type = 2;

            // big ship
            rand = Random.Range(1, 25);
            if (rand == 1 && type == 0)
                type = 3;

            // ace
            rand = Random.Range(1, 30);
            if (rand == 1 && type == 0)
                type = 4;

            int dir = Random.Range(1, 3) == 1 ? 1 : -1;

            float cam_size = camera_obj.orthographicSize * camera_obj.aspect;
            float spawn_x = getCameraX() + (cam_size * 1.3f + Random.Range(10, levelMaxs().x/2 - 10)) * dir;

            var spawn_pos = new Vector3(spawn_x, Random.Range(0, levelMaxs().y + 40), 0);
            //var spawn_pos = new Vector3( Random.Range(levelMins().x + 10, levelMaxs().x - 10), Random.Range(0, levelMaxs().y + 40), 0 );

            if ( Mathf.Abs( spawn_pos.x - getCameraX() ) < cam_size)
            {
                if (spawn_pos.x > getCameraX())
                    spawn_pos.x += cam_size * 1.5f;
                else
                    spawn_pos.x -= cam_size * 1.5f;
            }

            createEnemy(getEnemyType( type ), spawn_pos);
            incrementEnemyCount();
            nextEnemySpawn = Time.time + delay;
        }
    }

    public GameObject getProjectileType( int index )
    {
        return projectileTypes[index];
    }

    public GameObject getEnemyType( int index )
    {
        return enemyTypes[index];
    }

    private void incrementEnemyCount()
    {
        enemyCount += 1;
    }

    public void decrementEnemyCount()
    {
        enemyCount -= 1;
    }

    public int getEnemyCount()
    {
        return enemyCount;
    }

    public void setPlayerDead( bool dead )
    {
        player_dead = dead;
    }

    public bool isPlayerDead()
    {
        return player_dead;
    }

    public void restartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public bool isGameActive()
    {
        return gameStarted;
    }

    public void addScore( int sc, Vector3 pos )
    {
        if (player_dead)
            return;
        
        if (cur_combo < 20)
            cur_combo += 1;

        pos.z = 4.5f;

        GameObject scoreText = Instantiate(scoreTextPrefab, pos, Quaternion.identity);
        scoreText.GetComponentInChildren<TextMeshPro>().SetText((sc * cur_combo).ToString());

        temp_score += sc * cur_combo;
        combo_time = Time.time + combo_dur;
    }

    public void addStaticScore( int sc )
    {
        score += sc;
    }

    public int getScore()
    {
        return score;
    }

    public int getTempScore()
    {
        return temp_score;
    }

    private void forceFinishTempScore()
    {
        combo_time = 0;
    }

    private void handleScore()
    {
        if ( combo_time <= Time.time && temp_score > 0 )
        {
            score += temp_score;
            temp_score = 0;
            cur_combo = 0;
        }
    }

    private int getHighScore()
    {
        return PlayerPrefs.GetInt("UPG_HighScore", 0);
    }

    private bool checkHighScore()
    {
        if ( getScore() > getHighScore())
        {
            PlayerPrefs.SetInt("UPG_HighScore", getScore());
            return true;
        }
        return false;
    }

    public float getComboDelta()
    {
        return Mathf.Clamp01( 1 - ( combo_time - Time.time ) / combo_dur );
    }  
    
    public int getCombo()
    {
        return cur_combo;
    }

    public bool isOnScreen( Vector3 pos, float scale = 1 )
    {
        float cam_x = getCameraX();
        float cam_y = getCameraY();

        Vector2 cam_mins = new Vector2(cam_x - camera_obj.orthographicSize * camera_obj.aspect * scale, cam_y - camera_obj.orthographicSize * scale);
        Vector2 cam_maxs = new Vector2(cam_x + camera_obj.orthographicSize * camera_obj.aspect * scale, cam_y + camera_obj.orthographicSize * scale);

        return pos.x > cam_mins.x && pos.x < cam_maxs.x && pos.y > cam_mins.y && pos.y < cam_maxs.y;
    }

    public Vector3 toScreenView( Vector3 pos, float scale = 0.9f )
    {

        float cam_x = getCameraX();
        float cam_y = getCameraY();

        Vector2 cam_mins = new Vector2(cam_x - camera_obj.orthographicSize * camera_obj.aspect * scale, cam_y - camera_obj.orthographicSize * scale);
        Vector2 cam_maxs = new Vector2(cam_x + camera_obj.orthographicSize * camera_obj.aspect * scale, cam_y + camera_obj.orthographicSize * scale);

        pos.x = Mathf.Clamp(pos.x, cam_mins.x, cam_maxs.x);
        pos.y = Mathf.Clamp(pos.y, cam_mins.y, cam_maxs.y);

        return pos;
    }

    public void pauseGame( bool bl )
    {
        // not the best place for this but whatever
        if (optionsMenu.activeSelf)
        {
            optionsMenu.GetComponent<OptionsMenuLogic>().ReturnToMenu();
            return;
        }

        if (player_dead)
            resultsPanel.SetActive(!bl);

        gamePaused = bl;
        Time.timeScale = bl ? 0f : 1f;
        pauseMenu.SetActive(bl);
        
        Cursor.visible = bl;
    }

    public bool isGamePaused()
    {
        return gamePaused;
    } 
    
    private void showResultsScreen()
    {
        resultsPanel.SetActive(true);
        bool newHighscore = checkHighScore();
        var resPanelLogic = resultsPanel.GetComponent<ResultsPanelLogic>();
        resPanelLogic.updateScoreResults(getHighScore(), getScore(), newHighscore);
    }

}

