using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class Level01Tests
{
    private PlayerMovement movement;
    private PlayerHealth health;
    private Rigidbody2D body;

    [UnitySetUp]
    public IEnumerator LoadLevel()
    {
        SceneManager.LoadScene("Level_01");
        yield return null;
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        movement = player.GetComponent<PlayerMovement>();
        health = player.GetComponent<PlayerHealth>();
        body = player.GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerInputHandler>().enabled = false;
    }

    private void Teleport(Vector2 position)
    {
        body.position = position;
        body.linearVelocity = Vector2.zero;
        movement.transform.position = position;
        Physics2D.SyncTransforms();
    }

    private static T Find<T>(string name) where T : Component
    {
        foreach (T item in Object.FindObjectsByType<T>(FindObjectsInactive.Include))
            if (item.name == name) return item;
        return null;
    }

    [UnityTest]
    public IEnumerator Level_Has_All_Systems()
    {
        Assert.IsNotNull(GameManager.Instance);
        Assert.IsNotNull(AudioManager.Instance);
        Assert.IsNotNull(VFXManager.Instance);
        Assert.IsNotNull(Camera.main.GetComponent<CameraFollow>());
        Assert.AreEqual(9, GameManager.Instance.FruitsTotal);
        Assert.AreEqual(3, Object.FindObjectsByType<EnemyPatrol>().Length);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Player_Lands_And_Is_Grounded()
    {
        yield return new WaitForSeconds(1f);
        Assert.IsTrue(movement.IsGrounded, "El jugador debería estar sobre el suelo del Tilemap");
        Assert.That(body.position.y, Is.EqualTo(1.75f).Within(0.2f));
    }

    [UnityTest]
    public IEnumerator Player_Runs_And_Jumps_Raising_Events()
    {
        bool jumped = false;
        GameEvents.OnPlayerJumped += pos => jumped = true;
        yield return new WaitForSeconds(0.6f);

        float startX = body.position.x;
        movement.Move(1f);
        yield return new WaitForSeconds(0.8f);
        Assert.Greater(body.position.x, startX + 3f, "Debe avanzar a la derecha");

        float groundY = body.position.y;
        movement.SetJumpHeld(true);
        movement.RequestJump();
        yield return new WaitForSeconds(0.25f);
        Assert.IsTrue(jumped, "Debe publicarse OnPlayerJumped");
        Assert.Greater(body.position.y, groundY + 1f, "Debe elevarse al saltar");
    }

    [UnityTest]
    public IEnumerator Dash_Moves_Player_Fast()
    {
        yield return new WaitForSeconds(0.6f);
        float startX = body.position.x;
        movement.RequestDash();
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(movement.IsDashing);
        Assert.Greater(body.position.x - startX, 1.2f);
    }

    [UnityTest]
    public IEnumerator Collecting_Fruit_Updates_HUD_By_Events()
    {
        Collectible fruit = Object.FindFirstObjectByType<Collectible>();
        Teleport(fruit.transform.position);
        yield return new WaitForSeconds(0.3f);

        Assert.AreEqual(1, GameManager.Instance.FruitsCollected);
        Assert.AreEqual("1 / 9", Find<TMP_Text>("FruitsLabel").text);
    }

    [UnityTest]
    public IEnumerator Damage_Updates_Health_Bar_And_Blocks_Repeated_Hits()
    {
        health.TakeDamage(1, Vector2.zero);
        health.TakeDamage(1, Vector2.zero);
        yield return null;

        Assert.AreEqual(2, health.CurrentHealth, "La invulnerabilidad evita doble daño");
        Assert.That(Find<Image>("HealthFill").fillAmount, Is.EqualTo(2f / 3f).Within(0.01f));
        Assert.AreEqual("2/3", Find<TMP_Text>("HealthLabel").text);
    }

    [UnityTest]
    public IEnumerator Spikes_Hurt_The_Player()
    {
        Hazard spikes = Object.FindFirstObjectByType<Hazard>();
        foreach (Hazard hazard in Object.FindObjectsByType<Hazard>())
            if (hazard.name == "Spikes") { spikes = hazard; break; }

        Teleport(spikes.transform.position);
        yield return new WaitForSeconds(0.3f);
        Assert.Less(health.CurrentHealth, 3);
    }

    [UnityTest]
    public IEnumerator Death_Shows_GameOver_Panel()
    {
        Assert.IsNull(GameObject.Find("GameOverPanel"));
        health.Kill();
        yield return new WaitForSeconds(1.3f);
        Assert.IsNotNull(GameObject.Find("GameOverPanel"), "Debe aparecer el panel de Game Over");
        Assert.IsTrue(health.IsDead);
    }

    [UnityTest]
    public IEnumerator Falling_Into_The_Void_Kills_The_Player()
    {
        Teleport(new Vector2(22f, -3f));
        yield return new WaitForSeconds(1.5f);
        Assert.IsTrue(health.IsDead);
    }

    [UnityTest]
    public IEnumerator Pause_Freezes_Time_And_Shows_Panel()
    {
        GameEvents.PauseRequested();
        yield return null;
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsNotNull(GameObject.Find("PausePanel"));

        GameEvents.PauseRequested();
        yield return null;
        Assert.AreEqual(1f, Time.timeScale);
        Assert.IsNull(GameObject.Find("PausePanel"));
    }

    [UnityTest]
    public IEnumerator Stomping_An_Enemy_Defeats_It_And_Bounces()
    {
        bool defeated = false;
        GameEvents.OnEnemyDefeated += pos => defeated = true;

        EnemyPatrol enemy = Object.FindFirstObjectByType<EnemyPatrol>();
        yield return new WaitForSeconds(0.5f);
        Vector2 above = (Vector2)enemy.transform.position + new Vector2(0f, 2.6f);
        Teleport(above);
        body.linearVelocity = new Vector2(0f, -6f);
        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(defeated, "Pisar al enemigo lo derrota");
        Assert.AreEqual(3, health.CurrentHealth, "Pisotón no debe dañar al jugador");
    }

    [UnityTest]
    public IEnumerator Touching_An_Enemy_From_The_Side_Hurts()
    {
        EnemyPatrol enemy = Object.FindFirstObjectByType<EnemyPatrol>();
        yield return new WaitForSeconds(0.5f);
        Teleport((Vector2)enemy.transform.position + new Vector2(-1.2f, 0f));
        yield return new WaitForSeconds(0.6f);
        Assert.Less(health.CurrentHealth, 3);
    }

    [UnityTest]
    public IEnumerator Reaching_Goal_Without_Fruits_Shows_Hint_Not_Victory()
    {
        Goal goal = Object.FindFirstObjectByType<Goal>();
        Teleport(goal.transform.position);
        yield return new WaitForSeconds(0.4f);
        Assert.IsFalse(GameManager.Instance.IsFinished);
        StringAssert.Contains("frutas", Find<TMP_Text>("HintLabel").text);
    }

    [UnityTest]
    public IEnumerator Collecting_All_Fruits_Then_Goal_Completes_Level()
    {
        bool completed = false;
        GameEvents.OnLevelCompleted += () => completed = true;

        foreach (Collectible fruit in Object.FindObjectsByType<Collectible>())
        {
            Teleport(fruit.transform.position);
            yield return new WaitForSeconds(0.15f);
        }
        Assert.AreEqual(9, GameManager.Instance.FruitsCollected);

        Teleport(Object.FindFirstObjectByType<Goal>().transform.position);
        yield return new WaitForSeconds(0.4f);
        Assert.IsTrue(completed, "Debe publicarse OnLevelCompleted");
        yield return new WaitForSeconds(1f);
        Assert.IsNotNull(GameObject.Find("VictoryPanel"));
    }

    [UnityTest]
    public IEnumerator Camera_Follows_Player_Within_Level_Bounds()
    {
        yield return new WaitForSeconds(0.5f);
        Teleport(new Vector2(50f, 4f));
        yield return new WaitForSeconds(1.5f);
        Assert.That(Camera.main.transform.position.x, Is.EqualTo(50f).Within(4f));
        Assert.Greater(Camera.main.transform.position.x, 30f);
    }

    [UnityTest]
    public IEnumerator Animator_Controller_Reaches_Idle_And_Run_States()
    {
        Animator animator = movement.GetComponentInChildren<Animator>();
        yield return new WaitForSeconds(0.8f);
        Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

        movement.Move(1f);
        yield return new WaitForSeconds(0.4f);
        Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Run"));

        movement.Move(0f);
        movement.RequestJump();
        yield return new WaitForSeconds(0.15f);
        Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"));
    }
}

public class MainMenuTests
{
    [UnityTest]
    public IEnumerator MainMenu_Has_Buttons_And_Mixer_Parameters()
    {
        SceneManager.LoadScene("MainMenu");
        yield return null;
        yield return null;

        Assert.IsNotNull(GameObject.Find("PlayButton"));
        Assert.IsNotNull(GameObject.Find("QuitButton"));
        Assert.IsNotNull(AudioManager.Instance);

        Slider music = GameObject.Find("MusicSlider").GetComponent<Slider>();
        music.value = 0.25f;
        yield return null;
        Assert.That(AudioManager.Instance.MusicVolume, Is.EqualTo(0.25f).Within(0.001f));

        UnityEngine.Audio.AudioMixer mixer = Resources.FindObjectsOfTypeAll<UnityEngine.Audio.AudioMixer>()[0];
        Assert.IsTrue(mixer.GetFloat(AudioManager.MusicParameter, out float decibels));
        Assert.That(decibels, Is.EqualTo(-12.04f).Within(0.1f));
    }
}

public class CompletabilityTests
{
    // Un bot corre hacia la derecha y salta ante fosos o paredes: comprueba que el nivel se puede recorrer completo.
    [UnityTest]
    public IEnumerator Bot_Can_Traverse_Level_From_Start_To_Goal()
    {
        SceneManager.LoadScene("Level_01");
        yield return null;
        yield return null;

        foreach (EnemyPatrol enemy in Object.FindObjectsByType<EnemyPatrol>()) Object.Destroy(enemy.gameObject);
        foreach (Hazard hazard in Object.FindObjectsByType<Hazard>())
            if (hazard.name == "Spikes") Object.Destroy(hazard.gameObject);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerInputHandler>().enabled = false;
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        int ground = LayerMask.GetMask("Ground");
        float goalX = Object.FindFirstObjectByType<Goal>().transform.position.x;

        float elapsed = 0f;
        float jumpHold = 0f;
        while (elapsed < 40f && body.position.x < goalX - 1f && !player.GetComponent<PlayerHealth>().IsDead)
        {
            Vector2 pos = body.position;
            movement.Move(1f);
            jumpHold -= Time.deltaTime;
            movement.SetJumpHeld(jumpHold > 0f);

            bool gapAhead = !Physics2D.Raycast(pos + new Vector2(1.6f, -0.6f), Vector2.down, 2f, ground);
            bool wallAhead = Physics2D.Raycast(pos + new Vector2(0f, -0.5f), Vector2.right, 1.1f, ground);
            if (movement.IsGrounded && (gapAhead || wallAhead)) { movement.RequestJump(); jumpHold = 0.3f; }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.Greater(body.position.x, goalX - 2f, $"El bot quedó atascado en x={body.position.x:F1}, y={body.position.y:F1}");
    }
}
